# 📖 Narrative Graph Tool

<p align="center">
  <img src="Editor/Icons/narrative-graph-icon.png" width="96" alt="Narrative Graph Tool icon"/>
</p>

A **node-based narrative editor** for Unity 6.4+ built on the Graph Toolkit.
Design branching stories, dialogue, and interactive narratives visually — then play them back at runtime with a simple event-driven API.

---

## ✨ Features

- 🗂️ Visual graph editor with 14 node types
- 🌿 Branching choices with conditional visibility
- 🔢 Variable system — bool, int, float, string
- 🎭 Extensible line metadata (emotions, portraits, voice, etc.)
- 🎲 Random branching
- ⚡ Event nodes for gameplay hooks
- 🔁 Revisitable lines with alternate text
- 🏷️ Jump/Target anchors for non-linear flow
- 📝 Export any graph to plain text with one right-click
- 📦 Zero runtime dependencies — pure C# data layer

---

## 📋 Requirements

| Requirement | Version |
|---|---|
| Unity | 6.4+ |
| Graph Toolkit | Built-in (Unity 6.4) |

---

## 📦 Installation

Go to **Window → Package Manager → + → Add package from git URL** and paste:

```
https://github.com/marcusaasjensen/narrative-graph-tool.git
```

> Graph Toolkit is built into Unity 6.4 — no extra steps needed.

---

## 🚀 Quick Start

### 1. Create a Narrative Graph asset

Right-click in the **Project window → Create → Narrative Graph Tool → Narrative Graph**.

### 2. Open the graph editor

Double-click the `.narrativegraph` asset to open it in the graph editor.

### 3. Build your narrative

Add a **StartNode**, connect it to **NarrativeLineNodes** and **EndNodes**, wire up choices — the graph is automatically parsed and saved as a runtime `NarrativeGraphData` sub-asset whenever you save.

### 4. Set up the runner

Add a **NarrativeRunner** component to a GameObject, drag your `.narrativegraph` file into the **Graph Data** field.

### 5. Subscribe to events and drive your UI

```csharp
using NarrativeGraphTool;

[SerializeField] NarrativeRunner runner;

void Start()
{
    runner.OnLine   += line   => Debug.Log($"{line.speaker}: {line.text}");
    runner.OnChoice += opts   => ShowChoiceUI(opts);
    runner.OnEnd    += ()     => Debug.Log("Narrative ended.");

    runner.VariableProvider = id => myVariables[id];
    runner.VariableSetter   = (id, val) => myVariables[id] = val;

    runner.StartNarrative();
}

void Update()
{
    if (Input.GetKeyDown(KeyCode.Space)) runner.Continue();
}
```

---

## 🧩 Node Reference

### 🟢 Flow

| Node | Description |
|---|---|
| **Start** | Entry point of the narrative. Every graph needs exactly one. |
| **End** | Terminates the narrative and fires `OnEnd`. |

---

### 💬 Lines

| Node | Description |
|---|---|
| **NarrativeLine** | Displays a single line with a speaker, text, and optional metadata. Fires `OnLine`. |
| **RevisitableLine** | Displays different text on first visit vs. subsequent visits. Fires `OnLine`. |
| **NarrativeContext** | Groups a sequence of lines into a block. Fires `OnBlock` with all lines at once. |

> ℹ️ **NarrativeContext** is a container node — add **LineBlock** children inside it.

---

### 🔀 Choices

| Node | Description |
|---|---|
| **ChoiceContext** | Presents a list of choices to the player. Fires `OnChoice`. Optionally embed a **Speaker**, **Prompt** text, and **Metadata** directly on the node — or leave them empty and wire a `NarrativeLine` before it instead. |
| **ConditionalBoolChoice** | A choice that only appears when a bool variable matches. |
| **ConditionalIntChoice** | A choice that only appears when an int variable matches a condition. |
| **ConditionalFloatChoice** | A choice that only appears when a float variable matches a condition. |
| **ConditionalStringChoice** | A choice that only appears when a string variable matches a condition. |

> ℹ️ **ChoiceContext** is a container node — add **ChoiceBlock** or conditional choice children inside it.

