#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 
 *
 */
#endregion
using System;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public struct SerializedData
    {
        public string type;
        public string json;

        public void Serialize(object _object)
        {
            type = _object.GetType().FullName;
#if UNITY_EDITOR
            json = UnityEditor.EditorJsonUtility.ToJson(_object);
#else
            json = JsonUtility.ToJson(_object);
#endif
        }

        public object Deserialize()
        {
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(json)) return null;
            Type objectType = Type.GetType(type);

#if UNITY_EDITOR
            object overrideObject = Activator.CreateInstance(objectType, true);
            UnityEditor.EditorJsonUtility.FromJsonOverwrite(json, overrideObject);
            return overrideObject;
#else
            return JsonUtility.FromJson(json, objectType);
#endif
        }
    }
}