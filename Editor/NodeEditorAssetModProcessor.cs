﻿using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEditor.Callbacks;

namespace GraphProcessor.Editors
{
    public class NodeEditorAssetModProcessor : UnityEditor.AssetModificationProcessor
    {
        /// <summary> 删除节点脚本之前自动删除节点 </summary> 
        private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
        {
            // 即将被删除的资源路径
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

            if (obj is MonoScript)
            {
                // 检查脚本类型，如果不是节点类则返回
                MonoScript script = obj as MonoScript;
                System.Type scriptType = script.GetClass();
                if (scriptType != null && (scriptType == typeof(BaseNode) || scriptType.IsSubclassOf(typeof(BaseNode))))
                {

                    string[] graphGUIDs = AssetDatabase.FindAssets("t:" + typeof(BaseGraph));
                    foreach (string graphGUID in graphGUIDs)
                    {
                        string graphPath = AssetDatabase.GUIDToAssetPath(graphGUID);
                        BaseGraph graphData = AssetDatabase.LoadAssetAtPath<BaseGraph>(graphPath);
                        foreach (var item in graphData.Nodes.Values.ToArray())
                        {
                            if (item != null && scriptType == item.GetType())
                                graphData.RemoveNode(item);
                        }
                    }

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            else if (obj is BaseGraph)
            {
                if (obj != null)
                {
                    foreach (var graphWindow in Resources.FindObjectsOfTypeAll<BaseGraphWindow>().Where(w => w.GraphData == obj))
                        graphWindow.OnGraphDeleted();
                }
            }

            // 继续让unity删除应该删除的脚本
            return AssetDeleteResult.DidNotDelete;
        }
    }
}