using System.Collections.Generic;
using System.Globalization;
using System.Xml;

using BrokenEvent.NanoSerializer.Adapter;

namespace BrokenEvent.NanoSerializer.Tests
{
  public class SystemXmlAdapter: IDataAdapter, IDataArray
  {
    private readonly XmlElement element;

    public SystemXmlAdapter(XmlElement element)
    {
      this.element = element;
    }

    public void AddStringValue(string value, string name, bool isAttribute)
    {
      if (isAttribute)
        element.Attributes.Append(element.OwnerDocument.CreateAttribute(name)).Value = value;
      else
      { 
        XmlElement el = element.OwnerDocument.CreateElement(name);
        element.AppendChild(el);
        el.InnerText = value;
      }
    }

    public void AddIntValue(long value, string name, bool isAttribute)
    {
      AddStringValue(value.ToString(), name, isAttribute);
    }

    public void AddFloatValue(double value, string name, bool isAttribute)
    {
      AddStringValue(value.ToString(CultureInfo.InvariantCulture), name, isAttribute);
    }

    public void AddBoolValue(bool value, string name, bool isAttribute)
    {
      AddStringValue(value.ToString(), name, isAttribute);
    }

    public void AddNullValue(string name, bool isAttribute)
    {
      AddStringValue("null", name, isAttribute);
    }

    public void SetStringValue(string value)
    {
      element.InnerText = value;
    }

    public void SetIntValue(long value)
    {
      SetStringValue(value.ToString());
    }

    public void SetFloatValue(double value)
    {
      SetStringValue(value.ToString());
    }

    public void SetBoolValue(bool value)
    {
      SetStringValue(value.ToString());
    }

    public void SetNullValue()
    {
      SetStringValue("null");
    }

    public string GetStringValue()
    {
      return element.InnerText;
    }

    public long GetIntValue()
    {
      return long.Parse(element.InnerText);
    }

    public double GetFloatValue()
    {
      return double.Parse(element.InnerText, CultureInfo.InvariantCulture);
    }

    public bool GetBoolValue()
    {
      return bool.Parse(element.InnerText);
    }

    public string GetStringValue(string name, bool isAttribute)
    {
      if (isAttribute)
      {
        string result = element.GetAttribute(name);
        return string.IsNullOrEmpty(result) ? null : result;
      }

      foreach (XmlNode node in element.ChildNodes)
        if (node.NodeType == XmlNodeType.Element && node.Name == name)
          return node.InnerText;

      return null;
    }

    public long GetIntValue(string name, bool isAttribute)
    {
      string str = GetStringValue(name, isAttribute);
      return str == null ? default(long) : long.Parse(str);
    }

    public double GetFloatValue(string name, bool isAttribute)
    {
      string str = GetStringValue(name, isAttribute);
      return str == null ? default(double) : double.Parse(str);
    }

    public bool GetBoolValue(string name, bool isAttribute)
    {
      string str = GetStringValue(name, isAttribute);
      return str != null && bool.Parse(str);
    }

    public IDataAdapter GetChild(string name)
    {
      foreach (XmlNode node in element.ChildNodes)
        if (node.NodeType == XmlNodeType.Element && node.Name == name)
          return new SystemXmlAdapter((XmlElement)node);

      return null;
    }

    public IDataAdapter AddChild(string name)
    {
      XmlElement el = element.OwnerDocument.CreateElement(name);
      element.AppendChild(el);
      return new SystemXmlAdapter(el);
    }

    public IEnumerable<IDataAdapter> GetChildren()
    {
      foreach (XmlNode node in element.ChildNodes)
        if (node.NodeType == XmlNodeType.Element)
          yield return new SystemXmlAdapter((XmlElement)node);
    }

    public IEnumerable<IDataAdapter> GetChildrenReversed()
    {
      for (int i = element.ChildNodes.Count - 1; i >= 0; i--)
        if (element.ChildNodes[i].NodeType == XmlNodeType.Element)
          yield return new SystemXmlAdapter((XmlElement)element.ChildNodes[i]);
    }

    public static implicit operator SystemXmlAdapter(XmlElement e)
    {
      return new SystemXmlAdapter(e);
    }

    public IDataArray AddArray()
    {
      return this;
    }

    public IDataArray GetArray()
    {
      return this;
    }

    public IDataAdapter AddArrayValue()
    {
      return AddChild("Item");
    }

    public static implicit operator SystemXmlAdapter(XmlDocument e)
    {
      if (e.DocumentElement == null)
        e.AppendChild(e.CreateElement("test"));
      return new SystemXmlAdapter(e.DocumentElement);
    }
  }
}
