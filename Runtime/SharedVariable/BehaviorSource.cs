using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public class BehaviorSource : IVariableSource
    {
        [SerializeField]
        IGraphOwner owner;
        [SerializeField, SerializeReference]
        List<SharedVariable> variables;
        Dictionary<string, int> sharedVariableIndex;

        //[SerializeField, HideInInspector]
        //VariableSerializationDatas variableSerializationDatas = new VariableSerializationDatas();

        public List<SharedVariable> Variables
        {
            get { return this.variables; }
            set { this.variables = value; this.UpdateVariablesIndex(); }
        }

        public IGraphOwner Owner
        {
            get { return this.owner; }
            set { this.owner = value; }
        }

        public BehaviorSource(IGraphOwner owner)
        {
            this.Initialize(owner);
        }

        public void Initialize(IGraphOwner owner)
        {
            this.owner = owner;
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

        public void SetVariable(string _guid, SharedVariable sharedVariable)
        {
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
                    Debug.LogError(string.Format("Error: Unable to set SharedVariable {0} - the variable type {1} does not match the existing type {2}", _guid, sharedVariable2.GetType(), sharedVariable.GetType()));
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

        public void SetAllVariables(List<SharedVariable> _variables)
        {
            variables = _variables;
            UpdateVariablesIndex();
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

    [Serializable]
    public class VariableSerializationDatas
    {
        [SerializeField]
        List<string> types = new List<string>();

        [SerializeField]
        List<string> variableDatas = new List<string>();

        public void Load(List<SharedVariable> _variables)
        {
            types.Clear();
            variableDatas.Clear();
            foreach (var variable in _variables)
            {
                types.Add(variable.GetType().FullName);
                variableDatas.Add(JsonUtility.ToJson(variable));
            }
        }

        public List<SharedVariable> From()
        {
            List<SharedVariable> variables = new List<SharedVariable>();
            for (int i = 0; i < variableDatas.Count; i++)
            {
                Type type = Type.GetType(types[i]);
                variables.Add(JsonUtility.FromJson(variableDatas[i], type) as SharedVariable);
            }
            return variables;
        }
    }
}
