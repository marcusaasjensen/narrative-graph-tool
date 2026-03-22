using System;
using Unity.GraphToolkit.Editor;

namespace NarrativeGraphTool.Editor.Model.Nodes
{
    /// <summary>
    /// Suspends the narrative without ending it. The runner stops and fires <c>OnPause</c>.
    /// Call <c>NarrativeRunner.Resume()</c> to continue from the connected output node.
    /// </summary>
    [Serializable]
    public class PauseNode : NarrativeNodeBase
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddFlowPorts(context);
        }
    }
}
