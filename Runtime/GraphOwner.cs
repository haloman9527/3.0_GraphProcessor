using CZToolKit.Core.SharedVariable;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    public abstract class GraphOwner : MonoBehaviour, IVariableOwner/*, ISerializationCallbackReceiver*/
    {
        [SerializeField, SerializeReference, HideInInspector]
        List<SharedVariable> variables = new List<SharedVariable>();
        Dictionary<string, int> sharedVariableIndex;

        public abstract BaseGraph Graph { get; set; }
        public abstract Type GraphType { get; }

        #region Serialize
        //bool initializedVariables;

        //[SerializeField]
        //List<JsonElement> serializedVariables = new List<JsonElement>();

        //public virtual void OnBeforeSerialize()
        //{
        //    Serialize();
        //}

        //public virtual void OnAfterDeserialize() { }

        //void Serialize()
        //{
        //    if (variables == null) return;
        //    serializedVariables.Clear();
        //    foreach (var variable in variables)
        //    {
        //        if (variable == null) continue;
        //        if (variable is SharedGameObject)
        //        {
        //            Debug.Log((variable as SharedGameObject).Value == null);
        //        }
        //        serializedVariables.Add(JsonSerializer.Serialize(variable));
        //    }
        //}

        //void Deserialize()
        //{
        //    if (variables == null)
        //        variables = new List<SharedVariable>();
        //    else
        //        variables.Clear();
        //    foreach (var serializedVariable in serializedVariables)
        //    {
        //        SharedVariable variable = JsonSerializer.Deserialize(serializedVariable) as SharedVariable;
        //        if (variable == null) continue;
        //        variables.Add(variable);
        //    }
        //    UpdateVariablesIndex();
        //}

        //void CheckSerialization()
        //{
        //    if (!initializedVariables)
        //    {
        //        Deserialize();
        //        initializedVariables = true;
        //    }
        //}
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
            //CheckSerialization();
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
            //CheckSerialization();
            return variables;
        }

        public void SetVariable(SharedVariable sharedVariable)
        {
            if (sharedVariable == null) return;
            //CheckSerialization();

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

        public IList<SharedVariable> GetVariables()
        {
            //CheckSerialization();
            return variables;
        }

        public void SetVariables(List<SharedVariable> _variables)
        {
            variables = _variables;
            //UpdateVariablesIndex();
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
