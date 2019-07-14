using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PW.Smalltalk.Archive;
using PW.Smalltalk.Archive.Serialization;
using PW.Smalltalk.Archive.Primatives;
using System.Linq;

namespace PW.Smalltalk.Archive.Tests
{
    [TestClass]
    public class SmalltalkSerializerTests
    {
        System.IO.FileStream stream = null;

        [TestCleanup]
        public void CleanUp()
        {
            if (stream != null)
                stream.Close();
            stream = null;
        }


        [TestMethod]
        public void SerializeSmalltalkString()
        {
            stream = System.IO.File.OpenRead(@"Data\hello.obj");
            var serializer = new SmalltalkSerializer();
            var archive = serializer.Deserialize(stream);
            stream.Close();
            stream = null;

            var file = @"C:\tmp\helloOut.obj";
            if (System.IO.File.Exists(file))
                System.IO.File.Delete(file);
            stream = System.IO.File.OpenWrite(file);
            serializer = new SmalltalkSerializer();
            serializer.Serialize(stream, archive);
            stream.Close();
            stream = null;
        }

        [TestMethod]
        public void SerializeSimple()
        {
            stream = System.IO.File.OpenRead(@"Data\MyClass.obj");
            var serializer = new SmalltalkSerializer();
            var archive = serializer.Deserialize(stream);
            stream.Close();
            stream = null;

            var file = @"C:\tmp\MyClassOut.obj";
            if (System.IO.File.Exists(file))
                System.IO.File.Delete(file);
            stream = System.IO.File.OpenWrite(file);
            serializer = new SmalltalkSerializer();
            serializer.Serialize(stream, archive);
            stream.Close();
            stream = null;
        }

        [TestMethod]
        public void SerializeSmalltalkPolicy()
        {
            stream = System.IO.File.OpenRead(@"Data\policy.obj");
            var serializer = new SmalltalkSerializer();
            var archive = serializer.Deserialize(stream);
            stream.Close();
            stream = null;

            var file = @"C:\tmp\policyOut.obj";
            if (System.IO.File.Exists(file))
                System.IO.File.Delete(file);
            stream = System.IO.File.OpenWrite(file);
            serializer = new SmalltalkSerializer();
            serializer.Serialize(stream, archive);
            stream.Close();
            stream = null;
        }

        [TestMethod]
        public void DeserializeSmalltalkString()
        {
            stream = System.IO.File.OpenRead(@"Data\hello.obj");
            var serializer = new SmalltalkSerializer();
            var archive = serializer.Deserialize(stream);
            var header = archive.Header;

            Assert.AreEqual(12, header.Version);
            Assert.AreEqual(1, header.NumBehaviors);
            Assert.AreEqual(1, header.NumObjects);
            Assert.AreEqual(13, header.RootObjectId);

            var cls = archive.Classes[12];
            var obj = (SmalltalkBytesObject) archive.Objects[header.RootObjectId];

            Assert.AreEqual(12, cls.ClassId);
            Assert.AreEqual("String", cls.ClassName);
            Assert.AreEqual("hello World", obj.AsciiValue);
        }

        [TestMethod]
        public void DeserializeSmalltalkFloat()
        {
            stream = System.IO.File.OpenRead(@"Data\Float.obj");
            var serializer = new SmalltalkSerializer();
            var archive = serializer.Deserialize(stream);
            var header = archive.Header;

            Assert.AreEqual(12, header.Version);
            Assert.AreEqual(1, header.NumBehaviors);
            Assert.AreEqual(1, header.NumObjects);
            Assert.AreEqual(13, header.RootObjectId);

            var cls = archive.Classes[12];
            var obj = (SmalltalkBytesObject)archive.Objects[header.RootObjectId];

            Assert.AreEqual(12, cls.ClassId);
            Assert.AreEqual("Float", cls.ClassName);
            Assert.AreEqual(12345.06789, obj.DoubleValue);
        }

        [TestMethod]
        public void DeserializeSmallInteger()
        {
            stream = System.IO.File.OpenRead(@"Data\SmallInteger.obj");
            var serializer = new SmalltalkSerializer();
            var archive = serializer.Deserialize(stream);
            var header = archive.Header;            

            var root = (SmalltalkValueReference) archive.RootObject;

            Assert.AreEqual(12, header.Version);
            Assert.AreEqual(0, header.NumBehaviors);
            Assert.AreEqual(0, header.NumObjects);
            Assert.AreEqual(12345L, root.ParseValue<long>());
        }

        [TestMethod]
        public void DeserializeLargeInteger()
        {
            stream = System.IO.File.OpenRead(@"Data\Integer.obj");
            var serializer = new SmalltalkSerializer();
            var archive = serializer.Deserialize(stream);
            var header = archive.Header;

            var root = (SmalltalkValueReference)archive.RootObject;

            Assert.AreEqual(12, header.Version);
            Assert.AreEqual(0, header.NumBehaviors);
            Assert.AreEqual(0, header.NumObjects);
            Assert.AreEqual(1234567890L, root.ParseValue<long>());
        }

        [TestMethod]
        public void DeserializeLargeNegativeInteger()
        {
            stream = System.IO.File.OpenRead(@"Data\LargeNegativeInteger.obj");
            var serializer = new SmalltalkSerializer();
            var archive = serializer.Deserialize(stream);
            var header = archive.Header;

            var root = (SmalltalkValueReference)archive.RootObject;

            Assert.AreEqual(12, header.Version);
            Assert.AreEqual(0, header.NumBehaviors);
            Assert.AreEqual(0, header.NumObjects);
            Assert.AreEqual(-1234567890L, root.ParseValue<long>());
        }

