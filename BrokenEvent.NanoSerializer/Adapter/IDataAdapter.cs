
namespace BrokenEvent.NanoSerializer.Adapter
{
  /// <summary>
  /// Data abstraction for <see cref="Serializer"/> and <see cref="Deserializer"/>.
  /// </summary>
  public interface IDataAdapter: IBaseData
  {
    /*/// <summary>
    /// Add a system attribute for internal serializer data. Should add subelement if attributes are unavailable.
    /// </summary>
    /// <param name="name">Name of attribute</param>
    /// <param name="value">Value of attribute</param>
    /// <remarks>This method will be used to use internal serializer helper data while the <see cref="AddAttribute"/> will be used
    /// for regular data. Adapter may implement different mechanisms to avoid name collisions with regular data.</remarks>
    void AddSystemAttribute(string name, string value);*/

    /*/// <summary>
    /// Add attribute for regular data. Should add subelement if attributes are unavailable.
    /// </summary>
    /// <param name="name">Name of attribute</param>
    /// <param name="value">Value of attribute. Can be null if <see cref="SerializationSettings.SerializeNull"/> is set</param>
    /// <remarks>This method will be used for regular serialized objects data while the <see cref="AddSystemAttribute"/> will be used
    /// for serializer helper data. Adapter may implement different mechanisms to avoid name collisions with system data.</remarks>
    void AddAttribute(string name, string value);*/

    void AddStringValue(string value, string name, bool isAttribute);

    void AddIntValue(long value, string name, bool isAttribute);

    void AddFloatValue(double value, string name, bool isAttribute);

    void AddBoolValue(bool value, string name, bool isAttribute);

    void AddNullValue(string name, bool isAttribute);

    void SetStringValue(string value);

    void SetIntValue(long value);

    void SetFloatValue(double value);

    void SetBoolValue(bool value);

    void SetNullValue();

    string GetStringValue(string name, bool isAttribute);

    long GetIntValue(string name, bool isAttribute);

    double GetFloatValue(string name, bool isAttribute);

    bool GetBoolValue(string name, bool isAttribute);

    string GetStringValue();

    long GetIntValue();

    double GetFloatValue();

    bool GetBoolValue();

    /*/// <summary>
    /// Gets the system attribute (saved with <see cref="AddSystemAttribute"/>) value by name. Should return subelement if attributes are unavailable.
    /// </summary>
    /// <param name="name">Name of the attribute to get</param>
    /// <returns>Text value of the attribute or subelement. Null if no attribute with this name found</returns>
    /// <remarks>This method is used to get system data while <see cref="GetAttribute"/> used to get regular serialized object's data.
    /// Adapter may implement different mechanisms to avoid name collisions with regular data.</remarks>
    string GetSystemAttribute(string name);

    /// <summary>
    /// Gets the attribute (saved with <see cref="AddAttribute"/>) value by name. Should return subelement if attributes are unavailable.
    /// </summary>
    /// <param name="name">Name of the attribute to get</param>
    /// <returns>Text value of the attribute or subelement. Null if no attribute with this name found</returns>
    /// <remarks>This method is used to get regular data while <see cref="GetSystemAttribute"/> used to get system serializer data.
    /// Adapter may implement different mechanisms to avoid name collisions with system data.</remarks>
    string GetAttribute(string name);*/

    /// <summary>
    /// Gets the child element by name.
    /// </summary>
    /// <param name="name">Name of subelement to get</param>
    /// <returns>The element itself or null if no element with given name found</returns>
    IDataAdapter GetChild(string name);

    /// <summary>
    /// Adds child element by name.
    /// </summary>
    /// <param name="name">Name of the element to add</param>
    /// <returns>The added subelement.</returns>
    IDataAdapter AddChild(string name);

    IDataArray AddArray();

    IDataArray GetArray();

    /*/// <summary>
    /// Adds child element by name and value.
    /// </summary>
    /// <param name="name">Name of the element to add</param>
    /// <param name="value">Value of the element. Can be null if <see cref="SerializationSettings.SerializeNull"/> is set</param>
    void AddChild(string name, string value);*/
  }
}
