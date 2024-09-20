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

            var properties = type.GetRuntimeProperties()
                                 .Where(prop => typeof(T).IsAssignableFrom(prop.PropertyType)
                                            && prop.GetMethod is MethodInfo methodInfo
                                            && methodInfo.IsPublic && methodInfo.IsStatic
                                            && prop.GetCustomAttribute<ObsoleteAttribute>() == null)
                                 .ToArray();

            var baseProperties = type.BaseType?.GetStaticProperties<T>() ?? Array.Empty<PropertyInfo>();
            return properties.Concat(baseProperties).ToArray();
        }

        /// <summary>
        /// Gets all method information from a specified type.
        /// </summary>
        /// <param name="type">The type to get the methods from.</param>
        /// <returns>An enumerable of method information.</returns>
        internal static IEnumerable<MethodInfo> GetAllMethodInfo(this Type? type)
        {
            if (type == null) return Enumerable.Empty<MethodInfo>();

            var methods = type.GetRuntimeMethods()
                              .Where(method => !method.IsConstructor)
                              .ToArray();

            var baseMethods = type.BaseType?.GetAllMethodInfo() ?? Enumerable.Empty<MethodInfo>();
            return methods.Concat(baseMethods);
        }

        /// <summary>
        /// Gets the property information for a specified property name.
        /// </summary>
        /// <param name="type">The type to get the property from.</param>
        /// <param name="name">The name of the property.</param>
        /// <returns>The property information if found, otherwise null.</returns>
        internal static PropertyInfo? GetPropertyInfo(this Type type, string name)
        {
            var property = type.GetRuntimeProperties()
                               .FirstOrDefault(prop => prop.Name == name && prop.GetMethod is MethodInfo methodInfo && methodInfo.IsStatic);

            return property ?? type.BaseType?.GetPropertyInfo(name);
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

            var method = type.GetRuntimeMethods()
                             .FirstOrDefault(m => m.Name == name && m.IsStatic && !m.IsConstructor && m.ReturnType == typeof(bool));

            return method ?? type.BaseType?.GetMethodInfo(name);
        }
    }
}