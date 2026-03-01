using System;
using Unity.GraphToolkit.Editor;

namespace NarrativeGraphTool.Editor.Model.Nodes
{
    /// <summary>
    /// Context node that contains choice blocks. Add <see cref="ChoiceBlockNode"/> blocks for each choice;
    /// each block has editable text and an output port you can connect. Add, remove, or reorder blocks in the graph.
    /// </summary>
    [Serializable]
    public class ChoiceContextNode : ContextNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort(NarrativeNodeBase.ExecutionPortName)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }
}
