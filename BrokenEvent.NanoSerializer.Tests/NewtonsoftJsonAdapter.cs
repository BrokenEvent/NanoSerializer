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

    public IDataAdapter AddChild(string name)
    {
      EnsureObject();
      return new NewtonsoftJsonAdapter(jObject, name);
    }

    public IDataArray AddArray()
    {
      if (jObject == null)
      {
        AddSelfToOwner(jArray = new JArray());
        return this;
      }

      EnsureObject();
      JArray child = new JArray();
      jObject.Add(ARRAY_NAME, child);
      return new NewtonsoftJsonAdapter(child);
    }

    public void AddStringValue(string value, string name, bool isAttribute)
    {
      if (isAttribute)
        name = "@" + name;
      EnsureObject();
      jObject.Add(name, new JValue(value));
    }

    public void AddIntValue(long value, string name, bool isAttribute)
    {
      if (isAttribute)
        name = "@" + name;
      EnsureObject();
      jObject.Add(name, new JValue(value));
    }

    public void AddFloatValue(double value, string name, bool isAttribute)
    {
      if (isAttribute)
        name = "@" + name;
      EnsureObject();
      jObject.Add(name, new JValue(value));
    }

    public void AddBoolValue(bool value, string name, bool isAttribute)
    {
      if (isAttribute)
        name = "@" + name;
      EnsureObject();
      jObject.Add(name, new JValue(value));
    }

    public void AddNullValue(string name, bool isAttribute)
    {
      if (isAttribute)
        name = "@" + name;
      EnsureObject();
      jObject.Add(name, null);
    }

    public void SetStringValue(string value)
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

    public void SetIntValue(long value)
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

    public void SetFloatValue(double value)
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

    public void SetBoolValue(bool value)
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

    public void SetNullValue()
    {
      if (jObject != null)
      {
        jObject.Add(VALUE_NAME, null);
        return;
      }

      if (jValue != null)
        throw new Exception("Unable to set value twice");

      AddSelfToOwner(jValue = new JValue((object)null));
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

    public IDataAdapter GetChild(string name)
    {
      if (jObject.Type == JTokenType.Null)
        return null;
      JToken token = jObject[name];
      if (token == null || token.Type == JTokenType.Null)
        return null;

      if (token.Type == JTokenType.Object)
        return new NewtonsoftJsonAdapter((JObject)token);
      if (token.Type == JTokenType.Array)
        return new NewtonsoftJsonAdapter((JArray)token);

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
          else if (child.Type == JTokenType.Array)
            yield return new NewtonsoftJsonAdapter((JArray)child);
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

    private JToken GetValue()
    {
      if (jValue != null)
        return jValue;
      JToken token = jObject[VALUE_NAME];
      if (token == null || token.Type == JTokenType.Null)
        return null;

      return token;
    }

    public string GetStringValue()
    {
      JToken value = GetValue();
      return value?.Value<string>();
    }

    public long GetIntValue()
    {
      JToken value = GetValue();
      return value != null ? value.Value<int>() : default(int);
    }

    public double GetFloatValue()
    {
      JToken value = GetValue();
      return value != null ? value.Value<float>() : default(float);
    }

    public bool GetBoolValue()
    {
      JToken value = GetValue();
      return value != null && value.Value<bool>();
    }

    private JToken GetValue(string name, bool isAttribute)
    {
      if (jObject == null)
        return null;

      if (isAttribute)
        name = "@" + name;

      JToken token = jObject[name];
      if (token == null || token.Type == JTokenType.Null)
        return null;

      return token;
    }

    public string GetStringValue(string name, bool isAttribute)
    {
      JToken token = GetValue(name, isAttribute);
      return token == null ? null : token.Value<string>();
    }

    public long GetIntValue(string name, bool isAttribute)
    {
      JToken token = GetValue(name, isAttribute);
      return token == null ? default(int) : token.Value<int>();
    }

    public double GetFloatValue(string name, bool isAttribute)
    {
      JToken token = GetValue(name, isAttribute);
      return token == null ? default(float) : token.Value<float>();
    }

    public bool GetBoolValue(string name, bool isAttribute)
    {
      JToken token = GetValue(name, isAttribute);
      return token != null && token.Value<bool>();
    }

    #endregion

    public static implicit operator NewtonsoftJsonAdapter(JObject obj)
    {
      return new NewtonsoftJsonAdapter(obj);
    }
  }
}
