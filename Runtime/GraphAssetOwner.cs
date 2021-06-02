using CZToolKit.Core.SharedVariable;
using OdinSerializer;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{

    public abstract class GraphAssetOwner : MonoBehaviour, IGraphAssetOwner, ISerializationCallbackReceiver
    {
        List<SharedVariable> variables = new List<SharedVariable>();
        Dictionary<string, int> sharedVariableIndex;

        public abstract BaseGraphAsset GraphAsset { get; set; }
        public abstract IBaseGraph Graph { get; }
        public abstract Type GraphAssetType { get; }
        public abstract Type GraphType { get; }

        #region Serialize

        [HideInInspector]
        [SerializeField]
        string serializedVariables;
        [HideInInspector]
        [SerializeField]
        List<UnityObject> variablesUnityReference;
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
            serializedVariables = Encoding.UTF8.GetString(SerializationUtility.SerializeValue(variables, DataFormat.JSON, out variablesUnityReference));
        }

        void Deserialize()
        {
            variables = SerializationUtility.DeserializeValue<List<SharedVariable>>(Encoding.UTF8.GetBytes(serializedVariables), DataFormat.JSON, variablesUnityReference);
            UpdateVariablesIndex();
        }

        void CheckSerialization()
        {
            if (initializedVariables) return;
            initializedVariables = true;
            Deserialize();
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

    public abstract class GraphAssetOwner<GraphAssetClass, GraphClass> : GraphAssetOwner
        where GraphAssetClass : BaseGraphAsset<GraphClass>
        where GraphClass : IBaseGraph, IBaseGraphFromAsset, new()
    {
        [SerializeField]
        GraphAssetClass graphAsset;

        public override BaseGraphAsset GraphAsset
        {
            get { return graphAsset; }
            set
            {
                if (graphAsset != value)
                {
                    graphAsset = value as GraphAssetClass;
                    if (graphAsset != null)
                    {
                        foreach (var variable in graphAsset.Graph.Variables)
                        {
                            if (GetVariable(variable.GUID) == null)
                                SetVariable(variable.Clone() as SharedVariable);
                        }
                    }
                }
            }
        }

        public GraphAssetClass TGraphAsset
        {
            get { return graphAsset; }
            set
            {
                GraphAsset = value;
                if (graphAsset != value)
                {
                    graphAsset = value;
                    if (graphAsset != null)
                    {
                        foreach (var variable in GraphAsset.Graph.Variables)
                        {
                            if (GetVariable(variable.GUID) == null)
                                SetVariable(variable.Clone() as SharedVariable);
                        }
                    }
                }
            }
        }
        public override IBaseGraph Graph
        {
            get { return GraphAsset.Graph; }
        }

        public GraphClass TGraph
        {
            get { return TGraphAsset.TGraph; }
        }

        public override Type GraphAssetType { get { return typeof(GraphAssetClass); } }
        public override Type GraphType { get { return typeof(GraphClass); } }
    }
}
