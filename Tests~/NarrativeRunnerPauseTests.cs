using System.Collections.Generic;
using NarrativeGraphTool;
using NarrativeGraphTool.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NarrativeGraphTool.Tests
{
    /// <summary>
    /// Tests for PauseNode and blocking EventNode behaviour.
    /// </summary>
    [TestFixture]
    public class NarrativeRunnerPauseTests
    {
        GameObject      _go;
        NarrativeRunner _runner;
        List<Object>    _toDestroy;

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

        // ─── PauseNode ────────────────────────────────────────────────────────────

        [Test]
        public void PauseNode_StopsRunner_WithoutFiringOnEnd()
        {
            _runner.SetGraphData(BuildGraph("pause",
                GraphBuilder.Pause("pause", "end"),
                GraphBuilder.End("end")));

            bool ended = false;
            _runner.OnEnd   += () => ended = true;
            _runner.OnPause += () => { };

            _runner.StartNarrative();

            Assert.IsFalse(_runner.IsRunning);
            Assert.IsFalse(ended);
        }

        [Test]
        public void PauseNode_FiresOnPause()
        {
            _runner.SetGraphData(BuildGraph("pause",
                GraphBuilder.Pause("pause", "end"),
                GraphBuilder.End("end")));

            bool paused = false;
            _runner.OnPause += () => paused = true;

            _runner.StartNarrative();

            Assert.IsTrue(paused);
        }

        [Test]
        public void PauseNode_StoresResumeNodeId()
        {
            _runner.SetGraphData(BuildGraph("pause",
                GraphBuilder.Pause("pause", "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.AreEqual("end", _runner.ResumeNodeId);
        }

        [Test]
        public void Resume_AfterPause_ContinuesFromNextNode()
        {
            _runner.SetGraphData(BuildGraph("pause",
                GraphBuilder.Pause("pause", "line"),
                GraphBuilder.Line("line", "Hello", "end"),
                GraphBuilder.End("end")));

            string receivedText = null;
            _runner.OnLine += line => receivedText = line.text;

            _runner.StartNarrative();
            _runner.Resume();

            Assert.AreEqual("Hello", receivedText);
        }

        [Test]
        public void Resume_WithNoStoredPausePoint_LogsWarning()
        {
            _runner.SetGraphData(BuildGraph("end",
                GraphBuilder.End("end")));

            LogAssert.Expect(LogType.Warning, "[NarrativeRunner] Resume() called but no pause point is stored.");
            _runner.Resume();
        }

        [Test]
        public void PauseNode_ResumeNodeIdClearedAfterEnd()
        {
            _runner.SetGraphData(BuildGraph("pause",
                GraphBuilder.Pause("pause", "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative(); // hits PauseNode → stores ResumeNodeId
            _runner.Resume();         // hits EndNode → Finish() clears ResumeNodeId

            Assert.IsNull(_runner.ResumeNodeId);
        }

        [Test]
        public void PauseNode_BeforeAndAfterLine_FlowsCorrectly()
        {
            _runner.SetGraphData(BuildGraph("line1",
                GraphBuilder.Line("line1", "Before", "pause"),
                GraphBuilder.Pause("pause", "line2"),
                GraphBuilder.Line("line2", "After", "end"),
                GraphBuilder.End("end")));

            var received = new List<string>();
            _runner.OnLine += line => received.Add(line.text);

            _runner.StartNarrative();           // fires "Before", waits
            _runner.Continue();                 // advances to PauseNode, pauses
            _runner.Resume();                   // fires "After", waits
            _runner.Continue();                 // advances to End

            Assert.AreEqual(new List<string> { "Before", "After" }, received);
        }

        // ─── Blocking EventNode ───────────────────────────────────────────────────

        [Test]
        public void BlockingEvent_StopsRunnerAfterFiringEvent()
        {
            _runner.SetGraphData(BuildGraph("ev",
                GraphBuilder.BlockingEvent("ev", "PlayAnim", "end"),
                GraphBuilder.End("end")));

            bool eventFired = false;
            _runner.OnEvent += ev => eventFired = true;

            _runner.StartNarrative();

            Assert.IsTrue(eventFired);
            Assert.IsFalse(_runner.IsRunning);
        }

        [Test]
        public void BlockingEvent_DoesNotFireOnEnd()
        {
            _runner.SetGraphData(BuildGraph("ev",
                GraphBuilder.BlockingEvent("ev", "PlayAnim", "end"),
                GraphBuilder.End("end")));

            bool ended = false;
            _runner.OnEnd += () => ended = true;

            _runner.StartNarrative();

            Assert.IsFalse(ended);
        }

        [Test]
        public void BlockingEvent_StoresResumeNodeId()
        {
            _runner.SetGraphData(BuildGraph("ev",
                GraphBuilder.BlockingEvent("ev", "PlayAnim", "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.AreEqual("end", _runner.ResumeNodeId);
        }

        [Test]
        public void BlockingEvent_Resume_ContinuesToNextNode()
        {
            _runner.SetGraphData(BuildGraph("ev",
                GraphBuilder.BlockingEvent("ev", "PlayAnim", "line"),
                GraphBuilder.Line("line", "Done", "end"),
                GraphBuilder.End("end")));

            string receivedText = null;
            _runner.OnLine += line => receivedText = line.text;

            _runner.StartNarrative();
            _runner.Resume();

            Assert.AreEqual("Done", receivedText);
        }

        [Test]
        public void NonBlockingEvent_AutoAdvancesWithoutResume()
        {
            _runner.SetGraphData(BuildGraph("ev",
                GraphBuilder.Event("ev", "PlaySound", "", "line"),
                GraphBuilder.Line("line", "Hello", "end"),
                GraphBuilder.End("end")));

            string receivedText = null;
            _runner.OnLine += line => receivedText = line.text;

            _runner.StartNarrative();

            // Non-blocking: runner advanced past event and paused at line — no Resume() needed
            Assert.AreEqual("Hello", receivedText);
            Assert.IsTrue(_runner.IsRunning);
        }

        [Test]
        public void BlockingEvent_PassesCorrectEventData()
        {
            _runner.SetGraphData(BuildGraph("ev",
                GraphBuilder.BlockingEvent("ev", "Cutscene", "end", "intro"),
                GraphBuilder.End("end")));

            EventNodeData received = null;
            _runner.OnEvent += ev => received = ev;

            _runner.StartNarrative();

            Assert.IsNotNull(received);
            Assert.AreEqual("Cutscene", received.eventName);
            Assert.AreEqual("intro",    received.payload);
            Assert.IsTrue(received.waitForResume);
        }

        [Test]
        public void BlockingEvent_BetweenLines_FlowsCorrectly()
        {
            _runner.SetGraphData(BuildGraph("line1",
                GraphBuilder.Line("line1", "Before", "ev"),
                GraphBuilder.BlockingEvent("ev", "PlayAnim", "line2"),
                GraphBuilder.Line("line2", "After", "end"),
                GraphBuilder.End("end")));

            var received = new List<string>();
            _runner.OnLine += line => received.Add(line.text);

            _runner.StartNarrative();  // fires "Before", waits
            _runner.Continue();        // advances to blocking event, stops
            _runner.Resume();          // fires "After", waits
            _runner.Continue();        // advances to End

            Assert.AreEqual(new List<string> { "Before", "After" }, received);
        }
    }
}
