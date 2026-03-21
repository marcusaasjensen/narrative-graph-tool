using System.Collections.Generic;
using System.IO;
using System.Text;
using NarrativeGraphTool.Data;
using UnityEditor;
using UnityEngine;

namespace NarrativeGraphTool.Editor.Export
{
    /// <summary>
    /// Converts a <see cref="NarrativeGraphData"/> asset into a human-readable plain-text
    /// script and saves it as a .txt file next to the source .narrativegraph asset.
    ///
    /// Usage: right-click any .narrativegraph file in the Project window →
    ///        <b>Export Narrative As Text</b>.
    /// </summary>
    public static class NarrativeGraphTextExporter
    {
        // ─── Menu item ────────────────────────────────────────────────────────────

        [MenuItem("Assets/Export Narrative As Text", validate = true)]
        static bool ValidateExport() => Selection.activeObject is NarrativeGraphData;

        [MenuItem("Assets/Export Narrative As Text")]
        static void ExportFromMenu()
        {
            var graphData = Selection.activeObject as NarrativeGraphData;
            if (graphData == null) return;

            var assetPath = AssetDatabase.GetAssetPath(graphData);
            var dir       = Path.GetDirectoryName(assetPath);
            var fileName  = Path.GetFileNameWithoutExtension(assetPath) + ".txt";
            var outputPath = Path.Combine(dir, fileName).Replace('\\', '/');

            File.WriteAllText(outputPath, Convert(graphData), Encoding.UTF8);
            AssetDatabase.Refresh();

            Debug.Log($"[NarrativeGraphTool] Exported narrative to: {outputPath}");
            EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(outputPath));
        }

        // ─── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Converts <paramref name="data"/> into a plain-text narrative script and
        /// returns it as a string. Call this from code if you need the text without
        /// writing a file.
        /// </summary>
        public static string Convert(NarrativeGraphData data)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== {data.name} ===");
            sb.AppendLine();

            var start = data.GetNode(data.startNodeId) as StartNodeData;
            if (start == null)
            {
                sb.AppendLine("(no start node)");
                return sb.ToString();
            }

