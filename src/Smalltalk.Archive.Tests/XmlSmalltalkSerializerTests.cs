using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PW.Smalltalk.Archive.Serialization;
using PW.Smalltalk.Archive.Xml.Serialization;

namespace PW.Smalltalk.Archive.Tests
{
    [TestClass]
    public class XmlSmalltalkSerializerTests
    {
        System.IO.FileStream stream = null;

        [TestCleanup]
        public void CleanUp()
        {
            stream.Close();
            stream = null;
        }

        [TestMethod]
        public void TestXmlSerializeSimple()
        {
            stream = System.IO.File.OpenRead(@"Data\MyClass.obj");
            var objSerializer = new SmalltalkSerializer();
            var archive = objSerializer.Deserialize(stream);

            var xmlSerializer = new XmlSmalltalkSerializer();

            var xmlStream = new System.IO.MemoryStream();
            
            xmlSerializer.Serialize(xmlStream, archive);

            var xml = System.Text.Encoding.UTF8.GetString(xmlStream.GetBuffer(), 0, (int) xmlStream.Position);

           Assert.AreNotEqual("", xml);

        }

        [TestMethod]
        public void TestXmlSerializePolicy()
        {
            stream = System.IO.File.OpenRead(@"Data\policy.obj");
            var objSerializer = new SmalltalkSerializer();
            var archive = objSerializer.Deserialize(stream);

            var xmlSerializer = new XmlSmalltalkSerializer();

            var xmlStream = new System.IO.MemoryStream();

            xmlSerializer.Serialize(xmlStream, archive);

            var xml = System.Text.ASCIIEncoding.UTF8.GetString(xmlStream.GetBuffer(), 0, (int)xmlStream.Position);

            System.IO.File.WriteAllText(@"C:\tmp\policy.obj.xml", xml);
            Assert.AreNotEqual("", xml);
        }

        [TestMethod]
        public void TestBigPolicy()
        {
            stream = System.IO.File.OpenRead(@"Data\policybig.obj");
            var objSerializer = new SmalltalkSerializer();
            var archive = objSerializer.Deserialize(stream);

            var xmlSerializer = new XmlSmalltalkSerializer();

            var xmlStream = new System.IO.MemoryStream();

            xmlSerializer.Serialize(xmlStream, archive);

            var xml = System.Text.ASCIIEncoding.UTF8.GetString(xmlStream.GetBuffer(), 0, (int)xmlStream.Position);
        }


    }
}
