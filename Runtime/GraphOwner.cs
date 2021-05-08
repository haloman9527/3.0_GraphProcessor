using CZToolKit.Core.Attributes;
using System;
using UnityEngine;

using Object = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    public abstract class GraphOwner : MonoBehaviour, IGraphOwner
    {
        [SerializeField, ReadOnly]
        BehaviorSource behaviorSource;

        public abstract BaseGraph Graph { get; set; }
        public abstract Type GraphType { get; }

        public GraphOwner()
        {
            behaviorSource = new BehaviorSource(this);
        }

        public BehaviorSource GetBehaviorSource()
        {
            return behaviorSource;
        }

        public Object GetObject()
        {
            return this;
        }

        public string GetOwnerName()
        {
            return name;
        }

        public SharedVariable GetVariable(string name)
        {
            return behaviorSource.GetVariable(name);
        }

        public void SetVariable(string name, SharedVariable item)
        {
            behaviorSource.SetVariable(name, item);
        }

        public void SetVariableValue(string name, object value)
        {
            behaviorSource.GetVariable(name)?.SetValue(value);
        }
    }

    public abstract class GraphOwner<T> : GraphOwner where T : BaseGraph
    {
        [SerializeField]
        T graph;

        public T TGraph { get { return graph; } set { Graph = value; } }
        public override BaseGraph Graph
        {
            get { return graph; }
            set
            {
                if (graph != value)
                {
                    graph = value as T;
                    if (graph != null)
                    {
                        foreach (var variable in graph.GetVariables())
                        {
                            if (GetBehaviorSource().GetVariable(variable.GUID) == null)
                                GetBehaviorSource().SetVariable(variable.GUID, variable.Clone() as SharedVariable);
                        }
                    }
                }
            }
        }
        public override Type GraphType { get { return typeof(T); } }
    }
}
