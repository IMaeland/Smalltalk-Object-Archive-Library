using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PW.Smalltalk.Archive.Primatives;

namespace PW.Smalltalk.Archive.Xml.Serialization
{
    public class XmlSmalltalkSerializer
    {
        public SmalltalkArchive Deserialize(System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }

        public void Serialize(System.IO.Stream stream, SmalltalkArchive archive)
        {            
            var writer = System.Xml.XmlWriter.Create(stream);
            var root = Export(archive);
            root.WriteTo(writer);
            writer.Close();
        }

        private List<long> ExportedObjs = new List<long>();

        private string NodeName(object value)
        {
            if (!(value is SmalltalkObject)) return null;

            return ((SmalltalkObject)value).Class.ClassName;
        }

        public XElement Export(SmalltalkArchive archive)
        {
            var root = new XElement("SmalltalkArchive");

            root.Add(ExportHeader(archive.Header));
            root.Add(ExportClasses(archive));
            root.Add(ExportObjects(archive));
            return root;
        }

        private XElement ExportHeader(SmalltalkArchiveHeader header)
        {
            return new XElement("Header",
                new XAttribute("version", header.Version),
                new XAttribute("classes", header.NumBehaviors),
                new XAttribute("objects", header.NumObjects),
                new XAttribute("rootObjectId", header.RootObjectId)
            );
        }

        private XElement ExportClasses(SmalltalkArchive archive)
        {
            var root = new XElement("Classes");

            foreach (SmalltalkClass @class in archive.Classes.Values.Where(c => !c.IsHidden).OrderBy(c => c.ClassId))
            {
                var xmlClass = new XElement("Class");

                if (@class.IsMetaClass)
                    xmlClass.SetAttributeValue("isMetaClass", true);

                xmlClass.SetAttributeValue("cid", @class.ClassId);
                xmlClass.SetAttributeValue("rep", @class.Rep);

                if (!string.IsNullOrEmpty(@class.LibraryName))
                    xmlClass.SetAttributeValue("lib", @class.LibraryName);

                xmlClass.SetAttributeValue("name", @class.ClassName);

                foreach (var varName in @class.InstVarNames)
                {
                    xmlClass.Add(new XElement("Ivar", new XAttribute("name", varName)));
                }
                root.Add(xmlClass);
            }
            return root;
        }

        private XElement ExportObjects(SmalltalkArchive archive)
        {
            var root = new XElement("Objects");

            ExportedObjs = new List<long>();

            var lastBehaviorId = archive.LastBehaviorId;

            foreach (var keyAndValue in archive.Objects.OrderBy(k => k.Key))
            {
                if (!(keyAndValue.Value is SmalltalkObject))
                    continue;
                
                var objectId = keyAndValue.Key;

               

                // Some objects are "shortcut exported"
                // e.g. OrderedCollections
                if (ExportedObjs.Contains(objectId))
                    continue;

                var obj = (SmalltalkObject)keyAndValue.Value;
                long classId = obj.Class.ClassId;

                var xmlObject = new XElement("Object");

                xmlObject.SetAttributeValue("id", objectId);
                xmlObject.SetAttributeValue("refcid", classId);

                if (obj.Size > 0)
                    xmlObject.SetAttributeValue("size", obj.Size);

                xmlObject.SetAttributeValue("hash", obj.Hash);               

                if (obj is SmalltalkBytesObject)
                {
                    Export((SmalltalkBytesObject)obj, xmlObject);
                }
                else if (obj is SmalltalkVariableObject)
                {
                    Export((SmalltalkVariableObject)obj, xmlObject);
                }
                else if (obj is SmalltalkPointerObject)
                {
                    Export((SmalltalkPointerObject)obj, xmlObject);
                }
                else
                {
                    throw new NotImplementedException($"Unexpected object {obj.GetType().Name}");
                }              

                root.Add(xmlObject);
            }
            return root;
        }

        public void Export(SmalltalkBytesObject obj, XElement xmlObject)
        {
            if (obj.Class.ClassName == SmalltalkClass.StringClassName)
                xmlObject.Add(obj.AsciiValue);
            else if (obj.Class.ClassName == SmalltalkClass.SymbolClassName)
                xmlObject.Add(obj.AsciiValue);
            else if (obj.Class.ClassName == SmalltalkClass.GuidClassName)
                xmlObject.Add(obj.GuidValue);
            else if (obj.Class.ClassName == SmalltalkClass.FloatClassName)
                xmlObject.Add(obj.DoubleValue);
            else
                xmlObject.Add(obj.Bytes);
        }

        private void Export(SmalltalkVariableObject variable, XElement xmlObject)
        {
            for (int i = 0; i < variable.InstVars.Count; i++)
            {
                var nodeName = "Value";
                var value = variable.InstVars[i];
                if (value.HasObjectValue)
                    xmlObject.Add(new XElement(nodeName, new XAttribute("refid", value.ObjectId)));
                else if (value.HasLongValue)
                    xmlObject.Add(new XElement(nodeName, new XAttribute("int", value.LongValue)));
                else if (value.HasCharValue)
                    xmlObject.Add(new XElement(nodeName, new XAttribute("char", value.CharValue)));
                else
                    throw new NotImplementedException($"Unexpected value {value.Value}");
            }
        }

        private void Export(SmalltalkPointerObject obj, XElement xmlObject)
        {
            if (obj.Class.ClassName == SmalltalkClass.OrderedCollectionClassName)
            {
                int startPosition = (int)obj.InstVars[0].LongValue - 1; // convert to 0 based
                int endPosition = (int)obj.InstVars[1].LongValue - 1;
                var variable = ((SmalltalkVariableObject)obj.InstVars[2].Value);
                var size = 0;
                ExportedObjs.Add(variable.ObjectId);// Do not export this object
                for (int i = startPosition; i <= endPosition; i++)
                {
                    size++;
                    var nodeName = "Value";
                    var value = variable.InstVars[i];
                    if (value.HasObjectValue)
                        xmlObject.Add(new XElement(nodeName, new XAttribute("refid", value.ObjectId)));
                    else if (value.HasLongValue)
                        xmlObject.Add(new XElement(nodeName, new XAttribute("int", value.LongValue)));
                    else if (value.HasCharValue)
                        xmlObject.Add(new XElement(nodeName, new XAttribute("char", value.CharValue)));
                    else
                        throw new NotImplementedException($"Unexpected value {value.Value}");
                }
                xmlObject.SetAttributeValue("size", size);
                return;
            }

            var instVarNames = obj.Class.InstVarNames;
            for (int i = 0; i < instVarNames.Count; i++)
            {
                var nodeName = instVarNames[i];
                var value = obj.InstVars[i];
                if (value.HasObjectValue)
                    xmlObject.Add(new XElement(nodeName, new XAttribute("refid", value.ObjectId)));
                else if (value.HasLongValue)
                    xmlObject.Add(new XElement(nodeName, new XAttribute("int", value.LongValue)));
                else if (value.HasCharValue)
                    xmlObject.Add(new XElement(nodeName, new XAttribute("char", value.CharValue)));
                else
                    throw new NotImplementedException($"Unexpected value {value.Value}");
            }
        }
       
    }
}
