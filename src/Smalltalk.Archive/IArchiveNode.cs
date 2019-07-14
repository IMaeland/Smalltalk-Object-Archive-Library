using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PW.Smalltalk.Archive
{
    public interface IArchiveNode
    {
        string Title { get; }
        string Preview { get; }                
        List<object> Keys { get; }
        object this[object key] { get; }
    }
}
