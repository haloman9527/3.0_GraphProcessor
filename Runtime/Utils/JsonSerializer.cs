using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public struct JsonElement
    {
        public string type;
        public string json;
    }

    public static class JsonSerializer
    {
        public static JsonElement Serialize(object _targetObject)
        {
            JsonElement serializedData = new JsonElement();
            serializedData.type = _targetObject.GetType().AssemblyQualifiedName;
#if UNITY_EDITOR
            serializedData.json = EditorJsonUtility.ToJson(_targetObject);
#else
			serializedData.json = JsonUtility.ToJson(_targetObject);
#endif
            return serializedData;
        }

        public static T Deserialize<T>(JsonElement _serializedData)
        {
            if (string.IsNullOrEmpty(_serializedData.type) || string.IsNullOrEmpty(_serializedData.json))
                throw new ArgumentException("数据为空");
            if (typeof(T) != Type.GetType(_serializedData.type))
                throw new ArgumentException("类型不匹配");

            var targetObject = Activator.CreateInstance<T>();
#if UNITY_EDITOR
            EditorJsonUtility.FromJsonOverwrite(_serializedData.json, targetObject);
#else
			JsonUtility.FromJsonOverwrite(_serializedData.json, targetObject);
#endif
            return targetObject;
        }

        public static object Deserialize(JsonElement _serializeData)
        {
            if (string.IsNullOrEmpty(_serializeData.type) || string.IsNullOrEmpty(_serializeData.json))
                return null;
            Type type = Type.GetType(_serializeData.type);
            if (type == null) return null;
            var targetObject = Activator.CreateInstance(type);
#if UNITY_EDITOR
            EditorJsonUtility.FromJsonOverwrite(_serializeData.json, targetObject);
#else
			JsonUtility.FromJsonOverwrite(_serializeData.json, targetObject);
#endif
            return targetObject;
        }
    }
}