# Smalltalk-Object-Archive-Library

Visual Smalltalk Enterprise uses a flat file format to save object archives. This works good if you need read the files again from Visual Smalltalk Enterprise. However if you need to read the archive files from another program or library (like MS.NET) you need to know the format of the flat file. This library allows you to read the contents of the object archives in a structured way using properties and type information.

Examples

Test to read a simple hello world object:
 public void DeserializeSmalltalkString()
 {
    var stream = System.IO.File.OpenRead(@"Data\hello.obj");
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
 
