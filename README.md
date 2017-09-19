[![Build Status](https://img.shields.io/appveyor/ci/BrokenEvent/nanoserializer/master.svg?style=flat-square)](https://ci.appveyor.com/project/BrokenEvent/nanoserializer)
[![GitHub License](https://img.shields.io/badge/license-MIT-brightgreen.svg?style=flat-square)](https://raw.githubusercontent.com/BrokenEvent/nanoserializer/master/LICENSE)

# NanoSerializer
Carrier-independent serializer-deserializer dedicated to minimum user code restrictions.

# Features

* Custom constructors support. Serialized classes don't require parameterless constructors to be deserialized.
* Cycles unwind. Objects can reference each other and their data will not be duplicated even if two objects reference single another object.
* Carrier-independent. The serializer can be used with any data format without significant performance loss: Xml, Json... The only thing is required from user is to write simple data adapter.
* Private properties optional support.
* Custom serialization/deserialization support with ability to continue process for inner objects with current serializer/deserializer.

## Usage

### Data Adapters

To operate with different data formats, the universal data adapters are used. **System.Xml** data adapter is included in **Tests** project. To write your own adapter you need to implement the **BrokenEvent.NanoSerializer.IDataAdapter** interface. Data adapters use DOM model, as the deserializer requires random access.

### Serialization and Deserialization

The serialization is simple:

      xml = new XmlDocument();
      Serializer.Serialize((SystemXmlAdapter)xml, target);

To apply custom settings, create the **BrokenEvent.NanoSerializer.Serializer** instance and provide **BrokenEvent.NanoSerializer.SerializationSettings**.

The deserialization is also simple:

      ModelClass result = Deserializer.Deserialize<ModelClass>((SystemXmlAdapter)xml);
      
To provide additional settins, create the **BrokenEvent.NanoSerializer.Deserializer**.

### Custom constructors

As mentioned before, deserializer can create objects using custom constructors. The main thing needed is to attribute properties and fields to be used as constructor args with **BrokenEvent.NanoSerializer.NanoSerializationAttribute** with *ConstructorArg* set. After that their deserialized values will be passed into custom constructor with specified indexes.

Some deserialized objects may require some not serialized global environment objects. To pass them:

* Attribute constructor arguments with **BrokenEvent.NanoSerializer.NanoArgAttribute** and give each argument a name.
* When deserializing, fill **BrokenEvent.NanoSerializer.Deserializer.ConstructorArgs** with required global objects and their names.

Also, you can hint the constructor detector to use specified constructor among with others. To do that, attribute this constructor with **BrokenEvent.NanoSerializer.NanoConstructorAttribute**.

See tests as examples of all described above.

## Performance

For now, the NanoSerializer doesn't beat NGen-generated XmlSerializer while producing much lesser XML. It beats the BinaryFormatter while providing XML slightly larger than binary stream.
Comparison benchmarks are in tests project. Try to test Newtonsoft.Json fails as it works much-much faster and produce lesser JSON, but it unable to deserialize test model (400k objects) correctly.

## Credits
(C) 2017, Broken Event. [brokenevent.com](http://brokenevent.com)
