using System.Collections.Generic;
using System.Linq;
using NarrativeGraphTool.Editor.Model;
using NarrativeGraphTool.Editor.Model.Nodes;
using NarrativeGraphTool.Data;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace NarrativeGraphTool.Editor.Parser
{
    /// <summary>
    /// Converts a <see cref="NarrativeGraph"/> asset into a runtime-safe
    /// <see cref="NarrativeGraphData"/> ScriptableObject.
    ///
    /// <para><b>Automatic:</b> Parsing runs automatically inside
    /// <c>NarrativeGraphImporter.OnImportAsset</c> whenever a <c>.narrativegraph</c>
    /// file is saved or re-imported. No manual step needed.</para>
    ///
    /// <para><b>From code:</b></para>
    /// <code>
    /// var data = NarrativeGraphParser.Parse(myNarrativeGraph);
    /// NarrativeGraphParser.SaveAsset(data, "Assets/Data/MyStory.asset");
    /// </code>
    /// </summary>
    public static class NarrativeGraphParser
    {
        // ─── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Parses a <see cref="NarrativeGraph"/> and returns a populated
        /// <see cref="NarrativeGraphData"/> instance (not saved to disk).
        /// </summary>
        public static NarrativeGraphData Parse(NarrativeGraph graph)
        {
            var data = ScriptableObject.CreateInstance<NarrativeGraphData>();
            PopulateData(graph, data);
            return data;
        }

        /// <summary>
        /// Parses a <see cref="NarrativeGraph"/>, saves the result as a
        /// <see cref="NarrativeGraphData"/> asset at <paramref name="assetPath"/>,
        /// and returns it.
        /// </summary>
        /// <param name="graph">The source narrative graph.</param>
        /// <param name="assetPath">Project-relative path, e.g. <c>"Assets/Data/MyStory.asset"</c>.</param>
        public static NarrativeGraphData ParseToAsset(NarrativeGraph graph, string assetPath)
        {
            var data = Parse(graph);
            SaveAsset(data, assetPath);
            return data;
        }

        /// <summary>Saves a <see cref="NarrativeGraphData"/> to disk at the given asset path.</summary>
        public static void SaveAsset(NarrativeGraphData data, string assetPath)
        {
            var existing = AssetDatabase.LoadAssetAtPath<NarrativeGraphData>(assetPath);
            if (existing != null)
            {
                EditorUtility.CopySerialized(data, existing);
                EditorUtility.SetDirty(existing);
            }
            else
            {
                AssetDatabase.CreateAsset(data, assetPath);
            }
            AssetDatabase.SaveAssets();
        }

        // ─── Core parsing ─────────────────────────────────────────────────────────

        static void PopulateData(NarrativeGraph graph, NarrativeGraphData data)
        {
            data.nodes.Clear();
            data.startNodeId = null;

            // Step 1: assign stable IDs to every top-level node
            var allNodes = graph.GetNodes().ToList();
            var nodeToId = new Dictionary<INode, string>();
            int counter  = 0;

            foreach (var inode in allNodes)
                nodeToId[inode] = $"node_{counter++}";

            // Step 2: convert each node
            foreach (var inode in allNodes)
            {
                if (inode is not Node node) continue;

                var nodeData = ConvertNode(node, nodeToId);
                if (nodeData == null) continue;

                data.nodes.Add(nodeData);

                if (inode is StartNode)
                    data.startNodeId = nodeData.id;
            }

            if (string.IsNullOrEmpty(data.startNodeId))
                Debug.LogWarning("[NarrativeGraphParser] No StartNode found in graph. The parsed data will have no entry point.");
        }

        // ─── Node conversion ──────────────────────────────────────────────────────

        static NarrativeNodeData ConvertNode(Node node, Dictionary<INode, string> ids)
        {
            var id = ids[node];

            switch (node)
            {
                case StartNode start:
                    return new StartNodeData
                    {
                        id     = id,
                        nextId = ResolveFlowOutput(start, NarrativeNodeBase.ExecutionPortName, ids),
                    };

                case EndNode:
                    return new EndNodeData { id = id };

                case PauseNode pause:
                    return new PauseNodeData
                    {
                        id     = id,
                        nextId = ResolveFlowOutput(pause, NarrativeNodeBase.ExecutionPortName, ids),
                    };

                case RevisitableLineNode revLine:
                    revLine.GetInputPortByName(RevisitableLineNode.PortSpeaker).TryGetValue<string>(out var revSpeaker);
                    revLine.GetNodeOptionByName(RevisitableLineNode.OptionFirstText).TryGetValue<string>(out var firstText);
                    revLine.GetNodeOptionByName(RevisitableLineNode.OptionRevisitText).TryGetValue<string>(out var revisitText);
                    revLine.GetNodeOptionByName(RevisitableLineNode.OptionMetadata).TryGetValue<NarrativeLineMetadata>(out var revMeta);
                    return new RevisitableLineData
                    {
                        id             = id,
                        speaker        = revSpeaker  ?? "",
                        firstVisitText = firstText   ?? "",
                        revisitText    = revisitText ?? "",
                        metadata       = revMeta,
                        nextId         = ResolveFlowOutput(revLine, NarrativeNodeBase.ExecutionPortName, ids),
                    };

                case LineNode line:
                    line.GetInputPortByName(LineNode.PortSpeaker).TryGetValue<string>(out var speaker);
                    line.GetNodeOptionByName(LineNode.PortText).TryGetValue<string>(out var text);
                    line.GetNodeOptionByName(LineNode.OptionMetadata).TryGetValue<NarrativeLineMetadata>(out var lineMeta);
                    return new NarrativeLineData
                    {
                        id       = id,
                        speaker  = speaker  ?? "",
                        text     = text     ?? "",
                        metadata = lineMeta,
                        nextId   = ResolveFlowOutput(line, NarrativeNodeBase.ExecutionPortName, ids),
                    };

                case NarrativeContextNode narrativeCtx:
                    return ConvertNarrativeBlock(narrativeCtx, id, ids);

                case ChoiceContextNode choiceCtx:
                    return ConvertChoiceNode(choiceCtx, id, ids);

                case RandomBranchContextNode randomCtx:
                    return ConvertRandomBranchNode(randomCtx, id, ids);

                case EventNode ev:
                    ev.GetNodeOptionByName(EventNode.OptionEventName).TryGetValue<string>(out var evName);
                    ev.GetNodeOptionByName(EventNode.OptionPayload).TryGetValue<string>(out var payload);
                    ev.GetInputPortByName(EventNode.PortWaitForResume).TryGetValue<bool>(out var waitForResume);
                    return new EventNodeData
                    {
                        id            = id,
                        eventName     = evName  ?? "",
                        payload       = payload ?? "",
                        waitForResume = waitForResume,
                        nextId        = ResolveFlowOutput(ev, NarrativeNodeBase.ExecutionPortName, ids),
                    };

                case SetVariableBoolNode setBool:
                    setBool.GetNodeOptionByName(SetVariableBoolNode.OptionVariableId).TryGetValue<string>(out var sbVarId);
                    setBool.GetNodeOptionByName(SetVariableBoolNode.OptionOperator).TryGetValue<BoolSetOperator>(out var sbOp);
                    setBool.GetInputPortByName(SetVariableBoolNode.PortValue).TryGetValue<bool>(out var sbVal);
                    return new SetVariableBoolNodeData
                    {
                        id         = id,
                        variableId = sbVarId ?? "",
                        op         = sbOp,
                        value      = sbVal,
                        nextId     = ResolveFlowOutput(setBool, NarrativeNodeBase.ExecutionPortName, ids),
                    };

                case SetVariableIntNode setInt:
                    setInt.GetNodeOptionByName(SetVariableIntNode.OptionVariableId).TryGetValue<string>(out var siVarId);
                    setInt.GetNodeOptionByName(SetVariableIntNode.OptionOperator).TryGetValue<NumericSetOperator>(out var siOp);
                    setInt.GetInputPortByName(SetVariableIntNode.PortValue).TryGetValue<int>(out var siVal);
                    return new SetVariableIntNodeData
                    {
                        id         = id,
                        variableId = siVarId ?? "",
                        op         = siOp,
                        value      = siVal,
                        nextId     = ResolveFlowOutput(setInt, NarrativeNodeBase.ExecutionPortName, ids),
                    };

                case SetVariableFloatNode setFloat:
                    setFloat.GetNodeOptionByName(SetVariableFloatNode.OptionVariableId).TryGetValue<string>(out var sfVarId);
                    setFloat.GetNodeOptionByName(SetVariableFloatNode.OptionOperator).TryGetValue<NumericSetOperator>(out var sfOp);
                    setFloat.GetInputPortByName(SetVariableFloatNode.PortValue).TryGetValue<float>(out var sfVal);
                    return new SetVariableFloatNodeData
                    {
                        id         = id,
                        variableId = sfVarId ?? "",
                        op         = sfOp,
                        value      = sfVal,
                        nextId     = ResolveFlowOutput(setFloat, NarrativeNodeBase.ExecutionPortName, ids),
                    };

                case SetVariableStringNode setString:
                    setString.GetNodeOptionByName(SetVariableStringNode.OptionVariableId).TryGetValue<string>(out var ssVarId);
                    setString.GetNodeOptionByName(SetVariableStringNode.OptionOperator).TryGetValue<StringSetOperator>(out var ssOp);
                    setString.GetNodeOptionByName(SetVariableStringNode.OptionValue).TryGetValue<string>(out var ssVal);
                    return new SetVariableStringNodeData
                    {
                        id         = id,
                        variableId = ssVarId ?? "",
                        op         = ssOp,
                        value      = ssVal ?? "",
                        nextId     = ResolveFlowOutput(setString, NarrativeNodeBase.ExecutionPortName, ids),
                    };

                case ConditionalBooleanNode condBool:
                    condBool.GetInputPortByName(ConditionalBooleanNode.PortValue).TryGetValue<bool>(out var bCompare);
                    return new ConditionalBooleanNodeData
                    {
                        id           = id,
                        variableId   = "",
                        compareValue = bCompare,
                        op           = NarrativeConditionOperator.EqualTo,
                        trueId       = ResolveOutput(condBool, ConditionalBooleanNode.PortTrue,  ids),
                        falseId      = ResolveOutput(condBool, ConditionalBooleanNode.PortFalse, ids),
                    };

                case ConditionalIntegerNode condInt:
                    condInt.GetNodeOptionByName(ConditionalIntegerNode.OptionOperator).TryGetValue<NumericConditionalOperator>(out var intOp);
                    condInt.GetInputPortByName(ConditionalIntegerNode.PortCompare).TryGetValue<int>(out var iCompare);
                    return new ConditionalIntegerNodeData
                    {
                        id           = id,
                        variableId   = "",
                        compareValue = iCompare,
                        op           = MapNumericOp(intOp),
                        trueId       = ResolveOutput(condInt, ConditionalIntegerNode.PortTrue,  ids),
                        falseId      = ResolveOutput(condInt, ConditionalIntegerNode.PortFalse, ids),
                    };

                case ConditionalFloatNode condFloat:
                    condFloat.GetNodeOptionByName(ConditionalFloatNode.OptionOperator).TryGetValue<NumericConditionalOperator>(out var floatOp);
                    condFloat.GetInputPortByName(ConditionalFloatNode.PortCompare).TryGetValue<float>(out var fCompare);
                    return new ConditionalFloatNodeData
                    {
                        id           = id,
                        variableId   = "",
                        compareValue = fCompare,
                        op           = MapNumericOp(floatOp),
                        trueId       = ResolveOutput(condFloat, ConditionalFloatNode.PortTrue,  ids),
                        falseId      = ResolveOutput(condFloat, ConditionalFloatNode.PortFalse, ids),
                    };

                case ConditionalStringNode condStr:
                    condStr.GetNodeOptionByName(ConditionalStringNode.OptionOperator).TryGetValue<StringConditionalOperator>(out var strOp);
                    condStr.GetInputPortByName(ConditionalStringNode.PortCompare).TryGetValue<string>(out var sCompare);
                    return new ConditionalStringNodeData
                    {
                        id           = id,
                        variableId   = "",
                        compareValue = sCompare ?? "",
                        op           = MapStringOp(strOp),
                        trueId       = ResolveOutput(condStr, ConditionalStringNode.PortTrue,  ids),
                        falseId      = ResolveOutput(condStr, ConditionalStringNode.PortFalse, ids),
                    };

                default:
                    Debug.LogWarning($"[NarrativeGraphParser] Unrecognised node type '{node.GetType().Name}' — skipped.");
                    return null;
            }
        }

        // ─── Context node helpers ─────────────────────────────────────────────────

        static NarrativeBlockData ConvertNarrativeBlock(
            NarrativeContextNode ctx, string id, Dictionary<INode, string> ids)
        {
            var lines = new List<NarrativeLine>();

            foreach (var block in ctx.BlockNodes.OfType<LineBlock>())
            {
                block.GetInputPortByName(LineBlock.SpeakerName).TryGetValue<string>(out var s);
                block.GetNodeOptionByName(LineBlock.LineText).TryGetValue<string>(out var t);
                block.GetNodeOptionByName(LineBlock.OptionMetadata).TryGetValue<NarrativeLineMetadata>(out var m);
                lines.Add(new NarrativeLine { speaker = s ?? "", text = t ?? "", metadata = m });
            }

            return new NarrativeBlockData
            {
                id     = id,
                lines  = lines,
                nextId = ResolveFlowOutput(ctx, NarrativeNodeBase.ExecutionPortName, ids),
            };
        }

        static ChoiceNodeData ConvertChoiceNode(
            ChoiceContextNode ctx, string id, Dictionary<INode, string> ids)
        {
            var options = new List<ChoiceOption>();

            foreach (var block in ctx.BlockNodes)
            {
                switch (block)
                {
                    case ChoiceBlock simple:
                    {
                        simple.GetNodeOptionByName(ChoiceBlock.OptionChoiceText).TryGetValue<string>(out var choiceText);
                        options.Add(new ChoiceOption
                        {
                            text   = choiceText ?? "",
                            nextId = ResolveOutput(simple, ChoiceBlock.PortOutput, ids),
                        });
                        break;
                    }

                    case ConditionalBoolChoiceBlock condBool:
                    {
                        condBool.GetNodeOptionByName(ConditionalBoolChoiceBlock.OptionChoiceText).TryGetValue<string>(out var choiceText);
                        condBool.GetNodeOptionByName(ConditionalBoolChoiceBlock.OptionVariableId).TryGetValue<string>(out var varId);
                        condBool.GetInputPortByName(ConditionalBoolChoiceBlock.PortValue).TryGetValue<bool>(out var bVal);
                        options.Add(new ChoiceOption
                        {
                            text                = choiceText ?? "",
                            nextId              = ResolveOutput(condBool, ConditionalBoolChoiceBlock.PortOutput, ids),
                            conditionVariableId = varId ?? "",
                            conditionType       = NarrativeConditionType.Bool,
                            conditionOp         = NarrativeConditionOperator.EqualTo,
                            conditionBoolValue  = bVal,
                        });
                        break;
                    }

                    case ConditionalIntChoiceBlock condInt:
                    {
                        condInt.GetNodeOptionByName(ConditionalIntChoiceBlock.OptionChoiceText).TryGetValue<string>(out var choiceText);
                        condInt.GetNodeOptionByName(ConditionalIntChoiceBlock.OptionVariableId).TryGetValue<string>(out var varId);
                        condInt.GetNodeOptionByName(ConditionalIntChoiceBlock.OptionOperator).TryGetValue<NumericConditionalOperator>(out var op);
                        condInt.GetInputPortByName(ConditionalIntChoiceBlock.PortCompare).TryGetValue<int>(out var iVal);
                        options.Add(new ChoiceOption
                        {
                            text                = choiceText ?? "",
                            nextId              = ResolveOutput(condInt, ConditionalIntChoiceBlock.PortOutput, ids),
                            conditionVariableId = varId ?? "",
                            conditionType       = NarrativeConditionType.Int,
                            conditionOp         = MapNumericOp(op),
                            conditionIntValue   = iVal,
                        });
                        break;
                    }

                    case ConditionalFloatChoiceBlock condFloat:
                    {
                        condFloat.GetNodeOptionByName(ConditionalFloatChoiceBlock.OptionChoiceText).TryGetValue<string>(out var choiceText);
                        condFloat.GetNodeOptionByName(ConditionalFloatChoiceBlock.OptionVariableId).TryGetValue<string>(out var varId);
                        condFloat.GetNodeOptionByName(ConditionalFloatChoiceBlock.OptionOperator).TryGetValue<NumericConditionalOperator>(out var op);
                        condFloat.GetInputPortByName(ConditionalFloatChoiceBlock.PortCompare).TryGetValue<float>(out var fVal);
                        options.Add(new ChoiceOption
                        {
                            text                 = choiceText ?? "",
                            nextId               = ResolveOutput(condFloat, ConditionalFloatChoiceBlock.PortOutput, ids),
                            conditionVariableId  = varId ?? "",
                            conditionType        = NarrativeConditionType.Float,
                            conditionOp          = MapNumericOp(op),
                            conditionFloatValue  = fVal,
                        });
                        break;
                    }

                    case ConditionalStringChoiceBlock condStr:
                    {
                        condStr.GetNodeOptionByName(ConditionalStringChoiceBlock.OptionChoiceText).TryGetValue<string>(out var choiceText);
                        condStr.GetNodeOptionByName(ConditionalStringChoiceBlock.OptionVariableId).TryGetValue<string>(out var varId);
                        condStr.GetNodeOptionByName(ConditionalStringChoiceBlock.OptionOperator).TryGetValue<StringConditionalOperator>(out var op);
                        condStr.GetNodeOptionByName(ConditionalStringChoiceBlock.OptionCompare).TryGetValue<string>(out var sVal);
                        options.Add(new ChoiceOption
                        {
                            text                  = choiceText ?? "",
                            nextId                = ResolveOutput(condStr, ConditionalStringChoiceBlock.PortOutput, ids),
                            conditionVariableId   = varId ?? "",
                            conditionType         = NarrativeConditionType.String,
                            conditionOp           = MapStringOp(op),
                            conditionStringValue  = sVal ?? "",
                        });
                        break;
                    }
                }
            }

            ctx.GetNodeOptionByName(ChoiceContextNode.OptionPromptText).TryGetValue<string>(out var promptText);
            ctx.GetInputPortByName(ChoiceContextNode.PortPromptSpeaker).TryGetValue<string>(out var promptSpeaker);
            ctx.GetNodeOptionByName(ChoiceContextNode.OptionPromptMeta).TryGetValue<NarrativeLineMetadata>(out var promptMeta);

            var prompt = string.IsNullOrEmpty(promptText)
                ? null
                : new NarrativeLine { speaker = promptSpeaker ?? "", text = promptText, metadata = promptMeta };

            return new ChoiceNodeData { id = id, prompt = prompt, options = options };
        }

        static RandomBranchNodeData ConvertRandomBranchNode(
            RandomBranchContextNode ctx, string id, Dictionary<INode, string> ids)
        {
            var branchIds = new List<string>();

            foreach (var block in ctx.BlockNodes.OfType<RandomBranchBlock>())
            {
                var nextId = ResolveOutput(block, RandomBranchBlock.PortOutput, ids);
                if (nextId != null)
                    branchIds.Add(nextId);
            }

            return new RandomBranchNodeData { id = id, branchIds = branchIds };
        }

        // ─── Port resolution helpers ──────────────────────────────────────────────

        /// <summary>Resolves the first connected node ID on a named output port.</summary>
        static string ResolveOutput(Node node, string portName, Dictionary<INode, string> ids)
        {
            IPort outputPort;
            try { outputPort = node.GetOutputPortByName(portName); }
            catch { return null; }

            if (outputPort == null || !outputPort.IsConnected) return null;

            var connectedINode = outputPort.FirstConnectedPort?.GetNode();
            return connectedINode != null && ids.TryGetValue(connectedINode, out var nextId) ? nextId : null;
        }

        /// <summary>Resolves the flow output port (used by most nodes).</summary>
        static string ResolveFlowOutput(Node node, string portName, Dictionary<INode, string> ids)
            => ResolveOutput(node, portName, ids);

        // ─── Operator mapping ─────────────────────────────────────────────────────

        static NarrativeConditionOperator MapNumericOp(NumericConditionalOperator op) => op switch
        {
            NumericConditionalOperator.EqualTo          => NarrativeConditionOperator.EqualTo,
            NumericConditionalOperator.NotEqualTo       => NarrativeConditionOperator.NotEqualTo,
            NumericConditionalOperator.LessThan         => NarrativeConditionOperator.LessThan,
            NumericConditionalOperator.GreaterThan      => NarrativeConditionOperator.GreaterThan,
            NumericConditionalOperator.LessOrEqualTo    => NarrativeConditionOperator.LessOrEqualTo,
            NumericConditionalOperator.GreaterOrEqualTo => NarrativeConditionOperator.GreaterOrEqualTo,
            _                                           => NarrativeConditionOperator.EqualTo,
        };

        static NarrativeConditionOperator MapStringOp(StringConditionalOperator op) => op switch
        {
            StringConditionalOperator.EqualTo    => NarrativeConditionOperator.EqualTo,
            StringConditionalOperator.NotEqualTo => NarrativeConditionOperator.NotEqualTo,
            StringConditionalOperator.Contains   => NarrativeConditionOperator.Contains,
            _                                    => NarrativeConditionOperator.EqualTo,
        };
    }
}
