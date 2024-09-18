namespace RotationSolver.Basic.Helpers
{
    /// <summary>
    /// Helper class for reflection-related operations.
    /// </summary>
    internal static class ReflectionHelper
    {
        /// <summary>
        /// Gets the static properties of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of the properties to get.</typeparam>
        /// <param name="type">The type to get the properties from.</param>
        /// <returns>An array of static properties of the specified type.</returns>
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

        /// <summary>
        /// Gets all method information from a specified type.
        /// </summary>
        /// <param name="type">The type to get the methods from.</param>
        /// <returns>An enumerable of method information.</returns>
        internal static IEnumerable<MethodInfo> GetAllMethodInfo(this Type? type)
        {
            if (type == null) return Enumerable.Empty<MethodInfo>();

            var methods = from method in type.GetRuntimeMethods()
                          where !method.IsConstructor
                          select method;

            return methods.Union(type.BaseType?.GetAllMethodInfo() ?? Enumerable.Empty<MethodInfo>());
        }

        /// <summary>
        /// Gets the property information for a specified property name.
        /// </summary>
        /// <param name="type">The type to get the property from.</param>
        /// <param name="name">The name of the property.</param>
        /// <returns>The property information if found, otherwise null.</returns>
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

        /// <summary>
        /// Gets the method information for a specified method name.
        /// </summary>
        /// <param name="type">The type to get the method from.</param>
        /// <param name="name">The name of the method.</param>
        /// <returns>The method information if found, otherwise null.</returns>
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
}