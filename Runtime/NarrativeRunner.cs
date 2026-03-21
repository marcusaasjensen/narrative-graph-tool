using System;
using System.Collections.Generic;
using System.Linq;
using NarrativeGraphTool.Data;
using UnityEngine;
using UnityEngine.Events;

namespace NarrativeGraphTool
{
    /// <summary>
    /// Runtime executor for a <see cref="NarrativeGraphData"/> asset.
    /// Drives a choice-based narrative by stepping through nodes and raising
    /// events that your UI layer subscribes to.
    ///
    /// <para><b>Basic usage:</b></para>
    /// <code>
    /// runner.OnLine   += line  => uiLabel.text = $"{line.speaker}: {line.text}";
    /// runner.OnChoice += data  => ShowChoiceMenu(data.options);
    /// runner.OnEnd    += ()    => HideDialogueUI();
    /// runner.StartNarrative();
    ///
    /// // When the player clicks "Next":
    /// runner.Continue();
    ///
    /// // When the player picks a choice:
    /// runner.SelectChoice(index);
    /// </code>
    ///
    /// <para><b>Conditionals and variable reads:</b> Assign <see cref="VariableProvider"/> before starting.</para>
    /// <code>
    /// runner.VariableProvider = key => myGameState.Get(key);
    /// </code>
    ///
    /// <para><b>SetVariable nodes:</b> Also assign <see cref="VariableSetter"/>.</para>
    /// <code>
    /// runner.VariableSetter = (key, value) => myGameState.Set(key, value);
    /// </code>
    /// </summary>
    public class NarrativeRunner : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────────────

        [Tooltip("The parsed narrative graph data asset to execute.")]
        [SerializeField] NarrativeGraphData _graphData;

        [Header("Unity Events")]
        [Tooltip("Raised when a single narrative line is reached. Inspector-wirable equivalent of OnLine.")]
        [SerializeField] UnityEvent<NarrativeLineData> _onLine;

        [Tooltip("Raised when a block of sequential lines is reached. Inspector-wirable equivalent of OnBlock.")]
        [SerializeField] UnityEvent<NarrativeBlockData> _onBlock;

        [Tooltip("Raised when a choice menu should be shown. Inspector-wirable equivalent of OnChoice.")]
        [SerializeField] UnityEvent<ChoiceNodeData> _onChoice;

        [Tooltip("Raised when an EventNode is processed. Inspector-wirable equivalent of OnEvent.")]
        [SerializeField] UnityEvent<EventNodeData> _onEvent;

        [Tooltip("Raised when the narrative ends. Inspector-wirable equivalent of OnEnd.")]
        [SerializeField] UnityEvent _onNarrativeEnd;

        [Tooltip("Raised when execution enters any node. Supports dynamic NarrativeNodeData parameter or no parameter.")]
        [SerializeField] UnityEvent<NarrativeNodeData> _onNodeEnter;

        [Tooltip("Raised when execution exits any node. Supports dynamic NarrativeNodeData parameter or no parameter.")]
        [SerializeField] UnityEvent<NarrativeNodeData> _onNodeExit;

        // ─── State ────────────────────────────────────────────────────────────────

        NarrativeNodeData _current;
        bool _awaitingChoice;

        /// <summary>
        /// Filtered list of visible choices for the current choice node.
        /// Used by SelectChoice to map the UI index back to the right option.
        /// </summary>
        List<ChoiceOption> _visibleOptions;

        /// <summary>
        /// Set of node IDs reached at least once. Intentionally persists across restarts
        /// so revisitable lines remain correct when re-entering a conversation.
        /// Call <see cref="ResetVisitedNodes"/> to clear (e.g. when loading a fresh save).
        /// </summary>
        readonly HashSet<string> _visitedNodes = new();

        /// <summary>True while a narrative is actively running.</summary>
        public bool IsRunning { get; private set; }

        /// <summary>The node currently being processed, or null if not running.</summary>
        public NarrativeNodeData CurrentNode => _current;

        /// <summary>Read-only view of every node ID that has been reached at least once.</summary>
        public IReadOnlyCollection<string> VisitedNodes => _visitedNodes;

        /// <summary>Returns true if the node with the given ID has been reached at least once.</summary>
        public bool IsVisited(string nodeId) => _visitedNodes.Contains(nodeId);

