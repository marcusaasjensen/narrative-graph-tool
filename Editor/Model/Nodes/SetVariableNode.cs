using System;
using NarrativeGraphTool.Runtime.Data;
using Unity.GraphToolkit.Editor;

namespace NarrativeGraphTool.Editor.Model.Nodes
{
    // ─────────────────────────────────────────────────────────────────────────
    // Bool
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sets or toggles a boolean variable, then continues flow.
    /// Use <c>Toggle</c> to flip the current value without specifying one.
    /// </summary>
    [Serializable]
    public class SetVariableBoolNode : NarrativeNodeBase
    {
        public const string OptionVariableId = "VariableId";
        public const string OptionOperator   = "Operator";
        public const string PortValue        = "Value";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(OptionVariableId)
                .WithDisplayName("Variable ID")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Key passed to VariableSetter to identify which variable to write.")
                .Delayed();

            context.AddOption<BoolSetOperator>(OptionOperator)
                .WithDisplayName("Operator")
                .WithDefaultValue(BoolSetOperator.Set);
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddFlowPorts(context);

            context.AddInputPort<bool>(PortValue)
                .WithDisplayName("Value")
                .WithDefaultValue(false)
                .Build();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Int
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Applies an arithmetic operation to an integer variable, then continues flow.
    /// </summary>
    [Serializable]
    public class SetVariableIntNode : NarrativeNodeBase
    {
        public const string OptionVariableId = "VariableId";
        public const string OptionOperator   = "Operator";
        public const string PortValue        = "Value";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(OptionVariableId)
                .WithDisplayName("Variable ID")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Key passed to VariableSetter to identify which variable to write.")
                .Delayed();

            context.AddOption<NumericSetOperator>(OptionOperator)
                .WithDisplayName("Operator")
                .WithDefaultValue(NumericSetOperator.Set);
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddFlowPorts(context);

            context.AddInputPort<int>(PortValue)
                .WithDisplayName("Value")
                .WithDefaultValue(0)
                .Build();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Float
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Applies an arithmetic operation to a float variable, then continues flow.
    /// </summary>
    [Serializable]
    public class SetVariableFloatNode : NarrativeNodeBase
    {
        public const string OptionVariableId = "VariableId";
        public const string OptionOperator   = "Operator";
        public const string PortValue        = "Value";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(OptionVariableId)
                .WithDisplayName("Variable ID")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Key passed to VariableSetter to identify which variable to write.")
                .Delayed();

            context.AddOption<NumericSetOperator>(OptionOperator)
                .WithDisplayName("Operator")
                .WithDefaultValue(NumericSetOperator.Set);
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddFlowPorts(context);

            context.AddInputPort<float>(PortValue)
                .WithDisplayName("Value")
                .WithDefaultValue(0f)
                .Build();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // String
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sets or appends to a string variable, then continues flow.
    /// </summary>
    [Serializable]
    public class SetVariableStringNode : NarrativeNodeBase
    {
        public const string OptionVariableId = "VariableId";
        public const string OptionOperator   = "Operator";
        public const string OptionValue      = "Value";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(OptionVariableId)
                .WithDisplayName("Variable ID")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Key passed to VariableSetter to identify which variable to write.")
                .Delayed();

            context.AddOption<StringSetOperator>(OptionOperator)
                .WithDisplayName("Operator")
                .WithDefaultValue(StringSetOperator.Set);

            context.AddOption<string>(OptionValue)
                .WithDisplayName("Value")
                .WithDefaultValue(string.Empty)
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddFlowPorts(context);
        }
    }
}
