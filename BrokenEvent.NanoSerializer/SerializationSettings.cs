﻿namespace BrokenEvent.NanoSerializer
{
  /// <summary>
  /// The serializer settings describes the output of serialization
  /// </summary>
  public class SerializationSettings
  {
    private static SerializationSettings instance = new SerializationSettings();

    /// <summary>
    /// Gets the global default settings instance.
    /// </summary>
    public static SerializationSettings Instance
    {
      get { return instance; }
    }

    /// <summary>
    /// Creates serialization settings instance with default values.
    /// </summary>
    public SerializationSettings()
    {
      SaveOptimizationFlags = true;
      DictionaryKeyName = "Key";
      DictionaryValueName = "Value";
      AssemblyQualifiedNames = true;
      SerializePrivateProperties = false;
      PrimitiveAsBase64 = true;
      SerializeNull = false;
      EnumsAsValue = false;
      EnableObjectCache = true;
      EnableTypeMarkers = true;
    }

    /// <summary>
    /// Gets or sets the value indicating whether to save optimization flags.
    /// </summary>
    /// <remarks>Optimization flags adds additional attributes to top-level node and describe features, that wasn't used
    /// in serialization process. As these features are disabled, their abscence may speed up deserialization process.</remarks>
    public bool SaveOptimizationFlags { get; set; }

    /// <summary>
    /// Gets or sets the value indicating whether to serialize read-only properties.
    /// </summary>
    public bool SerializeReadOnly { get; set; }

    /// <summary>
    /// Gets or sets the value indicating whether to ignore system NotSerialized attribute.
    /// </summary>
    public bool IgnoreNotSerialized { get; set; }

    /// <summary>
    /// Gets or sets the node name for dictionary keys. Default is "Key".
    /// </summary>
    public string DictionaryKeyName { get; set; }

    /// <summary>
    /// Gets or sets the node name for dictionary values. Default is "Value".
    /// </summary>
    public string DictionaryValueName { get; set; }

    /// <summary>
    /// Gets or sets the value indicating whether to use assembly qualified names for all types which are not from mscorlib.
    /// If set to false, namespace qualified names will be used.
    /// </summary>
    public bool AssemblyQualifiedNames { get; set; }

    /// <summary>
    /// Gets or sets the value indicating whether the private properties will also be serialized.
    /// </summary>
    public bool SerializePrivateProperties { get; set; }

    /// <summary>
    /// Gets or sets the value indicationg whether containers of primitive types will be serialized as base64 string instead of separate items.
    /// </summary>
    public bool PrimitiveAsBase64 { get; set; }

    /// <summary>
    /// Gets or sets the value indicating whether to serialize null values.
    /// </summary>
    public bool SerializeNull { get; set; }

    /// <summary>
    /// Gets or sets the value indicating whether to serialize enums as int values. Enum member names will be used if disabled.
    /// </summary>
    public bool EnumsAsValue { get; set; }

    /// <summary>
    /// Gets or sets the value indicating whether to enable object caching. Object cache is used to resolve reference cycles when
    /// serializing object graphs. Warning: disabling this option may produce infinite recursion if objects reference each other.
    /// </summary>
    public bool EnableObjectCache { get; set; }

    /// <summary>
    /// Gets or set the value indicating whether to save type markers used in polymorphism. They are
    /// used in deserialization process if the actual object type differs from property or field type.
    /// Warning: disabling this option may cause deserialization to product wrong results in some cases.
    /// </summary>
    public bool EnableTypeMarkers { get; set; }
  }
}
