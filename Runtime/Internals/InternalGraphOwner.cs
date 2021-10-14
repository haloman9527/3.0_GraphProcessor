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
    public abstract class InternalGraphOwner : MonoBehaviour, IGraphOwner, IGraphAsset, IVariableOwner
    {
        #region 字段
        protected List<SharedVariable> variables = new List<SharedVariable>();
        protected Dictionary<string, int> sharedVariableIndex;
        #endregion

        #region 属性
        public abstract BaseGraph Graph { get; }
        public abstract Type GraphType { get; }
        #endregion

        #region Serialize
        public abstract void SaveVariables();

        public abstract void SaveGraph();

        public abstract void CheckGraphSerialization();

        protected abstract void CheckVaraiblesSerialization();
        #endregion

        #region API
        public UnityObject Self()
        {
            return this;
        }

        public string GetOwnerName()
        {
            return gameObject.name;
        }

        public SharedVariable GetVariable(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            CheckVaraiblesSerialization();
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
            CheckVaraiblesSerialization();
            return variables;
        }

        public void SetVariable(SharedVariable sharedVariable)
        {
            if (sharedVariable == null) return;
            CheckVaraiblesSerialization();

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
            CheckVaraiblesSerialization();
            return variables;
        }

        public void SetVariables(List<SharedVariable> variables)
        {
            this.variables = variables;
            UpdateVariablesIndex();
        }
        #endregion

        protected void UpdateVariablesIndex()
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
    }
}