using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PW.Smalltalk.Archive
{
    public class NodeList : IArchiveNode
    {
        List<object> values;
        string title = "";

        public NodeList(string title, IEnumerable<object> values)
        {
            this.title = title;
            this.values = values.ToList();
        }

        public object this[object key]
        {
            get
            {
                return values[(int)key];
            }
        }

        public List<object> Keys
        {
            get
            {
                var keys = new List<object>();
                for (int i = 0; i < values.Count(); i++)
                    keys.Add(i);
                return keys;
            }
        }

        public string Preview
        {
            get
            {
                var buff = new StringBuilder();
                buff.Append("<html>");
                buff.Append($"<h3>{Title}</h3>");
                buff.Append("<ol>");
                foreach (var key in Keys)
                {
                    var node = this[key] as IArchiveNode;
                    var value = node?.Title ?? this[key];
                    buff.Append($"<li>{value}</li>");
                }
                buff.Append("</ol>");
                buff.Append("</html>");
                return buff.ToString();
            }
        }

        public string Title => title;
    }
}
