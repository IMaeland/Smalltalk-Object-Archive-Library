using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PW.Smalltalk.Archive.Primatives
{
    public class SmalltalkClass : IArchiveNode
    {        
        public const string SymbolClassName = "Symbol";        
        public const string DBSymbolClassName = "DoubleByteSymbol";
        public const string StringClassName = "String";
        public const string GuidClassName = "GUID";
        public const string FloatClassName = "Float";        
        public const string UndefinedObjectName = "UndefinedObject";
        public const int UndefinedObjectId = 1;
        public const string TrueName = "True";
        public const int TrueId = 2;
        public const string FalseName = "False";
        public const int FalseId = 3;
        public const string MetaClassName = "MetaClass";
        public const int MetaClassId = 4;
        public const string SmalltalkName = "Smalltalk";
        public const int SmalltalkId = 5;
        public const string SymbolTableName = "SymbolTable";
        public const int SymbolTableId = 6;
        public const string NotifierName = "Notifier";
        public const int NotifierId = 7;
        public const string ProcessorName = "Processor";
        public const int ProcessorId = 8;
        public const string ClipboardName = "Clipboard";
        public const int ClipboardId = 9;
        public const string OperatingSystemName = "OperatingSystem";
        public const int OperatingSystemId = 10;
        public const string DisplayName = "Display";
        public const int DisplayId = 11;
        public const string OrderedCollectionClassName = "OrderedCollection";

        public const byte TagClass = 1;
        public const byte TagMetaClass = 2;
        public const byte RepFixedPointers = 1;
        public const byte RepVariablePointers = 2;
        public const byte RepVariableBytes = 3;
        public const byte ClassIdSymbol = 1;
        public const byte ClassIdDBSymbol = 2;

        public string LibraryName { get; }
        public string ClassName { get; }
        public long ClassId { get; }
        public byte Rep { get; set; }

        public bool IsMetaClass { get; set; }
        public bool IsPredefined { get; set; }

        public bool IsPointer => Rep == RepVariablePointers || Rep == RepFixedPointers;
        public bool IsVariable => Rep == RepVariablePointers;
        public bool IsBytes => Rep == RepVariableBytes;

        public bool IsHidden => ClassId == ClassIdDBSymbol || ClassId == ClassIdSymbol;

        public IList<string> InstVarNames { get; }

        string IArchiveNode.Title => "Class";
        

        string IArchiveNode.Preview
        {
            get
            {
                var buff = new System.Text.StringBuilder();
                buff.Append("<html>");
                buff.Append($"<h1>{ClassName}</h1>");
                buff.Append("<ol>");
                foreach (var ivar in InstVarNames)
                {
                    buff.Append($"<li>{ivar}</li>");
                }
                buff.Append("</ol>");
                buff.Append("</html>");
                return buff.ToString();
            }
        }

        public List<object> Keys
        {
            get
            {
                var list = new List<object>();
                list.Add("self");
                list.Add("ClassName");
                list.Add("LibraryName");
                list.Add("InstVarNames");
                list.Add("IsPointer");
                list.Add("IsVariable");
                list.Add("IsBytes");
                list.Add("IsPredefined");                
                return list;
            }
        }

        object IArchiveNode.this[object key]
        {
            get
            {
                switch (key as string)
                {
                    case "ClassName":
                        return ClassName;
                    case "InstVarNames":
                        return string.Join(" ", InstVarNames);
                    case "IsPointer":
                        return IsPointer;
                    case "IsVariable":
                        return IsVariable;
                    case "IsBytes":
                        return IsBytes;
                    case "IsPredefined":
                        return IsPredefined;
                    case "LibraryName":
                        return LibraryName;
                    default:
                        return this;
                }
            }
        }

        public SmalltalkClass(long classId, string className, byte rep, IList<string> instVarNames)
        {
            ClassId = classId;
            ClassName = className;
            Rep = rep;
            InstVarNames = instVarNames??new List<string>();
        }

        public SmalltalkClass(int tag, long classId, string className, byte rep, IList<string> instVarNames) : this(classId, className, rep, instVarNames)
        {
            IsMetaClass = tag == TagMetaClass;
        }

        internal static SmalltalkClass PredefinedClass(int classId, string className, byte rep) 
        {
            return new SmalltalkClass(classId, className, rep, null) { IsPredefined = true };
        }

        public override string ToString()
        {
            return IsMetaClass ? $"{ClassName} class" : ClassName;
        }

        public override int GetHashCode()
        {
            return ClassName.GetHashCode();
        }
    }
}
