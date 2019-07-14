using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PW.Smalltalk.Archive
{
    public class SmalltalkArchiveList : IEnumerable<SmalltalkArchive>, IArchiveNode
    {
        Dictionary<object, SmalltalkArchive> Archives = new Dictionary<object, SmalltalkArchive>();

        public SmalltalkArchiveList() { }

        public void Add(SmalltalkArchive archive)
        {
            Archives.Add(Archives.Count(), archive);
        }
        
        public string Title => $"ArchiveList";

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

        public List<object> Keys => Archives.Keys.OrderBy(i=> i).ToList();

        public object this[object key] => Archives[key];

        #region IEnumerator    
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Archives.GetEnumerator();
        }

        IEnumerator<SmalltalkArchive> IEnumerable<SmalltalkArchive>.GetEnumerator()
        {
            return Archives.Values.GetEnumerator();
        }
        #endregion
    }
}
