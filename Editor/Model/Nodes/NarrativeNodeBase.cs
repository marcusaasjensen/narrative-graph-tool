using System;
using Unity.GraphToolkit.Editor;

namespace NarrativeGraphTool.Editor.Model.Nodes
{
    /// <summary>
    /// Base type for all nodes in the choice-based narrative graph.
    /// </summary>
    [Serializable]
    public abstract class NarrativeNodeBase : Node
    {
        /// <summary>Name of the execution flow port used by all narrative nodes.</summary>
        public const string ExecutionPortName = "Flow";

        /// <summary>
        /// Adds the standard input and output flow ports used for narrative sequence.
        /// </summary>
        protected static void AddFlowPorts(IPortDefinitionContext context)
        {
            context.AddInputPort(ExecutionPortName)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();

            context.AddOutputPort(ExecutionPortName)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }
}