---

### 🔵 Conditionals

Branch the flow based on a variable's value.

| Node | Compares |
|---|---|
| **ConditionalBoolean** | Bool variable |
| **ConditionalInteger** | Int variable with: `==` `!=` `<` `>` `<=` `>=` |
| **ConditionalFloat** | Float variable with: `==` `!=` `<` `>` `<=` `>=` |
| **ConditionalString** | String variable with: `==` `!=` `Contains` |

All conditional nodes have a **True** and **False** output port.

---

### 📝 Set Variable

Modify a variable as the narrative flows through. Execution continues automatically.

| Node | Operations |
|---|---|
| **SetVariableBool** | `Set`, `Toggle` |
| **SetVariableInt** | `Set`, `Add`, `Subtract`, `Multiply` |
| **SetVariableFloat** | `Set`, `Add`, `Subtract`, `Multiply` |
| **SetVariableString** | `Set`, `Append` |

---

### 🎲 Random Branch

| Node | Description |
|---|---|
| **RandomBranchContext** | Picks one of its **RandomBranchBlock** children at random and continues from there. |

---

### ⚡ Events

| Node | Description |
|---|---|
| **Event** | Fires `OnEvent` with a name and optional string payload. By default auto-advances immediately. Enable **Wait For Resume** to stop the runner until `Resume()` is called — use this for animations, cutscenes, or any async effect that must finish before the narrative continues. |

---

### ⏸️ Pause

| Node | Description |
|---|---|
| **Pause** | Suspends the narrative without ending it. Fires `OnPause`. Call `Resume()` to continue from the connected output node. Use this to exit a dialogue mid-scene and re-enter exactly where you left off. |

---

### 🏷️ Jump & Target

| Node | Description |
|---|---|
| **Jump** | Teleports execution to a **Target** node by name. |
| **Target** | A named anchor that execution can jump to. |

> 💡 Use Jump/Target for loops or shared nodes you want to revisit from multiple places.

---

## ⚙️ NarrativeRunner API

### C# Events

Subscribe from code for full typed access to node data:

```csharp
using NarrativeGraphTool;
using NarrativeGraphTool.Data;

// A single narrative line is ready to display
runner.OnLine += (NarrativeLineData line) => { };

// A sequential block of lines is ready to display
runner.OnBlock += (NarrativeBlockData block) => { };

// A choice menu should be shown — contains only the visible choices
runner.OnChoice += (ChoiceNodeData data) => { };

// An event node was reached — use name + payload for gameplay hooks
runner.OnEvent += (EventNodeData ev) => { };

// The narrative has ended
runner.OnEnd += () => { };

// A PauseNode was reached — runner stopped, waiting for Resume()
runner.OnPause += () => { };
```

---

### 🔌 Unity Events (Inspector)

Every C# event has an Inspector-wirable **UnityEvent** equivalent, visible under the **Unity Events** header on the `NarrativeRunner` component. Wire up any method directly in the Inspector — no code required.

| Unity Event | Equivalent C# event | Parameter |
|---|---|---|
| `_onLine` | `OnLine` | `NarrativeLineData` |
| `_onBlock` | `OnBlock` | `NarrativeBlockData` |
| `_onChoice` | `OnChoice` | `ChoiceNodeData` |
| `_onEvent` | `OnEvent` | `EventNodeData` |
| `_onNarrativeEnd` | `OnEnd` | — |
| `_onNodeEnter` | — | `NarrativeNodeData` |
| `_onNodeExit` | — | `NarrativeNodeData` |

> ℹ️ `_onNodeEnter` and `_onNodeExit` fire for **every** node — useful for driving animations, audio, or analytics without caring about the specific node type. In the Inspector, select the **Dynamic** version of a method to receive the node as a parameter, or wire a no-parameter method to ignore it.

---

### Controlling flow

```csharp
runner.StartNarrative();              // Begin from StartNode
runner.StartNarrative(savedNodeId);   // Resume from a saved node
runner.Continue();                    // Advance past a line or block
runner.SelectChoice(index);           // Choose an option (0-based, filtered index)
runner.Resume();                      // Continue after a PauseNode or blocking EventNode
runner.SetGraphData(data);            // Swap graph data at runtime
runner.ResetVisitedNodes();           // Clear visit history
```

