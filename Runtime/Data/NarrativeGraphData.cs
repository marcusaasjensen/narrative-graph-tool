using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NarrativeGraphTool.Runtime.Data
{
    /// <summary>
    /// Self-contained, runtime-safe snapshot of a NarrativeGraph asset.
    /// Automatically generated and kept in sync alongside its source <c>.narrativegraph</c>
    /// file by <c>NarrativeGraphPostprocessor</c> whenever the graph is saved.
    /// Assign to a <see cref="NarrativeRunner"/> to play the narrative at runtime.
    /// </summary>
    public class NarrativeGraphData : ScriptableObject
    {
        /// <summary>ID of the StartNodeData — the first node executed by NarrativeRunner.</summary>
        public string startNodeId;

        /// <summary>
        /// All nodes in this narrative graph.
        /// Uses <c>[SerializeReference]</c> to preserve polymorphic types across serialization.
        /// </summary>
        [SerializeReference]
        public List<NarrativeNodeData> nodes = new();

        // ─── Lookup ───────────────────────────────────────────────────────────────

        Dictionary<string, NarrativeNodeData> _lookup;

        /// <summary>
        /// Finds a node by its ID. Builds a lookup dictionary on first call.
        /// Returns null if no node with that ID exists.
        /// </summary>
        public NarrativeNodeData GetNode(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            BuildLookupIfNeeded();
            return _lookup.TryGetValue(id, out var node) ? node : null;
        }

        /// <summary>
        /// Finds the first <see cref="TargetNodeData"/> whose label matches <paramref name="label"/>.
        /// Returns null if no match is found.
        /// </summary>
        public TargetNodeData GetTargetByLabel(string label)
        {
            return nodes.OfType<TargetNodeData>()
                        .FirstOrDefault(t => t.labelName == label);
        }

        void BuildLookupIfNeeded()
        {
            if (_lookup != null && _lookup.Count == nodes.Count)
                return;

            _lookup = new Dictionary<string, NarrativeNodeData>(nodes.Count);
            foreach (var node in nodes)
            {
                if (node != null && !string.IsNullOrEmpty(node.id))
                    _lookup[node.id] = node;
            }
        }

        void OnEnable() => _lookup = null; // Rebuild on asset reload
    }
}
