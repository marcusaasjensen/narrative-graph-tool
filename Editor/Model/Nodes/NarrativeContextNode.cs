using System;
using Unity.GraphToolkit.Editor;

namespace NarrativeGraphTool.Editor.Model.Nodes
{
    /// <summary>
    /// Context node that stacks multiple narrative lines played sequentially.
    /// Add <see cref="LineBlockNode"/> blocks for each line of narrative;
    /// flow enters through the input port and exits through the output port after all lines.
    /// </summary>
    [Serializable]
    public class NarrativeContextNode : ContextNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort(NarrativeNodeBase.ExecutionPortName)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();

            context.AddOutputPort(NarrativeNodeBase.ExecutionPortName)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }
}