**Blocking event example** (animation, cutscene, close UI):

```csharp
runner.OnEvent += ev =>
{
    switch (ev.eventName)
    {
        case "PlayCutscene":
            // ev.waitForResume is true — must call Resume() when done
            StartCoroutine(PlayCutsceneAndResume());
            break;

        case "PlaySound":
            // ev.waitForResume is false — runner already advanced
            audioSource.Play();
            break;
    }
};

IEnumerator PlayCutsceneAndResume()
{
    animator.Play("cutscene");
    yield return new WaitUntil(() => animator.IsInTransition(0) == false);
    runner.Resume();
}
```

**Save & restore example:**
```csharp
// Save — store the current node ID anywhere (PlayerPrefs, JSON, ScriptableObject…)
string savedNodeId = runner.CurrentNode.id;

// Restore — resume exactly where the player left off
runner.StartNarrative(savedNodeId);
```

---

### Variables

Provide callbacks so the runner can read and write your game's variables:

```csharp
// Called when a conditional or set-variable node needs a value
runner.VariableProvider = (string id) => myVariables[id];

// Called when a set-variable node writes a value
runner.VariableSetter = (string id, object value) => myVariables[id] = value;
```

> 💡 Variable IDs are plain strings you define in the graph nodes — they map to whatever storage system your game uses (Dictionary, ScriptableObjects, PlayerPrefs, etc.)

---

### State

```csharp
using NarrativeGraphTool.Data;

bool running                          = runner.IsRunning;
NarrativeNodeData current             = runner.CurrentNode;
IReadOnlyCollection<string> visited   = runner.VisitedNodes;
```

---

## 📝 Export as Plain Text

Right-click any `.narrativegraph` asset in the Project window and choose **Export Narrative As Text**.

A `.txt` file is saved next to the asset and highlighted automatically. The output is a human-readable script that mirrors the graph flow:

```
=== MyGraph ===

[Alice]: What will you do?
> CHOICE
  [1] Fight
        [Alice]: Brave choice.
        === END ===
  [2] Run
        [Alice]: Live to fight another day.
        === END ===
```

All node types are supported — conditionals, set-variable, events, jumps, random branches, and cycle detection (`↑ loops back to [label]`).

You can also call it from code:

```csharp
string script = NarrativeGraphTextExporter.Convert(myGraphData);
```

---

## 🎨 Custom Line Metadata

Attach game-specific data to any narrative line (emotion, portrait, audio clip, etc.) by extending `NarrativeLineMetadata`:

```csharp
using NarrativeGraphTool.Data;

[CreateAssetMenu(menuName = "Narrative Graph > My Line Metadata")]
public class MyLineMetadata : NarrativeLineMetadata
{
    public Sprite portrait;
    public AudioClip voiceClip;
    public Color subtitleColor = Color.white;
}
```

Then assign it to a **NarrativeLine** or **NarrativeContext** node in the graph editor, and read it at runtime:

```csharp
runner.OnLine += line =>
{
    if (line.metadata is MyLineMetadata meta)
    {
        portraitImage.sprite = meta.portrait;
        audioSource.PlayOneShot(meta.voiceClip);
    }
};
```

---

## 🧪 Sample

The package includes a ready-to-use example in `Samples/`:

| File | Description |
|---|---|
| `SimpleNarrativeUI.cs` | Minimal console-based runner showing the full API (input, choices, events, variables) |

> 🕹️ **SimpleNarrativeUI** controls: **Space** to advance, **1–4** keys to select choices.

---

## 📁 Package Structure

```
NarrativeGraphTool/
├── Editor/          # Graph editor nodes, parser, asset importer, text exporter
├── Runtime/         # NarrativeRunner, NarrativeGraphData, node data classes
├── Samples/         # SimpleNarrativeUI example
├── Tests/           # Edit-mode unit tests (~90 tests)
└── Resources/       # Package icons
```

---

## 📄 License

MIT — see [LICENSE](LICENSE) for details.
