using System;
using Unity.GraphToolkit.Editor;

namespace NarrativeGraphTool.Editor.Model.Nodes
{
    // ─────────────────────────────────────────────────────────────────────────
    // Enums — one per meaningful comparison set.
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Comparison operators for integer and float condition nodes.</summary>
    public enum NumericConditionalOperator
    {
        EqualTo,
        NotEqualTo,
        LessThan,
        GreaterThan,
        LessOrEqualTo,
        GreaterOrEqualTo,
    }

    /// <summary>Comparison operators for string condition nodes.</summary>
    public enum StringConditionalOperator
    {
        EqualTo,
        NotEqualTo,
        Contains,
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Bool — one input, no comparison.
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Routes flow to <c>True</c> or <c>False</c> based on a boolean variable.
    /// No operator or comparison value — the value itself decides the branch.
    /// </summary>
    [Serializable]
    public class ConditionalBooleanNode : NarrativeNodeBase
    {
        public const string PortValue = "Value";
        public const string PortTrue  = "True";
        public const string PortFalse = "False";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort(ExecutionPortName)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();

            context.AddInputPort<bool>(PortValue)
                .WithDisplayName("Value")
                .WithDefaultValue(false)
                .Build();

            context.AddOutputPort(PortTrue)
                .WithDisplayName("True")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();

            context.AddOutputPort(PortFalse)
                .WithDisplayName("False")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Int — numeric operators only.
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Compares two integers with a numeric operator and routes flow accordingly.
    /// Leave <c>Compare</c> unconnected to edit its value inline.
    /// </summary>
    [Serializable]
    public class ConditionalIntegerNode : NarrativeNodeBase
    {
        public const string PortVariable   = "Variable";
        public const string PortCompare    = "Compare";
        public const string PortTrue       = "True";
        public const string PortFalse      = "False";
        public const string OptionOperator = "Operator";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<NumericConditionalOperator>(OptionOperator)
                .WithDisplayName("Operator")
                .WithDefaultValue(NumericConditionalOperator.EqualTo);
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort(ExecutionPortName)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();

            context.AddInputPort<int>(PortVariable)
                .WithDisplayName("Variable")
                .WithDefaultValue(0)
                .Build();

            context.AddInputPort<int>(PortCompare)
                .WithDisplayName("Compare")
                .WithDefaultValue(0)
                .Build();

            context.AddOutputPort(PortTrue)
                .WithDisplayName("True")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();

            context.AddOutputPort(PortFalse)
                .WithDisplayName("False")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Float — numeric operators only.
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Compares two floats with a numeric operator and routes flow accordingly.
    /// Leave <c>Compare</c> unconnected to edit its value inline.
    /// </summary>
    [Serializable]
    public class ConditionalFloatNode : NarrativeNodeBase
    {
        public const string PortVariable   = "Variable";
        public const string PortCompare    = "Compare";
        public const string PortTrue       = "True";
        public const string PortFalse      = "False";
        public const string OptionOperator = "Operator";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<NumericConditionalOperator>(OptionOperator)
                .WithDisplayName("Operator")
                .WithDefaultValue(NumericConditionalOperator.EqualTo);
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort(ExecutionPortName)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();

            context.AddInputPort<float>(PortVariable)
                .WithDisplayName("Variable")
                .WithDefaultValue(0f)
                .Build();

            context.AddInputPort<float>(PortCompare)
                .WithDisplayName("Compare")
                .WithDefaultValue(0f)
                .Build();

            context.AddOutputPort(PortTrue)
                .WithDisplayName("True")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();

            context.AddOutputPort(PortFalse)
                .WithDisplayName("False")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // String — EqualTo, NotEqualTo, Contains only.
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Compares two strings with a string-appropriate operator and routes flow accordingly.
    /// Leave <c>Compare</c> unconnected to edit its value inline.
    /// </summary>
    [Serializable]
    public class ConditionalStringNode : NarrativeNodeBase
    {
        public const string PortVariable   = "Variable";
        public const string PortCompare    = "Compare";
        public const string PortTrue       = "True";
        public const string PortFalse      = "False";
        public const string OptionOperator = "Operator";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<StringConditionalOperator>(OptionOperator)
                .WithDisplayName("Operator")
                .WithDefaultValue(StringConditionalOperator.EqualTo);
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort(ExecutionPortName)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();

            context.AddInputPort<string>(PortVariable)
                .WithDisplayName("Variable")
                .WithDefaultValue(string.Empty)
                .Build();

            context.AddInputPort<string>(PortCompare)
                .WithDisplayName("Compare")
                .WithDefaultValue(string.Empty)
                .Build();

            context.AddOutputPort(PortTrue)
                .WithDisplayName("True")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();

            context.AddOutputPort(PortFalse)
                .WithDisplayName("False")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }
}
