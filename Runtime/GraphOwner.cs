using CZToolKit.Core.SharedVariable;
using OdinSerializer;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    public abstract class GraphOwner : MonoBehaviour, IVariableOwner, ISerializationCallbackReceiver
    {
        List<SharedVariable> variables = new List<SharedVariable>();
        Dictionary<string, int> sharedVariableIndex;

        public abstract BaseGraph Graph { get; }
        public abstract Type GraphType { get; }

        #region Serialize
        [SerializeField]
        string serializedVariables;
        [SerializeField]
        List<UnityObject> unityReferences;
        [NonSerialized]
        bool initializedVariables;

        public virtual void OnBeforeSerialize()
        {
            Serialize();
        }

        public virtual void OnAfterDeserialize()
        {
            CheckSerialization();
        }

        void Serialize()
        {
            serializedVariables = Encoding.UTF8.GetString(SerializationUtility.SerializeValue(variables, DataFormat.JSON, out unityReferences));
        }

        void Deserialize()
        {
            variables = SerializationUtility.DeserializeValue<List<SharedVariable>>(Encoding.UTF8.GetBytes(serializedVariables), DataFormat.JSON, unityReferences);
            UpdateVariablesIndex();
        }

        void CheckSerialization()
        {
            if (initializedVariables) return;
            initializedVariables = true;
            Deserialize();
            Graph.SetFrom(this);
            Graph.Flush();
            Graph.InitializePropertyMapping(this);
        }

        #endregion

        public UnityObject Self()
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

    public abstract class GraphOwner<GraphClass> : GraphOwner
        where GraphClass : IBaseGraph, new()
    {
        [SerializeField]
        GraphClass graph = new GraphClass();

        public override BaseGraph Graph
        {
            get { return Graph; }
        }

        public GraphClass TGraph
        {
            get { return graph; }
        }

        #region Serialize
        [SerializeField]
        string serializedGraph;
        [SerializeField]
        List<UnityObject> graphUnityReferences;

        public override void OnBeforeSerialize()
        {
            serializedGraph = Encoding.UTF8.GetString(SerializationUtility.SerializeValue(Graph, DataFormat.JSON, out graphUnityReferences));
            base.OnBeforeSerialize();
        }

        public override void OnAfterDeserialize()
        {
            graph = SerializationUtility.DeserializeValue<GraphClass>(Encoding.UTF8.GetBytes(serializedGraph), DataFormat.JSON, graphUnityReferences);
            base.OnAfterDeserialize();
        }

        #endregion

        public override Type GraphType { get { return typeof(GraphClass); } }
    }
}
