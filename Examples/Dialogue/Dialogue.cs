using UnityEngine;

namespace GraphProcessor.Dialogue
{
    [CreateAssetMenu(menuName = "GraphProcessor/New Dialogue", fileName = "New Dialogue")]
    public class Dialogue : BaseGraph
    {
        string startNodeGUID;
        protected override void OnEnable()
        {
            base.OnEnable();

            if (string.IsNullOrEmpty(startNodeGUID) || !Nodes.ContainsKey(startNodeGUID))
            {
                foreach (var item in Nodes)
                {
                    if (item.Value is DialogueStartNode)
                    {
                        startNodeGUID = item.Value.GUID;
                        return;
                    }
                }

                DialogueStartNode startNode = BaseNode.CreateNew<DialogueStartNode>(Vector2.one * 150);
                startNode.OnCreated();
                startNodeGUID = startNode.GUID;
                AddNode(startNode);
            }
        }

        public void Start()
        {
            if (string.IsNullOrEmpty(startNodeGUID) && Nodes.TryGetValue(startNodeGUID,out BaseNode startNode))
            {
                DialogueStartNode dialogueStartNode = startNode as DialogueStartNode;
                dialogueStartNode.Start();
            }
        }
    }
}
