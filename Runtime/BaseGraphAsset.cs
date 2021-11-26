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
using CZToolKit.GraphProcessor.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public abstract class BaseGraphAsset<GraphClass> : InternalBaseGraphAsset, ISerializationCallbackReceiver
        where GraphClass : BaseGraph, new()
    {
        #region 字段
        [HideInInspector]
        [SerializeField]
        GraphClass graph = new GraphClass();
        #endregion

        #region 属性
        public GraphClass T_Graph { get { return graph; } }
        public override BaseGraph Graph { get { return graph; } }
        #endregion

        public BaseGraphAsset() { }

        protected virtual void OnEnable()
        {
            CheckGraphSerialization();
        }

        #region Serialize
        [NonSerialized]
        bool initializedGraph;
        [HideInInspector]
        [SerializeField]
        [TextArea(20, 20)]
        string serializedGraph = String.Empty;
        [HideInInspector]
        [SerializeField]
        List<UnityObject> graphUnityReferences = new List<UnityObject>();

        public override void SaveGraph()
        {
            serializedGraph = GraphSerializer.SerializeValue(graph, out graphUnityReferences);
        }

        void DeserializeGraph()
        {
            graph = GraphSerializer.DeserializeValue<GraphClass>(serializedGraph, graphUnityReferences);
            if (graph == null)
                graph = new GraphClass();
            graph.Enable();
        }

        public void OnBeforeSerialize()
        {
            //SaveGraph();
        }

        public void OnAfterDeserialize()
        {
            CheckGraphSerialization();
        }

        public override void CheckGraphSerialization()
        {
            if (initializedGraph) return;
            initializedGraph = true;
            DeserializeGraph();
        }
        #endregion
    }
}