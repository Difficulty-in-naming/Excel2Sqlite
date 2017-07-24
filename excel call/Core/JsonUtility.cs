using System;
using System.Reflection;
using Newtonsoft.Json.Serialization;

public class NullableValueProvider : IValueProvider
{
    private readonly object _defaultValue;
    private readonly IValueProvider _underlyingValueProvider;
    public NullableValueProvider(MemberInfo memberInfo, Type type)
    {
        _underlyingValueProvider = new DynamicValueProvider(memberInfo);
        if (type == typeof(string))
        {
            _defaultValue = string.Empty;
            return;
        }
        _defaultValue = Activator.CreateInstance(type);
    }
    public void SetValue(object target, object value)
    {
        _underlyingValueProvider.SetValue(target, value);
    }
    public object GetValue(object target)
    {
        return _underlyingValueProvider.GetValue(target) ?? _defaultValue;
    }
}

public class ArrayValueProvider : IValueProvider
{
    private readonly object _defaultValue;
    private readonly IValueProvider _underlyingValueProvider;
    public ArrayValueProvider(MemberInfo memberInfo, Type type)
    {
        _underlyingValueProvider = new DynamicValueProvider(memberInfo);
        _defaultValue = Array.CreateInstance(type, 0);
    }
    public void SetValue(object target, object value)
    {
        _underlyingValueProvider.SetValue(target, value);
    }
    public object GetValue(object target)
    {
        return _underlyingValueProvider.GetValue(target) ?? _defaultValue;
    }
}
public class SpecialContractResolver : DefaultContractResolver
{
    protected override IValueProvider CreateMemberValueProvider(MemberInfo member)
    {
        if (member.MemberType == MemberTypes.Property)
        {
            var pi = (PropertyInfo)member;
            var t = pi.PropertyType;
            if (t.IsArray)
            {
                return new ArrayValueProvider(member, t.GetElementType());
            }
            return new NullableValueProvider(member, pi.PropertyType);
        }
        return base.CreateMemberValueProvider(member);
    }
}