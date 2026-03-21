using System.Collections.Generic;
using NarrativeGraphTool.Runtime;
using NarrativeGraphTool.Runtime.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NarrativeGraphTool.Tests
{
    /// <summary>
    /// Tests for NarrativeRunner covering choice nodes and conditional choice visibility.
    /// </summary>
    [TestFixture]
    public class NarrativeRunnerChoiceTests
    {
        GameObject      _go;
        NarrativeRunner _runner;
        List<Object>    _toDestroy;
        Dictionary<string, object> _variables;

        [SetUp]
        public void SetUp()
        {
            LogAssert.ignoreFailingMessages = true;
            _go        = new GameObject("TestRunner");
            _runner    = _go.AddComponent<NarrativeRunner>();
            _toDestroy = new List<Object> { _go };
            _variables = new Dictionary<string, object>();

            _runner.VariableProvider = key => _variables.TryGetValue(key, out var v) ? v : null;
            _runner.VariableSetter   = (key, val) => _variables[key] = val;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _toDestroy)
                if (obj != null) Object.DestroyImmediate(obj);
        }

        NarrativeGraphData BuildGraph(string startNextId, params NarrativeNodeData[] nodes)
        {
            var g = GraphBuilder.Build(startNextId, nodes);
            _toDestroy.Add(g);
            return g;
        }

        // ─── Basic choice ─────────────────────────────────────────────────────────

        [Test]
        public void ChoiceNode_FiresOnChoice_WithAllOptions()
        {
            _runner.SetGraphData(BuildGraph("choice0",
                GraphBuilder.Choice("choice0",
                    GraphBuilder.Option("Yes", "end"),
                    GraphBuilder.Option("No",  "end")),
                GraphBuilder.End("end")));

            ChoiceNodeData received = null;
            _runner.OnChoice += d => received = d;

            _runner.StartNarrative();

            Assert.IsNotNull(received);
            Assert.AreEqual(2,     received.options.Count);
            Assert.AreEqual("Yes", received.options[0].text);
            Assert.AreEqual("No",  received.options[1].text);
        }

        [Test]
        public void SelectChoice_Index0_AdvancesToFirstBranch()
        {
            _runner.SetGraphData(BuildGraph("choice0",
                GraphBuilder.Choice("choice0",
                    GraphBuilder.Option("Yes", "lineA"),
                    GraphBuilder.Option("No",  "lineB")),
                GraphBuilder.Line("lineA", "Chose yes", "end"),
                GraphBuilder.Line("lineB", "Chose no",  "end"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();
            _runner.SelectChoice(0);

            Assert.AreEqual("Chose yes", received.text);
        }

        [Test]
        public void SelectChoice_Index1_AdvancesToSecondBranch()
        {
            _runner.SetGraphData(BuildGraph("choice0",
                GraphBuilder.Choice("choice0",
                    GraphBuilder.Option("Yes", "lineA"),
                    GraphBuilder.Option("No",  "lineB")),
                GraphBuilder.Line("lineA", "Chose yes", "end"),
                GraphBuilder.Line("lineB", "Chose no",  "end"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();
            _runner.SelectChoice(1);

            Assert.AreEqual("Chose no", received.text);
        }

        [Test]
        public void SelectChoice_OutOfRange_IsIgnored()
        {
            _runner.SetGraphData(BuildGraph("choice0",
                GraphBuilder.Choice("choice0",
                    GraphBuilder.Option("Only option", "end")),
                GraphBuilder.End("end")));

            bool ended = false;
            _runner.OnEnd += () => ended = true;

            _runner.StartNarrative();
            _runner.SelectChoice(99); // out of range

            Assert.IsFalse(ended);
            Assert.IsTrue(_runner.IsRunning);
        }

        [Test]
        public void AllChoicesHidden_ByCondition_EndsNarrative()
        {
            _variables["flag"] = false;

            _runner.SetGraphData(BuildGraph("choice0",
                GraphBuilder.Choice("choice0",
                    GraphBuilder.ConditionalOption("Hidden", "end",
                        "flag", NarrativeConditionType.Bool,
                        NarrativeConditionOperator.EqualTo, boolValue: true)),
                GraphBuilder.End("end")));

            bool ended = false;
            _runner.OnEnd += () => ended = true;

            _runner.StartNarrative();

            Assert.IsTrue(ended);
        }

        [Test]
        public void ChoiceNode_FilteredOptions_IndexMapsByVisibleOptions()
        {
            // Only "Secret" should be hidden; SelectChoice(1) should pick "Goodbye", not "Secret"
            _variables["hasKey"] = false;

            _runner.SetGraphData(BuildGraph("choice0",
                GraphBuilder.Choice("choice0",
                    GraphBuilder.Option("Hello", "lineA"),
                    GraphBuilder.ConditionalOption("Secret", "lineB",
                        "hasKey", NarrativeConditionType.Bool,
                        NarrativeConditionOperator.EqualTo, boolValue: true),
                    GraphBuilder.Option("Goodbye", "lineC")),
                GraphBuilder.Line("lineA", "Hello branch",   "end"),
                GraphBuilder.Line("lineB", "Secret branch",  "end"),
                GraphBuilder.Line("lineC", "Goodbye branch", "end"),
                GraphBuilder.End("end")));

            ChoiceNodeData receivedChoice = null;
            _runner.OnChoice += d => receivedChoice = d;

            NarrativeLineData receivedLine = null;
            _runner.OnLine += d => receivedLine = d;

            _runner.StartNarrative();

            // Only 2 visible options: Hello (0) and Goodbye (1)
            Assert.AreEqual(2, receivedChoice.options.Count);

            _runner.SelectChoice(1); // should go to Goodbye, not Secret

            Assert.AreEqual("Goodbye branch", receivedLine.text);
        }

        // ─── Conditional choice: Bool ─────────────────────────────────────────────

        [Test]
        public void ConditionalChoice_Bool_Visible_WhenEqualToMet()
        {
            _variables["hasKey"] = true;

            _runner.SetGraphData(BuildGraph("choice0",
                GraphBuilder.Choice("choice0",
                    GraphBuilder.Option("Base", "end"),
                    GraphBuilder.ConditionalOption("Use key", "end",
                        "hasKey", NarrativeConditionType.Bool,
                        NarrativeConditionOperator.EqualTo, boolValue: true)),
                GraphBuilder.End("end")));

            ChoiceNodeData received = null;
            _runner.OnChoice += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual(2, received.options.Count);
        }

        [Test]
        public void ConditionalChoice_Bool_Hidden_WhenEqualToNotMet()
        {
            _variables["hasKey"] = false;

            _runner.SetGraphData(BuildGraph("choice0",
                GraphBuilder.Choice("choice0",
                    GraphBuilder.Option("Base", "end"),
                    GraphBuilder.ConditionalOption("Use key", "end",
                        "hasKey", NarrativeConditionType.Bool,
                        NarrativeConditionOperator.EqualTo, boolValue: true)),
                GraphBuilder.End("end")));

            ChoiceNodeData received = null;
            _runner.OnChoice += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual(1,      received.options.Count);
            Assert.AreEqual("Base", received.options[0].text);
        }

        [Test]
        public void ConditionalChoice_Bool_NotEqualTo_WorksCorrectly()
        {
            _variables["defeated"] = true;

            _runner.SetGraphData(BuildGraph("choice0",
                GraphBuilder.Choice("choice0",
                    GraphBuilder.ConditionalOption("Fight again", "end",
                        "defeated", NarrativeConditionType.Bool,
                        NarrativeConditionOperator.NotEqualTo, boolValue: true)),
                GraphBuilder.End("end")));

            ChoiceNodeData received = null;
            _runner.OnChoice += d => received = d;
            bool ended = false;
            _runner.OnEnd += () => ended = true;

            _runner.StartNarrative();

            // "defeated" == true and op is NotEqualTo true → condition is false → hidden → ends
            Assert.IsTrue(ended);
        }

        // ─── Conditional choice: Int ──────────────────────────────────────────────

        [Test]
        public void ConditionalChoice_Int_GreaterThan_Visible_WhenMet()
        {
            _variables["gold"] = 100;

            _runner.SetGraphData(BuildGraph("choice0",
                GraphBuilder.Choice("choice0",
                    GraphBuilder.Option("Base", "end"),
                    GraphBuilder.ConditionalOption("Buy sword", "end",
                        "gold", NarrativeConditionType.Int,
                        NarrativeConditionOperator.GreaterThan, intValue: 50)),
                GraphBuilder.End("end")));

            ChoiceNodeData received = null;
            _runner.OnChoice += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual(2, received.options.Count);
        }

        [Test]
        public void ConditionalChoice_Int_GreaterThan_Hidden_WhenNotMet()
        {
            _variables["gold"] = 10;

            _runner.SetGraphData(BuildGraph("choice0",
                GraphBuilder.Choice("choice0",
                    GraphBuilder.Option("Base", "end"),
                    GraphBuilder.ConditionalOption("Buy sword", "end",
                        "gold", NarrativeConditionType.Int,
                        NarrativeConditionOperator.GreaterThan, intValue: 50)),
                GraphBuilder.End("end")));

            ChoiceNodeData received = null;
            _runner.OnChoice += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual(1, received.options.Count);
        }

        [Test]
        public void ConditionalChoice_Int_LessOrEqualTo_Visible_WhenEqual()
        {
            _variables["level"] = 5;

            _runner.SetGraphData(BuildGraph("choice0",
                GraphBuilder.Choice("choice0",
                    GraphBuilder.ConditionalOption("Beginner content", "end",
                        "level", NarrativeConditionType.Int,
                        NarrativeConditionOperator.LessOrEqualTo, intValue: 5)),
                GraphBuilder.End("end")));

            ChoiceNodeData received = null;
            _runner.OnChoice += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual(1, received.options.Count);
        }

        // ─── Conditional choice: Float ────────────────────────────────────────────

        [Test]
        public void ConditionalChoice_Float_LessThan_Visible_WhenMet()
        {
            _variables["health"] = 0.2f;

            _runner.SetGraphData(BuildGraph("choice0",
                GraphBuilder.Choice("choice0",
                    GraphBuilder.ConditionalOption("Use potion", "end",
                        "health", NarrativeConditionType.Float,
                        NarrativeConditionOperator.LessThan, floatValue: 0.5f)),
                GraphBuilder.End("end")));

            ChoiceNodeData received = null;
            _runner.OnChoice += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual(1, received.options.Count);
            Assert.AreEqual("Use potion", received.options[0].text);
        }

        // ─── Conditional choice: String ───────────────────────────────────────────

        [Test]
        public void ConditionalChoice_String_Contains_Visible_WhenMet()
        {
            _variables["inventory"] = "sword,shield,potion";

            _runner.SetGraphData(BuildGraph("choice0",
                GraphBuilder.Choice("choice0",
                    GraphBuilder.Option("Base", "end"),
                    GraphBuilder.ConditionalOption("Use sword", "end",
                        "inventory", NarrativeConditionType.String,
                        NarrativeConditionOperator.Contains, stringValue: "sword")),
                GraphBuilder.End("end")));

            ChoiceNodeData received = null;
            _runner.OnChoice += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual(2, received.options.Count);
        }

        [Test]
        public void ConditionalChoice_String_Contains_Hidden_WhenNotMet()
        {
            _variables["inventory"] = "shield,potion";

            _runner.SetGraphData(BuildGraph("choice0",
                GraphBuilder.Choice("choice0",
                    GraphBuilder.Option("Base", "end"),
                    GraphBuilder.ConditionalOption("Use sword", "end",
                        "inventory", NarrativeConditionType.String,
                        NarrativeConditionOperator.Contains, stringValue: "sword")),
                GraphBuilder.End("end")));

            ChoiceNodeData received = null;
            _runner.OnChoice += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual(1, received.options.Count);
        }

        [Test]
        public void ConditionalChoice_String_EqualTo_WorksCorrectly()
        {
            _variables["faction"] = "rebels";

            _runner.SetGraphData(BuildGraph("choice0",
                GraphBuilder.Choice("choice0",
                    GraphBuilder.ConditionalOption("Join empire", "end",
                        "faction", NarrativeConditionType.String,
                        NarrativeConditionOperator.EqualTo, stringValue: "empire")),
                GraphBuilder.End("end")));

            bool ended = false;
            _runner.OnEnd += () => ended = true;

            _runner.StartNarrative();

            // faction is "rebels" ≠ "empire" → hidden → all hidden → ends
            Assert.IsTrue(ended);
        }

        // ─── No VariableProvider ──────────────────────────────────────────────────

        [Test]
        public void ConditionalChoice_WithNoVariableProvider_DefaultsToVisible()
        {
            _runner.VariableProvider = null;

            _runner.SetGraphData(BuildGraph("choice0",
                GraphBuilder.Choice("choice0",
                    GraphBuilder.ConditionalOption("Conditional", "end",
                        "someVar", NarrativeConditionType.Bool,
                        NarrativeConditionOperator.EqualTo, boolValue: true)),
                GraphBuilder.End("end")));

            ChoiceNodeData received = null;
            _runner.OnChoice += d => received = d;

            _runner.StartNarrative();

            // Without a provider, conditional choices default to visible
            Assert.AreEqual(1, received.options.Count);
        }
    }
}
