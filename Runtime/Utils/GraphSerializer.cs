#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
#define USE_ODIN

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
#endif

#if USE_ODIN
using Sirenix.Serialization;
#endif

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public struct JsonElement
    {
        public string type;
        public string json;
    }

    public static class GraphSerializer
    {
#if USE_ODIN
        public static string SerializeValue<T>(T targetObject, out List<UnityObject> referencedUnityObjects)
        {
            return System.Text.Encoding.UTF8.GetString(Sirenix.Serialization.SerializationUtility.SerializeValue(targetObject, DataFormat.JSON, out referencedUnityObjects));
        }


        public static T DeserializeValue<T>(string _json, List<UnityObject> _referencedUnityObjects)
        {
            return Sirenix.Serialization.SerializationUtility.DeserializeValue<T>(System.Text.Encoding.UTF8.GetBytes(_json), DataFormat.JSON, _referencedUnityObjects);
        }
#else
        public static string SerializeToJson(object _targetObject)
        {
#if UNITY_EDITOR
            return EditorJsonUtility.ToJson(_targetObject);
#else
			return JsonUtility.ToJson(_targetObject);
#endif
        }

        public static JsonElement SerializeToJsonElement(object _targetObject)
        {
            JsonElement serializedData = new JsonElement();
            serializedData.type = _targetObject.GetType().AssemblyQualifiedName;
            serializedData.json = SerializeToJson(_targetObject);
            return serializedData;
        }

        public static T Deserialize<T>(string _json)
        {
            var targetObject = Activator.CreateInstance<T>();
#if UNITY_EDITOR
            EditorJsonUtility.FromJsonOverwrite(_json, targetObject);
#else
			JsonUtility.FromJsonOverwrite(_json, targetObject);
#endif
            return targetObject;
        }

        public static object Deserialize(string _json, Type _type)
        {
            var targetObject = Activator.CreateInstance(_type);
#if UNITY_EDITOR
            EditorJsonUtility.FromJsonOverwrite(_json, targetObject);
#else
			JsonUtility.FromJsonOverwrite(_json, targetObject);
#endif
            return targetObject;
        }

        public static T Deserialize<T>(JsonElement _serializedData)
        {
            if (string.IsNullOrEmpty(_serializedData.type) || string.IsNullOrEmpty(_serializedData.json))
                throw new ArgumentException("数据为空");
            if (typeof(T) != Type.GetType(_serializedData.type))
                throw new ArgumentException("类型不匹配");
            return Deserialize<T>(_serializedData.json);
        }

        public static object Deserialize(JsonElement _serializeData)
        {
            if (string.IsNullOrEmpty(_serializeData.type) || string.IsNullOrEmpty(_serializeData.json))
                return null;
            Type type = Type.GetType(_serializeData.type);
            if (type == null) return null;
            return Deserialize(_serializeData.json, type);
        }
#endif
    }
}