namespace RotationSolver.Basic.Helpers;

internal static class ReflectionHelper
{
    internal static PropertyInfo[] GetStaticProperties<T>(this Type? type)
    {
        if (type == null) return Array.Empty<PropertyInfo>();

        var properties = from prop in type.GetRuntimeProperties()
                         where typeof(T).IsAssignableFrom(prop.PropertyType)
                               && prop.GetMethod is MethodInfo methodInfo
                               && methodInfo.IsPublic && methodInfo.IsStatic
                               && prop.GetCustomAttribute<ObsoleteAttribute>() == null
                         select prop;

        return properties.Union(type.BaseType?.GetStaticProperties<T>() ?? Array.Empty<PropertyInfo>()).ToArray();
    }

    internal static IEnumerable<MethodInfo> GetAllMethodInfo(this Type? type)
    {
        if (type == null) return Enumerable.Empty<MethodInfo>();

        var methods = from method in type.GetRuntimeMethods()
                      where !method.IsConstructor
                      select method;

        return methods.Union(type.BaseType?.GetAllMethodInfo() ?? Enumerable.Empty<MethodInfo>());
    }

    internal static PropertyInfo? GetPropertyInfo(this Type type, string name)
    {
        foreach (var property in type.GetRuntimeProperties())
        {
            if (property.Name == name && property.GetMethod is MethodInfo methodInfo
                && methodInfo.IsStatic)
            {
                return property;
            }
        }

        return type.BaseType?.GetPropertyInfo(name);
    }

    internal static MethodInfo? GetMethodInfo(this Type? type, string name)
    {
        if (type == null) return null;

        foreach (var method in type.GetRuntimeMethods())
        {
            if (method.Name == name && method.IsStatic
                && !method.IsConstructor && method.ReturnType == typeof(bool))
            {
                return method;
            }
        }

        return type.BaseType?.GetMethodInfo(name);
    }
}
