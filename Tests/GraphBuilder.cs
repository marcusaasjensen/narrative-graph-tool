using NarrativeGraphTool.Runtime.Data;
using UnityEngine;

namespace NarrativeGraphTool.Tests
{
    /// <summary>
    /// Fluent factory for building NarrativeGraphData objects in tests.
    /// Every graph starts with a StartNode already wired in.
    /// </summary>
    internal static class GraphBuilder
    {
        // ─── Graph root ───────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a graph whose StartNode points to <paramref name="startNextId"/>.
        /// Additional nodes are appended after the implicit StartNode.
        /// </summary>
        internal static NarrativeGraphData Build(string startNextId, params NarrativeNodeData[] nodes)
        {
            var data = ScriptableObject.CreateInstance<NarrativeGraphData>();
            data.startNodeId = "start";
            data.nodes.Add(new StartNodeData { id = "start", nextId = startNextId });
            foreach (var node in nodes)
                data.nodes.Add(node);
            return data;
        }

        // ─── Node factories ───────────────────────────────────────────────────────

        internal static EndNodeData End(string id)
            => new() { id = id };

        internal static NarrativeLineData Line(string id, string text, string nextId = null, string speaker = "")
            => new() { id = id, text = text, nextId = nextId, speaker = speaker };

        internal static NarrativeBlockData Block(string id, string nextId, params (string speaker, string text)[] lines)
        {
            var block = new NarrativeBlockData { id = id, nextId = nextId };
            foreach (var (speaker, text) in lines)
                block.lines.Add(new NarrativeLine { speaker = speaker, text = text });
            return block;
        }

        internal static RevisitableLineData Revisitable(
            string id, string firstText, string revisitText,
            string nextId = null, string speaker = "")
            => new()
            {
                id            = id,
                firstVisitText = firstText,
                revisitText   = revisitText,
                nextId        = nextId,
                speaker       = speaker,
            };

        internal static EventNodeData Event(string id, string eventName, string payload, string nextId)
            => new() { id = id, eventName = eventName, payload = payload, nextId = nextId };

        // ─── Choice factories ─────────────────────────────────────────────────────

        internal static ChoiceNodeData Choice(string id, params ChoiceOption[] options)
        {
            var node = new ChoiceNodeData { id = id };
            node.options.AddRange(options);
            return node;
        }

        internal static ChoiceOption Option(string text, string nextId)
            => new() { text = text, nextId = nextId };

        internal static ChoiceOption ConditionalOption(
            string text, string nextId,
            string variableId,
            NarrativeConditionType type,
            NarrativeConditionOperator op,
            bool   boolValue   = false,
            int    intValue    = 0,
            float  floatValue  = 0f,
            string stringValue = "")
            => new()
            {
                text                 = text,
                nextId               = nextId,
                conditionVariableId  = variableId,
                conditionType        = type,
                conditionOp          = op,
                conditionBoolValue   = boolValue,
                conditionIntValue    = intValue,
                conditionFloatValue  = floatValue,
                conditionStringValue = stringValue,
            };

        // ─── Random branch factory ────────────────────────────────────────────────

        internal static RandomBranchNodeData RandomBranch(string id, params string[] branchIds)
        {
            var node = new RandomBranchNodeData { id = id };
            node.branchIds.AddRange(branchIds);
            return node;
        }

        // ─── Conditional node factories ───────────────────────────────────────────

        internal static ConditionalBooleanNodeData ConditionalBool(
            string id, string variableId, bool compareValue,
            NarrativeConditionOperator op, string trueId, string falseId)
            => new()
            {
                id           = id,
                variableId   = variableId,
                compareValue = compareValue,
                op           = op,
                trueId       = trueId,
                falseId      = falseId,
            };

        internal static ConditionalIntegerNodeData ConditionalInt(
            string id, string variableId, int compareValue,
            NarrativeConditionOperator op, string trueId, string falseId)
            => new()
            {
                id           = id,
                variableId   = variableId,
                compareValue = compareValue,
                op           = op,
                trueId       = trueId,
                falseId      = falseId,
            };

        internal static ConditionalFloatNodeData ConditionalFloat(
            string id, string variableId, float compareValue,
            NarrativeConditionOperator op, string trueId, string falseId)
            => new()
            {
                id           = id,
                variableId   = variableId,
                compareValue = compareValue,
                op           = op,
                trueId       = trueId,
                falseId      = falseId,
            };

        internal static ConditionalStringNodeData ConditionalString(
            string id, string variableId, string compareValue,
            NarrativeConditionOperator op, string trueId, string falseId)
            => new()
            {
                id           = id,
                variableId   = variableId,
                compareValue = compareValue,
                op           = op,
                trueId       = trueId,
                falseId      = falseId,
            };

        // ─── Set variable factories ───────────────────────────────────────────────

        internal static SetVariableBoolNodeData SetBool(
            string id, string variableId, bool value, BoolSetOperator op, string nextId)
            => new() { id = id, variableId = variableId, value = value, op = op, nextId = nextId };

        internal static SetVariableIntNodeData SetInt(
            string id, string variableId, int value, NumericSetOperator op, string nextId)
            => new() { id = id, variableId = variableId, value = value, op = op, nextId = nextId };

        internal static SetVariableFloatNodeData SetFloat(
            string id, string variableId, float value, NumericSetOperator op, string nextId)
            => new() { id = id, variableId = variableId, value = value, op = op, nextId = nextId };

        internal static SetVariableStringNodeData SetString(
            string id, string variableId, string value, StringSetOperator op, string nextId)
            => new() { id = id, variableId = variableId, value = value, op = op, nextId = nextId };
    }
}
