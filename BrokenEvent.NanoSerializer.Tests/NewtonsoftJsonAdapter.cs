using System;
using System.Collections.Generic;

using BrokenEvent.NanoSerializer.Adapter;

using Newtonsoft.Json.Linq;

namespace BrokenEvent.NanoSerializer.Tests
{
  public class NewtonsoftJsonAdapter: IDataAdapter, IDataArray
  {
    private JObject jObject;
    private JValue jValue;
    private JArray jArray;

    private JContainer owner;
    private string itemName;

    private const string ARRAY_NAME = "#items";
    private const string VALUE_NAME = "#value";

    public NewtonsoftJsonAdapter(JObject obj)
    {
      jObject = obj;
    }

    private NewtonsoftJsonAdapter(JContainer owner, string name)
    {
      this.owner = owner;
      itemName = name;
    }

    private NewtonsoftJsonAdapter(JValue value)
    {
      jValue = value;
    }

    public NewtonsoftJsonAdapter(JArray array)
    {
      jArray = array;
    }

    #region Write

    private void EnsureObject()
    {
      if (jObject != null)
        return;
      if (jValue != null)
        throw new Exception("Unable to use value as object");

      AddSelfToOwner(jObject = new JObject());
    }

    private void AddSelfToOwner(JToken token)
    {
      if (itemName == null)
        owner.Add(token);
      else
        owner.Add(new JProperty(itemName, token));
    }

    public void AddSystemAttribute(string name, string value)
    {
      AddChild("@" + name, value);
    }

    public void AddAttribute(string name, string value)
    {
      AddChild("@" + name, value);
    }

    public IDataAdapter AddChild(string name)
    {
      EnsureObject();
      return new NewtonsoftJsonAdapter(jObject, name);
    }

    public IDataArray AddArray()
    {
      EnsureObject();
      JArray child = new JArray();
      jObject.Add(ARRAY_NAME, child);
      return new NewtonsoftJsonAdapter(child);
    }

    public void AddValue(string value)
    {
      if (jObject != null)
      {
        jObject.Add(VALUE_NAME, value);
        return;
      }

      if (jValue != null)
        throw new Exception("Unable to set value twice");

      AddSelfToOwner(jValue = new JValue(value));
    }

    public string GetValue()
    {
      if (jValue != null)
        return jValue.Value<string>();
      JToken token = jObject[VALUE_NAME];
      if (token == null || token.Type == JTokenType.Null)     
        return null;

      return token.Value<string>();
    }

    public void AddChild(string name, string value)
    {
      EnsureObject();
      if (value == null)
        jObject.Add(name, null);
      else
        jObject.Add(name, new JValue(value));
    }

    public IDataAdapter AddArrayValue()
    {
      if (jValue != null)
        throw new Exception("Unable to use value as array");
      if (jObject != null)
        throw new Exception("Unable to use object as array");

      if (jArray == null)
        AddSelfToOwner(jArray = new JArray());

      return new NewtonsoftJsonAdapter(jArray, null);
    }

    #endregion

    #region Read

    public string GetSystemAttribute(string name)
    {
      return GetAttribute(name);
    }

    public string GetAttribute(string name)
    {
      if (jValue != null)
        return null;
      if (jObject.Type == JTokenType.Null)
        return null;
      JToken token = jObject["@" + name];
      if (token == null || token.Type == JTokenType.Null)
        return null;
      return token.Value<string>();
    }

    public IDataAdapter GetChild(string name)
    {
      if (jObject.Type == JTokenType.Null)
        return null;
      JToken token = jObject[name];
      if (token == null || token.Type == JTokenType.Null)
        return null;

      if (token.Type == JTokenType.Object)
        return new NewtonsoftJsonAdapter((JObject)token);

      return new NewtonsoftJsonAdapter((JValue)token);
    }

    public IDataArray GetArray()
    {
      if (jArray != null)
        return this;

      if (jObject.Type == JTokenType.Null)
        return null;
      JToken token = jObject[ARRAY_NAME];
      if (token == null || token.Type != JTokenType.Array)
        return null;

      return new NewtonsoftJsonAdapter((JArray)token);
    }

    public IEnumerable<IDataAdapter> GetChildren()
    {
      if (jArray != null)
      {
        foreach (JToken child in jArray)
          if (child.Type == JTokenType.Object)
            yield return new NewtonsoftJsonAdapter((JObject)child);
          else
            yield return new NewtonsoftJsonAdapter((JValue)child);

        yield break;
      }

      foreach (JProperty child in jObject.Properties())
      {
        if (child.Name == ARRAY_NAME || child.Name == VALUE_NAME)
          continue;

        if (child.Value.Type == JTokenType.Object)
          yield return new NewtonsoftJsonAdapter((JObject)child.Value);
        else
          yield return new NewtonsoftJsonAdapter((JValue)child.Value);
      }
    }

    public IEnumerable<IDataAdapter> GetChildrenReversed()
    {
      List<IDataAdapter> list = new List<IDataAdapter>(GetChildren());
      for (int i = list.Count - 1; i >= 0; i--)
        yield return list[i];
    }

    #endregion

    public static implicit operator NewtonsoftJsonAdapter(JObject obj)
    {
      return new NewtonsoftJsonAdapter(obj);
    }
  }
}
