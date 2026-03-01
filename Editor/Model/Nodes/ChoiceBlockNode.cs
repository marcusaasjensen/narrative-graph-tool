using System;
using Unity.GraphToolkit.Editor;

namespace NarrativeGraphTool.Editor.Model.Nodes
{
    /// <summary>
    /// One choice inside a <see cref="ChoicesContextNode"/>. Edit the text and connect the output port to the next node for this branch.
    /// </summary>
    [UseWithContext(typeof(ChoiceContextNode))]
    [Serializable]
    public class ChoiceBlock : BlockNode
    {
        public const string OptionChoiceText = "Choice";
        public const string PortOutput = "Out";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(OptionChoiceText)
                .WithDisplayName("Choice")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Label shown for this choice.")
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort(PortOutput)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }
}
