using System;
using System.Runtime.CompilerServices;
using UnityEditor;

namespace Moyo.GraphProcessor.Editors
{
    public class GraphProcessorEditorSettings
    {
        private static bool s_MiniMapActive = EditorPrefs.GetBool(GetKey(), false);

        public static event Action<bool> OnMiniMapActiveChanged;

        public static bool MiniMapActive
        {
            get { return s_MiniMapActive; }
            set
            {
                if (s_MiniMapActive == value)
                {
                    return;
                }

                s_MiniMapActive = value;
                EditorPrefs.SetBool(GetKey(nameof(s_MiniMapActive)), s_MiniMapActive);
                OnMiniMapActiveChanged?.Invoke(s_MiniMapActive);
            }
        }

        public static string GetKey([CallerMemberName] string memberName = null)
        {
            return $"{nameof(GraphProcessorEditorSettings)}.{memberName}";
        }
    }
}