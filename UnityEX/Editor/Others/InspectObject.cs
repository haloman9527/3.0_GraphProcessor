using UnityEngine;

namespace Atom.GraphProcessor.UnityEX.Editor
{
    public class InspectObject : ScriptableObject
    {
        [SerializeReference]
        public BaseGraph graph;
    }
}