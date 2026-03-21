using System;
using System.Collections.Generic;

namespace NarrativeGraphTool.Data
{
    // ─────────────────────────────────────────────────────────────────────────────
    // Base
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Base class for all serializable node data produced by the NarrativeGraphParser.
    /// </summary>
    [Serializable]
    public abstract class NarrativeNodeData
    {
        /// <summary>Stable unique identifier assigned during parsing.</summary>
        public string id;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Flow nodes
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>Entry point of the narrative. Always the first node executed.</summary>
    [Serializable]
    public class StartNodeData : NarrativeNodeData
    {
        /// <summary>ID of the first node to execute after start.</summary>
        public string nextId;
    }

    /// <summary>Terminal node. Signals that a narrative branch has finished.</summary>
    [Serializable]
    public class EndNodeData : NarrativeNodeData { }

    // ─────────────────────────────────────────────────────────────────────────────
    // Narrative line nodes
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>A single line of narrative with an optional speaker name.</summary>
    [Serializable]
    public class NarrativeLineData : NarrativeNodeData
    {
        /// <summary>Speaker name. Empty string means no speaker label.</summary>
        public string speaker;

        /// <summary>The line of text to display.</summary>
        public string text;

        /// <summary>
        /// Optional game-specific ScriptableObject attached to this line.
        /// Cast to your concrete <see cref="NarrativeLineMetadata"/> subtype at runtime.
        /// </summary>
        public NarrativeLineMetadata metadata;

        /// <summary>ID of the next node once this line is acknowledged.</summary>
        public string nextId;
    }

    /// <summary>One line entry inside a NarrativeBlockData.</summary>
    [Serializable]
    public class NarrativeLine
    {
        /// <summary>Speaker name. Empty string means no speaker label.</summary>
        public string speaker;

        /// <summary>The line of text to display.</summary>
        public string text;

        /// <summary>
        /// Optional game-specific ScriptableObject attached to this line.
        /// Cast to your concrete <see cref="NarrativeLineMetadata"/> subtype at runtime.
        /// </summary>
        public NarrativeLineMetadata metadata;
    }

    /// <summary>
    /// A sequence of narrative lines played one after another.
    /// Maps to a NarrativeContextNode with LineBlock children.
    /// </summary>
    [Serializable]
    public class NarrativeBlockData : NarrativeNodeData
    {
        /// <summary>Ordered list of lines to present in sequence.</summary>
        public List<NarrativeLine> lines = new();

        /// <summary>ID of the next node once all lines are acknowledged.</summary>
        public string nextId;
    }

    /// <summary>
    /// A line with two text variants: one shown on first visit, another on all subsequent visits.
    /// The NarrativeRunner automatically selects the correct text based on its visited-node tracking.
    /// </summary>
    [Serializable]
    public class RevisitableLineData : NarrativeNodeData
    {
        /// <summary>Speaker name. Empty string means no speaker label.</summary>
        public string speaker;

        /// <summary>Text displayed the first time this node is reached.</summary>
        public string firstVisitText;

        /// <summary>Text displayed on every subsequent visit.</summary>
        public string revisitText;

        /// <summary>
        /// Optional game-specific ScriptableObject attached to this line.
        /// Cast to your concrete <see cref="NarrativeLineMetadata"/> subtype at runtime.
        /// </summary>
        public NarrativeLineMetadata metadata;

        /// <summary>ID of the next node once this line is acknowledged.</summary>
        public string nextId;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Choice nodes
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>One selectable option inside a ChoiceNodeData.</summary>
    [Serializable]
    public class ChoiceOption
    {
        /// <summary>Label shown to the player for this option.</summary>
        public string text;

        /// <summary>ID of the node to go to when this option is chosen.</summary>
        public string nextId;

        // ── Condition (empty conditionVariableId = always visible) ────────────────

        /// <summary>
        /// Key passed to VariableProvider to retrieve the runtime value.
        /// Leave empty to show this choice unconditionally.
        /// </summary>
        public string conditionVariableId;

        /// <summary>Type of the variable being tested.</summary>
        public NarrativeConditionType conditionType;

        /// <summary>Comparison operator to apply.</summary>
        public NarrativeConditionOperator conditionOp;

        /// <summary>Compare value used when conditionType is Bool.</summary>
        public bool conditionBoolValue;

        /// <summary>Compare value used when conditionType is Int.</summary>
        public int conditionIntValue;

        /// <summary>Compare value used when conditionType is Float.</summary>
        public float conditionFloatValue;

        /// <summary>Compare value used when conditionType is String.</summary>
        public string conditionStringValue;
    }

    /// <summary>
    /// A branching point that presents one or more choices to the player.
    /// Maps to a ChoiceContextNode with ChoiceBlockNode / ConditionalChoiceBlockNode children.
    /// </summary>
    [Serializable]
    public class ChoiceNodeData : NarrativeNodeData
    {
        /// <summary>
        /// Optional line shown above the choices (speaker, text, and metadata).
        /// When non-null the UI can display it alongside the options in the same <c>OnChoice</c> event,
        /// removing the need for a separate NarrativeLine node wired before this node.
        /// Null means no built-in prompt — use the wired-line pattern instead.
        /// </summary>
        public NarrativeLine prompt;

        /// <summary>All choices in the order they were defined in the graph.</summary>
        public List<ChoiceOption> options = new();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Random branch node
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Picks one of its branch IDs at random (uniform probability) and continues flow there.
    /// Maps to a RandomBranchContextNode with RandomBranchBlockNode children.
    /// </summary>
    [Serializable]
    public class RandomBranchNodeData : NarrativeNodeData
    {
        /// <summary>IDs of possible next nodes. One is chosen at random each execution.</summary>
        public List<string> branchIds = new();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Set variable nodes
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>Operator applied when setting a boolean variable.</summary>
    public enum BoolSetOperator
    {
        /// <summary>Assign the specified value directly.</summary>
        Set,
        /// <summary>Flip the current value. Requires VariableProvider to be set.</summary>
        Toggle,
    }

