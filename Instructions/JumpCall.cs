
namespace JCowgill.PicoBlazeSim.Instructions
{
    /// <summary>
    /// A jump or call instruction
    /// </summary>
    public class JumpCall : Conditional
    {
        /// <summary>
        /// True if this instruction is a call
        /// </summary>
        public bool IsCall
        {
            get;
            private set;
        }

        /// <summary>
        /// The destination address of this instruction
        /// </summary>
        public short Destination
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new Jump or Call instruction
        /// </summary>
        /// <param name="condition">the condition to execute this instruction on</param>
        public JumpCall(bool isCall,
                        short dest,
                        ConditionType condition = ConditionType.Unconditional)
            : base(condition)
        {
            this.IsCall = isCall;
            this.Destination = dest;
        }

        public override void Accept(IInstructionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
