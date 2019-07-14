using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PW.Smalltalk.Archive.Primatives
{
    public class SmalltalkPointerObject : SmalltalkObject, IArchiveNode
    {
        public SmalltalkPointerObject(long objectId, int size, long hash, SmalltalkClass @class) : base(objectId, size, hash, @class)
        {
            InstVars = new List<SmalltalkValueReference>();
            for (int i = 0; i < @class.InstVarNames.Count(); i++)
                InstVars.Add(null);
        }

        public List<SmalltalkValueReference> InstVars { get; }
        public SmalltalkValueReference this[string name]
        {
            get
            {
                return InstVars[InstVarIndex(name)];
            }
            set
            {
                InstVars[InstVarIndex(name)] = value;
            }
        }

        public override string Title => "SmalltalkPointerObject";

        protected int InstVarIndex(string name)
        {
            return Class.InstVarNames.IndexOf(name);
        }

        public override List<object> Keys
        {
            get
            {
                var list = new List<object>() { "self" };
                list.AddRange(Class.InstVarNames.Select(c=> (object)c));
                return list;
            }
        }

        object IArchiveNode.this[object key]
        {
            get
            {
                if ("self".Equals(this)) return this;
                return this[(string)key];
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
                foreach (var ivar in Class.InstVarNames)
                {

                    buff.Append($"<li><b>{ivar}</b>: {this[ivar]}</li>");
                }
                buff.Append("</ol>");

                buff.Append("</html>");
                return buff.ToString();

            }
        }
    }
}
