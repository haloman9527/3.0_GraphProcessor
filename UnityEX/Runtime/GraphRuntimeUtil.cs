using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace Atom.GraphProcessor
{
    public static class GraphRuntimeUtil
    {
        public static BaseGraph Clone(this BaseGraph graph)
        {
            return graph == null ? null : (BaseGraph)CloneObject(graph, new Dictionary<object, object>(ReferenceEqualityComparer.Instance));
        }

        private static object CloneObject(object source, IDictionary<object, object> visited)
        {
            if (source == null)
                return null;

            var type = source.GetType();
            if (IsImmutable(type) || source is UnityEngine.Object)
                return source;

            if (visited.TryGetValue(source, out var existing))
                return existing;

            if (type.IsArray)
            {
                var sourceArray = (Array)source;
                var elementType = type.GetElementType();
                var cloneArray = Array.CreateInstance(elementType, sourceArray.Length);
                visited[source] = cloneArray;
                for (var i = 0; i < sourceArray.Length; i++)
                {
                    cloneArray.SetValue(CloneObject(sourceArray.GetValue(i), visited), i);
                }

                return cloneArray;
            }

            if (source is IList sourceList)
            {
                var cloneList = (IList)CreateInstance(type);
                visited[source] = cloneList;
                foreach (var item in sourceList)
                {
                    cloneList.Add(CloneObject(item, visited));
                }

                return cloneList;
            }

            if (source is IDictionary sourceDictionary)
            {
                var cloneDictionary = (IDictionary)CreateInstance(type);
                visited[source] = cloneDictionary;
                foreach (DictionaryEntry entry in sourceDictionary)
                {
                    var key = CloneObject(entry.Key, visited);
                    var value = CloneObject(entry.Value, visited);
                    cloneDictionary.Add(key, value);
                }

                return cloneDictionary;
            }

            if (type.IsValueType)
                return source;

            var clone = CreateInstance(type);
            visited[source] = clone;

            foreach (var field in GetAllFields(type))
            {
                if (field.IsStatic)
                    continue;

                field.SetValue(clone, CloneObject(field.GetValue(source), visited));
            }

            return clone;
        }

        private static bool IsImmutable(Type type)
        {
            return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(Type);
        }

        private static object CreateInstance(Type type)
        {
            try
            {
                return Activator.CreateInstance(type, true);
            }
            catch
            {
                return FormatterServices.GetUninitializedObject(type);
            }
        }

        private static IEnumerable<FieldInfo> GetAllFields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            while (type != null && type != typeof(object))
            {
                foreach (var field in type.GetFields(flags))
                {
                    yield return field;
                }

                type = type.BaseType;
            }
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

            public new bool Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
