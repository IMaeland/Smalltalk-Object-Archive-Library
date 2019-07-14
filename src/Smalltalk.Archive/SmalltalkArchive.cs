using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PW.Smalltalk.Archive.Primatives;

namespace PW.Smalltalk.Archive
{
    public class SmalltalkArchive : IArchiveNode
    {
        public SmalltalkArchiveHeader Header { get; set; }

        public Dictionary<long, SmalltalkClass> Classes { get; set; }
        public Dictionary<long, object> Objects { get; set; }

        public long RootObjectId => Header.RootObjectId;
                
        public SmalltalkValueReference RootObject => new SmalltalkValueReference(this, RootObjectId);
        
        public SmalltalkClass RootClass => Classes.ContainsKey(Header.RootObjectId) ? Classes[Header.RootObjectId] : null;

        internal long LastObjectId => LastBehaviorId + NumObjects;

        internal long LastBehaviorId => LastPredefinedId + NumBehaviors;

        internal int LastPredefinedId => NumPredefinedIds;

        internal int NumPredefinedIds => 11;

        public long NumObjects => Header.NumObjects;

        public long NumBehaviors => Header.NumBehaviors;

        internal long IdCharacterZero => LastObjectId + 1;
        internal long IdIntegerZero => IdCharacterZero + 256;

        string IArchiveNode.Title => $"SmalltalkArchive";        

        string IArchiveNode.Preview
        {
            get
            {
                var buff = new System.Text.StringBuilder();

                buff.Append("<html>");
                buff.Append($"<h1>Smalltalk Archive</h1>");

                buff.Append($"<h3>Classes</h3>");
                buff.Append("<ol>");
                foreach (var @class in Classes.Values.Where(c => !c.IsPredefined))
                {
                    var metaClassTag = @class.IsMetaClass ? " class" : "";
                    buff.Append($"<li>{@class.ClassName} {metaClassTag}</li>");
                }
                buff.Append("</ol>");
                buff.Append($"<h3>Objects</h3>");
                buff.Append("<ol>");
                foreach (Archive.Primatives.SmalltalkObject obj in Objects.Values.Where(o => o is Archive.Primatives.SmalltalkObject))
                {
                    buff.Append($"<li>{obj}</li>");
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
                list.Add("Archive");
                list.Add("Header");
                list.Add("Classes");                
                list.Add("Objects");
                list.Add("RootClass");
                list.Add("RootObject");
                return list;
            }
        }               

        public object this[object key]
        {
            get
            {
                switch (key as string)
                {
                    case "Archive":
                        return this;
                    case "Header":
                        return Header;
                    case "Classes":
                        return new NodeList("Classes", Classes.OrderBy(c => c.Key).Select(k => k.Value));                            
                    case "Objects":
                        return new NodeList("Objects", Objects.OrderBy(c => c.Key).Select(k => k.Value));                        
                    case "RootClass":
                        return RootClass;
                    case "RootObject":
                        return RootObject;
                    default:
                        return this;
                }                
            }
        }
    }
}
