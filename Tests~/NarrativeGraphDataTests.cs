using NarrativeGraphTool.Runtime.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NarrativeGraphTool.Tests
{
    [TestFixture]
    public class NarrativeGraphDataTests
    {
        NarrativeGraphData _data;

        [SetUp]
        public void SetUp()
        {
            LogAssert.ignoreFailingMessages = true;
            _data = ScriptableObject.CreateInstance<NarrativeGraphData>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_data);
        }

        // ─── GetNode ──────────────────────────────────────────────────────────────

        [Test]
        public void GetNode_ReturnsCorrectNode()
        {
            var line = new NarrativeLineData { id = "node1", text = "Hello" };
            _data.nodes.Add(line);

            var result = _data.GetNode("node1");

            Assert.AreSame(line, result);
        }

        [Test]
        public void GetNode_ReturnsNull_WhenIdNotFound()
        {
            var result = _data.GetNode("nonexistent");

            Assert.IsNull(result);
        }

        [Test]
        public void GetNode_ReturnsNull_ForNullId()
        {
            var result = _data.GetNode(null);

            Assert.IsNull(result);
        }

        [Test]
        public void GetNode_ReturnsNull_ForEmptyId()
        {
            var result = _data.GetNode(string.Empty);

            Assert.IsNull(result);
        }

        [Test]
        public void GetNode_BuildsLookupAcrossMultipleNodes()
        {
            _data.nodes.Add(new NarrativeLineData { id = "a" });
            _data.nodes.Add(new NarrativeLineData { id = "b" });
            _data.nodes.Add(new EndNodeData       { id = "c" });

            Assert.AreEqual("a", _data.GetNode("a").id);
            Assert.AreEqual("b", _data.GetNode("b").id);
            Assert.AreEqual("c", _data.GetNode("c").id);
        }

        [Test]
        public void GetNode_IgnoresNullNodeEntries()
        {
            _data.nodes.Add(null);
            _data.nodes.Add(new NarrativeLineData { id = "valid" });

            // Should not throw and should still find the valid node
            Assert.DoesNotThrow(() => _data.GetNode("valid"));
            Assert.IsNotNull(_data.GetNode("valid"));
        }

        // ─── GetTargetByLabel ─────────────────────────────────────────────────────

        [Test]
        public void GetTargetByLabel_ReturnsMatchingTarget()
        {
            var target = new TargetNodeData { id = "t0", labelName = "hub", nextId = "line0" };
            _data.nodes.Add(target);

            var result = _data.GetTargetByLabel("hub");

            Assert.AreSame(target, result);
        }

        [Test]
        public void GetTargetByLabel_ReturnsNull_WhenNotFound()
        {
            var result = _data.GetTargetByLabel("missing");

            Assert.IsNull(result);
        }

        [Test]
        public void GetTargetByLabel_ReturnsNull_ForNullLabel()
        {
            _data.nodes.Add(new TargetNodeData { id = "t0", labelName = "hub" });

            var result = _data.GetTargetByLabel(null);

            Assert.IsNull(result);
        }

        [Test]
        public void GetTargetByLabel_ReturnsFirstMatch_WhenMultipleExist()
        {
            var first  = new TargetNodeData { id = "t0", labelName = "hub" };
            var second = new TargetNodeData { id = "t1", labelName = "hub" };
            _data.nodes.Add(first);
            _data.nodes.Add(second);

            var result = _data.GetTargetByLabel("hub");

            Assert.AreSame(first, result);
        }

        [Test]
        public void GetTargetByLabel_IsCaseSensitive()
        {
            _data.nodes.Add(new TargetNodeData { id = "t0", labelName = "Hub" });

            Assert.IsNull(_data.GetTargetByLabel("hub"));
            Assert.IsNotNull(_data.GetTargetByLabel("Hub"));
        }
    }
}
