using System.Runtime.CompilerServices;
using Atom.UnityEditors;

namespace Atom.GraphProcessor.Editors
{
    public class GraphProcessorEditorSettings
    {
        public static EditorPrefsVariable<bool> MiniMapActive { get; } = new EditorPrefsVariable<bool>(GetKey());

        public static string GetKey([CallerMemberName] string memberName = null)
        {
            return $"{nameof(GraphProcessorEditorSettings)}.{memberName}";
        }
    }
}