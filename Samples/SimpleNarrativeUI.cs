// ─────────────────────────────────────────────────────────────────────────────
// SimpleNarrativeUI.cs  —  Minimal end-to-end example
//
// HOW TO USE THIS PACKAGE  (step by step)
//
//  1. BUILD YOUR GRAPH
//     • In Unity: Assets > Create > Narrative Graph Tool > Narrative Graph
//     • Open the asset, build your story with nodes, save.
//
//  2. PARSE THE GRAPH INTO RUNTIME DATA
//     • Select the .narrativegraph file in the Project window.
//     • Menu: Tools > Narrative Graph Tool > Parse Selected Graph
//     • A  MyGraph_Data.asset  file is saved next to your graph.
//       (Re-parse whenever you edit the graph.)
//
//  3. SET UP A SCENE
//     • Create a GameObject, add a NarrativeRunner component.
//     • Drag the MyGraph_Data.asset onto the runner's "Graph Data" field.
//     • Add this SimpleNarrativeUI component to the same (or another) GameObject.
//     • Drag the NarrativeRunner reference into this component's "Runner" field.
//
//  4. PRESS PLAY
//     • The narrative starts automatically.
//     • Press Space to advance lines/blocks.
//     • Press 1/2/3 to select choices.
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections.Generic;
using NarrativeGraphTool;
using NarrativeGraphTool.Data;
using UnityEngine;

/// <summary>
/// Minimal console-based narrative UI that demonstrates the NarrativeRunner API.
/// Replace the Debug.Log calls with your own UI (TextMeshPro, Unity UI, etc.).
/// </summary>
public class SimpleNarrativeUI : MonoBehaviour
{
    [Tooltip("The NarrativeRunner component that drives the narrative.")]
    [SerializeField] NarrativeRunner runner;

    // ── Simulated game variables used by conditional nodes ────────────────────
    // In a real game these would come from your save/state system.
    readonly Dictionary<string, object> _gameVariables = new()
    {
        { "hasKey",    false },
        { "gold",      10    },
        { "heroName",  "Aryn" },
    };

    // ── Active choices (stored so keyboard input can select them) ─────────────
    ChoiceNodeData _pendingChoices;

    // ── Typewriter gate ───────────────────────────────────────────────────────
    // Set _isLineComplete = false when a line starts animating, true when done.
    // The runner will refuse to advance until this is true.
    bool _isLineComplete = true;

    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        if (runner == null)
        {
            Debug.LogError("[SimpleNarrativeUI] NarrativeRunner is not assigned.", this);
            return;
        }

        // 1. Provide a variable resolver so conditional nodes can evaluate.
        //    The key is whatever you put in ConditionalNodeData.variableId.
        runner.VariableProvider = key =>
            _gameVariables.TryGetValue(key, out var val) ? val : null;

        // 2. Block Continue() while a line is still animating.
        //    Replace _isLineComplete with your typewriter's IsComplete flag.
        runner.ContinueGate = () => _isLineComplete;

        // 3. Subscribe to runner events.
        runner.OnLine   += HandleLine;
        runner.OnBlock  += HandleBlock;
        runner.OnChoice += HandleChoice;
        runner.OnEvent  += HandleEvent;
        runner.OnEnd    += HandleEnd;

        // 3. Start.
        runner.StartNarrative();
    }

    void OnDestroy()
    {
        if (runner == null) return;
        runner.OnLine   -= HandleLine;
        runner.OnBlock  -= HandleBlock;
        runner.OnChoice -= HandleChoice;
        runner.OnEvent  -= HandleEvent;
        runner.OnEnd    -= HandleEnd;
    }

    // ─── Keyboard input ───────────────────────────────────────────────────────

    void Update()
    {
        if (!runner.IsRunning) return;

        // Advance a line or block
        if (Input.GetKeyDown(KeyCode.Space))
        {
            runner.Continue();
        }

        // Select choices 1–4 via number keys
        if (_pendingChoices != null)
        {
            for (int i = 0; i < _pendingChoices.options.Count && i < 4; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    Debug.Log($"[Choice selected] {i + 1}: {_pendingChoices.options[i].text}");
                    _pendingChoices = null;
                    runner.SelectChoice(i);
                    break;
                }
            }
        }
    }

    // ─── Event handlers ───────────────────────────────────────────────────────

    /// <summary>A single narrative line arrived. Show it in your UI.</summary>
    void HandleLine(NarrativeLineData line)
    {
        var prefix = string.IsNullOrEmpty(line.speaker) ? "" : $"[{line.speaker}] ";
        Debug.Log($"{prefix}{line.text}  (Press Space to continue)");

        // Mark as complete immediately (no typewriter in this sample).
        // With a real typewriter, set _isLineComplete = false here and
        // set it back to true in your typewriter's OnComplete callback.
        _isLineComplete = true;

        // Example with TextMeshPro + typewriter (uncomment and assign fields):
        // _isLineComplete = false;
        // speakerLabel.text = line.speaker;
        // myTypewriter.Play(line.text, onComplete: () => _isLineComplete = true);
    }

    /// <summary>A block of sequential lines arrived. Show each line in your UI.</summary>
    void HandleBlock(NarrativeBlockData block)
    {
        Debug.Log($"--- Block ({block.lines.Count} lines) ---");
        foreach (var l in block.lines)
        {
            var prefix = string.IsNullOrEmpty(l.speaker) ? "" : $"[{l.speaker}] ";
            Debug.Log($"  {prefix}{l.text}");
        }
        Debug.Log("(Press Space to continue)");

        // Tip: for a typewriter effect, queue the lines and advance them one
        // at a time internally, then call runner.Continue() after the last one.
    }

    /// <summary>A choice menu should be shown. Display the options in your UI.</summary>
    void HandleChoice(ChoiceNodeData data)
    {
        _pendingChoices = data;
        Debug.Log("--- Choose ---");
        for (int i = 0; i < data.options.Count; i++)
            Debug.Log($"  [{i + 1}] {data.options[i].text}");

        // Example with buttons:
        // for (int i = 0; i < data.options.Count; i++)
        // {
        //     int captured = i;
        //     choiceButtons[i].GetComponentInChildren<TMP_Text>().text = data.options[i].text;
        //     choiceButtons[i].onClick.RemoveAllListeners();
        //     choiceButtons[i].onClick.AddListener(() => {
        //         runner.SelectChoice(captured);
        //         HideChoicePanel();
        //     });
        // }
        // choicePanel.SetActive(true);
    }

    /// <summary>A game event was fired. React to it however your game needs.</summary>
    void HandleEvent(EventNodeData ev)
    {
        Debug.Log($"[Event] {ev.eventName}  payload={ev.payload}");

        // Example dispatcher pattern:
        // switch (ev.eventName)
        // {
        //     case "PlayCutscene": CutsceneManager.Play(ev.payload); break;
        //     case "GiveItem":     Inventory.Add(ev.payload);        break;
        //     case "SetFlag":      flags[ev.payload] = true;         break;
        // }
    }

    /// <summary>The narrative has ended.</summary>
    void HandleEnd()
    {
        Debug.Log("[Narrative ended]");

        // Hide your dialogue UI, resume gameplay, etc.
        // dialoguePanel.SetActive(false);
    }
}
