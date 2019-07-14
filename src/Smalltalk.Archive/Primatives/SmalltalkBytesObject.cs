using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PW.Smalltalk.Archive.Primatives
{
    public class SmalltalkBytesObject : SmalltalkObject, IArchiveNode
    {
        public byte[] Bytes { get; }

        public SmalltalkBytesObject(long objectId, int size, long hash, SmalltalkClass @class, byte[] bytes) : base(objectId, size, hash, @class)
        {
            Bytes = bytes;
        }

        public string AsciiValue
        {
            get
            {
                try
                {
                    return Encoding.ASCII.GetString(Bytes);
                } catch { return null; }
            }
        }

        public Guid GuidValue
        {
            get
            {
                try
                {
                    return new Guid(Bytes);
                } catch { return Guid.Empty; }
            }
        }
        
        public double DoubleValue
        {
            get
            {
                try
                {
                    return System.BitConverter.ToDouble(Bytes, 0);
                }
                catch { return 0; }
            }
        }


        public override string Title => $"BytesObject";       

        public override string Preview
        {
            get
            {
                if (Class?.ClassName == "String")
                    return $"<html>'{AsciiValue}'</html>";

                if (Class?.ClassName == "Symbol")
                    return $"<html><b>{AsciiValue}</b></html>";

                if (Class?.ClassName == "GUID")
                    return $"<html>{{{GuidValue}}}</html>";

                if (Class?.ClassName == "Float")
                    return $"<html>{DoubleValue}</html>";

                return base.Preview;
            }
        }

        public override List<object> Keys
        {
            get
            {
                var list = new List<object>();
                                
                list.Add("Class");
                list.Add("Size");
                list.Add("GuidValue");
                list.Add("AsciiValue");                         
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
                    case "Size":
                        return Size;
                    case "GuidValue":
                        return GuidValue;
                    case "AsciiValue":
                        return AsciiValue;
                    case "Bytes":
                        return Bytes;
                    default:
                        return this;
                }
            }
        }
    }
}
