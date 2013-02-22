
namespace JCowgill.PicoBlazeSim.Instructions
{
    /// <summary>
    /// A return instruction
    /// </summary>
    public class Return : Conditional
    {
        /// <summary>
        /// Creates a new Return instruction
        /// </summary>
        /// <param name="condition">the condition to execute this instruction on</param>
        public Return(ConditionType condition = ConditionType.Unconditional)
            : base(condition)
        {
        }

        public override void Accept(IInstructionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return string.Format("Return{0}", ConditionStr);
        }
    }
}