            // Seed visited with the start node so it is never printed twice.
            WriteNode(data.GetNode(start.nextId), data, sb, "", new HashSet<string> { start.id });
            return sb.ToString();
        }

        // ─── Graph traversal ──────────────────────────────────────────────────────

        static void WriteNode(
            NarrativeNodeData   node,
            NarrativeGraphData  data,
            StringBuilder       sb,
            string              indent,
            HashSet<string>     visited)
        {
            if (node == null)
            {
                sb.AppendLine(indent + "(disconnected)");
                return;
            }

            // Cycle guard — show which anchor we'd loop back to.
            if (visited.Contains(node.id))
            {
                var label = node is TargetNodeData t ? $"[{t.labelName}]" : node.GetType().Name;
                sb.AppendLine(indent + $"↑ (loops back to {label})");
                return;
            }

            visited.Add(node.id);

            switch (node)
            {
                // ── Flow ──────────────────────────────────────────────────────────

                case EndNodeData:
                    sb.AppendLine(indent + "=== END ===");
                    break;

                // ── Lines ─────────────────────────────────────────────────────────

                case NarrativeLineData line:
                    sb.AppendLine(indent + FormatLine(line.speaker, line.text));
                    WriteNode(data.GetNode(line.nextId), data, sb, indent, visited);
                    break;

                case NarrativeBlockData block:
                    foreach (var l in block.lines)
                        sb.AppendLine(indent + FormatLine(l.speaker, l.text));
                    WriteNode(data.GetNode(block.nextId), data, sb, indent, visited);
                    break;

                case RevisitableLineData rev:
                    sb.AppendLine(indent + FormatLine(rev.speaker, rev.firstVisitText) + "  [first visit]");
                    sb.AppendLine(indent + FormatLine(rev.speaker, rev.revisitText)    + "  [revisit]");
                    WriteNode(data.GetNode(rev.nextId), data, sb, indent, visited);
                    break;

                // ── Choices ───────────────────────────────────────────────────────

                case ChoiceNodeData choice:
                    if (choice.prompt != null)
                        sb.AppendLine(indent + FormatLine(choice.prompt.speaker, choice.prompt.text));
                    sb.AppendLine(indent + "> CHOICE");
                    for (var i = 0; i < choice.options.Count; i++)
                    {
                        var opt     = choice.options[i];
                        var condStr = BuildChoiceCondition(opt);
                        sb.AppendLine(indent + $"  [{i + 1}] {opt.text}{condStr}");
                        WriteNode(data.GetNode(opt.nextId), data, sb, indent + "        ", new HashSet<string>(visited));
                    }
                    break;

                // ── Conditionals ──────────────────────────────────────────────────

                case ConditionalBooleanNodeData cb:
                    sb.AppendLine(indent + $"IF {cb.variableId} {OpSymbol(cb.op)} {cb.compareValue.ToString().ToLower()}:");
                    WriteNode(data.GetNode(cb.trueId),  data, sb, indent + "  ", new HashSet<string>(visited));
                    sb.AppendLine(indent + "ELSE:");
                    WriteNode(data.GetNode(cb.falseId), data, sb, indent + "  ", new HashSet<string>(visited));
                    break;

                case ConditionalIntegerNodeData ci:
                    sb.AppendLine(indent + $"IF {ci.variableId} {OpSymbol(ci.op)} {ci.compareValue}:");
                    WriteNode(data.GetNode(ci.trueId),  data, sb, indent + "  ", new HashSet<string>(visited));
                    sb.AppendLine(indent + "ELSE:");
                    WriteNode(data.GetNode(ci.falseId), data, sb, indent + "  ", new HashSet<string>(visited));
                    break;

                case ConditionalFloatNodeData cf:
                    sb.AppendLine(indent + $"IF {cf.variableId} {OpSymbol(cf.op)} {cf.compareValue}:");
                    WriteNode(data.GetNode(cf.trueId),  data, sb, indent + "  ", new HashSet<string>(visited));
                    sb.AppendLine(indent + "ELSE:");
                    WriteNode(data.GetNode(cf.falseId), data, sb, indent + "  ", new HashSet<string>(visited));
                    break;

                case ConditionalStringNodeData cs:
                    sb.AppendLine(indent + $"IF {cs.variableId} {OpSymbol(cs.op)} \"{cs.compareValue}\":");
                    WriteNode(data.GetNode(cs.trueId),  data, sb, indent + "  ", new HashSet<string>(visited));
                    sb.AppendLine(indent + "ELSE:");
                    WriteNode(data.GetNode(cs.falseId), data, sb, indent + "  ", new HashSet<string>(visited));
                    break;

                // ── Random branch ─────────────────────────────────────────────────

                case RandomBranchNodeData rand:
                    sb.AppendLine(indent + "RANDOM BRANCH:");
                    for (var i = 0; i < rand.branchIds.Count; i++)
                    {
                        sb.AppendLine(indent + $"  [Branch {i + 1}]");
                        WriteNode(data.GetNode(rand.branchIds[i]), data, sb, indent + "    ", new HashSet<string>(visited));
                    }
                    break;

                // ── Set variable ──────────────────────────────────────────────────

                case SetVariableBoolNodeData svb:
                    var boolLine = svb.op == BoolSetOperator.Toggle
                        ? $"TOGGLE {svb.variableId}"
                        : $"SET {svb.variableId} = {svb.value.ToString().ToLower()}";
                    sb.AppendLine(indent + boolLine);
                    WriteNode(data.GetNode(svb.nextId), data, sb, indent, visited);
                    break;

                case SetVariableIntNodeData svi:
                    sb.AppendLine(indent + $"SET {svi.variableId} {NumericOp(svi.op)} {svi.value}");
                    WriteNode(data.GetNode(svi.nextId), data, sb, indent, visited);
                    break;

                case SetVariableFloatNodeData svf:
                    sb.AppendLine(indent + $"SET {svf.variableId} {NumericOp(svf.op)} {svf.value}");
                    WriteNode(data.GetNode(svf.nextId), data, sb, indent, visited);
                    break;

                case SetVariableStringNodeData svs:
                    var strLine = svs.op == StringSetOperator.Append
                        ? $"APPEND \"{svs.value}\" TO {svs.variableId}"
                        : $"SET {svs.variableId} = \"{svs.value}\"";
                    sb.AppendLine(indent + strLine);
                    WriteNode(data.GetNode(svs.nextId), data, sb, indent, visited);
                    break;

                // ── Event ─────────────────────────────────────────────────────────

                case EventNodeData ev:
                    var payloadStr = string.IsNullOrEmpty(ev.payload) ? "" : $" \"{ev.payload}\"";
                    sb.AppendLine(indent + $"EVENT: {ev.eventName}{payloadStr}");
                    WriteNode(data.GetNode(ev.nextId), data, sb, indent, visited);
                    break;

                // ── Jump / Target ─────────────────────────────────────────────────

                case JumpNodeData jump:
                    sb.AppendLine(indent + $"JUMP → {jump.targetLabel}");
                    WriteNode(data.GetTargetByLabel(jump.targetLabel), data, sb, indent, visited);
                    break;

                case TargetNodeData tgt:
                    sb.AppendLine();
                    sb.AppendLine(indent + $"--- [{tgt.labelName}] ---");
                    WriteNode(data.GetNode(tgt.nextId), data, sb, indent, visited);
                    break;
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        static string FormatLine(string speaker, string text) =>
            string.IsNullOrEmpty(speaker) ? text : $"[{speaker}]: {text}";

        static string BuildChoiceCondition(ChoiceOption opt)
        {
            if (string.IsNullOrEmpty(opt.conditionVariableId)) return "";

            var value = opt.conditionType switch
            {
                NarrativeConditionType.Bool   => opt.conditionBoolValue.ToString().ToLower(),
                NarrativeConditionType.Int    => opt.conditionIntValue.ToString(),
                NarrativeConditionType.Float  => opt.conditionFloatValue.ToString(),
                NarrativeConditionType.String => $"\"{opt.conditionStringValue}\"",
                _                             => "?"
            };
            return $"  [if {opt.conditionVariableId} {OpSymbol(opt.conditionOp)} {value}]";
        }

        static string OpSymbol(NarrativeConditionOperator op) => op switch
        {
            NarrativeConditionOperator.EqualTo          => "==",
            NarrativeConditionOperator.NotEqualTo       => "!=",
            NarrativeConditionOperator.LessThan         => "<",
            NarrativeConditionOperator.GreaterThan      => ">",
            NarrativeConditionOperator.LessOrEqualTo    => "<=",
            NarrativeConditionOperator.GreaterOrEqualTo => ">=",
            NarrativeConditionOperator.Contains         => "contains",
            _                                           => "?"
        };

        static string NumericOp(NumericSetOperator op) => op switch
        {
            NumericSetOperator.Set      => "=",
            NumericSetOperator.Add      => "+=",
            NumericSetOperator.Subtract => "-=",
            NumericSetOperator.Multiply => "*=",
            _                           => "="
        };
    }
}
