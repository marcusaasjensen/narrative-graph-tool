using System;
using NarrativeGraphTool.Data;
using Unity.GraphToolkit.Editor;

namespace NarrativeGraphTool.Editor.Model.Nodes
{
    /// <summary>
    /// Context node that contains choice blocks. Add <see cref="ChoiceBlock"/> blocks for each choice;
    /// each block has editable text and an output port you can connect. Add, remove, or reorder blocks in the graph.
    /// <para>
    /// <b>Optional prompt:</b> fill in <c>Prompt Text</c> (and optionally <c>Prompt Speaker</c> and
    /// <c>Prompt Metadata</c>) to embed a full narrative line directly on this node. It will be
    /// delivered as <c>ChoiceNodeData.prompt</c> inside the <c>OnChoice</c> event so the UI can show
    /// it above the options without a separate NarrativeLine node.
    /// Leave <c>Prompt Text</c> empty to use the wired-line pattern instead.
    /// </para>
    /// </summary>
    [Serializable]
    public class ChoiceContextNode : ContextNode
    {
        public const string PortPromptSpeaker  = "PromptSpeaker";
        public const string OptionPromptText   = "PromptText";
        public const string OptionPromptMeta   = "PromptMetadata";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(OptionPromptText)
                .WithDisplayName("Prompt")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Optional line shown above the choices. Leave empty to wire a NarrativeLine before this node instead.")
                .Build();

            context.AddOption<NarrativeLineMetadata>(OptionPromptMeta)
                .WithDisplayName("Metadata")
                .WithTooltip("Optional ScriptableObject with game-specific data (emotion, portrait, etc.) for the prompt line.")
                .Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<string>(PortPromptSpeaker)
                .WithDisplayName("Speaker")
                .Build();

            context.AddInputPort(NarrativeNodeBase.ExecutionPortName)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }
}
