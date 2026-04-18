using System;
using System.Collections.Generic;

namespace Atom.GraphProcessor
{
    public sealed class GraphValidationResult
    {
        private readonly List<string> m_Messages = new List<string>(16);

        public IReadOnlyList<string> Messages => m_Messages;
        public bool HasIssues => m_Messages.Count > 0;

        internal void Add(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                m_Messages.Add(message);
        }
    }

    public static class GraphValidationUtil
    {
        public static GraphValidationResult Validate(BaseGraph graph)
        {
            return Analyze(graph, false);
        }

        public static GraphValidationResult Repair(BaseGraph graph)
        {
            return Analyze(graph, true);
        }

        private static GraphValidationResult Analyze(BaseGraph graph, bool repair)
        {
            var result = new GraphValidationResult();
            if (graph == null)
            {
                result.Add("[InvalidGraph] Graph is null.");
                return result;
            }

            if (graph.pan == default)
            {
                result.Add("[InvalidGraph] Pan was default and normalized.");
                if (repair)
                    graph.pan = InternalVector2Int.zero;
            }

            if (graph.zoom == 0)
            {
                result.Add("[InvalidGraph] Zoom was 0 and normalized to 1.");
                if (repair)
                    graph.zoom = 1;
            }

            EnsureList(ref graph.nodes, result, repair, "nodes");
            EnsureList(ref graph.connections, result, repair, "connections");
            EnsureList(ref graph.groups, result, repair, "groups");
            EnsureList(ref graph.notes, result, repair, "notes");
            EnsureList(ref graph.placemats, result, repair, "placemats");

            var nodeIds = ValidateNodes(graph, result, repair);
            ValidateConnections(graph, nodeIds, result, repair);
            ValidateGroups(graph, nodeIds, result, repair);
            ValidateStickyNotes(graph, result, repair);
            ValidatePlacemats(graph, result, repair);

            return result;
        }

        private static void EnsureList<T>(ref List<T> list, GraphValidationResult result, bool repair, string name)
        {
            if (list != null)
                return;

            result.Add($"[InvalidGraph] {name} was null." + (repair ? " Recreated." : string.Empty));
            if (repair)
                list = new List<T>();
        }

        private static HashSet<long> ValidateNodes(BaseGraph graph, GraphValidationResult result, bool repair)
        {
            var nodeIds = new HashSet<long>();
            if (graph.nodes == null)
                return nodeIds;

            for (var i = 0; i < graph.nodes.Count; i++)
            {
                var node = graph.nodes[i];
                if (node == null)
                {
                    result.Add($"[MissingNode] Null node at index {i}." + (repair ? " Removed." : string.Empty));
                    if (repair)
                        graph.nodes.RemoveAt(i--);
                    continue;
                }

                if (!nodeIds.Add(node.id))
                {
                    result.Add($"[DuplicateNode] Node id={node.id} duplicated." + (repair ? " Later entry removed." : string.Empty));
                    if (repair)
                        graph.nodes.RemoveAt(i--);
                }
            }

            return nodeIds;
        }

        private static void ValidateConnections(BaseGraph graph, HashSet<long> nodeIds, GraphValidationResult result, bool repair)
        {
            if (graph.connections == null)
                return;

            var connectionKeys = new HashSet<string>();
            for (var i = 0; i < graph.connections.Count; i++)
            {
                var connection = graph.connections[i];
                if (connection == null)
                {
                    result.Add($"[MissingConnection] Null connection at index {i}." + (repair ? " Removed." : string.Empty));
                    if (repair)
                        graph.connections.RemoveAt(i--);
                    continue;
                }

                if (!nodeIds.Contains(connection.fromNode) || !nodeIds.Contains(connection.toNode))
                {
                    result.Add($"[MissingConnection] Missing endpoint node for {connection.fromNode}:{connection.fromPort}->{connection.toNode}:{connection.toPort}." + (repair ? " Removed." : string.Empty));
                    if (repair)
                        graph.connections.RemoveAt(i--);
                    continue;
                }

                var key = connection.fromNode + ":" + connection.fromPort + "->" + connection.toNode + ":" + connection.toPort;
                if (!connectionKeys.Add(key))
                {
                    result.Add($"[DuplicateConnection] Duplicate edge {key}." + (repair ? " Later entry removed." : string.Empty));
                    if (repair)
                        graph.connections.RemoveAt(i--);
                }
            }
        }

