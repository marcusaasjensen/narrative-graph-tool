using System;
using NarrativeGraphTool.Runtime.Data;
using Unity.GraphToolkit.Editor;

namespace NarrativeGraphTool.Editor.Model.Nodes
{
    /// <summary>
    /// A single narrative line inside a <see cref="NarrativeContextNode"/>.
    /// Edit the text; the context node's output port handles flow continuation.
    /// </summary>
    [UseWithContext(typeof(NarrativeContextNode))]
    [Serializable]
    public class LineBlock : BlockNode
    {
        public const string SpeakerName    = "Speaker";
        public const string LineText       = "Text";
        public const string OptionMetadata = "Metadata";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<string>(SpeakerName)
                .WithDisplayName("Speaker")
                .WithDefaultValue(string.Empty)
                .Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(LineText)
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