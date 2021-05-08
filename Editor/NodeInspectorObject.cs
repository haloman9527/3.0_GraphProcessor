using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using CZToolKit.Core.Editors;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
#endif

namespace CZToolKit.GraphProcessor.Editors
{
    /// <summary> Custom editor of the node inspector, you can inherit from this class to customize your node inspector. </summary>
    [CustomEditor(typeof(NodeInspectorObject))]
    public class NodeInspectorObjectEditor : Editor
    {
        NodeInspectorObject inspector;
#if ODIN_INSPECTOR
        PropertyTree tree;
#endif

        protected virtual void OnEnable()
        {
            inspector = target as NodeInspectorObject;
#if ODIN_INSPECTOR
            tree = PropertyTree.Create(inspector);
            tree.DrawMonoScriptObjectField = true;
#endif
        }

        public override void OnInspectorGUI()
        {
#if ODIN_INSPECTOR
            if (tree != null)
            {
                tree.BeginDraw(true);
                tree.Draw(true);
                tree.EndDraw();
            }
#else
            EditorGUILayoutExtension.DrawFields(inspector.Node);
#endif
        }
    }

    /// <summary> Node inspector object, you can inherit from this class to customize your node inspector. </summary>
#if ODIN_INSPECTOR
    public class NodeInspectorObject : SerializedScriptableObject
#else
    public class NodeInspectorObject : ScriptableObject
#endif
    {
        [HideInInspector]
        /// <summary>Previously selected object by the inspector</summary>
        public Object previouslySelectedObject;
        /// <summary>List of currently selected nodes</summary>
        public HashSet<BaseNodeView> selectedNodes { get; private set; } = new HashSet<BaseNodeView>();

#if ODIN_INSPECTOR
        [HideLabel, HideReferenceObjectPicker]
#else
        [SerializeReference]
#endif
        [SerializeField]
        BaseNode node;

        public BaseNode Node { get { return node; } }

        public virtual void UpdateSelectedNodes(HashSet<BaseNodeView> views)
        {
            selectedNodes = views;
            if (selectedNodes.Count == 1)
                node = selectedNodes.First().NodeData;
            else
                node = null;
        }

        public virtual void NodeViewRemoved(BaseNodeView view)
        {
            selectedNodes.Remove(view);
            if (selectedNodes.Count == 1)
                node = selectedNodes.First().NodeData;
            else
                node = null;
        }

        //public HashSet<EdgeView> selectedEdges { get; private set; } = new HashSet<EdgeView>();
        //[SerializeField]
        //public SerializableEdge edge;
        //public virtual void UpdateSelectedEdges(HashSet<EdgeView> views)
        //{
        //    selectedEdges = views;
        //    if (selectedEdges.Count == 1)
        //        edge = selectedEdges.First().serializedEdge;
        //    else
        //        edge = null;
        //}

        //public virtual void EdgeViewRemoved(EdgeView view)
        //{
        //    selectedEdges.Remove(view);
        //    if (selectedEdges.Count == 1)
        //        edge = selectedEdges.First().serializedEdge;
        //    else
        //        edge = null;
        //}
    }
}