        private static void ValidateGroups(BaseGraph graph, HashSet<long> nodeIds, GraphValidationResult result, bool repair)
        {
            if (graph.groups == null)
                return;

            var groupIds = new HashSet<long>();
            for (var i = 0; i < graph.groups.Count; i++)
            {
                var group = graph.groups[i];
                if (group == null)
                {
                    result.Add($"[MissingGroup] Null group at index {i}." + (repair ? " Removed." : string.Empty));
                    if (repair)
                        graph.groups.RemoveAt(i--);
                    continue;
                }

                if (!groupIds.Add(group.id))
                {
                    result.Add($"[DuplicateGroup] Group id={group.id} duplicated." + (repair ? " Later entry removed." : string.Empty));
                    if (repair)
                        graph.groups.RemoveAt(i--);
                    continue;
                }

                if (group.nodes == null)
                {
                    result.Add($"[InvalidGroup] Group id={group.id} nodes list was null." + (repair ? " Recreated." : string.Empty));
                    if (repair)
                        group.nodes = new List<long>();
                    continue;
                }

                var localNodeIds = new HashSet<long>();
                for (var j = 0; j < group.nodes.Count; j++)
                {
                    var nodeId = group.nodes[j];
                    if (!nodeIds.Contains(nodeId))
                    {
                        result.Add($"[InvalidGroup] Group id={group.id} contains missing node {nodeId}." + (repair ? " Removed." : string.Empty));
                        if (repair)
                            group.nodes.RemoveAt(j--);
                        continue;
                    }

                    if (!localNodeIds.Add(nodeId))
                    {
                        result.Add($"[InvalidGroup] Group id={group.id} contains duplicated node {nodeId}." + (repair ? " Later entry removed." : string.Empty));
                        if (repair)
                            group.nodes.RemoveAt(j--);
                    }
                }
            }
        }

        private static void ValidateStickyNotes(BaseGraph graph, GraphValidationResult result, bool repair)
        {
            if (graph.notes == null)
                return;

            var ids = new HashSet<long>();
            for (var i = 0; i < graph.notes.Count; i++)
            {
                var note = graph.notes[i];
                if (note == null)
                {
                    result.Add($"[MissingNote] Null note at index {i}." + (repair ? " Removed." : string.Empty));
                    if (repair)
                        graph.notes.RemoveAt(i--);
                    continue;
                }

                if (!ids.Add(note.id))
                {
                    result.Add($"[DuplicateNote] Note id={note.id} duplicated." + (repair ? " Later entry removed." : string.Empty));
                    if (repair)
                        graph.notes.RemoveAt(i--);
                }
            }
        }

        private static void ValidatePlacemats(BaseGraph graph, GraphValidationResult result, bool repair)
        {
            if (graph.placemats == null)
                return;

            var ids = new HashSet<long>();
            for (var i = 0; i < graph.placemats.Count; i++)
            {
                var placemat = graph.placemats[i];
                if (placemat == null)
                {
                    result.Add($"[MissingPlacemat] Null placemat at index {i}." + (repair ? " Removed." : string.Empty));
                    if (repair)
                        graph.placemats.RemoveAt(i--);
                    continue;
                }

                if (!ids.Add(placemat.id))
                {
                    result.Add($"[DuplicatePlacemat] Placemat id={placemat.id} duplicated." + (repair ? " Later entry removed." : string.Empty));
                    if (repair)
                        graph.placemats.RemoveAt(i--);
                }
            }
        }
    }
}
