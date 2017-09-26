using System.Collections.Generic;
using System.Xml;

using BrokenEvent.NanoSerializer.Adapter;

namespace BrokenEvent.NanoSerializer.Tests
{
  public class SystemXmlAdapter: UntypedDataAdapter, IDataArray
  {
    private readonly XmlElement element;

    public SystemXmlAdapter(XmlElement element)
    {
      this.element = element;
    }

    #region IDataAdapter

    public override void AddStringValue(string value, string name, bool isAttribute)
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

    public override void SetStringValue(string value)
    {
      element.InnerText = value;
    }

    public override string GetStringValue()
    {
      return element.InnerText;
    }

    public override string GetStringValue(string name, bool isAttribute)
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

    public override IDataAdapter GetChild(string name)
    {
      foreach (XmlNode node in element.ChildNodes)
        if (node.NodeType == XmlNodeType.Element && node.Name == name)
          return new SystemXmlAdapter((XmlElement)node);

      return null;
    }

    public override IDataAdapter AddChild(string name)
    {
      XmlElement el = element.OwnerDocument.CreateElement(name);
      element.AppendChild(el);
      return new SystemXmlAdapter(el);
    }

    public override IDataArray AddArray()
    {
      return this;
    }

    public override IDataArray GetArray()
    {
      return this;
    }

    #endregion

    #region IDataArray

    public IDataAdapter AddArrayValue()
    {
      return AddChild("Item");
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

    #endregion

    public static implicit operator SystemXmlAdapter(XmlElement e)
    {
      return new SystemXmlAdapter(e);
    }

    public static implicit operator SystemXmlAdapter(XmlDocument e)
    {
      if (e.DocumentElement == null)
        e.AppendChild(e.CreateElement("test"));
      return new SystemXmlAdapter(e.DocumentElement);
    }
  }
}
