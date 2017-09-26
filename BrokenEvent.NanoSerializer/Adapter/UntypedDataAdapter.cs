using System.Globalization;

namespace BrokenEvent.NanoSerializer.Adapter
{
  /// <summary>
  /// Helper adapter to simplify untyped adapters (such as for XML) writing.
  /// The main purpose of this helper is collapsing typed getters and setters into strings.
  /// </summary>
  public abstract class UntypedDataAdapter: IDataAdapter
  {
    #region AddXXXValue

    /// <inheritdoc/>
    public abstract void AddStringValue(string value, string name, bool isAttribute);

    /// <inheritdoc/>
    public virtual void AddIntValue(long value, string name, bool isAttribute)
    {
      AddStringValue(value.ToString(), name, isAttribute);
    }

    /// <inheritdoc />
    public virtual void AddFloatValue(double value, string name, bool isAttribute)
    {
      AddStringValue(value.ToString(CultureInfo.InvariantCulture), name, isAttribute);
    }

    /// <inheritdoc />
    public virtual void AddBoolValue(bool value, string name, bool isAttribute)
    {
      AddStringValue(value.ToString(), name, isAttribute);
    }

    /// <inheritdoc />
    public virtual void AddNullValue(string name, bool isAttribute)
    {
      AddStringValue("null", name, isAttribute);
    }

    #endregion

    #region SetXXXValue

    /// <inheritdoc />
    public abstract void SetStringValue(string value);

    /// <inheritdoc />
    public virtual void SetIntValue(long value)
    {
      SetStringValue(value.ToString());
    }

    /// <inheritdoc />
    public virtual void SetFloatValue(double value)
    {
      SetStringValue(value.ToString());
    }

    /// <inheritdoc />
    public virtual void SetBoolValue(bool value)
    {
      SetStringValue(value.ToString());
    }

    /// <inheritdoc />
    public virtual void SetNullValue()
    {
      SetStringValue("null");
    }

    #endregion

    #region GetXXXValue(name, isAttribute)

    /// <inheritdoc />
    public abstract string GetStringValue(string name, bool isAttribute);

    /// <inheritdoc />
    public virtual long GetIntValue(string name, bool isAttribute)
    {
      string str = GetStringValue(name, isAttribute);
      return str == null ? default(long) : long.Parse(str);
    }

    /// <inheritdoc />
    public virtual double GetFloatValue(string name, bool isAttribute)
    {
      string str = GetStringValue(name, isAttribute);
      return str == null ? default(double) : double.Parse(str);
    }

    /// <inheritdoc />
    public virtual bool GetBoolValue(string name, bool isAttribute)
    {
      string str = GetStringValue(name, isAttribute);
      return str != null && bool.Parse(str);
    }

    #endregion

    #region GetXXXValue

    /// <inheritdoc />
    public abstract string GetStringValue();

    /// <inheritdoc />
    public virtual long GetIntValue()
    {
      return long.Parse(GetStringValue());
    }

    /// <inheritdoc />
    public virtual double GetFloatValue()
    {
      return double.Parse(GetStringValue(), CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public virtual bool GetBoolValue()
    {
      return bool.Parse(GetStringValue());
    }

    #endregion

    /// <inheritdoc />
    public abstract IDataAdapter GetChild(string name);

    /// <inheritdoc />
    public abstract IDataAdapter AddChild(string name);

    /// <inheritdoc />
    public abstract IDataArray AddArray();

    /// <inheritdoc />
    public abstract IDataArray GetArray();
  }
}