        /// <summary>
        /// Clears all visited-node records.
        /// Call this when loading a save file that tracks its own visited-state data.
        /// </summary>
        public void ResetVisitedNodes() => _visitedNodes.Clear();

        // ─── Variable callbacks ───────────────────────────────────────────────────

        /// <summary>
        /// Callback used to resolve runtime variable values for conditional nodes
        /// and conditional choices.
        /// Receives the variable ID and must return the current value as an <c>object</c>.
        /// <para>Example: <c>runner.VariableProvider = key => myState.GetVariable(key);</c></para>
        /// </summary>
        public Func<string, object> VariableProvider { get; set; }

        /// <summary>
        /// Callback invoked when a SetVariable node is processed.
        /// Receives the variable ID and the new value to write.
        /// <para>Example: <c>runner.VariableSetter = (key, val) => myState.SetVariable(key, val);</c></para>
        /// </summary>
        public Action<string, object> VariableSetter { get; set; }

        /// <summary>
        /// Optional gate that blocks <see cref="Continue"/> from advancing the narrative.
        /// Set this to a function that returns <c>false</c> while a typewriter or line
        /// animation is still playing, and <c>true</c> once the player is allowed to advance.
        /// When null, <see cref="Continue"/> always advances immediately.
        /// <para>Example: <c>runner.ContinueGate = () => !myTypewriter.IsPlaying;</c></para>
        /// </summary>
        public Func<bool> ContinueGate { get; set; }

        // ─── Events ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Raised when a single narrative line is reached.
        /// Call <see cref="Continue"/> once the player acknowledges it.
        /// </summary>
        public event Action<NarrativeLineData> OnLine;

        /// <summary>
        /// Raised when a block of sequential lines is reached.
        /// Call <see cref="Continue"/> once the player acknowledges the full block.
        /// </summary>
        public event Action<NarrativeBlockData> OnBlock;

        /// <summary>
        /// Raised when a choice menu should be shown.
        /// The data contains only the choices that pass their visibility conditions.
        /// Call <see cref="SelectChoice"/> with the chosen index — do NOT call Continue.
        /// </summary>
        public event Action<ChoiceNodeData> OnChoice;

        /// <summary>
        /// Raised when an EventNode is processed. The runner auto-advances after firing.
        /// </summary>
        public event Action<EventNodeData> OnEvent;

        /// <summary>Raised when an EndNode is reached or the graph runs out of nodes.</summary>
        public event Action OnEnd;

        // ─── Public API ───────────────────────────────────────────────────────────

        /// <summary>Assign graph data from code instead of the inspector.</summary>
        public void SetGraphData(NarrativeGraphData data) => _graphData = data;

        /// <summary>
        /// Starts the narrative from the beginning (StartNode → first node).
        /// Safe to call multiple times to restart.
        /// </summary>
        public void StartNarrative()
        {
            if (_graphData == null)
            {
                Debug.LogError("[NarrativeRunner] No NarrativeGraphData assigned.", this);
                return;
            }

            var startNode = _graphData.GetNode(_graphData.startNodeId) as StartNodeData;
            if (startNode == null)
            {
                Debug.LogError("[NarrativeRunner] StartNodeData not found in graph data.", this);
                return;
            }

            BeginSession();
            Step(startNode.nextId);
        }

        /// <summary>
        /// Resumes the narrative from a previously saved node ID.
        /// Use <see cref="CurrentNode"/>.<c>.id</c> to get the node ID to save,
        /// then pass it here when loading.
        /// </summary>
        /// <example>
        /// // Saving:
        /// string savedNodeId = runner.CurrentNode.id;
        ///
        /// // Restoring:
        /// runner.StartNarrative(savedNodeId);
        /// </example>
        /// <param name="fromNodeId">ID of the node to resume from.</param>
        public void StartNarrative(string fromNodeId)
        {
            if (_graphData == null)
            {
                Debug.LogError("[NarrativeRunner] No NarrativeGraphData assigned.", this);
                return;
            }

            if (_graphData.GetNode(fromNodeId) == null)
            {
                Debug.LogError($"[NarrativeRunner] Cannot resume — node '{fromNodeId}' not found in graph data.", this);
                return;
            }

            BeginSession();
            Step(fromNodeId);
        }

