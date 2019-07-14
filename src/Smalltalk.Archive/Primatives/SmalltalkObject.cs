using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PW.Smalltalk.Archive.Primatives
{
    public abstract class SmalltalkObject : IArchiveNode
    {
        public long ObjectId { get; }
        public int Size { get; }
        public SmalltalkClass Class { get; }
        protected SmalltalkObject(long objectId, int size, long hash, SmalltalkClass @class)
        {
            ObjectId = objectId;
            Size = size;
            Hash = hash;
            Class = @class;
        }
        public bool IsNil => ObjectId == SmalltalkClass.UndefinedObjectId;
        public bool IsTrue => ObjectId == SmalltalkClass.TrueId;
        public bool IsFalse => ObjectId == SmalltalkClass.FalseId;

        public bool IsBoolean => IsTrue | IsFalse;

        public long Hash { get; set; }

        public virtual string Title => "Object";       

        public virtual string Preview
        {
            get
            {
                if (IsNil)
                    return "<html><b>nil</b></html>";
                if (IsTrue)
                    return "<html><b>true</b></html>";
                if (IsFalse)
                    return "<html><b>false</b></html>";

                var buff = new System.Text.StringBuilder();
                buff.Append("<html>");
                buff.Append($"<h1>{Class.ClassName}</h1>");               
                buff.Append("</ol>");
                buff.Append("</html>");
                return buff.ToString();
            }
        }

        public virtual List<object> Keys
        {
            get
            {
                return new List<object>() { "self" };
            }
        }

        object IArchiveNode.this[object key]
        {
            get
            {
                return this;
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Class.ClassName))
            {
                if (IsNil)
                    return "nil";
                if (IsTrue)
                    return "True";
                if (IsFalse)
                    return "False";

                return "Unknown Class";
            }

            char firstChar = Class.ClassName[0];            
            return $"{prefix(Class.ClassName)}{Class.ClassName}";
        }

        private string prefix(string className)
        {
            if ("'aAeEiIoOuU'".Contains(className.Substring(0, 1)))
                return "an";
            return "a";
        }

        
    }
}
