using System;
using System.Collections.Generic;

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
  }

  [Serializable]
  public class ModelSubclass: ModelClass
  {
    public ModelSubclass()
    {
      Subitems = new List<ModelClass>();
    }

    public List<ModelClass> Subitems { get; }
  }
}