    /// <summary>Operator applied when setting an integer or float variable.</summary>
    public enum NumericSetOperator
    {
        /// <summary>Assign the specified value directly.</summary>
        Set,
        /// <summary>Add the specified value to the current value.</summary>
        Add,
        /// <summary>Subtract the specified value from the current value.</summary>
        Subtract,
        /// <summary>Multiply the current value by the specified value.</summary>
        Multiply,
    }

    /// <summary>Operator applied when setting a string variable.</summary>
    public enum StringSetOperator
    {
        /// <summary>Replace the current value with the specified string.</summary>
        Set,
        /// <summary>Append the specified string to the current value.</summary>
        Append,
    }

    /// <summary>
    /// Base for all set-variable nodes. Writes a value to a named variable
    /// then continues flow.
    /// </summary>
    [Serializable]
    public abstract class SetVariableNodeData : NarrativeNodeData
    {
        /// <summary>Key passed to VariableSetter to identify which variable to write.</summary>
        public string variableId;

        /// <summary>ID of the next node after the variable is written.</summary>
        public string nextId;
    }

    /// <summary>Writes to a boolean variable.</summary>
    [Serializable]
    public class SetVariableBoolNodeData : SetVariableNodeData
    {
        public bool value;
        public BoolSetOperator op;
    }

    /// <summary>Writes to an integer variable.</summary>
    [Serializable]
    public class SetVariableIntNodeData : SetVariableNodeData
    {
        public int value;
        public NumericSetOperator op;
    }

    /// <summary>Writes to a float variable.</summary>
    [Serializable]
    public class SetVariableFloatNodeData : SetVariableNodeData
    {
        public float value;
        public NumericSetOperator op;
    }

    /// <summary>Writes to a string variable.</summary>
    [Serializable]
    public class SetVariableStringNodeData : SetVariableNodeData
    {
        public string value;
        public StringSetOperator op;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Event node
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Fires a named event and automatically advances to the next node.
    /// Use your runtime dispatcher to react to events (e.g. play audio, trigger animation).
    /// </summary>
    [Serializable]
    public class EventNodeData : NarrativeNodeData
    {
        /// <summary>Key used to route the event to subscribed handlers.</summary>
        public string eventName;

        /// <summary>Optional string payload passed to the handler.</summary>
        public string payload;

        /// <summary>ID of the next node after the event fires.</summary>
        public string nextId;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Jump / Target nodes
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Teleports execution to the TargetNodeData with a matching label.
    /// Requires no wire — the runner resolves the jump by label name at runtime.
    /// </summary>
    [Serializable]
    public class JumpNodeData : NarrativeNodeData
    {
        /// <summary>Label name of the TargetNodeData to jump to.</summary>
        public string targetLabel;
    }

    /// <summary>
    /// A named anchor. JumpNodeData instances with a matching label will land here.
    /// </summary>
    [Serializable]
    public class TargetNodeData : NarrativeNodeData
    {
        /// <summary>Label used by JumpNodeData to locate this anchor.</summary>
        public string labelName;

        /// <summary>ID of the node to execute after arriving at this target.</summary>
        public string nextId;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Conditional nodes
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Comparison operators available for conditional nodes and conditional choices.
    /// Contains is only valid for string comparisons.
    /// </summary>
    public enum NarrativeConditionOperator
    {
        EqualTo,
        NotEqualTo,
        LessThan,
        GreaterThan,
        LessOrEqualTo,
        GreaterOrEqualTo,
        Contains,
    }

    /// <summary>The variable type being tested in a conditional node or conditional choice.</summary>
    public enum NarrativeConditionType
    {
        Bool,
        Int,
        Float,
        String,
    }

    /// <summary>
    /// Base for all conditional nodes. Branches to trueId or falseId
    /// depending on whether the condition evaluates to true.
    /// </summary>
    [Serializable]
    public abstract class ConditionalNodeData : NarrativeNodeData
    {
        /// <summary>
        /// Key passed to NarrativeRunner.VariableProvider to retrieve the runtime value.
        /// Set this to the name/key of the variable you want to evaluate.
        /// </summary>
        public string variableId;

        /// <summary>ID of the node to go to when the condition is true.</summary>
        public string trueId;

        /// <summary>ID of the node to go to when the condition is false.</summary>
        public string falseId;

        /// <summary>Comparison operator to use when evaluating the condition.</summary>
        public NarrativeConditionOperator op;
    }

    /// <summary>Evaluates a boolean variable. Use EqualTo/NotEqualTo operators.</summary>
    [Serializable]
    public class ConditionalBooleanNodeData : ConditionalNodeData
    {
        /// <summary>The boolean value to compare against the runtime variable.</summary>
        public bool compareValue;
    }

    /// <summary>Compares an integer variable against a fixed value.</summary>
    [Serializable]
    public class ConditionalIntegerNodeData : ConditionalNodeData
    {
        /// <summary>The integer value to compare against the runtime variable.</summary>
        public int compareValue;
    }

    /// <summary>Compares a float variable against a fixed value.</summary>
    [Serializable]
    public class ConditionalFloatNodeData : ConditionalNodeData
    {
        /// <summary>The float value to compare against the runtime variable.</summary>
        public float compareValue;
    }

    /// <summary>Compares a string variable against a fixed value.</summary>
    [Serializable]
    public class ConditionalStringNodeData : ConditionalNodeData
    {
        /// <summary>The string value to compare against the runtime variable.</summary>
        public string compareValue;
    }
}
