using System;
using System.Collections.Generic;
using FastMember;

namespace LibOneBot
{
    internal static class Magic
    {
        private static readonly Dictionary<Type, TypeAccessor> _accessors = new();

        private static TypeAccessor GetTypeAccessor(Type type)
        {
            bool result = _accessors.TryGetValue(type, out TypeAccessor? accessor);
            if (result) return accessor!;
            accessor = TypeAccessor.Create(type);
            _accessors[type] = accessor;
            return accessor;
        }

        public static T? GetProperty<T>(
            Type type,
            object obj,
            string propName)
        {
            TypeAccessor accessor = GetTypeAccessor(type);
            return (T?)accessor[obj, propName];
        }

        public static void SetProperty(
            Type type,
            object obj,
            string propName,
            object prop)
        {
            TypeAccessor accessor = GetTypeAccessor(type);
            accessor[obj, propName] = prop;
        }
    }
}
