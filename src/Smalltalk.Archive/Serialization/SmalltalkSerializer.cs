using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PW.Smalltalk.Archive.Primatives;

namespace PW.Smalltalk.Archive.Serialization
{
    public class SmalltalkSerializer
    {        

        public SmalltalkArchive Deserialize(System.IO.Stream stream)
        {
            return readFrom(stream);
        }

        public void Serialize(System.IO.Stream stream, SmalltalkArchive archive)
        {
            dumpHeader(stream, archive.Header);
            dumpBehaviorDescriptors(stream, archive);
            dumpObjectDescriptors(stream, archive);
            dumpSoftSlots(stream, archive);
        }

        #region Archive Constants

        const int ESC = 27;
        const int NUL = 0;

        #endregion

        #region Writing

        private void dumpHeader(System.IO.Stream stream, SmalltalkArchiveHeader header)
        {
            stream.WriteByte(ESC);            
            WriteInteger(stream, header.Version);
            WriteInteger(stream, header.NumBehaviors);
            WriteInteger(stream, header.NumObjects);
            WriteInteger(stream, header.RootObjectId);            
            stream.WriteByte(NUL);
        }

        /* Write a <behavior descriptor> entry for each behavior
         * <behavior type> <id> <nameString> <NUL><representation><number of named instance vars> [<inst var name>]...<DLL name> <NUL>[version-dependent]
         * for class, store names of all the inst vars in <name> <NUL> form
         */
        private void dumpBehaviorDescriptors(System.IO.Stream stream, SmalltalkArchive archive)
        {
            foreach (SmalltalkClass @class in archive.Classes.Values.Where(c=> !c.IsHidden))
            {
                if (@class.IsMetaClass)
                {
                    stream.WriteByte(SmalltalkClass.TagMetaClass);                   
                } else
                {
                    stream.WriteByte(SmalltalkClass.TagClass);
                }

                WriteInteger(stream, @class.ClassId);
                WriteString(stream, @class.ClassName);
                stream.WriteByte(NUL);
                stream.WriteByte(@class.Rep);
                WriteInteger(stream, @class.InstVarNames.Count);

                foreach (var varName in @class.InstVarNames)
                {
                    WriteString(stream, varName);
                    stream.WriteByte(NUL);
                }

                WriteString(stream, @class.LibraryName??"");
                stream.WriteByte(NUL);                
            }
            stream.WriteByte(NUL);
        }

        /* Write an <object descriptor> for each (surrogate) object:
         * <object id> <class id> <number of indexed instance variables><basicHash> <bytesOrPointers>
         */
        private void dumpObjectDescriptors(System.IO.Stream stream, SmalltalkArchive archive)
        {
            var lastBehaviorId = archive.LastBehaviorId;

            foreach (var keyAndValue in archive.Objects)
            {
                if (!(keyAndValue.Value is SmalltalkObject))
                    continue;

                var objectId = keyAndValue.Key;
                var obj = (SmalltalkObject)keyAndValue.Value;

                long classId = obj.Class.ClassId;
                int objectSize = obj.Size;
                long objectHash = obj.Hash;
                bool isBytes = obj.Class.IsBytes;

                WriteInteger(stream, objectId);
                WriteInteger(stream, classId);
                WriteInteger(stream, objectSize);
                WriteInteger(stream, objectHash);

                if (obj is SmalltalkBytesObject)
                {
                    var bytes = ((SmalltalkBytesObject)obj).Bytes;
                    stream.Write(bytes, 0, bytes.Length);                    
                } else if (obj is SmalltalkVariableObject)
                {
                    var variable = (SmalltalkVariableObject)obj;

                    for (int i = 0; i < variable.InstVars.Count; i++)
                    {
                        var id = variable.InstVars[i].ObjectId;
                        WriteInteger(stream, id);
                    }

                } else if (obj is SmalltalkPointerObject)
                {
                    var ptr = (SmalltalkPointerObject)obj;
                    for (int i = 0; i < ptr.Class.InstVarNames.Count; i++)
                    {
                        var id = ptr.InstVars[i].ObjectId; 
                        WriteInteger(stream, id);
                    }
                } else
                {
                    throw new NotImplementedException($"Unexpected object {obj.GetType().Name}");
                }                
            }
            stream.WriteByte(NUL);
        }

        /* The soft slots section format is:
         * <soft slot type count>[ <soft slot values list> ]...NUL
         * where each type of soft slot has an entry containing the set selector
         * for that soft slot and a list of the id pairs containing the id of the
         * object with the soft slot value and the filed id of its value:
         * <set selector> [ <object id> <soft slot value id> ]... NUL
        */
        private void dumpSoftSlots(System.IO.Stream stream, SmalltalkArchive archive)
        {
            // Softslots disabled in Smalltalk
            WriteInteger(stream, 0); // Number of softslots
            stream.WriteByte(NUL); // Section Terminator
        }

        private void WriteInteger(System.IO.Stream stream, long value)
        {
            if (value < 128)
            {
                stream.WriteByte((byte)value);
            }
            else
            {
                byte a = (byte)((value % 128) + 128);
                long b = value / 128;

                stream.WriteByte(a);
                WriteInteger(stream, b);
            }
        }

