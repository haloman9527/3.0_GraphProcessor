using CZToolKit.Core.SharedVariable;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    public abstract class GraphOwner : MonoBehaviour, IVariableOwner, ISerializationCallbackReceiver
    {
        List<SharedVariable> variables = new List<SharedVariable>();
        Dictionary<string, int> sharedVariableIndex;

        public abstract BaseGraph Graph { get; set; }
        public abstract Type GraphType { get; }

        #region Serialize

        [Serializable]
        public struct ObjectKV
        {
            public string guid;
            public Object single;
            public List<Object> multi;
        }

        [NonSerialized]
        bool initializedVariables;
        [SerializeField, HideInInspector]
        List<JsonElement> serializedVariables = new List<JsonElement>();
        [SerializeField, HideInInspector]
        List<ObjectKV> objectsCache = new List<ObjectKV>();

        public virtual void OnBeforeSerialize()
        {
            Serialize();
        }

        public virtual void OnAfterDeserialize() { }

        void Serialize()
        {
            if (variables.Count == 0) return;
            if (Application.isPlaying) return;
            serializedVariables.Clear();
            objectsCache.Clear();
            for (int i = 0; i < variables.Count; i++)
            {
                SharedVariable variable = variables[i];
                if (variable == null) continue;
                serializedVariables.Add(JsonSerializer.SerializeToJsonElement(variable));
                if (variable is ISharedObject sharedObject)
                {
                    ObjectKV kv = new ObjectKV() { guid = variable.GUID, single = sharedObject.GetObject() };
                    objectsCache.Add(kv);
                }
                else if (variable is ISharedObjectList sharedList && typeof(Object).IsAssignableFrom(sharedList.GetElementType()))
                {
                    List<Object> objs = new List<Object>();
                    foreach (var item in sharedList.GetList())
                    {
                        objs.Add(item as Object);
                    }
                    ObjectKV kv = new ObjectKV() { guid = variable.GUID, multi = objs };
                    objectsCache.Add(kv);
                }
            }
        }

        void Deserialize()
        {
            if (variables == null)
                variables = new List<SharedVariable>();
            else
                variables.Clear();
            foreach (var serializedVariable in serializedVariables)
            {
                SharedVariable variable = JsonSerializer.Deserialize(serializedVariable) as SharedVariable;
                if (variable == null) continue;
                variables.Add(variable);
            }
            UpdateVariablesIndex();

            foreach (var item in objectsCache)
            {
                SharedVariable variable = GetVariable(item.guid);
                if (variable == null) continue;
                if (variable is ISharedObject sharedObject)
                {
                    sharedObject.SetObject(item.single);
                }
                else if (variable is ISharedObjectList sharedList)
                {
                    sharedList.FillList(item.multi);
                }
            }
        }

        void CheckSerialization()
        {
            if (initializedVariables) return;
            initializedVariables = true;
            Deserialize();
        }
        #endregion

        public Object GetObject()
        {
            return this;
        }

        public string GetOwnerName()
        {
            return name;
        }

        public SharedVariable GetVariable(string _guid)
        {
            if (string.IsNullOrEmpty(_guid)) return null;
            CheckSerialization();
            if (variables != null)
            {
                if (sharedVariableIndex == null || sharedVariableIndex.Count != variables.Count)
                    UpdateVariablesIndex();
                int index;
                if (sharedVariableIndex.TryGetValue(_guid, out index))
                    return variables[index];
            }
            return null;
        }

        public List<SharedVariable> GetAllVariables()
        {
            CheckSerialization();
            return variables;
        }

        public void SetVariable(SharedVariable sharedVariable)
        {
            if (sharedVariable == null) return;
            CheckSerialization();

            if (variables == null)
                variables = new List<SharedVariable>();
            else if (sharedVariableIndex == null)
                UpdateVariablesIndex();
            int index;
            if (sharedVariableIndex != null && sharedVariableIndex.TryGetValue(sharedVariable.GUID, out index))
            {
                SharedVariable sharedVariable2 = variables[index];
                if (!sharedVariable2.GetType().Equals(typeof(SharedVariable)) && !sharedVariable2.GetType().Equals(sharedVariable.GetType()))
                    Debug.LogError(string.Format("Error: Unable to set SharedVariable {0} - the variable type {1} does not match the existing type {2}", sharedVariable.GUID, sharedVariable2.GetType(), sharedVariable.GetType()));
                else
                    sharedVariable2.SetValue(sharedVariable.GetValue());
            }
            else
            {
                variables.Add(sharedVariable);
                UpdateVariablesIndex();
            }
        }

        public void SetVariableValue(string _guid, object _value)
        {
            GetVariable(_guid)?.SetValue(_value);
        }

        private void UpdateVariablesIndex()
        {
            if (variables == null)
            {
                if (sharedVariableIndex != null)
                    sharedVariableIndex = null;
                return;
            }
            if (sharedVariableIndex == null)
                sharedVariableIndex = new Dictionary<string, int>(variables.Count);
            else
                sharedVariableIndex.Clear();
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i] != null)
                    sharedVariableIndex.Add(variables[i].GUID, i);
            }
        }

        public IReadOnlyList<SharedVariable> GetVariables()
        {
            CheckSerialization();
            return variables;
        }

        public void SetVariables(List<SharedVariable> _variables)
        {
            variables = _variables;
            UpdateVariablesIndex();
        }
    }

    public abstract class GraphOwner<T> : GraphOwner where T : BaseGraph
    {
        [SerializeField]
        T graph;

        public T TGraph { get { return graph; } set { Graph = value; } }
        public override BaseGraph Graph
        {
            get { return graph; }
            set
            {
                if (graph != value)
                {
                    graph = value as T;
                    if (graph != null)
                    {
                        foreach (var variable in graph.Variables)
                        {
                            if (GetVariable(variable.GUID) == null)
                                SetVariable(variable.Clone() as SharedVariable);
                        }
                    }
                }
            }
        }
        public override Type GraphType { get { return typeof(T); } }
    }
}
