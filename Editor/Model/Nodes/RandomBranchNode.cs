using System;
using Unity.GraphToolkit.Editor;

namespace NarrativeGraphTool.Editor.Model.Nodes
{
    /// <summary>
    /// Randomly selects one of its branch outputs each time it is executed.
    /// Add <see cref="RandomBranchBlock"/> blocks for each possible outcome —
    /// each block has an equal probability of being chosen.
    /// </summary>
    [Serializable]
    public class RandomBranchContextNode : ContextNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort(NarrativeNodeBase.ExecutionPortName)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }

    /// <summary>
    /// One possible outcome inside a <see cref="RandomBranchContextNode"/>.
    /// Connect the output port to the node that should execute if this branch is picked.
    /// </summary>
    [UseWithContext(typeof(RandomBranchContextNode))]
    [Serializable]
    public class RandomBranchBlock : BlockNode
    {
        public const string PortOutput = "Out";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort(PortOutput)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }
}
