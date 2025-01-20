using Moyo.GraphProcessor;
using UnityEngine;

namespace ThirdParty.Moyo.Moyo.GraphProcessor.UnityEX.Editor
{
    public class InspectObject : ScriptableObject
    {
        [SerializeReference]
        public BaseGraph graph;
    }
}