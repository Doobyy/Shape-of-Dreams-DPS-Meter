using System;
using System.Reflection;

public static class GetAttributeExtensions
{
	public static T GetCustomAttribute<T>(this MemberInfo member) where T : Attribute
	{
		return Attribute.GetCustomAttribute(member, typeof(T)) as T;
	}
}
