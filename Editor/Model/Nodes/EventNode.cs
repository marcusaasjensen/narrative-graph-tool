using System;
using Unity.GraphToolkit.Editor;

namespace NarrativeGraphTool.Editor.Model.Nodes
{
    /// <summary>
    /// Fires a named event during narrative execution, then continues flow.
    /// </summary>
    /// <remarks>
    /// The node stores a plain string name and an optional string payload.
    /// Your runtime narrative runner is responsible for dispatching the event
    /// to whatever systems care about it (quest logic, animations, audio, etc.).
    ///
    /// Why not UnityEvent? The graph is a project asset and cannot hold serialized
    /// references to scene GameObjects or MonoBehaviour callbacks. Use the event
    /// name as a key in a central dispatcher instead.
    ///
    /// Example runtime pattern:
    /// <code>
    /// // Registration (anywhere in your game):
    /// DialogueEvents.Register("PlayCutscene", payload => CutsceneManager.Play(payload));
    ///
    /// // The runner calls this when it processes an EventNode:
    /// DialogueEvents.Fire(node.EventName, node.Payload);
    /// </code>
    /// </remarks>
    [Serializable]
    public class EventNode : NarrativeNodeBase
    {
        public const string OptionEventName    = "EventName";
        public const string OptionPayload      = "Payload";
        public const string PortWaitForResume  = "WaitForResume";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(OptionEventName)
                .WithDisplayName("Event")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Name of the event to fire. Subscribe to this name in your runtime dispatcher.")
                .Delayed();

            context.AddOption<string>(OptionPayload)
                .WithDisplayName("Payload")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Optional data string passed along with the event (e.g. an item ID, clip name, or flag).")
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddFlowPorts(context);

            context.AddInputPort<bool>(PortWaitForResume)
                .WithDisplayName("Wait For Resume")
                .WithDefaultValue(false)
                .WithTooltip("When enabled, the runner stops after firing the event and waits for Resume() to be called. Use for animations or cutscenes that must finish before the narrative continues.")
                .Build();
        }
    }
}
