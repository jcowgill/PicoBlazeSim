using System;

namespace JCowgill.PicoBlazeSim.Instructions
{
    /// <summary>
    /// Base class for conditional instructions
    /// </summary>
    public abstract class Conditional : IInstruction
    {
        /// <summary>
        /// Gets the condition associated with this instruction
        /// </summary>
        public ConditionType Condition
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new Conditional instruction
        /// </summary>
        /// <param name="condition">the condition to execute this instruction on</param>
        protected Conditional(ConditionType condition = ConditionType.Unconditional)
        {
            this.Condition = condition;
        }

        /// <summary>
        /// Evaluates the condition in this instruction
        /// </summary>
        /// <param name="zero">the value of the zero flag</param>
        /// <param name="carry">the value of the carry flag</param>
        /// <returns>true if this instruction should be executed</returns>
        public bool EvaluateCondition(bool zero, bool carry)
        {
            switch (Condition)
            {
                case ConditionType.Unconditional:
                    return true;

                case ConditionType.Zero:
                    return zero;

                case ConditionType.Carry:
                    return carry;

                case ConditionType.NotZero:
                    return !zero;

                case ConditionType.NotCarry:
                    return !carry;

                default:
                    throw new InvalidOperationException("Invalid conditional type");
            }
        }

        public abstract void Accept(IInstructionVisitor visitor);
    }
}
