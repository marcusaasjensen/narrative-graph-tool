using System.Collections.Generic;
using NarrativeGraphTool.Runtime;
using NarrativeGraphTool.Runtime.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NarrativeGraphTool.Tests
{
    /// <summary>
    /// Tests for NarrativeRunner covering SetVariable nodes (all types and operators)
    /// and ConditionalNode routing (bool, int, float, string).
    /// </summary>
    [TestFixture]
    public class NarrativeRunnerVariableTests
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

        // ─── SetVariable: Bool ────────────────────────────────────────────────────

        [Test]
        public void SetBool_Set_WritesTrue()
        {
            _runner.SetGraphData(BuildGraph("sv0",
                GraphBuilder.SetBool("sv0", "myFlag", true, BoolSetOperator.Set, "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.AreEqual(true, _variables["myFlag"]);
        }

        [Test]
        public void SetBool_Set_WritesFalse()
        {
            _variables["myFlag"] = true;

            _runner.SetGraphData(BuildGraph("sv0",
                GraphBuilder.SetBool("sv0", "myFlag", false, BoolSetOperator.Set, "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.AreEqual(false, _variables["myFlag"]);
        }

        [Test]
        public void SetBool_Toggle_FlipsFromTrue_ToFalse()
        {
            _variables["myFlag"] = true;

            _runner.SetGraphData(BuildGraph("sv0",
                GraphBuilder.SetBool("sv0", "myFlag", false, BoolSetOperator.Toggle, "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.AreEqual(false, _variables["myFlag"]);
        }

        [Test]
        public void SetBool_Toggle_FlipsFromFalse_ToTrue()
        {
            _variables["myFlag"] = false;

            _runner.SetGraphData(BuildGraph("sv0",
                GraphBuilder.SetBool("sv0", "myFlag", true, BoolSetOperator.Toggle, "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.AreEqual(true, _variables["myFlag"]);
        }

        // ─── SetVariable: Int ─────────────────────────────────────────────────────

        [Test]
        public void SetInt_Set_WritesValue()
        {
            _runner.SetGraphData(BuildGraph("sv0",
                GraphBuilder.SetInt("sv0", "score", 42, NumericSetOperator.Set, "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.AreEqual(42, _variables["score"]);
        }

        [Test]
        public void SetInt_Add_IncrementsCurrentValue()
        {
            _variables["score"] = 10;

            _runner.SetGraphData(BuildGraph("sv0",
                GraphBuilder.SetInt("sv0", "score", 5, NumericSetOperator.Add, "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.AreEqual(15, _variables["score"]);
        }

        [Test]
        public void SetInt_Subtract_DecrementsCurrentValue()
        {
            _variables["score"] = 10;

            _runner.SetGraphData(BuildGraph("sv0",
                GraphBuilder.SetInt("sv0", "score", 3, NumericSetOperator.Subtract, "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.AreEqual(7, _variables["score"]);
        }

        [Test]
        public void SetInt_Multiply_MultipliesCurrentValue()
        {
            _variables["score"] = 4;

            _runner.SetGraphData(BuildGraph("sv0",
                GraphBuilder.SetInt("sv0", "score", 3, NumericSetOperator.Multiply, "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.AreEqual(12, _variables["score"]);
        }

        [Test]
        public void SetInt_Add_WithNoExistingValue_TreatsCurrentAsZero()
        {
            // No initial value set — should default to 0 + 5 = 5
            _runner.SetGraphData(BuildGraph("sv0",
                GraphBuilder.SetInt("sv0", "score", 5, NumericSetOperator.Add, "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.AreEqual(5, _variables["score"]);
        }

        // ─── SetVariable: Float ───────────────────────────────────────────────────

        [Test]
        public void SetFloat_Set_WritesValue()
        {
            _runner.SetGraphData(BuildGraph("sv0",
                GraphBuilder.SetFloat("sv0", "volume", 0.75f, NumericSetOperator.Set, "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.AreEqual(0.75f, (float)_variables["volume"], 0.0001f);
        }

        [Test]
        public void SetFloat_Add_IncrementsCurrentValue()
        {
            _variables["volume"] = 0.5f;

            _runner.SetGraphData(BuildGraph("sv0",
                GraphBuilder.SetFloat("sv0", "volume", 0.2f, NumericSetOperator.Add, "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.AreEqual(0.7f, (float)_variables["volume"], 0.0001f);
        }

        [Test]
        public void SetFloat_Subtract_DecrementsCurrentValue()
        {
            _variables["health"] = 1.0f;

            _runner.SetGraphData(BuildGraph("sv0",
                GraphBuilder.SetFloat("sv0", "health", 0.3f, NumericSetOperator.Subtract, "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.AreEqual(0.7f, (float)_variables["health"], 0.0001f);
        }

        [Test]
        public void SetFloat_Multiply_MultipliesCurrentValue()
        {
            _variables["speed"] = 2.0f;

            _runner.SetGraphData(BuildGraph("sv0",
                GraphBuilder.SetFloat("sv0", "speed", 1.5f, NumericSetOperator.Multiply, "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.AreEqual(3.0f, (float)_variables["speed"], 0.0001f);
        }

        // ─── SetVariable: String ──────────────────────────────────────────────────

        [Test]
        public void SetString_Set_WritesValue()
        {
            _runner.SetGraphData(BuildGraph("sv0",
                GraphBuilder.SetString("sv0", "playerName", "Aria", StringSetOperator.Set, "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.AreEqual("Aria", _variables["playerName"]);
        }

        [Test]
        public void SetString_Append_AppendsToCurrentValue()
        {
            _variables["log"] = "Entry1";

            _runner.SetGraphData(BuildGraph("sv0",
                GraphBuilder.SetString("sv0", "log", ",Entry2", StringSetOperator.Append, "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.AreEqual("Entry1,Entry2", _variables["log"]);
        }

        [Test]
        public void SetString_Append_WithNoExistingValue_TreatsCurrentAsEmpty()
        {
            _runner.SetGraphData(BuildGraph("sv0",
                GraphBuilder.SetString("sv0", "log", "First", StringSetOperator.Append, "end"),
                GraphBuilder.End("end")));

            _runner.StartNarrative();

            Assert.AreEqual("First", _variables["log"]);
        }

        // ─── SetVariable: flow continues ─────────────────────────────────────────

        [Test]
        public void SetVariable_AutoAdvances_ToNextNode()
        {
            _runner.SetGraphData(BuildGraph("sv0",
                GraphBuilder.SetInt("sv0", "x", 1, NumericSetOperator.Set, "line1"),
                GraphBuilder.Line("line1", "After set", "end"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();

            Assert.IsNotNull(received);
            Assert.AreEqual("After set", received.text);
        }

        [Test]
        public void SetVariable_WithNoSetter_StillContinuesFlow()
        {
            _runner.VariableSetter = null;

            _runner.SetGraphData(BuildGraph("sv0",
                GraphBuilder.SetInt("sv0", "x", 1, NumericSetOperator.Set, "line1"),
                GraphBuilder.Line("line1", "After set", "end"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();

            // Flow continues even without a setter (the set is just a no-op)
            Assert.IsNotNull(received);
            Assert.AreEqual("After set", received.text);
        }

        // ─── Conditional: Bool ────────────────────────────────────────────────────

        [Test]
        public void ConditionalBool_RoutesToTrueBranch_WhenTrue()
        {
            _variables["hasKey"] = true;

            _runner.SetGraphData(BuildGraph("cond0",
                GraphBuilder.ConditionalBool("cond0", "hasKey", true,
                    NarrativeConditionOperator.EqualTo, "lineTrue", "lineFalse"),
                GraphBuilder.Line("lineTrue",  "True branch",  "end"),
                GraphBuilder.Line("lineFalse", "False branch", "end"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual("True branch", received.text);
        }

        [Test]
        public void ConditionalBool_RoutesToFalseBranch_WhenFalse()
        {
            _variables["hasKey"] = false;

            _runner.SetGraphData(BuildGraph("cond0",
                GraphBuilder.ConditionalBool("cond0", "hasKey", true,
                    NarrativeConditionOperator.EqualTo, "lineTrue", "lineFalse"),
                GraphBuilder.Line("lineTrue",  "True branch",  "end"),
                GraphBuilder.Line("lineFalse", "False branch", "end"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual("False branch", received.text);
        }

        [Test]
        public void ConditionalBool_NotEqualTo_RoutesCorrectly()
        {
            _variables["defeated"] = true;

            _runner.SetGraphData(BuildGraph("cond0",
                GraphBuilder.ConditionalBool("cond0", "defeated", true,
                    NarrativeConditionOperator.NotEqualTo, "lineTrue", "lineFalse"),
                GraphBuilder.Line("lineTrue",  "Not defeated", "end"),
                GraphBuilder.Line("lineFalse", "Is defeated",  "end"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();

            // defeated == true, op is NotEqualTo true → false → lineFalse
            Assert.AreEqual("Is defeated", received.text);
        }

        // ─── Conditional: Int (parametrised) ─────────────────────────────────────

        [Test]
        [TestCase(NarrativeConditionOperator.EqualTo,          10, 10, true)]
        [TestCase(NarrativeConditionOperator.EqualTo,          10,  5, false)]
        [TestCase(NarrativeConditionOperator.NotEqualTo,       10,  5, true)]
        [TestCase(NarrativeConditionOperator.NotEqualTo,       10, 10, false)]
        [TestCase(NarrativeConditionOperator.LessThan,          5, 10, true)]
        [TestCase(NarrativeConditionOperator.LessThan,         10,  5, false)]
        [TestCase(NarrativeConditionOperator.GreaterThan,      10,  5, true)]
        [TestCase(NarrativeConditionOperator.GreaterThan,       5, 10, false)]
        [TestCase(NarrativeConditionOperator.LessOrEqualTo,    10, 10, true)]
        [TestCase(NarrativeConditionOperator.LessOrEqualTo,    11, 10, false)]
        [TestCase(NarrativeConditionOperator.GreaterOrEqualTo, 10, 10, true)]
        [TestCase(NarrativeConditionOperator.GreaterOrEqualTo,  9, 10, false)]
        public void ConditionalInt_AllOperators_RouteCorrectly(
            NarrativeConditionOperator op, int variableValue, int compareValue, bool expectTrue)
        {
            _variables["x"] = variableValue;

            _runner.SetGraphData(BuildGraph("cond0",
                GraphBuilder.ConditionalInt("cond0", "x", compareValue, op, "lineTrue", "lineFalse"),
                GraphBuilder.Line("lineTrue",  "True",  "end"),
                GraphBuilder.Line("lineFalse", "False", "end"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual(expectTrue ? "True" : "False", received.text);
        }

        // ─── Conditional: Float ───────────────────────────────────────────────────

        [Test]
        public void ConditionalFloat_GreaterThan_RoutesToTrueBranch()
        {
            _variables["health"] = 0.8f;

            _runner.SetGraphData(BuildGraph("cond0",
                GraphBuilder.ConditionalFloat("cond0", "health", 0.5f,
                    NarrativeConditionOperator.GreaterThan, "lineTrue", "lineFalse"),
                GraphBuilder.Line("lineTrue",  "Healthy",  "end"),
                GraphBuilder.Line("lineFalse", "Low HP",   "end"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual("Healthy", received.text);
        }

        [Test]
        public void ConditionalFloat_LessOrEqualTo_RoutesToFalseBranch_WhenGreater()
        {
            _variables["health"] = 0.9f;

            _runner.SetGraphData(BuildGraph("cond0",
                GraphBuilder.ConditionalFloat("cond0", "health", 0.5f,
                    NarrativeConditionOperator.LessOrEqualTo, "lineTrue", "lineFalse"),
                GraphBuilder.Line("lineTrue",  "Low HP",  "end"),
                GraphBuilder.Line("lineFalse", "Healthy", "end"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual("Healthy", received.text);
        }

        // ─── Conditional: String ──────────────────────────────────────────────────

        [Test]
        public void ConditionalString_EqualTo_RoutesToTrueBranch_WhenMatch()
        {
            _variables["faction"] = "empire";

            _runner.SetGraphData(BuildGraph("cond0",
                GraphBuilder.ConditionalString("cond0", "faction", "empire",
                    NarrativeConditionOperator.EqualTo, "lineTrue", "lineFalse"),
                GraphBuilder.Line("lineTrue",  "Welcome, soldier",  "end"),
                GraphBuilder.Line("lineFalse", "You are not one of us", "end"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual("Welcome, soldier", received.text);
        }

        [Test]
        public void ConditionalString_EqualTo_RoutesToFalseBranch_WhenNoMatch()
        {
            _variables["faction"] = "rebels";

            _runner.SetGraphData(BuildGraph("cond0",
                GraphBuilder.ConditionalString("cond0", "faction", "empire",
                    NarrativeConditionOperator.EqualTo, "lineTrue", "lineFalse"),
                GraphBuilder.Line("lineTrue",  "Welcome, soldier",     "end"),
                GraphBuilder.Line("lineFalse", "You are not one of us", "end"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual("You are not one of us", received.text);
        }

        [Test]
        public void ConditionalString_Contains_RoutesToTrueBranch()
        {
            _variables["tags"] = "warrior,mage,thief";

            _runner.SetGraphData(BuildGraph("cond0",
                GraphBuilder.ConditionalString("cond0", "tags", "mage",
                    NarrativeConditionOperator.Contains, "lineTrue", "lineFalse"),
                GraphBuilder.Line("lineTrue",  "Mage detected",    "end"),
                GraphBuilder.Line("lineFalse", "No mage here",     "end"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual("Mage detected", received.text);
        }

        [Test]
        public void ConditionalString_NotEqualTo_RoutesCorrectly()
        {
            _variables["mood"] = "happy";

            _runner.SetGraphData(BuildGraph("cond0",
                GraphBuilder.ConditionalString("cond0", "mood", "sad",
                    NarrativeConditionOperator.NotEqualTo, "lineTrue", "lineFalse"),
                GraphBuilder.Line("lineTrue",  "Not sad",  "end"),
                GraphBuilder.Line("lineFalse", "Is sad",   "end"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual("Not sad", received.text);
        }

        // ─── Conditional: no VariableProvider ────────────────────────────────────

        [Test]
        public void Conditional_WithNoVariableProvider_DefaultsToFalseBranch()
        {
            _runner.VariableProvider = null;

            _runner.SetGraphData(BuildGraph("cond0",
                GraphBuilder.ConditionalBool("cond0", "someVar", true,
                    NarrativeConditionOperator.EqualTo, "lineTrue", "lineFalse"),
                GraphBuilder.Line("lineTrue",  "True",  "end"),
                GraphBuilder.Line("lineFalse", "False", "end"),
                GraphBuilder.End("end")));

            NarrativeLineData received = null;
            _runner.OnLine += d => received = d;

            _runner.StartNarrative();

            Assert.AreEqual("False", received.text);
        }
    }
}
