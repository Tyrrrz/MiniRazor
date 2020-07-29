using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MiniRazor.Internal.Extensions
{
    internal static class TypeExtensions
    {
        public static bool Implements(this Type type, Type interfaceType) => type.GetInterfaces().Contains(interfaceType);

        public static bool IsAnonymousType(this Type type) =>
            type.IsDefined(typeof(CompilerGeneratedAttribute)) &&
            type.Name.Contains("AnonymousType", StringComparison.Ordinal);

        public static ExpandoObject ToExpando(this object anonymousObject)
        {
            var expando = new ExpandoObject();
            var expandoMap = (IDictionary<string, object?>) expando;

            foreach (var property in anonymousObject.GetType().GetTypeInfo().GetProperties())
            {
                var obj = property.GetValue(anonymousObject);
                if (obj != null && obj.GetType().IsAnonymousType())
                    obj = obj.ToExpando();

                expandoMap.Add(property.Name, obj);
            }

            return expando;
        }
    }
}