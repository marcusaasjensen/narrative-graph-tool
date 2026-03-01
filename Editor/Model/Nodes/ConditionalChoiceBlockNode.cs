using System;
using Unity.GraphToolkit.Editor;

namespace NarrativeGraphTool.Editor.Model.Nodes
{
    // ─────────────────────────────────────────────────────────────────────────
    // Bool
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// A choice shown only when a boolean variable equals the specified value.
    /// Mix freely with regular <see cref="ChoiceBlockNode"/>s inside a <see cref="ChoiceContextNode"/>.
    /// </summary>
    [UseWithContext(typeof(ChoiceContextNode))]
    [Serializable]
    public class ConditionalBoolChoiceBlock : BlockNode
    {
        public const string OptionChoiceText = "Choice";
        public const string PortOutput       = "Out";
        public const string OptionVariableId = "VariableId";
        public const string PortValue        = "Value";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(OptionChoiceText)
                .WithDisplayName("Choice")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Label shown to the player when the condition is met.")
                .Delayed();

            context.AddOption<string>(OptionVariableId)
                .WithDisplayName("Variable ID")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Key passed to VariableProvider to retrieve the runtime value.")
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<bool>(PortValue)
                .WithDisplayName("Value")
                .WithDefaultValue(false)
                .Build();

            context.AddOutputPort(PortOutput)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Int
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// A choice shown only when an integer variable satisfies the given comparison.
    /// Mix freely with regular <see cref="ChoiceBlockNode"/>s inside a <see cref="ChoiceContextNode"/>.
    /// </summary>
    [UseWithContext(typeof(ChoiceContextNode))]
    [Serializable]
    public class ConditionalIntChoiceBlock : BlockNode
    {
        public const string OptionChoiceText = "Choice";
        public const string PortOutput       = "Out";
        public const string OptionVariableId = "VariableId";
        public const string OptionOperator   = "Operator";
        public const string PortCompare      = "Compare";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(OptionChoiceText)
                .WithDisplayName("Choice")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Label shown to the player when the condition is met.")
                .Delayed();

            context.AddOption<string>(OptionVariableId)
                .WithDisplayName("Variable ID")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Key passed to VariableProvider to retrieve the runtime value.")
                .Delayed();

            context.AddOption<NumericConditionalOperator>(OptionOperator)
                .WithDisplayName("Operator")
                .WithDefaultValue(NumericConditionalOperator.EqualTo);
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<int>(PortCompare)
                .WithDisplayName("Compare")
                .WithDefaultValue(0)
                .Build();

            context.AddOutputPort(PortOutput)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Float
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// A choice shown only when a float variable satisfies the given comparison.
    /// Mix freely with regular <see cref="ChoiceBlockNode"/>s inside a <see cref="ChoiceContextNode"/>.
    /// </summary>
    [UseWithContext(typeof(ChoiceContextNode))]
    [Serializable]
    public class ConditionalFloatChoiceBlock : BlockNode
    {
        public const string OptionChoiceText = "Choice";
        public const string PortOutput       = "Out";
        public const string OptionVariableId = "VariableId";
        public const string OptionOperator   = "Operator";
        public const string PortCompare      = "Compare";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(OptionChoiceText)
                .WithDisplayName("Choice")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Label shown to the player when the condition is met.")
                .Delayed();

            context.AddOption<string>(OptionVariableId)
                .WithDisplayName("Variable ID")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Key passed to VariableProvider to retrieve the runtime value.")
                .Delayed();

            context.AddOption<NumericConditionalOperator>(OptionOperator)
                .WithDisplayName("Operator")
                .WithDefaultValue(NumericConditionalOperator.EqualTo);
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<float>(PortCompare)
                .WithDisplayName("Compare")
                .WithDefaultValue(0f)
                .Build();

            context.AddOutputPort(PortOutput)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // String
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// A choice shown only when a string variable satisfies the given comparison.
    /// Mix freely with regular <see cref="ChoiceBlockNode"/>s inside a <see cref="ChoiceContextNode"/>.
    /// </summary>
    [UseWithContext(typeof(ChoiceContextNode))]
    [Serializable]
    public class ConditionalStringChoiceBlock : BlockNode
    {
        public const string OptionChoiceText = "Choice";
        public const string PortOutput       = "Out";
        public const string OptionVariableId = "VariableId";
        public const string OptionOperator   = "Operator";
        public const string OptionCompare    = "Compare";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(OptionChoiceText)
                .WithDisplayName("Choice")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Label shown to the player when the condition is met.")
                .Delayed();

            context.AddOption<string>(OptionVariableId)
                .WithDisplayName("Variable ID")
                .WithDefaultValue(string.Empty)
                .WithTooltip("Key passed to VariableProvider to retrieve the runtime value.")
                .Delayed();

            context.AddOption<StringConditionalOperator>(OptionOperator)
                .WithDisplayName("Operator")
                .WithDefaultValue(StringConditionalOperator.EqualTo);

            context.AddOption<string>(OptionCompare)
                .WithDisplayName("Compare")
                .WithDefaultValue(string.Empty)
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort(PortOutput)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }
}
