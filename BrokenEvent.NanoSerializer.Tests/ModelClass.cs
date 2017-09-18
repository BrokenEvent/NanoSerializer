using System;
using System.Collections.Generic;
using System.Xml;

namespace BrokenEvent.NanoSerializer.Tests
{
  [Serializable]
  public class ModelClass
  {
    public static int ObjectsCount = 0;

    public ModelClass()
    {
      ObjectsCount++;
    }

    public string Data { get; set; }
    public ModelClass Subitem { get; set; }

    public static ModelClass BuildObjectsModel(int levelsRemains, int subitemsCount)
    {
      if (levelsRemains % 2 == 0)
      {
        ModelClass result = new ModelClass();
        result.Data = $"Levels remains: {levelsRemains}";
        if (levelsRemains > 0)
          result.Subitem = BuildObjectsModel(levelsRemains - 1, subitemsCount);

        return result;
      }
      else
      {
        ModelSubclass result = new ModelSubclass();
        result.Data = $"Levels remains: {levelsRemains}";
        if (levelsRemains > 0)
        {
          result.Subitem = BuildObjectsModel(levelsRemains - 1, subitemsCount);
          for (int i = 0; i < subitemsCount; i++)
            result.Subitems.Add(BuildObjectsModel(levelsRemains - 1, subitemsCount));
        }

        return result;
      }
    }

    public static bool CompareModel(ModelClass a, ModelClass b)
    {
      if (a == null && b == null)
        return true;

      if (a == null || b == null)
        return false;

      ModelSubclass subA = a as ModelSubclass;
      ModelSubclass subB = b as ModelSubclass;

      if ((subA == null) != (subB == null))
        return false;

      if (a.Data != b.Data)
        return false;

      if (!CompareModel(a.Subitem, b.Subitem))
        return false;

      if (subA == null)
        return true;

      if (subA.Subitems.Count != subB.Subitems.Count)
        return false;

      for (int i = 0; i < subA.Subitems.Count; i++)
        if (!CompareModel(subA.Subitems[i], subB.Subitems[i]))
          return false;

      return true;
    }

    public virtual void Serialize(XmlElement el)
    {
      el.Attributes.Append(el.OwnerDocument.CreateAttribute("data")).InnerText = Data;
      if (Subitem != null)
        Subitem.Serialize((XmlElement)el.AppendChild(el.OwnerDocument.CreateElement("subitem")));
    }

    public virtual void Deserialize(XmlElement el)
    {
      Data = el.GetAttribute("data");
      XmlElement subEl = el["subitem"];
      if (subEl != null)
        Subitem = DeserializeCreate(subEl);
    }

    public static ModelClass DeserializeCreate(XmlElement el)
    {
      ModelClass result;
      if (el.HasAttribute("sub"))
        result = new ModelSubclass();
      else
        result = new ModelClass();
      result.Deserialize(el);
      return result;
    }
  }

  [Serializable]
  public class ModelSubclass: ModelClass
  {
    public ModelSubclass()
    {
      Subitems = new List<ModelClass>();
    }

    public List<ModelClass> Subitems { get; }

    public override void Serialize(XmlElement el)
    {
      base.Serialize(el);
      foreach (ModelClass subitem in Subitems)
        subitem.Serialize((XmlElement)el.AppendChild(el.OwnerDocument.CreateElement("item")));

      el.Attributes.Append(el.OwnerDocument.CreateAttribute("sub")).InnerText = "true";
    }

    public override void Deserialize(XmlElement el)
    {
      base.Deserialize(el);
      foreach (XmlNode node in el.ChildNodes)
      {
        if (node.NodeType == XmlNodeType.Element && node.Name == "item")
          Subitems.Add(DeserializeCreate((XmlElement)node));
      }
    }
  }
}
