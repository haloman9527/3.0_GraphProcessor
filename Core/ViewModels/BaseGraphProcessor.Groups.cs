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
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.haloman.net/
 *
 */

#endregion

using System;
using System.Collections.Generic;

namespace Atom.GraphProcessor
{
    public partial class BaseGraphProcessor
    {
        #region Fields
        private Groups m_Groups;

        #endregion

        #region Properties

        public Groups Groups
        {
            get { return m_Groups; }
        }

        #endregion

        private void InitGroups()
        {
            this.m_Groups = new Groups();
            
            for (int i = 0; i < Model.groups.Count; i++)
            {
                var group = Model.groups[i];
                if (group == null)
                {
                    Model.groups.RemoveAt(i--);
                    continue;
                }

                for (int j = group.nodes.Count - 1; j >= 0; j--)
                {
                    if (!m_Nodes.ContainsKey(group.nodes[j]))
                        group.nodes.RemoveAt(j);
                }

                var groupVM = (GroupProcessor)ViewModelFactory.ProduceViewModel(group);
                groupVM.Owner = this;
                m_Groups.AddGroup(groupVM);
            }
        }

        #region API

        public void AddGroup(GroupProcessor group)
        {
            m_Groups.AddGroup(group);
            m_Model.groups.Add(group.Model);
            group.Owner = this;
            m_GraphEvents.Publish(new AddGroupEventArgs(group));
        }

        public void RemoveGroup(GroupProcessor group)
        {
            m_Groups.RemoveGroup(group);
            m_Model.groups.Remove(group.Model);
            m_GraphEvents.Publish(new RemoveGroupEventArgs(group));
        }

        public virtual GroupProcessor NewGroup(string groupName)
        {
            var group = new Group()
            {
                id = GraphProcessorUtil.GenerateId(),
                groupName = groupName
            };
            return ViewModelFactory.ProduceViewModel(group) as GroupProcessor;
        }

        #endregion
    }

    public class Groups
    {
        private Dictionary<long, GroupProcessor> m_GroupMap = new Dictionary<long, GroupProcessor>();
        private Dictionary<long, GroupProcessor> m_NodeGroupMap = new Dictionary<long, GroupProcessor>();

        public IReadOnlyDictionary<long, GroupProcessor> GroupMap
        {
            get { return m_GroupMap; }
        }

        public IReadOnlyDictionary<long, GroupProcessor> NodeGroupMap
        {
            get { return m_NodeGroupMap; }
        }

        public void AddNodeToGroup(GroupProcessor group, BaseNodeProcessor node)
        {
            if (m_NodeGroupMap.TryGetValue(node.ID, out var _group))
            {
                if (_group == group)
                {
                    return;
                }
                else
                {
                    _group.Model.nodes.Remove(node.ID);
                    _group.NotifyNodeRemoved(node);
                }
            }

            m_NodeGroupMap[node.ID] = group;
            group.Model.nodes.Add(node.ID);
            group.NotifyNodeAdded(node);
        }

        public void RemoveNodeFromGroup(BaseNodeProcessor node)
        {
            if (!m_NodeGroupMap.TryGetValue(node.ID, out var group))
                return;

            m_NodeGroupMap.Remove(node.ID);
            group.Model.nodes.Remove(node.ID);
            group.NotifyNodeRemoved(node);
        }

        public void AddGroup(GroupProcessor group)
        {
            this.m_GroupMap.Add(group.ID, group);
            // 只处理新加入的 group 中的节点映射，不重复遍历已有 group
            foreach (var nodeID in group.Nodes)
            {
                this.m_NodeGroupMap[nodeID] = group;
            }
        }

        public bool RemoveGroup(GroupProcessor group)
        {
            if (m_GroupMap.Remove(group.ID))
            {
                foreach (var nodeID in group.Nodes)
                {
                    m_NodeGroupMap.Remove(nodeID);
                }

                return true;
            }

            return false;
        }
    }
}