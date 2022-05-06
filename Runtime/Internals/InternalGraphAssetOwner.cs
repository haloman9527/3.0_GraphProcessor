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
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor.Internal
{
    public abstract class InternalGraphAssetOwner : MonoBehaviour, IGraphOwner, IGraphAssetOwner, IVariableOwner, IVariableSerialization, ISerializationCallbackReceiver
    {
        #region Fields
        List<SharedVariable> variables = new List<SharedVariable>();
        Dictionary<string, int> sharedVariableIndex;
        #endregion

        #region Properties
        public abstract UnityObject GraphAsset { get; set; }
        public abstract Type GraphAssetType { get; }
        public abstract IGraph Graph { get; }
        public abstract Type GraphType { get; }
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
            serializedVariables = GraphSerializer.SerializeValue(variables, out variablesUnityReference);
        }

        void DeserializeVariables()
        {
            if (string.IsNullOrEmpty(serializedVariables))
                variables = new List<SharedVariable>();
            else
                variables = GraphSerializer.DeserializeValue<List<SharedVariable>>(serializedVariables, variablesUnityReference);
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
        public SharedVariable GetVariable(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            CheckSerialization();
            if (variables != null)
            {
                if (sharedVariableIndex == null || sharedVariableIndex.Count != variables.Count)
                    UpdateVariablesIndex();
                int index;
                if (sharedVariableIndex.TryGetValue(guid, out index))
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

        public void SetVariableValue(string guid, object value)
        {
            GetVariable(guid)?.SetValue(value);
        }

        public IReadOnlyList<SharedVariable> GetVariables()
        {
            CheckSerialization();
            return variables;
        }

        public void SetVariables(List<SharedVariable> variables)
        {
            this.variables = variables;
            UpdateVariablesIndex();
        }
        #endregion

        #region Helper
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
}