using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PW.Smalltalk.Archive.Primatives
{
    public class SmalltalkVariableObject : SmalltalkPointerObject, IArchiveNode
    {
        public SmalltalkVariableObject(long objectId, int size, long hash, SmalltalkClass @class) : base(objectId, size, hash, @class)
        {            
        }

        public override string Title => "SmalltalkVariableObject";        

        public SmalltalkValueReference this[int index]
        {
            get
            {
                return InstVars[index];
            }
            set
            {
                InstVars[index] = value;
            }
        }

        public override List<object> Keys
        {
            get
            {
                var list = new List<object>
                {
                    "self"
                };
                for (int i = 0; i < Size; i++)
                    list.Add(i);

                return list;
            }
        }

        object IArchiveNode.this[object key]
        {
            get
            {
                if ("self".Equals(key)) return this;

                return this[(int)key];
            }
        }

        public override string Preview
        {
            get

            {
                var buff = new System.Text.StringBuilder();
                buff.Append("<html>");
                buff.Append($"<h1>{Class.ClassName}</h1>");
                buff.Append("<ol>");
                for(int i = 0; i < Size; i++)
                {

                    buff.Append($"<li>{this[i]}</li>");
                }
                buff.Append("</ol>");

                buff.Append("</html>");
                return buff.ToString();

            }
        }
    }
}
