using System;
using Unity.GraphToolkit.Editor;

namespace NarrativeGraphTool.Editor.Model.Nodes
{
    /// <summary>
    /// End of a narrative branch. Has a single input flow; no outputs.
    /// </summary>
    [Serializable]
    public class EndNode : NarrativeNodeBase
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort(ExecutionPortName)
                .WithDisplayName("In")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }
}
