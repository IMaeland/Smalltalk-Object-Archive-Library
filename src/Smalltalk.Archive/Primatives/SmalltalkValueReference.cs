using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PW.Smalltalk.Archive.Primatives
{
    public class SmalltalkValueReference : IArchiveNode
    {
        SmalltalkArchive Archive { get; }
        public long ObjectId { get; }

        public SmalltalkValueReference(SmalltalkArchive archive, long objectId)
        {
            Archive = archive;
            ObjectId = objectId;
        }

        private long IdIntegerZero => Archive.IdIntegerZero;
        private long IdCharacterZero => Archive.IdCharacterZero;

        public SmalltalkClass Class => (Value as SmalltalkObject)?.Class;

        public Guid? GuidValue => Bytes?.GuidValue;
        public string AsciiValue => Bytes?.AsciiValue;
        public double? DoubleValue => Bytes?.DoubleValue;

        public bool? BooleanValue => IsBoolean ? (bool?) IsTrue : null;

        public bool IsNil => ObjectId == SmalltalkClass.UndefinedObjectId;
        public bool IsTrue => ObjectId == SmalltalkClass.TrueId;
        public bool IsFalse => ObjectId == SmalltalkClass.FalseId;

        public bool IsBoolean => IsTrue | IsFalse;

        public bool HasLongValue => IsLongValue(ObjectId, Archive);
        public long? LongValue => IsLongValue(ObjectId, Archive) ? (long?)GetLongValue(ObjectId, IdIntegerZero) : null;

        public bool HasCharValue => IsCharacterValue(ObjectId, Archive);
        public char? CharValue => IsCharacterValue(ObjectId, Archive) ? (char?)(ObjectId - IdCharacterZero) : null;

        public bool HasObjectValue => IsObjectValue(ObjectId, Archive);


        public T ParseValue<T>()
        {
            object value = Value;
            return (T)Value;
        }

        public SmalltalkBytesObject Bytes
        {
            get
            {
                return (Value is SmalltalkBytesObject) ? (SmalltalkBytesObject)Value : null;
            }
        }

        public object Value
        {
            get
            {
                if (IsObjectValue(ObjectId, Archive))
                    return GetObject(ObjectId, Archive);
                else if (IsCharacterValue(ObjectId, Archive))
                    return CharValue;
                else
                    return GetLongValue(ObjectId, IdIntegerZero);
            }
        }

        public override string ToString()
        {
            return $"{Value}";
        }

        string IArchiveNode.Title => "ValueReference";

        public string Preview
        {
            get
            {
                if (IsNil)
                    return $"<html><b>nil</b></html>";

                if (IsBoolean)
                    return $"<html><b>{Value}</b></html>";
                
                if (Class?.ClassName == "String")
                    return $"<html>'{AsciiValue}'</html>";

                if (Class?.ClassName == "Symbol")
                    return $"<html><b>{AsciiValue}</b></html>";

                if (Class?.ClassName == "GUID")
                    return $"<html>{{{GuidValue}}}</html>";

                if (HasLongValue)
                    return $"<html>{LongValue}</html>";

                var node = Value as IArchiveNode;
                if (node != null)
                    return node.Preview;

                var buff = new System.Text.StringBuilder();
                buff.Append("<html>");
                buff.Append($"<h1>{((IArchiveNode)this).Title}</h1>");
                buff.Append($"<h3>{Value}</h3>");                
                buff.Append("</html>");
                return buff.ToString();
            }
        }

        public List<object> Keys
        {
            get
            {
                var list = new List<object>();
                list.Add("Class");
                list.Add("ObjectId");
                list.Add("Value");
                list.Add("GuidValue");
                list.Add("AsciiValue");
                list.Add("LongValue");                
                list.Add("DoubleValue");
                list.Add("Bytes");

                return list;
            }
        }

        object IArchiveNode.this[object key]
        {
            get
            {
                switch (key as string)
                {
                    case "Class":
                        return Class;
                    case "ObjectId":
                        return ObjectId;
                    case "AsciiValue":
                        return AsciiValue;                        
                    case "Value":
                        return Value;
                    case "GuidValue":
                        return GuidValue;
                    case "LongValue":
                        return LongValue;
                    case "DoubleValue":
                        return DoubleValue;
                    case "Bytes":
                        return Bytes;
                    default:
                        return this;
                }
            }
        }

        private static bool IsLongValue(long objectId, SmalltalkArchive archive)
        {
            if (IsObjectValue(objectId, archive)) return false;
            return !IsCharacterValue(objectId, archive);
        }

        private static bool IsCharacterValue(long objectId, SmalltalkArchive archive)
        {
            if (IsObjectValue(objectId, archive)) return false;
            return objectId < archive.IdIntegerZero;
        }

        private static bool IsObjectValue(long objectId, SmalltalkArchive archive)
        {
            return objectId < archive.IdCharacterZero;
        }

        private static object GetObject(long objectId, SmalltalkArchive archive)
        {
            if (archive.Objects.ContainsKey(objectId))
                return archive.Objects[objectId];
            else
                return null;
        }

        private static long GetLongValue(long objectId, long idIntegerZero)
        {
            var integerId = objectId - idIntegerZero;
            if (integerId % 2 == 0)
                return (integerId / 2); // even is positive                 
            else
                return -1 * (long)(integerId / 2); // odd is negative
        }
    }
}
