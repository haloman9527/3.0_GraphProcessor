using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    public class TaskUtility
    {
        public static object CreateInstance(Type t)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                t = Nullable.GetUnderlyingType(t);
            }
            return Activator.CreateInstance(t, true);
        }

        //public static FieldInfo[] GetAllFields(Type t)
        //{
        //    FieldInfo[] array = null;
        //    if (!TaskUtility.allFieldsLookup.TryGetValue(t, out array))
        //    {
        //        List<FieldInfo> list = ObjectPool.Get<List<FieldInfo>>();
        //        list.Clear();
        //        BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        //        TaskUtility.GetFields(t, ref list, (int)flags);
        //        array = list.ToArray();
        //        ObjectPool.Return<List<FieldInfo>>(list);
        //        TaskUtility.allFieldsLookup.Add(t, array);
        //    }
        //    return array;
        //}

        // Token: 0x060001D7 RID: 471 RVA: 0x0000DE14 File Offset: 0x0000C014
        //public static FieldInfo[] GetPublicFields(Type t)
        //{
        //    FieldInfo[] array = null;
        //    if (!TaskUtility.publicFieldsLookup.TryGetValue(t, out array))
        //    {
        //        List<FieldInfo> list = ObjectPool.Get<List<FieldInfo>>();
        //        list.Clear();
        //        BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;
        //        TaskUtility.GetFields(t, ref list, (int)flags);
        //        array = list.ToArray();
        //        ObjectPool.Return<List<FieldInfo>>(list);
        //        TaskUtility.publicFieldsLookup.Add(t, array);
        //    }
        //    return array;
        //}

        // Token: 0x060001D8 RID: 472 RVA: 0x0000DE68 File Offset: 0x0000C068
        //public static FieldInfo[] GetSerializableFields(Type t)
        //{
        //    FieldInfo[] array = null;
        //    if (!TaskUtility.serializableFieldsLookup.TryGetValue(t, out array))
        //    {
        //        List<FieldInfo> list = ObjectPool.Get<List<FieldInfo>>();
        //        list.Clear();
        //        BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        //        TaskUtility.GetSerializableFields(t, list, (int)flags);
        //        array = list.ToArray();
        //        ObjectPool.Return<List<FieldInfo>>(list);
        //        TaskUtility.serializableFieldsLookup.Add(t, array);
        //    }
        //    return array;
        //}

        private static void GetSerializableFields(Type t, IList<FieldInfo> fieldList, int flags)
        {
            if (t == null || t.Equals(typeof(SharedVariable)))
            {
                return;
            }
            FieldInfo[] fields = t.GetFields((BindingFlags)flags);
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].IsPublic || TaskUtility.HasAttribute(fields[i], typeof(SerializeField)))
                {
                    fieldList.Add(fields[i]);
                }
            }
            TaskUtility.GetSerializableFields(t.BaseType, fieldList, flags);
        }

        private static void GetFields(Type t, ref List<FieldInfo> fieldList, int flags)
        {
            if (t == null || t.Equals(typeof(SharedVariable)))
            {
                return;
            }
            FieldInfo[] fields = t.GetFields((BindingFlags)flags);
            for (int i = 0; i < fields.Length; i++)
            {
                fieldList.Add(fields[i]);
            }
            TaskUtility.GetFields(t.BaseType, ref fieldList, flags);
        }

        public static Type GetTypeWithinAssembly(string typeName)
        {
            Type type;
            if (TaskUtility.typeLookup.TryGetValue(typeName, out type))
            {
                return type;
            }
            type = Type.GetType(typeName);
            if (type == null)
            {
                if (TaskUtility.loadedAssemblies == null)
                {
                    TaskUtility.loadedAssemblies = new List<Assembly>();
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    for (int i = 0; i < assemblies.Length; i++)
                    {
                        TaskUtility.loadedAssemblies.Add(assemblies[i]);
                    }
                }
                for (int j = 0; j < TaskUtility.loadedAssemblies.Count; j++)
                {
                    type = TaskUtility.loadedAssemblies[j].GetType(typeName);
                    if (type != null)
                    {
                        break;
                    }
                }
            }
            if (type != null)
            {
                TaskUtility.typeLookup.Add(typeName, type);
            }
            else if (typeName.Contains("BehaviorDesigner.Runtime.Tasks.Basic"))
            {
                return TaskUtility.GetTypeWithinAssembly(typeName.Replace("BehaviorDesigner.Runtime.Tasks.Basic", "BehaviorDesigner.Runtime.Tasks.Unity"));
            }
            return type;
        }

        public static bool CompareType(Type t, string typeName)
        {
            Type type = Type.GetType(typeName + ", Assembly-CSharp");
            if (type == null)
            {
                type = Type.GetType(typeName + ", Assembly-CSharp-firstpass");
            }
            return t.Equals(type);
        }

        public static bool HasAttribute(FieldInfo field, Type attribute)
        {
            if (field == null)
            {
                return false;
            }
            Dictionary<Type, bool> dictionary;
            if (!TaskUtility.hasFieldLookup.TryGetValue(field, out dictionary))
            {
                dictionary = new Dictionary<Type, bool>();
                TaskUtility.hasFieldLookup.Add(field, dictionary);
            }
            bool flag;
            if (!dictionary.TryGetValue(attribute, out flag))
            {
                flag = (field.GetCustomAttributes(attribute, false).Length > 0);
                dictionary.Add(attribute, flag);
            }
            return flag;
        }

        public static char[] TrimCharacters = new char[]
        {
            '/'
        };

        private static Dictionary<string, Type> typeLookup = new Dictionary<string, Type>();

        private static List<Assembly> loadedAssemblies = null;

        private static Dictionary<Type, FieldInfo[]> allFieldsLookup = new Dictionary<Type, FieldInfo[]>();

        private static Dictionary<Type, FieldInfo[]> serializableFieldsLookup = new Dictionary<Type, FieldInfo[]>();

        private static Dictionary<Type, FieldInfo[]> publicFieldsLookup = new Dictionary<Type, FieldInfo[]>();

        private static Dictionary<FieldInfo, Dictionary<Type, bool>> hasFieldLookup = new Dictionary<FieldInfo, Dictionary<Type, bool>>();
    }
}
