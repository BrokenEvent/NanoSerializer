namespace BrokenEvent.NanoSerializer
{
  /// <summary>
  /// The serializer settings describes the output of serialization
  /// </summary>
  public class SerializationSettings
  {
    /// <summary>
    /// Creates serialization settings instance with default values.
    /// </summary>
    public SerializationSettings()
    {
      SaveOptimizationFlags = true;
      ContainerItemName = "Item";
      DictionaryKeyName = "Key";
      DictionaryValueName = "Value";
      ArrayItemName = "A";
    }

    /// <summary>
    /// Gets or sets the value indicating whether to save optimization flags.
    /// </summary>
    /// <remarks>Optimization flags adds additional attributes to top-level node and describe features, that wasn't used
    /// in serialization process. As these features are disabled, their abscence may speed up deserialization process.</remarks>
    public bool SaveOptimizationFlags { get; set; }

    /// <summary>
    /// Gets or sets the value indicating whether to serialize read-only fields.
    /// </summary>
    public bool SerializeReadOnly { get; set; }

    /// <summary>
    /// Gets or sets the value indicating whether to ignore system NotSerialized attribute.
    /// </summary>
    public bool IgnoreNotSerialized { get; set; }

    /// <summary>
    /// Gets or sets the node name used for container items. Default is "Item".
    /// </summary>
    public string ContainerItemName { get; set; }

    /// <summary>
    /// Gets or sets the node name used for array items. Default is "A".
    /// </summary>
    public string ArrayItemName { get; set; }

    /// <summary>
    /// Gets or sets the node name for dictionary keys. Default is "Key".
    /// </summary>
    public string DictionaryKeyName { get; set; }

    /// <summary>
    /// Gets or sets the ndoe name for dictionary values. Default is "Value".
    /// </summary>
    public string DictionaryValueName { get; set; }
  }
}
