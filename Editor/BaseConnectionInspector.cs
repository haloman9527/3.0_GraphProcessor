#region ◊¢  Õ
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: ∞Î÷ª¡˙œ∫»À
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
#if UNITY_EDITOR
using CZToolKit.Core.Editors;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomObjectEditor(typeof(BaseConnectionView))]
    public class BaseConnectionInspector : ObjectEditor
    {

        static GUIHelper.ContextDataCache ContextDataCache = new GUIHelper.ContextDataCache();

        protected HashSet<string> ignoreProperties;
        protected HashSet<string> IgnoreProperties
        {
            get
            {
                if (ignoreProperties == null)
                    ignoreProperties = new HashSet<string>(BuildIgnoreProperties());
                return ignoreProperties;
            }
        }

        public override void OnInspectorGUI()
        {
            if (!ContextDataCache.TryGetContextData<GUIStyle>("BigLabel", out var bigLabel))
            {
                bigLabel.value = new GUIStyle(GUI.skin.label);
                bigLabel.value.fontSize = 18;
                bigLabel.value.fontStyle = FontStyle.Bold;
                bigLabel.value.alignment = TextAnchor.MiddleLeft;
                bigLabel.value.stretchWidth = true;
            }
            EditorGUILayoutExtension.BeginBoxGroup();
            GUILayout.Label("Connection", bigLabel.value);
            EditorGUILayoutExtension.EndBoxGroup();

            if (Target is BaseConnectionView view && view.Model != null)
            {
                EditorGUILayoutExtension.BeginBoxGroup();
                GUILayout.Label(string.Concat(view.output?.node.title, "£∫", view.Model.FromPortName, "  >>  ", view.input?.node.title, "£∫", view.Model.ToPortName), bigLabel.value);
                EditorGUILayoutExtension.EndBoxGroup();

                EditorGUI.BeginChangeCheck();
                EditorGUILayoutExtension.BeginBoxGroup();
                foreach (var property in view.Model)
                {
                    if (IgnoreProperties.Contains(property.Key)) continue;

                    object newValue = EditorGUILayoutExtension.DrawField(property.Value.ValueType, property.Value.ValueBoxed, GraphProcessorEditorUtility.GetDisplayName(property.Key));
                    if (!newValue.Equals(property.Value.ValueBoxed))
                        property.Value.ValueBoxed = newValue;

                }
                EditorGUILayoutExtension.EndBoxGroup();

                if (EditorGUI.EndChangeCheck())
                {

                }
            }
        }

        public virtual IEnumerable<string> BuildIgnoreProperties()
        {
            yield break;
        }
    }
}
#endif