        [TestMethod]
        public void DeserializeSmalltalkTwoString()
        {
            stream = System.IO.File.OpenRead(@"Data\hello2.obj");
            var serializer = new SmalltalkSerializer();
            var archive = serializer.Deserialize(stream);
            var header = archive.Header;

            Assert.AreEqual(12, header.Version);
            Assert.AreEqual(1, header.NumBehaviors);
            Assert.AreEqual(1, header.NumObjects);
            Assert.AreEqual(13, header.RootObjectId);

            var cls = archive.Classes[12];
            var obj = archive.RootObject;

            Assert.AreEqual(12, cls.ClassId);
            Assert.AreEqual("String", cls.ClassName);
            Assert.AreEqual("hello World", obj.AsciiValue);
                        
            archive = serializer.Deserialize(stream);
            header = archive.Header;

            Assert.AreEqual(12, header.Version);
            Assert.AreEqual(1, header.NumBehaviors);
            Assert.AreEqual(1, header.NumObjects);
            Assert.AreEqual(13, header.RootObjectId);

            cls = archive.Classes[12];
            obj = archive.RootObject;

            Assert.AreEqual(12, cls.ClassId);
            Assert.AreEqual("String", cls.ClassName);
            Assert.AreEqual("Hello World2", obj.AsciiValue);
        }

        [TestMethod]
        public void DeserializeMetaClass()
        {
            stream = System.IO.File.OpenRead(@"Data\MetaClass.obj");
            var serializer = new SmalltalkSerializer();
            var archive = serializer.Deserialize(stream);
            var header = archive.Header;

            Assert.AreEqual(12, header.Version);
            Assert.AreEqual(1, header.NumBehaviors);
            Assert.AreEqual(0, header.NumObjects);
            Assert.AreEqual(12, header.RootObjectId);

            Assert.AreEqual("Policy", archive.RootClass.ClassName);
            Assert.AreEqual(true, archive.RootClass.IsMetaClass);
        }

        [TestMethod]
        public void DeserializeSimple()
        {
            stream = System.IO.File.OpenRead(@"Data\MyClass.obj");
            var serializer = new SmalltalkSerializer();
            var archive = serializer.Deserialize(stream);
            var header = archive.Header;

            Assert.AreEqual(12, header.Version);
            Assert.AreEqual(2, header.NumBehaviors);
            Assert.AreEqual(2, header.NumObjects);
            Assert.AreEqual(14, header.RootObjectId);

            var obj = (Primatives.SmalltalkPointerObject)archive.Objects[header.RootObjectId];
                        
            Assert.AreEqual(12, obj.Class.ClassId);
            Assert.AreEqual("MyClass", obj.Class.ClassName);
            Assert.AreEqual("The Quick Fox", obj["stringVar"].AsciiValue);
            var value = obj["intVar"];
            Assert.AreEqual(1234L, value.LongValue);
        }

        [TestMethod]
        public void DeserializePolicy()
        {
            stream = System.IO.File.OpenRead(@"Data\policy.obj");
            var serializer = new SmalltalkSerializer();
            var archive = serializer.Deserialize(stream);
            var header = archive.Header;

            Assert.AreEqual(12, header.Version);
            Assert.AreEqual(58, header.NumBehaviors);
            Assert.AreEqual(517, header.NumObjects);
            Assert.AreEqual(70, header.RootObjectId);

            var obj = (Primatives.SmalltalkPointerObject)archive.Objects[header.RootObjectId];

            Assert.AreEqual(12, obj.Class.ClassId);
            Assert.AreEqual("Policy", obj.Class.ClassName);

            var guid = obj["guid"];
            Assert.AreEqual(new Guid("5e800853-c754-4acd-80ab-56b09b86f4f6"), guid.GuidValue);

            var dict = obj["locations"].ParseValue< SmalltalkPointerObject>()["contents"].ParseValue< SmalltalkPointerObject>();
            var locations = dict["contents"].ParseValue< SmalltalkVariableObject>();
            var location = locations[1].ParseValue< SmalltalkPointerObject>();
            Assert.AreEqual("Association", location.Class.ClassName);

        }

        [TestMethod]
        public void DeserializeBigPolicy()
        {
            stream = System.IO.File.OpenRead(@"Data\policybig.obj");
            var serializer = new SmalltalkSerializer();
            var archive = serializer.Deserialize(stream);
            var header = archive.Header;

            Assert.AreEqual(12, header.Version);
            Assert.AreEqual(63, header.NumBehaviors);
            Assert.AreEqual(12103, header.NumObjects);
            Assert.AreEqual(75, header.RootObjectId);

            var obj = (Primatives.SmalltalkPointerObject)archive.Objects[header.RootObjectId];

            Assert.AreEqual(12, obj.Class.ClassId);
            Assert.AreEqual("Policy", obj.Class.ClassName);

            var guid = obj["guid"].ParseValue<SmalltalkBytesObject>();
            Assert.AreEqual(new Guid("56bee155-d814-4f45-bd35-6b40227483ec"), guid.GuidValue);

            var dict = obj["locations"].ParseValue< SmalltalkPointerObject>()["contents"].ParseValue< SmalltalkPointerObject>();
            var locations = dict["contents"].ParseValue< SmalltalkVariableObject>();
            var location = locations[1].ParseValue<SmalltalkPointerObject>();
            Assert.AreEqual("Association", location.Class.ClassName);

        }
    }
}