        private void WriteString(System.IO.Stream stream, string value)
        {
            // TODO Add Double Byte String support
            var bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        #endregion

        #region Parsing
       
        private Dictionary<long, SmalltalkClass> DefaultClasses => new Dictionary<long, SmalltalkClass>
        {            
            { SmalltalkClass.ClassIdSymbol, SmalltalkClass.PredefinedClass(SmalltalkClass.ClassIdSymbol, SmalltalkClass.SymbolClassName, SmalltalkClass.RepVariableBytes)},
            { SmalltalkClass.ClassIdDBSymbol, SmalltalkClass.PredefinedClass(SmalltalkClass.ClassIdDBSymbol, SmalltalkClass.DBSymbolClassName, SmalltalkClass.RepVariableBytes)}
        };

        private Dictionary<long, object> DefaultObjects => new Dictionary<long, object>
        {
            { SmalltalkClass.UndefinedObjectId, null},
            { SmalltalkClass.TrueId, true},
            { SmalltalkClass.FalseId, false}            
        };

        private SmalltalkArchive readFrom(System.IO.Stream stream)
        {
            var header = readHeaderFrom(stream);

            var classes = DefaultClasses;

            int tag;
            while ((tag = stream.ReadByte()) != NUL)
            {
                var classId = readInteger(stream);
                var className = readString(stream, NUL);
                byte rep = (byte) stream.ReadByte();
                var numNamedInstVars = readInteger(stream);
                var filedInstVarNames = new List<string>();

                for (int i = 0; i < numNamedInstVars; i++)
                {
                    filedInstVarNames.Add(readString(stream, NUL));
                }

                var componentName = readString(stream, NUL);

                classes.Add(classId, new SmalltalkClass(tag, classId, className, rep, filedInstVarNames));
            }

            var archive = new SmalltalkArchive()
            {
                Header = header,
                Classes = classes
            };

            loadObjectDescriptors(stream, archive);
            //restoreInstanceVariables(archive);
            
            // Read to end or next ESC
            int nextByte = stream.ReadByte();
            while (nextByte >= 0)
            {
                if (nextByte == ESC) // Start of next archive
                {
                    stream.Position -= 1; // backup 1
                    break; // then quick
                }
                nextByte = stream.ReadByte();
            }

            return archive;
        } 
        
        private void loadObjectDescriptors(System.IO.Stream stream, SmalltalkArchive archive)
        {
            long objectId = 0;
            var objects = DefaultObjects;
            while ((objectId = readInteger(stream)) != NUL)
            {
                var classId = readInteger(stream);
                var size = (int) readInteger(stream);
                var @class = archive.Classes[classId];
                var hash = readInteger(stream);

                if (@class.IsBytes)
                {
                    var bytes = new byte[size];
                    stream.Read(bytes, 0, size);
                    var obj = new SmalltalkBytesObject(objectId, size, hash, @class, bytes);
                    objects.Add(objectId, obj);
                }
                else if (@class.IsVariable)
                {
                    var obj = new SmalltalkVariableObject(objectId, size, hash, @class);
                    loadInstVarIds(stream, archive, obj, size);
                    objects.Add(objectId, obj);
                }
                else
                {
                    var obj = new SmalltalkPointerObject(objectId, size, hash, @class);
                    loadInstVarIds(stream, archive, obj, size);
                    objects.Add(objectId, obj);
                }

            }
            archive.Objects = objects;
        }

        private SmalltalkArchiveHeader readHeaderFrom(System.IO.Stream stream)
        {
            if (stream.ReadByte() != ESC)
                throw new InvalidOperationException("Invalid Smalltalk Header");

            var objectVersion = readInteger(stream);
            var numBehaviors = readInteger(stream);
            var numObjects = readInteger(stream);
            var p = stream.Position;
            var rootObjectId = readInteger(stream);            
            if (stream.ReadByte() != NUL)
                throw new InvalidOperationException("Invalid Smalltalk Header");

            return new SmalltalkArchiveHeader
            {
                Version = objectVersion,
                NumBehaviors = numBehaviors,
                NumObjects = numObjects,
                RootObjectId = rootObjectId
            };
        }

        private void loadInstVarIds(System.IO.Stream stream, SmalltalkArchive archive, SmalltalkPointerObject obj, int size)
        {
            for (int i = 0; i < obj.Class.InstVarNames.Count; i++)
            {
                obj.InstVars[i] = new SmalltalkValueReference(archive, readInteger(stream));
            }

            if (size > 0)
            {
                if (obj.Class.IsVariable)
                {
                    for (int i = 0; i < size; i++)
                    {
                        obj.InstVars.Add(new SmalltalkValueReference(archive, (readInteger(stream))));
                    }
                }
                else
                { // Drop filed indexed values on the floor
                    for (int i = 0; i < size; i++)
                    {
                        readInteger(stream);
                    }
                }
            }
        }

        private long readInteger(System.IO.Stream stream)
        {
            var b = stream.ReadByte();
            if (b < 128) return b;
            var c = readInteger(stream) * 128 + b - 128;
            return c;
        }

        private long readLongInteger(System.IO.Stream stream)
        {
            var b = stream.ReadByte();
            if (b < 128) return b;
            var c = readLongInteger(stream) * 128 + b - 128;

            return c;
        }

        private string readString(System.IO.Stream stream, int terminator)
        {
            var buff = new StringBuilder();
            int b;

            while ((b = stream.ReadByte()) != terminator)
            {
                buff.Append((char)b);
            }

            return buff.ToString();
        }

        #endregion
    }
}


