using System;
using NarrativeGraphTool.Runtime.Data;
using Unity.GraphToolkit.Editor;

namespace NarrativeGraphTool.Editor.Model.Nodes
{
    /// <summary>
    /// A narrative line with two text variants: one shown on the player's first visit,
    /// another shown on every subsequent visit to this node.
    /// <para>
    /// Visited state is tracked automatically by <see cref="NarrativeGraphTool.Runtime.NarrativeRunner"/>
    /// — no variables or conditionals needed.
    /// </para>
    /// </summary>
    [Serializable]
    public class RevisitableLineNode : NarrativeNodeBase
    {
        public const string PortSpeaker       = "Speaker";
        public const string OptionFirstText   = "FirstVisitText";
        public const string OptionRevisitText = "RevisitText";
        public const string OptionMetadata    = "Metadata";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddFlowPorts(context);

            context.AddInputPort<string>(PortSpeaker)
                .WithDisplayName("Speaker")
                .WithDefaultValue(string.Empty)
                .Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(OptionFirstText)
                .WithDisplayName("First Visit")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Text shown the first time this node is reached.")
                .Delayed();

            context.AddOption<string>(OptionRevisitText)
                .WithDisplayName("Revisit")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Text shown on every subsequent visit.")
                .Delayed();

            context.AddOption<NarrativeLineMetadata>(OptionMetadata)
                .WithDisplayName("Metadata")
                .WithTooltip("Optional ScriptableObject with game-specific data (emotion, portrait, etc.). Assign any NarrativeLineMetadata subclass.")
                .Build();
        }
    }
}
