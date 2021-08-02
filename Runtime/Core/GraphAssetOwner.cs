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
using CZToolKit.Core.SharedVariable;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    public abstract class GraphAssetOwner : MonoBehaviour, IGraphAssetOwner, IGraphOwner, ISerializationCallbackReceiver
    {
        #region 字段
        List<SharedVariable> variables = new List<SharedVariable>();
        Dictionary<string, int> sharedVariableIndex;
        #endregion

        #region 属性
        public abstract BaseGraphAsset GraphAsset { get; set; }
        public abstract BaseGraph Graph { get; }
        public abstract Type GraphAssetType { get; }
        #endregion

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
            //SaveVariables();
        }

        public virtual void OnAfterDeserialize()
        {
            CheckSerialization();
        }

        public void SaveVariables()
        {
            serializedVariables = JsonSerializer.SerializeValue(variables, out variablesUnityReference);
        }

        void DeserializeVariables()
        {
            if (string.IsNullOrEmpty(serializedVariables))
                variables = new List<SharedVariable>();
            else
                variables = JsonSerializer.DeserializeValue<List<SharedVariable>>(serializedVariables, variablesUnityReference);
            if (variables == null)
                variables = new List<SharedVariable>();

            UpdateVariablesIndex();
        }

        void CheckSerialization()
        {
            if (initializedVariables) return;
            initializedVariables = true;
            DeserializeVariables();
        }

        #endregion

        #region API
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
        #endregion

        #region 帮助方法
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
        #endregion
    }

    public abstract class GraphAssetOwner<TGraphAsset, TGraph> : GraphAssetOwner
        where TGraphAsset : BaseGraphAsset<TGraph>
        where TGraph : BaseGraph, new()
    {
        #region 字段
        [SerializeField]
        TGraphAsset graphAsset;
        #endregion

        #region 属性
        public override BaseGraphAsset GraphAsset
        {
            get { return graphAsset; }
            set
            {
                if (graphAsset != value)
                {
                    graphAsset = value as TGraphAsset;
                    if (graphAsset != null)
                    {
                        foreach (var variable in T_Graph.Variables)
                        {
                            if (GetVariable(variable.GUID) == null)
                                SetVariable(variable.Clone() as SharedVariable);
                        }
                    }
                }
            }
        }

        public override Type GraphAssetType { get { return typeof(TGraphAsset); } }

        public TGraphAsset T_GraphAsset { get { return graphAsset; } }

        public override BaseGraph Graph { get { return T_Graph; } }

        public TGraph T_Graph { get { return graphAsset.T_Graph; } }

        #endregion
    }
}
