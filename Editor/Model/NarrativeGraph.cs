using System;
using System.Collections.Generic;
using System.Linq;
using NarrativeGraphTool.Editor.Model.Nodes;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace NarrativeGraphTool.Editor.Model
{
    /// <summary>
    /// Choice-based narrative graph for the Narrative Graph Tool.
    /// </summary>
    /// <remarks>
    /// Extends Graph Toolkit <see cref="Graph"/> to define a narrative flow with lines and choices.
    /// Use <see cref="StartNode"/> as entry, <see cref="NarrativeLineNode"/> for a single speaker/text line,
    /// <see cref="NarrativeContextNode"/> with <see cref="LineBlockNode"/> blocks to stack multiple lines,
    /// <see cref="ChoicesContextNode"/> with <see cref="ChoiceBlockNode"/> blocks for branches, and <see cref="EndNode"/> to end a branch.
    /// </remarks>
    [Serializable]
    [Graph(AssetExtension)]
    public class NarrativeGraph : Graph
    {
        const string k_GraphDisplayName = "Narrative Graph";

        /// <summary>File extension for narrative graph assets.</summary>
        public const string AssetExtension = "narrativegraph";

        [MenuItem("Assets/Create/Narrative Graph Tool/Narrative Graph")]
        static void CreateAssetFile()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<NarrativeGraph>(k_GraphDisplayName);
        }

        /// <inheritdoc />
        public override void OnGraphChanged(GraphLogger infos)
        {
            base.OnGraphChanged(infos);
            ValidateNarrativeGraph(infos);
        }

        void ValidateNarrativeGraph(GraphLogger infos)
        {
            var allNodes = GetNodes().ToList();
            if (allNodes.Count == 0)
                return;

            var startNodes = allNodes.OfType<StartNode>().ToList();
            switch (startNodes.Count)
            {
                case 0:
                    infos.LogError("Add a Start node as the entry point of the narrative.", this);
                    break;
                case 1:
                    break;
                default:
                    infos.LogWarning("Only one Start node is used as the entry point. Consider removing extras.", startNodes[1]);
                    break;
            }
        }
    }
}
