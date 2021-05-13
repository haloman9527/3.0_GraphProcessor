using CZToolKit.Core.SharedVariable;
using System;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    public abstract class GraphOwner : MonoBehaviour, IVariableOwner
    {
        [SerializeField, SerializeReference, HideInInspector]
        List<SharedVariable> variables;
        Dictionary<string, int> sharedVariableIndex;

        public List<SharedVariable> Variables
        {
            get { return this.variables; }
            set
            {
                this.variables = value;
                this.UpdateVariablesIndex();
            }
        }

        public abstract BaseGraph Graph { get; set; }
        public abstract Type GraphType { get; }

        public GraphOwner()
        {
            variables = new List<SharedVariable>();
        }

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
            if (this.variables != null)
            {
                if (this.sharedVariableIndex == null || this.sharedVariableIndex.Count != this.variables.Count)
                    this.UpdateVariablesIndex();
                int index;
                if (this.sharedVariableIndex.TryGetValue(_guid, out index))
                    return this.variables[index];
            }
            return null;
        }

        public List<SharedVariable> GetAllVariables()
        {
            return this.variables;
        }

        public void SetVariable(SharedVariable sharedVariable)
        {
            if (sharedVariable == null) return;

            if (variables == null)
                variables = new List<SharedVariable>();
            else if (sharedVariableIndex == null)
                UpdateVariablesIndex();
            int index;
            if (sharedVariableIndex != null && this.sharedVariableIndex.TryGetValue(sharedVariable.GUID, out index))
            {
                SharedVariable sharedVariable2 = this.variables[index];
                if (!sharedVariable2.GetType().Equals(typeof(SharedVariable)) && !sharedVariable2.GetType().Equals(sharedVariable.GetType()))
                {
                    Debug.LogError(string.Format("Error: Unable to set SharedVariable {0} - the variable type {1} does not match the existing type {2}", sharedVariable.GUID, sharedVariable2.GetType(), sharedVariable.GetType()));
                }
                else
                {
                    sharedVariable2.SetValue(sharedVariable.GetValue());
                }
            }
            else
            {
                variables.Add(sharedVariable);
                //sharedVariable.InitializePropertyMapping(this);
                UpdateVariablesIndex();
            }
        }

        public void SetVariableValue(string _guid, object _value)
        {
            GetVariable(_guid)?.SetValue(_value);
        }

        private void UpdateVariablesIndex()
        {
            if (this.variables == null)
            {
                if (this.sharedVariableIndex != null)
                    this.sharedVariableIndex = null;
                return;
            }
            if (this.sharedVariableIndex == null)
                this.sharedVariableIndex = new Dictionary<string, int>(this.variables.Count);
            else
                this.sharedVariableIndex.Clear();
            for (int i = 0; i < this.variables.Count; i++)
            {
                if (this.variables[i] != null)
                    this.sharedVariableIndex.Add(this.variables[i].GUID, i);
            }
        }

        //[SerializeField, HideInInspector]
        //VariableSerializationDatas variableSerializationDatas = new VariableSerializationDatas();

        //public void OnBeforeSerialize()
        //{
        //    if (variableSerializationDatas == null)
        //        variableSerializationDatas = new VariableSerializationDatas();
        //    variableSerializationDatas.Load(variables);
        //}

        //public void OnAfterDeserialize()
        //{
        //    SetAllVariables(variableSerializationDatas.From());

        //    if (Owner == null) return;
        //    if (Owner.Graph == null) return;
        //    foreach (var variable in Owner.Graph.GetVariables())
        //    {
        //        if (GetVariable(variable.GUID) == null)
        //        {
        //            SharedVariable v = variable.Clone() as SharedVariable;
        //            SetVariable(v.GUID, v);
        //        }
        //    }
        //}
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