        /// <summary>
        /// Advances to the next node after a line or block has been acknowledged.
        /// Do <b>not</b> call this when waiting for a choice — use <see cref="SelectChoice"/> instead.
        /// </summary>
        public void Continue()
        {
            if (!IsRunning)
            {
                Debug.LogWarning("[NarrativeRunner] Continue() called but narrative is not running.", this);
                return;
            }

            if (_awaitingChoice)
            {
                Debug.LogWarning("[NarrativeRunner] Continue() called while awaiting a choice. Use SelectChoice() instead.", this);
                return;
            }

            if (ContinueGate != null && !ContinueGate()) return;

            if (_current == null) return;

            var exiting = _current;
            _onNodeExit.Invoke(exiting);
            Step(GetLinearNextId(exiting));
        }

        /// <summary>
        /// Selects a choice by its zero-based index into the visible options list.
        /// Only valid after an <see cref="OnChoice"/> event has been raised.
        /// </summary>
        /// <param name="index">Zero-based index into the visible options passed to OnChoice.</param>
        public void SelectChoice(int index)
        {
            if (!IsRunning)
            {
                Debug.LogWarning("[NarrativeRunner] SelectChoice() called but narrative is not running.", this);
                return;
            }

            if (_current is not ChoiceNodeData)
            {
                Debug.LogWarning("[NarrativeRunner] SelectChoice() called but current node is not a ChoiceNodeData.", this);
                return;
            }

            if (_visibleOptions == null || index < 0 || index >= _visibleOptions.Count)
            {
                Debug.LogWarning($"[NarrativeRunner] Choice index {index} is out of range (0–{(_visibleOptions?.Count ?? 0) - 1}).", this);
                return;
            }

            _awaitingChoice = false;
            _onNodeExit.Invoke(_current);
            Step(_visibleOptions[index].nextId);
        }

        // ─── Internal helpers ─────────────────────────────────────────────────────

        void BeginSession()
        {
            IsRunning       = true;
            _awaitingChoice = false;
            _visibleOptions = null;
            _current        = null;
        }

        // ─── Internal step logic ──────────────────────────────────────────────────

        void Step(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                Finish();
                return;
            }

            var node = _graphData.GetNode(nodeId);
            if (node == null)
            {
                Debug.LogError($"[NarrativeRunner] Node with id '{nodeId}' not found in graph data.", this);
                Finish();
                return;
            }

            _current = node;

            // Capture visited state BEFORE marking so first-visit reads false on initial pass.
            bool wasVisited = _visitedNodes.Contains(node.id);
            _visitedNodes.Add(node.id);

            _onNodeEnter.Invoke(node);

