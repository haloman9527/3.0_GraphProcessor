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
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text;

namespace CZToolKit.GraphProcessor.Editors
{
    public class ScriptCreatorPopupWindow : PopupWindowContent
    {
        public const string M = @"
using CZToolKit.GraphProcessor;
using UnityEngine;

public partial class #ClassName# : BaseNode
{

}
    ";

        string className;

        public ScriptCreatorPopupWindow() { }

        public override void OnGUI(Rect rect)
        {
            className = EditorGUILayout.TextField("ClassName", className);

            if (GUILayout.Button("Generate"))
            {
                string path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
                string vPath = path + $"/{className}.Model.cs";
                string vmPath = path + $"/{className}.ViewModel.cs";
                if (!File.Exists(vPath) && !File.Exists(vmPath))
                {
                    string code = M;
                    code = code.Replace("#ClassName#", className);
                    using (FileStream fs = new FileStream(vPath, FileMode.OpenOrCreate))
                    {
                        using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                        {
                            sw.Write(code);
                        }
                    }

                    code = M;
                    code = code.Replace("#ClassName#", className);
                    using (FileStream fs = new FileStream(vmPath, FileMode.OpenOrCreate))
                    {
                        using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                        {
                            sw.Write(code);
                        }
                    }
                    AssetDatabase.Refresh();
                }
            }
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(300, 100);
        }
    }

    public class GraphProcessorAssetModProcessor : UnityEditor.AssetModificationProcessor
    {
        [MenuItem("Assets/Create/Graph Processor/Generate Node Script")]
        public static void GenerateNodeScript()
        {

            PopupWindow.Show(new Rect(Screen.width / 2, Screen.width / 2, 0, 0), new ScriptCreatorPopupWindow());
        }

        //        /// <summary> 删除节点脚本之前自动删除节点 </summary> 
        //        private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
        //        {
        //            // 即将被删除的资源路径
        //            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

        //            if (obj is MonoScript)
        //            {
        //                // 检查脚本类型，如果不是节点类则返回
        //                MonoScript script = obj as MonoScript;
        //                System.Type scriptType = script.GetClass();
        //                if (scriptType != null && (scriptType == typeof(BaseNode) || scriptType.IsSubclassOf(typeof(BaseNode))))
        //                {
        //                    string[] graphGUIDs = AssetDatabase.FindAssets("t:" + typeof(BaseGraphAsset));
        //                    foreach (string graphGUID in graphGUIDs)
        //                    {
        //                        string graphPath = AssetDatabase.GUIDToAssetPath(graphGUID);
        //                        BaseGraphAsset graphAsset = AssetDatabase.LoadAssetAtPath<BaseGraphAsset>(graphPath);
        //                        foreach (var item in graphAsset.Graph.Nodes.Values.ToArray())
        //                        {
        //                            if (item != null && scriptType == item.GetType())
        //                                graphAsset.Graph.RemoveNode(item);
        //                        }
        //                    }

        //                    AssetDatabase.SaveAssets();
        //                    AssetDatabase.Refresh();
        //                }
        //            }
        //            else if (obj is BaseGraphAsset)
        //            {
        //                if (obj != null)
        //                {
        //                    foreach (var graphWindow in Resources.FindObjectsOfTypeAll<BaseGraphWindow>().Where(w => w.GraphAsset == obj))
        //                        graphWindow.Clear();
        //                }
        //            }

        //            // 继续让unity删除应该删除的脚本
        //            return AssetDeleteResult.DidNotDelete;
        //        }
    }
}