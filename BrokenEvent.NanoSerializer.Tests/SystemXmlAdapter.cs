using System.Collections.Generic;
using System.Xml;

namespace BrokenEvent.NanoSerializer.Tests
{
  public class SystemXmlAdapter: IDataAdapter
  {
    private readonly XmlElement element;

    public SystemXmlAdapter(XmlElement element)
    {
      this.element = element;
    }

    public void AddAttribute(string name, string value, bool isSystem)
    {
      element.Attributes.Append(element.OwnerDocument.CreateAttribute(name)).Value = value;
    }

    public string Value
    {
      get { return element.InnerText; }
      set { element.InnerText = value; }
    }

    public string GetAttribute(string name, bool isSystem)
    {
      string result = element.GetAttribute(name);
      return string.IsNullOrWhiteSpace(result) ? null : result;
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

    public static implicit operator SystemXmlAdapter(XmlDocument e)
    {
      if (e.DocumentElement == null)
        e.AppendChild(e.CreateElement("test"));
      return new SystemXmlAdapter(e.DocumentElement);
    }
  }
}
