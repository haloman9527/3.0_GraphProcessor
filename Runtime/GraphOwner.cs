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
using CZToolKit.GraphProcessor.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    public abstract class GraphOwner<TGraph> : InternalGraphOwner where TGraph : BaseGraph, new()
    {
        #region 属性
        public override Type GraphType { get { return typeof(TGraph); } }

        #endregion

        #region Serialize
        #region Graph
        [NonSerialized]
        bool initializedGraph;
        [HideInInspector]
        [SerializeField]
        string serializedGraph;
        [HideInInspector]
        [SerializeField]
        List<UnityObject> graphUnityReferences;

        public override void SaveGraph(BaseGraph graph)
        {
            serializedGraph = GraphSerializer.SerializeValue(graph, out graphUnityReferences);
        }
        #endregion

        #region Variables
        [NonSerialized]
        bool initializedVariables;
        [HideInInspector]
        [SerializeField]
        string serializedVariables;
        [HideInInspector]
        [SerializeField]
        List<UnityObject> variablesUnityReferences;

        public override void SaveVariables()
        {
            serializedVariables = GraphSerializer.SerializeValue(variables, out variablesUnityReferences);
        }

        void DeserializeVariables()
        {
            variables = GraphSerializer.DeserializeValue<List<SharedVariable>>(serializedVariables, variablesUnityReferences);
            UpdateVariablesIndex();
        }

        protected override void CheckVaraiblesSerialization()
        {
            if (initializedVariables) return;
            initializedVariables = true;
            DeserializeVariables();
        }
        #endregion

        #endregion

        private void Reset()
        {
            SaveGraph(new TGraph());
        }
    }
}
