using System;
using NarrativeGraphTool.Runtime.Data;
using Unity.GraphToolkit.Editor;

namespace NarrativeGraphTool.Editor.Model.Nodes
{
    /// <summary>
    /// A single line of narrative: optional speaker name and text. Flow continues to the next node.
    /// </summary>
    [Serializable]
    public class LineNode : NarrativeNodeBase
    {
        public const string PortSpeaker    = "Speaker";
        public const string PortText       = "Text";
        public const string OptionMetadata = "Metadata";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddFlowPorts(context);

            context.AddInputPort<string>(PortSpeaker)
                .WithDisplayName("Speaker")
                .Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(PortText)
                .WithDisplayName("Line")
                .WithDefaultValue(string.Empty)
                .Build();

            context.AddOption<NarrativeLineMetadata>(OptionMetadata)
                .WithDisplayName("Metadata")
                .WithTooltip("Optional ScriptableObject with game-specific data (emotion, portrait, etc.). Assign any NarrativeLineMetadata subclass.")
                .Build();
        }
    }
}
