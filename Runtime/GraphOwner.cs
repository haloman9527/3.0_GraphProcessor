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
    public abstract class GraphOwner<TGraph> : InternalGraphOwner, ISerializationCallbackReceiver
        where TGraph : BaseGraph, new()
    {
        #region 字段
        [HideInInspector]
        [SerializeField]
        TGraph graph = new TGraph();
        #endregion

        #region 属性
        public override BaseGraph Graph
        {
            get { return graph; }
        }

        public TGraph T_Graph
        {
            get { return graph; }
        }

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

        public override void SaveGraph()
        {
            serializedGraph = JsonSerializer.SerializeValue(graph, out graphUnityReferences);
        }

        void DeserializeGraph()
        {
            graph = JsonSerializer.DeserializeValue<TGraph>(serializedGraph, graphUnityReferences);
            graph.Enable();
            graph.Initialize(this);
        }

        public override void CheckGraphSerialization()
        {
            if (initializedGraph) return;
            initializedGraph = true;
            DeserializeGraph();
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
            serializedVariables = JsonSerializer.SerializeValue(variables, out variablesUnityReferences);
        }

        void DeserializeVariables()
        {
            variables = JsonSerializer.DeserializeValue<List<SharedVariable>>(serializedVariables, variablesUnityReferences);
            UpdateVariablesIndex();
        }

        protected override void CheckVaraiblesSerialization()
        {
            if (initializedVariables) return;
            initializedVariables = true;
            DeserializeVariables();
        }
        #endregion

        public void OnBeforeSerialize()
        {
            //SaveGraph();
            //SaveVariables();
        }

        public void OnAfterDeserialize()
        {
            CheckGraphSerialization();
            CheckVaraiblesSerialization();
        }

        #endregion

        private void Reset()
        {
            graph = new TGraph();
        }
    }
}