            switch (node)
            {
                // ── Displayable nodes — pause and wait for Continue() ─────────────
                case RevisitableLineData rev:
                    var revisitedLine = new NarrativeLineData
                    {
                        id       = rev.id,
                        speaker  = rev.speaker,
                        text     = wasVisited ? rev.revisitText : rev.firstVisitText,
                        metadata = rev.metadata,
                        nextId   = rev.nextId,
                    };
                    OnLine?.Invoke(revisitedLine);
                    _onLine.Invoke(revisitedLine);
                    break;

                case NarrativeLineData line:
                    OnLine?.Invoke(line);
                    _onLine.Invoke(line);
                    break;

                case NarrativeBlockData block:
                    OnBlock?.Invoke(block);
                    _onBlock.Invoke(block);
                    break;

                // ── Choice — filter visible options, pause for SelectChoice() ─────
                case ChoiceNodeData choiceData:
                    _visibleOptions = choiceData.options
                        .Where(IsChoiceVisible)
                        .ToList();

                    if (_visibleOptions.Count == 0)
                    {
                        Debug.LogWarning("[NarrativeRunner] All choices are hidden by conditions. Ending narrative.", this);
                        Finish();
                        return;
                    }

                    _awaitingChoice = true;
                    // Raise a view with only visible options so the UI doesn't need to filter.
                    var filteredChoice = new ChoiceNodeData { id = choiceData.id, prompt = choiceData.prompt, options = _visibleOptions };
                    OnChoice?.Invoke(filteredChoice);
                    _onChoice.Invoke(filteredChoice);
                    break;

                // ── Auto-advancing nodes ──────────────────────────────────────────
                case RandomBranchNodeData randomBranch:
                    if (randomBranch.branchIds.Count == 0)
                    {
                        Debug.LogWarning("[NarrativeRunner] RandomBranchNode has no branches. Ending narrative.", this);
                        _onNodeExit.Invoke(node);
                        Finish();
                        return;
                    }
                    _onNodeExit.Invoke(node);
                    Step(randomBranch.branchIds[UnityEngine.Random.Range(0, randomBranch.branchIds.Count)]);
                    break;

                case SetVariableNodeData setVar:
                    ApplySetVariable(setVar);
                    _onNodeExit.Invoke(node);
                    Step(setVar.nextId);
                    break;

                case EventNodeData ev:
                    OnEvent?.Invoke(ev);
                    _onEvent.Invoke(ev);
                    _onNodeExit.Invoke(node);
                    Step(ev.nextId);
                    break;

                case JumpNodeData jump:
                    var target = _graphData.GetTargetByLabel(jump.targetLabel);
                    if (target == null)
                    {
                        Debug.LogError($"[NarrativeRunner] Jump target '{jump.targetLabel}' not found.", this);
                        _onNodeExit.Invoke(node);
                        Finish();
                        return;
                    }
                    _onNodeExit.Invoke(node);
                    Step(target.nextId);
                    break;

                case TargetNodeData targetNode:
                    // Labels are transparent — pass straight through.
                    _onNodeExit.Invoke(node);
                    Step(targetNode.nextId);
                    break;

                case ConditionalNodeData conditional:
                    _onNodeExit.Invoke(node);
                    Step(EvaluateConditional(conditional) ? conditional.trueId : conditional.falseId);
                    break;

                case EndNodeData:
                    _onNodeExit.Invoke(node);
                    Finish();
                    break;

                default:
                    Debug.LogWarning($"[NarrativeRunner] Unhandled node type '{node.GetType().Name}'. Stopping.", this);
                    _onNodeExit.Invoke(node);
                    Finish();
                    break;
            }
        }

        // ─── Set variable ─────────────────────────────────────────────────────────

        void ApplySetVariable(SetVariableNodeData setVar)
        {
            if (VariableSetter == null)
            {
                Debug.LogWarning("[NarrativeRunner] VariableSetter is not set. SetVariable node has no effect.", this);
                return;
            }

            switch (setVar)
            {
                case SetVariableBoolNodeData boolSet:
                    bool newBool = boolSet.op == BoolSetOperator.Toggle
                        ? !(VariableProvider?.Invoke(boolSet.variableId) is bool b && b)
                        : boolSet.value;
                    VariableSetter(boolSet.variableId, newBool);
                    break;

                case SetVariableIntNodeData intSet:
                    int curInt = VariableProvider?.Invoke(intSet.variableId) is int ci ? ci : 0;
                    int newInt = intSet.op switch
                    {
                        NumericSetOperator.Set      => intSet.value,
                        NumericSetOperator.Add      => curInt + intSet.value,
                        NumericSetOperator.Subtract => curInt - intSet.value,
                        NumericSetOperator.Multiply => curInt * intSet.value,
                        _                           => intSet.value,
                    };
                    VariableSetter(intSet.variableId, newInt);
                    break;

                case SetVariableFloatNodeData floatSet:
                    float curFloat = VariableProvider?.Invoke(floatSet.variableId) is float cf ? cf : 0f;
                    float newFloat = floatSet.op switch
                    {
                        NumericSetOperator.Set      => floatSet.value,
                        NumericSetOperator.Add      => curFloat + floatSet.value,
                        NumericSetOperator.Subtract => curFloat - floatSet.value,
                        NumericSetOperator.Multiply => curFloat * floatSet.value,
                        _                           => floatSet.value,
                    };
                    VariableSetter(floatSet.variableId, newFloat);
                    break;

                case SetVariableStringNodeData stringSet:
                    string curString = VariableProvider?.Invoke(stringSet.variableId) as string ?? "";
                    string newString = stringSet.op switch
                    {
                        StringSetOperator.Set    => stringSet.value,
                        StringSetOperator.Append => curString + stringSet.value,
                        _                        => stringSet.value,
                    };
                    VariableSetter(stringSet.variableId, newString);
                    break;
            }
        }

        // ─── Conditional choice visibility ────────────────────────────────────────

        bool IsChoiceVisible(ChoiceOption option)
        {
            if (string.IsNullOrEmpty(option.conditionVariableId))
                return true;

            if (VariableProvider == null)
            {
                Debug.LogWarning("[NarrativeRunner] VariableProvider not set — conditional choice defaults to visible.", this);
                return true;
            }

            var rawValue = VariableProvider(option.conditionVariableId);

            return option.conditionType switch
            {
                NarrativeConditionType.Bool =>
                    option.conditionOp == NarrativeConditionOperator.EqualTo
                        ? (rawValue is bool bv && bv) == option.conditionBoolValue
                        : (rawValue is bool bv2 && bv2) != option.conditionBoolValue,

                NarrativeConditionType.Int =>
                    EvaluateNumeric(rawValue is int iv ? iv : 0, option.conditionIntValue, option.conditionOp),

                NarrativeConditionType.Float =>
                    EvaluateNumeric(rawValue is float fv ? fv : 0f, option.conditionFloatValue, option.conditionOp),

                NarrativeConditionType.String =>
                    option.conditionOp switch
                    {
                        NarrativeConditionOperator.EqualTo    => (rawValue as string ?? "") == option.conditionStringValue,
                        NarrativeConditionOperator.NotEqualTo => (rawValue as string ?? "") != option.conditionStringValue,
                        NarrativeConditionOperator.Contains   => (rawValue as string ?? "").Contains(option.conditionStringValue ?? ""),
                        _                                     => false,
                    },

                _ => true,
            };
        }

        // ─── Conditional node evaluation ──────────────────────────────────────────

        bool EvaluateConditional(ConditionalNodeData cond)
        {
            if (VariableProvider == null)
            {
                Debug.LogWarning("[NarrativeRunner] VariableProvider is not set. Conditional defaults to false.", this);
                return false;
            }

            var rawValue = VariableProvider(cond.variableId);

            switch (cond)
            {
                case ConditionalBooleanNodeData boolCond:
                    bool bVal = rawValue is bool b ? b : false;
                    return cond.op == NarrativeConditionOperator.EqualTo
                        ? bVal == boolCond.compareValue
                        : bVal != boolCond.compareValue;

                case ConditionalIntegerNodeData intCond:
                    return EvaluateNumeric(rawValue is int i ? i : 0, intCond.compareValue, cond.op);

                case ConditionalFloatNodeData floatCond:
                    return EvaluateNumeric(rawValue is float f ? f : 0f, floatCond.compareValue, cond.op);

                case ConditionalStringNodeData strCond:
                    string sVal = rawValue as string ?? "";
                    return cond.op switch
                    {
                        NarrativeConditionOperator.EqualTo    => sVal == strCond.compareValue,
                        NarrativeConditionOperator.NotEqualTo => sVal != strCond.compareValue,
                        NarrativeConditionOperator.Contains   => sVal.Contains(strCond.compareValue),
                        _                                     => false,
                    };

                default:
                    return false;
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        static string GetLinearNextId(NarrativeNodeData node) => node switch
        {
            StartNodeData s          => s.nextId,
            NarrativeLineData l      => l.nextId,
            RevisitableLineData rev  => rev.nextId,
            NarrativeBlockData b     => b.nextId,
            EventNodeData e          => e.nextId,
            TargetNodeData t         => t.nextId,
            SetVariableNodeData sv   => sv.nextId,
            _                        => null,
        };

        static bool EvaluateNumeric<T>(T a, T b, NarrativeConditionOperator op)
            where T : IComparable<T>
        {
            return op switch
            {
                NarrativeConditionOperator.EqualTo          => a.CompareTo(b) == 0,
                NarrativeConditionOperator.NotEqualTo       => a.CompareTo(b) != 0,
                NarrativeConditionOperator.LessThan         => a.CompareTo(b) < 0,
                NarrativeConditionOperator.GreaterThan      => a.CompareTo(b) > 0,
                NarrativeConditionOperator.LessOrEqualTo    => a.CompareTo(b) <= 0,
                NarrativeConditionOperator.GreaterOrEqualTo => a.CompareTo(b) >= 0,
                _                                           => false,
            };
        }

        void Finish()
        {
            IsRunning = false;
            _awaitingChoice = false;
            _visibleOptions = null;
            _current = null;
            OnEnd?.Invoke();
            _onNarrativeEnd.Invoke();
        }
    }
}
