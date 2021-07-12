using System;
using System.Collections.Generic;
using System.Reflection;

[Obsolete]
internal class ReflectionCache<T>
{
    private readonly Dictionary<string, FieldInfo> fields = new Dictionary<string, FieldInfo>();
    private readonly Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();
    private readonly Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();

    public ReflectionCache(T instance)
    {
        Instance = instance;
    }

    public T Instance { get; }

    public void SetField(string name, object value, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic)
    {
        if (!fields.TryGetValue(name, out FieldInfo field))
        {
            field = typeof(T).GetField(name, bindingFlags);
            fields.Add(name, field);
        }

        field.SetValue(Instance, value);
    }

    public TResult GetField<TResult>(string name, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic)
    {
        if (!fields.TryGetValue(name, out FieldInfo field))
        {
            field = typeof(T).GetField(name, bindingFlags);
            fields.Add(name, field);
        }

        return (TResult) field.GetValue(Instance);
    }

    public void SetProperty(string name, object value, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic)
    {
        if (!properties.TryGetValue(name, out PropertyInfo property))
        {
            property = typeof(T).GetProperty(name, bindingFlags);
            properties.Add(name, property);
        }

        property.SetValue(Instance, value);
    }

    public TResult GetProperty<TResult>(string name, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic)
    {
        if (!properties.TryGetValue(name, out PropertyInfo property))
        {
            property = typeof(T).GetProperty(name, bindingFlags);
            properties.Add(name, property);
        }

        return (TResult) property.GetValue(Instance);
    }

    public TResult InvokeMethod<TResult>(string name, object[] parameters = null, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic)
    {
        if (!methods.TryGetValue(name, out MethodInfo method))
        {
            method = typeof(T).GetMethod(name, bindingFlags);
            methods.Add(name, method);
        }

        return (TResult) method.Invoke(Instance, parameters);
    }
}
