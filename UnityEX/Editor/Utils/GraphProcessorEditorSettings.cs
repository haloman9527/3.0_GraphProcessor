#if UNITY_EDITOR
using System.Runtime.CompilerServices;
using Atom.UnityEditors;

namespace Atom.GraphProcessor.Editors
{
    public class GraphProcessorEditorSettings
    {
        public static EditorPrefsVariable<bool> MiniMapActive { get; } = new EditorPrefsVariable<bool>(GetKey());
        public static EditorPrefsVariable<bool> GridSnapActive { get; } = new EditorPrefsVariable<bool>(GetKey(), true);
        public static EditorPrefsVariable<int> GridSnapSize { get; } = new EditorPrefsVariable<int>(GetKey(), 16);

        public static string GetKey([CallerMemberName] string memberName = null)
        {
            return $"{nameof(GraphProcessorEditorSettings)}.{memberName}";
        }
    }
}
#endif
