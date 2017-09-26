
namespace BrokenEvent.NanoSerializer.Adapter
{
  /// <summary>
  /// Data abstraction for <see cref="Serializer"/> and <see cref="Deserializer"/>.
  /// </summary>
  public interface IDataAdapter
  {
    /// <summary>
    /// Adds an inner element with  given name and string value to the current element.
    /// </summary>
    /// <param name="value">Value of element to be added</param>
    /// <param name="name">Name of element to be added</param>
    /// <param name="isAttribute">True if the value should be added as an attribute and false otherwise.</param>
    /// <remarks>If carrier does not supports attributes (such as JSON), it may add an inner value in both cases, but the adapter is responsible
    /// for name collisions between attributes and subelements.</remarks>
    void AddStringValue(string value, string name, bool isAttribute);

    /// <summary>
    /// Adds an inner element with  given name and integer value to the current element.
    /// </summary>
    /// <param name="value">Value of element to be added</param>
    /// <param name="name">Name of element to be added</param>
    /// <param name="isAttribute">True if the value should be added as an attribute and false otherwise.</param>
    /// <remarks>If carrier does not supports attributes (such as JSON), it may add an inner value in both cases, but the adapter is responsible
    /// for name collisions between attributes and subelements.</remarks>
    void AddIntValue(long value, string name, bool isAttribute);

    /// <summary>
    /// Adds an inner element with  given name and float value to the current element.
    /// </summary>
    /// <param name="value">Value of element to be added</param>
    /// <param name="name">Name of element to be added</param>
    /// <param name="isAttribute">True if the value should be added as an attribute and false otherwise.</param>
    /// <remarks>If carrier does not supports attributes (such as JSON), it may add an inner value in both cases, but the adapter is responsible
    /// for name collisions between attributes and subelements.</remarks>
    void AddFloatValue(double value, string name, bool isAttribute);

    /// <summary>
    /// Adds an inner element with  given name and boolean value to the current element.
    /// </summary>
    /// <param name="value">Value of element to be added</param>
    /// <param name="name">Name of element to be added</param>
    /// <param name="isAttribute">True if the value should be added as an attribute and false otherwise.</param>
    /// <remarks>If carrier does not supports attributes (such as JSON), it may add an inner value in both cases, but the adapter is responsible
    /// for name collisions between attributes and subelements.</remarks>
    void AddBoolValue(bool value, string name, bool isAttribute);

    /// <summary>
    /// Adds an inner element with  given name and null value to the current element.
    /// </summary>
    /// <param name="name">Name of element to be added</param>
    /// <param name="isAttribute">True if the value should be added as an attribute and false otherwise.</param>
    /// <remarks>If carrier does not supports attributes (such as JSON), it may add an inner value in both cases, but the adapter is responsible
    /// for name collisions between attributes and subelements. If carrier does not support null elements, it may add a specific marker, such as "null" string.</remarks>
    void AddNullValue(string name, bool isAttribute);

    /// <summary>
    /// Sets the string value to current element.
    /// </summary>
    /// <param name="value">Value to set</param>
    void SetStringValue(string value);

    /// <summary>
    /// Sets the integer value to current element.
    /// </summary>
    /// <param name="value">Value to set</param>
    void SetIntValue(long value);

    /// <summary>
    /// Sets the float value to current element.
    /// </summary>
    /// <param name="value">Value to set</param>
    void SetFloatValue(double value);

    /// <summary>
    /// Sets the boolean value to current element.
    /// </summary>
    /// <param name="value">Value to set</param>
    void SetBoolValue(bool value);

    /// <summary>
    /// Sets the null value to current element.
    /// </summary>
    void SetNullValue();

    /// <summary>
    /// Gets the string value of inner element with given name.
    /// </summary>
    /// <param name="name">Name of value to get</param>
    /// <param name="isAttribute">True if attribute element is required and false if usual subelement</param>
    /// <returns>String value of an element. null should be returned if no such element found.</returns>
    /// <remarks>If carrier does not support attributes, it may return inner element, but is is responsible for
    /// element name collisions of elements added as attribute and others.</remarks>
    string GetStringValue(string name, bool isAttribute);

    /// <summary>
    /// Gets the integer value of inner element with given name.
    /// </summary>
    /// <param name="name">Name of value to get</param>
    /// <param name="isAttribute">True if attribute element is required and false if usual subelement</param>
    /// <returns>Integer value of an element. default(long) should be returned if no such element found.</returns>
    /// <remarks>If carrier does not support attributes, it may return inner element, but is is responsible for
    /// element name collisions of elements added as attribute and others.</remarks>
    long GetIntValue(string name, bool isAttribute);

    /// <summary>
    /// Gets the float value of inner element with given name.
    /// </summary>
    /// <param name="name">Name of value to get</param>
    /// <param name="isAttribute">True if attribute element is required and false if usual subelement</param>
    /// <returns>Float value of an element. default(double) should be returned if no such element found.</returns>
    /// <remarks>If carrier does not support attributes, it may return inner element, but is is responsible for
    /// element name collisions of elements added as attribute and others.</remarks>
    double GetFloatValue(string name, bool isAttribute);

    /// <summary>
    /// Gets the boolean value of inner element with given name.
    /// </summary>
    /// <param name="name">Name of value to get</param>
    /// <param name="isAttribute">True if attribute element is required and false if usual subelement</param>
    /// <returns>Boolean value of an element. False should be returned if no such element found.</returns>
    /// <remarks>If carrier does not support attributes, it may return inner element, but is is responsible for
    /// element name collisions of elements added as attribute and others.</remarks>
    bool GetBoolValue(string name, bool isAttribute);

    /// <summary>
    /// Gets the string value of the current element
    /// </summary>
    /// <returns>String value of the element</returns>
    string GetStringValue();

    /// <summary>
    /// Gets the integer value of the current element
    /// </summary>
    /// <returns>Integer value of the element</returns>
    long GetIntValue();

    /// <summary>
    /// Gets the float value of the current element
    /// </summary>
    /// <returns>Float value of the element</returns>
    double GetFloatValue();

    /// <summary>
    /// Gets the boolean value of the current element
    /// </summary>
    /// <returns>Boolean value of the element</returns>
    bool GetBoolValue();

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
    /// <remarks>At this time we don't know what is the type of value to add. Untyped carriers (such as XML) may simply add a value
    /// to alter it later. Typed carriers (such as JSON) may not add a value right now, but the adapter returned should be ready to add it to current
    /// element as necessary.</remarks>
    IDataAdapter AddChild(string name);

    /// <summary>
    /// Adds an array to the current element.
    /// </summary>
    /// <returns>The array added</returns>
    /// <remarks>Untyped carriers may not add an array element and may simply return array adapter for current element.</remarks>
    IDataArray AddArray();

    /// <summary>
    /// Gets the array in this element.
    /// </summary>
    /// <returns>The array adapter</returns>
    /// <remarks>Untyped carriers may not have an array element and may simply return array adapter for current element. In this case, the adapter
    /// is responsible for elements filtering as it used in <see cref="IDataArray.GetChildren"/> and <see cref="IDataArray.GetChildrenReversed"/>.</remarks>
    IDataArray GetArray();
  }
}
