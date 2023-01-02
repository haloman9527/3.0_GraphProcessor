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

using System;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor
{
    public class Blackboard
    {
        public interface IDataContainer
        {
            object Get(string key);

            bool TryGet(string key, out object value);

            void Remove(string key);

            void Clear();
        }

        public class DataContainer<T> : IDataContainer
        {
            private Dictionary<string, T> data = new Dictionary<string, T>();

            public T this[string key]
            {
                get { return Get(key); }
                set { Set(key, value); }
            }

            object IDataContainer.Get(string key)
            {
                if (this.data.TryGetValue(key, out var value))
                    return value;
                return null;
            }

            bool IDataContainer.TryGet(string key, out object value)
            {
                var result = this.data.TryGetValue(key, out var v);
                value = v;
                return result;
            }

            public T Get(string key)
            {
                if (this.data.TryGetValue(key, out var value))
                    return value;
                return default;
            }

            public bool TryGet(string key, out T value)
            {
                return this.data.TryGetValue(key, out value);
            }

            public void Set(string key, T value)
            {
                this.data[key] = value;
            }

            public void Remove(string key)
            {
                data.Remove(key);
            }

            public void Clear()
            {
                data.Clear();
            }
        }

        private DataContainer<object> objectDataContainer = new DataContainer<object>();
        private Dictionary<string, IDataContainer> keyContainerMap = new Dictionary<string, IDataContainer>();
        private Dictionary<Type, IDataContainer> structDataContainers = new Dictionary<Type, IDataContainer>();

        public T Get<T>(string key)
        {
            if (!this.keyContainerMap.TryGetValue(key, out var dataContainer))
                return default;
            var type = typeof(T);
            var isValueType = type.IsValueType;
            if (isValueType)
                return ((DataContainer<T>)dataContainer).Get(key);
            else
                return (T)((DataContainer<object>)dataContainer).Get(key);
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (!this.keyContainerMap.TryGetValue(key, out var dataContainer))
            {
                value = default;
                return false;
            }

            var type = typeof(T);
            var isValueType = type.IsValueType;
            if (isValueType)
                return ((DataContainer<T>)dataContainer).TryGet(key, out value);
            else
            {
                var result = ((DataContainer<object>)dataContainer).TryGet(key, out var v);
                value = (T)v;
                return result;
            }
        }

        public void Set<T>(string key, T value)
        {
            var type = typeof(T);
            var isValueType = type.IsValueType;
            var exists = true;
            if (!keyContainerMap.TryGetValue(key, out var dataContainer))
            {
                exists = false;
                if (isValueType)
                {
                    if (!structDataContainers.TryGetValue(type, out dataContainer))
                        structDataContainers[type] = dataContainer = new DataContainer<T>();

                    keyContainerMap[key] = dataContainer;
                }
                else
                    keyContainerMap[key] = dataContainer = objectDataContainer;
            }

            if (isValueType)
                ((DataContainer<T>)dataContainer).Set(key, value);
            else
                ((DataContainer<object>)dataContainer).Set(key, value);
        }

        public void Remove(string key)
        {
            if (!keyContainerMap.TryGetValue(key, out var dataContainer))
                return;

            keyContainerMap.Remove(key);
            dataContainer.Remove(key);
        }

        public void Clear()
        {
            objectDataContainer.Clear();
            structDataContainers.Clear();
            keyContainerMap.Clear();
        }
    }
}