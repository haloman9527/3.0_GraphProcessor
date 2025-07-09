using System;

namespace Atom.GraphProcessor
{
    public sealed class GraphEvents
    {
        private EventStation<Type> m_EventStation = new EventStation<Type>();

        public bool HasEvent<TArg>() where TArg : struct, IGraphEvent
        {
            return m_EventStation.HasEvent(typeof(TArg));
        }

        public object GetEvent<TArg>() where TArg : struct, IGraphEvent
        {
            return m_EventStation.GetEvent(typeof(TArg));
        }

        public void UnRegisterEvent<TArg>() where TArg : struct, IGraphEvent
        {
            m_EventStation.UnRegisterEvent(typeof(TArg));
        }

        public void UnRegisterAllEvents()
        {
            m_EventStation.UnRegisterAllEvents();
        }

        public void Subscribe<TArg>(Action<TArg> handler) where TArg : struct, IGraphEvent
        {
            m_EventStation.Subscribe(typeof(TArg), handler);
        }

        public void Unsubscribe<TArg>(Action<TArg> handler) where TArg : struct, IGraphEvent
        {
            m_EventStation.Unsubscribe(typeof(TArg), handler);
        }

        public void Publish<TArg>(TArg arg) where TArg : struct, IGraphEvent
        {
            m_EventStation.Publish(typeof(TArg), arg);
        }
    }

    public interface IGraphEvent
    {
    }

    public struct AddNodeEventArgs : IGraphEvent
    {
        public BaseNodeProcessor Node;

        public AddNodeEventArgs(BaseNodeProcessor addedNode)
        {
            Node = addedNode;
        }
    }

    public struct RemoveNodeEventArgs : IGraphEvent
    {
        public BaseNodeProcessor Node;

        public RemoveNodeEventArgs(BaseNodeProcessor removedNode)
        {
            Node = removedNode;
        }
    }

    public struct AddConnectionEventArgs : IGraphEvent
    {
        public BaseConnectionProcessor Connection;

        public AddConnectionEventArgs(BaseConnectionProcessor addedConnection)
        {
            Connection = addedConnection;
        }
    }

    public struct RemoveConnectionEventArgs : IGraphEvent
    {
        public BaseConnectionProcessor Connection;

        public RemoveConnectionEventArgs(BaseConnectionProcessor removedConnection)
        {
            Connection = removedConnection;
        }
    }

    public struct AddGroupEventArgs : IGraphEvent
    {
        public GroupProcessor Group;

        public AddGroupEventArgs(GroupProcessor addedGroup)
        {
            Group = addedGroup;
        }
    }

    public struct RemoveGroupEventArgs : IGraphEvent
    {
        public GroupProcessor Group;

        public RemoveGroupEventArgs(GroupProcessor removedGroup)
        {
            Group = removedGroup;
        }
    }

    public struct AddNodesToGroupEventArgs : IGraphEvent
    {
        public GroupProcessor Group;
        public BaseNodeProcessor[] Nodes;

        public AddNodesToGroupEventArgs(GroupProcessor group, BaseNodeProcessor[] addedNodes)
        {
            Group = group;
            Nodes = addedNodes;
        }
    }

    public struct RemoveNodesFromGroupEventArgs : IGraphEvent
    {
        public GroupProcessor Group;
        public BaseNodeProcessor[] Nodes;

        public RemoveNodesFromGroupEventArgs(GroupProcessor group, BaseNodeProcessor[] removedNodes)
        {
            Group = group;
            Nodes = removedNodes;
        }
    }

    public struct AddNoteEventArgs : IGraphEvent
    {
        public StickyNoteProcessor Note;

        public AddNoteEventArgs(StickyNoteProcessor addedNote)
        {
            Note = addedNote;
        }
    }

    public struct RemoveNoteEventArgs : IGraphEvent
    {
        public StickyNoteProcessor Note;

        public RemoveNoteEventArgs(StickyNoteProcessor removedNote)
        {
            Note = removedNote;
        }
    }
}