using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PW.Smalltalk.Archive.Primatives
{
    public class SmalltalkArchiveHeader : IArchiveNode
    {
        public long Version { get; set; }
        public long NumBehaviors { get; set; }
        public long NumObjects { get; set; }
        public long RootObjectId { get; set; }

        public string Title => "Header";

        public string Preview
        {
            get
            {
                var buff = new System.Text.StringBuilder();
                buff.Append("<html>");
                buff.Append($"<h1>Smalltalk Archive Header</h1>");
                buff.Append($"<h3>Classes: {NumBehaviors}</h3>");
                buff.Append($"<h3>Objects: {NumObjects}</h3>");
                buff.Append("</html>");
                return buff.ToString();
            }
        }

        public List<object> Keys
        {
            get
            {
                var list = new List<object>();
                list.Add("Version");
                list.Add("Classes");
                list.Add("Objects");
                list.Add("RootObjectId");
                return list;
            }
        }

        public object this[object key]
        {
            get
            {
                switch (key as string)
                {
                    case "Header":
                        return this;
                    case "Classes":
                        return NumBehaviors;
                    case "Objects":
                        return NumObjects;
                    case "RootObjectId":
                        return RootObjectId;
                    default:
                        return this;
                }
            }
        }
    }
}
