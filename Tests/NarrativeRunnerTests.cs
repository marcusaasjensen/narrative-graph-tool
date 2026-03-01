using System.Collections.Generic;
using NarrativeGraphTool.Runtime;
using NarrativeGraphTool.Runtime.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NarrativeGraphTool.Tests
{
    /// <summary>
    /// Tests for NarrativeRunner covering: linear flow, guard conditions,
    /// narrative blocks, event nodes, revisitable lines, and random branches.
    /// </summary>
    [TestFixture]
    public class NarrativeRunnerTests
    {
        GameObject     _go;
        NarrativeRunner _runner;
        List<Object>   _toDestroy;

        [SetUp]
        public void SetUp()
        {
            LogAssert.ignoreFailingMessages = true;
            _go        = new GameObject("TestRunner");
            _runner    = _go.AddComponent<NarrativeRunner>();
            _toDestroy = new List<Object> { _go };
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

        // ─── Guard conditions ─────────────────────────────────────────────────────

        [Test]
        public void StartNarrative_WithNoGraph_DoesNotSetRunning()
        {
            LogAssert.Expect(LogType.Error, "[NarrativeRunner] No NarrativeGraphData assigned.");
            _runner.StartNarrative();

            Assert.IsFalse(_runner.IsRunning);
        }

        [Test]
        public void Continue_WhenNotRunning_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _runner.Continue());
        }

        [Test]
        public void SelectChoice_WhenNotRunning_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _runner.SelectChoice(0));
        }

        [Test]
        public void SelectChoice_WhenNotAtChoiceNode_DoesNotAdvance()
        {
            _runner.SetGraphData(BuildGraph("line0",
                GraphBuilder.Line("line0", "Hello", "end"),
                GraphBuilder.End("end")));

            bool ended = false;
            _runner.OnEnd += () => ended = true;

            _runner.StartNarrative(); // paused at line0 — not a choice
            _runner.SelectChoice(0);  // should be ignored

            Assert.IsFalse(ended);
        }

        // ─── Basic linear flow ────────────────────────────────────────────────────

        [Test]
        public void StartNarrative_SetsIsRunning_True()
        {
            _runner.SetGraphData(BuildGraph("line0",
                GraphBuilder.Line("line0", "Hi", "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.IsTrue(_runner.IsRunning);
        }

        [Test]
        public void StartNarrative_FiresOnLine_WithCorrectSpeakerAndText()
        {
            _runner.SetGraphData(BuildGraph("line0",
                GraphBuilder.Line("line0", "Hello world", "end", "Aria"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();

            Assert.IsNotNull(received);
            Assert.AreEqual("Aria",        received.speaker);
            Assert.AreEqual("Hello world", received.text);
        }

        [Test]
        public void Continue_AfterLine_AdvancesToNextLine()
        {
            _runner.SetGraphData(BuildGraph("line0",
                GraphBuilder.Line("line0", "First",  "line1"),
                GraphBuilder.Line("line1", "Second", "end"),
                GraphBuilder.End("end")));

            var texts = new List<string>();
            _runner.OnLine += d => texts.Add(d.text);

            _runner.StartNarrative();
            _runner.Continue();

            Assert.AreEqual(2,        texts.Count);
            Assert.AreEqual("First",  texts[0]);
            Assert.AreEqual("Second", texts[1]);
        }

        [Test]
        public void EndNode_FiresOnEnd_AndClearsIsRunning()
        {
            _runner.SetGraphData(BuildGraph("end",
                GraphBuilder.End("end")));

            bool ended = false;
            _runner.OnEnd += () => ended = true;

            _runner.StartNarrative();

            Assert.IsTrue(ended);
            Assert.IsFalse(_runner.IsRunning);
        }

        [Test]
        public void NullNextId_FiresOnEnd_AfterContinue()
        {
            // A line with no nextId — narrative ends on Continue
            _runner.SetGraphData(BuildGraph("line0",
                GraphBuilder.Line("line0", "Final", nextId: null)));

            bool ended = false;
            _runner.OnEnd += () => ended = true;

            _runner.StartNarrative();
            _runner.Continue();

            Assert.IsTrue(ended);
        }

        [Test]
        public void StartNarrative_CanRestartAfterEnd()
        {
            _runner.SetGraphData(BuildGraph("line0",
                GraphBuilder.Line("line0", "Hello", "end"),
                GraphBuilder.End("end")));

            int lineCount = 0;
            _runner.OnLine += _ => lineCount++;

            _runner.StartNarrative();
            _runner.Continue();     // reaches EndNode
            _runner.StartNarrative(); // restart

            Assert.AreEqual(2, lineCount);
            Assert.IsTrue(_runner.IsRunning);
        }

        [Test]
        public void CurrentNode_ReflectsCurrentlyPausedNode()
        {
            _runner.SetGraphData(BuildGraph("line0",
                GraphBuilder.Line("line0", "Hi", "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.IsNotNull(_runner.CurrentNode);
            Assert.AreEqual("line0", _runner.CurrentNode.id);
        }

        [Test]
        public void CurrentNode_IsNull_WhenNotRunning()
        {
            Assert.IsNull(_runner.CurrentNode);
        }

        // ─── NarrativeBlock ───────────────────────────────────────────────────────

        [Test]
        public void BlockNode_FiresOnBlock_WithAllLines()
        {
            _runner.SetGraphData(BuildGraph("block0",
                GraphBuilder.Block("block0", "end",
                    ("Aria", "Line one"),
                    ("Aria", "Line two"),
                    ("Eli",  "Line three")),
                GraphBuilder.End("end")));

            NarrativeBlockData received = null;
            _runner.OnBlock += d => received = d;

            _runner.StartNarrative();

            Assert.IsNotNull(received);
            Assert.AreEqual(3,            received.lines.Count);
            Assert.AreEqual("Line one",   received.lines[0].text);
            Assert.AreEqual("Line two",   received.lines[1].text);
            Assert.AreEqual("Line three", received.lines[2].text);
            Assert.AreEqual("Eli",        received.lines[2].speaker);
        }

        [Test]
        public void Continue_AfterBlock_AdvancesToNextNode()
        {
            _runner.SetGraphData(BuildGraph("block0",
                GraphBuilder.Block("block0", "line1", ("", "Block line")),
                GraphBuilder.Line("line1", "After block", "end"),
                GraphBuilder.End("end")));

            NarrativeLineData lineAfter = null;
            _runner.OnLine += d => lineAfter = d;

            _runner.StartNarrative();
            _runner.Continue();

            Assert.IsNotNull(lineAfter);
            Assert.AreEqual("After block", lineAfter.text);
        }

        // ─── Event node ───────────────────────────────────────────────────────────

        [Test]
        public void EventNode_FiresOnEvent_WithCorrectNameAndPayload()
        {
            _runner.SetGraphData(BuildGraph("ev0",
                GraphBuilder.Event("ev0", "PlayMusic", "theme_main", "end"),
                GraphBuilder.End("end")));

            EventNodeData received = null;
            _runner.OnEvent += d => received = d;

            _runner.StartNarrative();

            Assert.IsNotNull(received);
            Assert.AreEqual("PlayMusic",  received.eventName);
            Assert.AreEqual("theme_main", received.payload);
        }

        [Test]
        public void EventNode_AutoAdvances_WithoutContinue()
        {
            _runner.SetGraphData(BuildGraph("ev0",
                GraphBuilder.Event("ev0", "SomeEvent", "", "line1"),
                GraphBuilder.Line("line1", "After event", "end"),
                GraphBuilder.End("end")));

            NarrativeLineData lineAfter = null;
            _runner.OnLine += d => lineAfter = d;

            _runner.StartNarrative(); // event fires and auto-advances to line1

            Assert.IsNotNull(lineAfter);
            Assert.AreEqual("After event", lineAfter.text);
        }

        [Test]
        public void MultipleEvents_AllFire_BeforePausingAtLine()
        {
            _runner.SetGraphData(BuildGraph("ev0",
                GraphBuilder.Event("ev0", "EventA", "", "ev1"),
                GraphBuilder.Event("ev1", "EventB", "", "line0"),
                GraphBuilder.Line("line0", "After events", "end"),
                GraphBuilder.End("end")));

            var firedEvents = new List<string>();
            _runner.OnEvent += d => firedEvents.Add(d.eventName);

            _runner.StartNarrative();

            Assert.AreEqual(2,        firedEvents.Count);
            Assert.AreEqual("EventA", firedEvents[0]);
            Assert.AreEqual("EventB", firedEvents[1]);
        }

        // ─── Revisitable line ─────────────────────────────────────────────────────

        [Test]
        public void RevisitableLine_FirstVisit_UsesFirstVisitText()
        {
            _runner.SetGraphData(BuildGraph("rev0",
                GraphBuilder.Revisitable("rev0", "First time!", "Been here.", "end"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual("First time!", received.text);
        }

        [Test]
        public void RevisitableLine_SecondVisit_UsesRevisitText()
        {
            _runner.SetGraphData(BuildGraph("rev0",
                GraphBuilder.Revisitable("rev0", "First time!", "Been here.", "end"),
                GraphBuilder.End("end")));

            NarrativeLineData lastReceived = null;
            _runner.OnLine += d => lastReceived = d;

            _runner.StartNarrative();
            _runner.Continue();         // advances to EndNode
            _runner.StartNarrative();   // restart — visited nodes persist

            Assert.AreEqual("Been here.", lastReceived.text);
        }

        [Test]
        public void RevisitableLine_PassesSpeakerThrough()
        {
            _runner.SetGraphData(BuildGraph("rev0",
                GraphBuilder.Revisitable("rev0", "Hi", "Hi again", "end", speaker: "Aria"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual("Aria", received.speaker);
        }

        // ─── Visited node tracking ────────────────────────────────────────────────

        [Test]
        public void IsVisited_ReturnsFalse_BeforeVisit()
        {
            Assert.IsFalse(_runner.IsVisited("some-node"));
        }

        [Test]
        public void IsVisited_ReturnsTrue_AfterVisit()
        {
            _runner.SetGraphData(BuildGraph("line0",
                GraphBuilder.Line("line0", "Hi", "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.IsTrue(_runner.IsVisited("line0"));
        }

        [Test]
        public void VisitedNodes_PersistAcrossRestarts()
        {
            _runner.SetGraphData(BuildGraph("line0",
                GraphBuilder.Line("line0", "Hi", "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();
            _runner.Continue();
            _runner.StartNarrative(); // restart

            Assert.IsTrue(_runner.IsVisited("line0")); // still tracked
        }

        [Test]
        public void ResetVisitedNodes_ClearsAllTracking()
        {
            _runner.SetGraphData(BuildGraph("line0",
                GraphBuilder.Line("line0", "Hi", "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();
            Assert.IsTrue(_runner.IsVisited("line0"));

            _runner.ResetVisitedNodes();

            Assert.IsFalse(_runner.IsVisited("line0"));
            Assert.AreEqual(0, _runner.VisitedNodes.Count);
        }

        // ─── Random branch ────────────────────────────────────────────────────────

        [Test]
        public void RandomBranch_AlwaysSelectsOneOfTheValidBranches()
        {
            _runner.SetGraphData(BuildGraph("rand0",
                GraphBuilder.RandomBranch("rand0", "lineA", "lineB"),
                GraphBuilder.Line("lineA", "Branch A", "end"),
                GraphBuilder.Line("lineB", "Branch B", "end"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();

            Assert.IsNotNull(received);
            Assert.That(received.text, Is.EqualTo("Branch A").Or.EqualTo("Branch B"));
        }

        [Test]
        public void RandomBranch_WithNoBranches_EndsNarrative()
        {
            _runner.SetGraphData(BuildGraph("rand0",
                GraphBuilder.RandomBranch("rand0"))); // no branches

            bool ended = false;
            _runner.OnEnd += () => ended = true;

            _runner.StartNarrative();

            Assert.IsTrue(ended);
        }

        // ─── Continue guard while awaiting choice ─────────────────────────────────

        [Test]
        public void Continue_WhileAwaitingChoice_IsIgnored()
        {
            _runner.SetGraphData(BuildGraph("choice0",
                GraphBuilder.Choice("choice0",
                    GraphBuilder.Option("Yes", "lineA"),
                    GraphBuilder.Option("No",  "lineB")),
                GraphBuilder.Line("lineA", "Chose yes", "end"),
                GraphBuilder.Line("lineB", "Chose no",  "end"),
                GraphBuilder.End("end")));

            NarrativeLineData lineFired = null;
            _runner.OnLine += d => lineFired = d;

            _runner.StartNarrative(); // pauses at choice
            _runner.Continue();       // should be ignored

            Assert.IsNull(lineFired);
            Assert.IsTrue(_runner.IsRunning);
        }
    }
}
