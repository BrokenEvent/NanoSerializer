using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrokenEvent.NanoSerializer.Caching
{
  internal class TypeWrapper
  {
    private readonly Type type;
    private readonly List<PropertyWrapper> properties = new List<PropertyWrapper>();
    private readonly List<FieldWrapper> fields = new List<FieldWrapper>();

    public TypeWrapper(Type type)
    {
      this.type = type;
      UpdateFields();
      UpdateProperties();
    }

    private void UpdateFields()
    {
      foreach (FieldInfo info in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
      {
        NanoSerializationAttribute attr = info.GetCustomAttribute<NanoSerializationAttribute>();

        if (attr != null && attr.State == NanoState.Ignore)
          continue;

        NanoLocation location = NanoLocation.Auto;
        if (attr != null)
          location = attr.Location;
        fields.Add(new FieldWrapper(type, info, location));
      }
    }

    private void UpdateProperties()
    {
      foreach (PropertyInfo info in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
      {
        if (!info.CanRead)
          continue;

        NanoSerializationAttribute attr = info.GetCustomAttribute<NanoSerializationAttribute>();
        NanoLocation location = NanoLocation.Auto;
        int constructorArg = -1;

        if (attr != null)
        {
          if (attr.State == NanoState.Ignore)
            continue;

          location = attr.Location;
          constructorArg = attr.ConstructorArg;
        }

        PropertyWrapper property = new PropertyWrapper(type, info, location);
        if (constructorArg != -1)
          property.ConstructorArg = constructorArg;

        properties.Add(property);
      }
    }

    public IList<PropertyWrapper> Properties
    {
      get { return properties; }
    }

    public IList<FieldWrapper> Fields
    {
      get { return fields; }
    }
  }
}
