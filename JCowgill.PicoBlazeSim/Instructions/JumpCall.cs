
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
        public bool IsCall { get; private set; }

        /// <summary>
        /// The destination address of this instruction
        /// </summary>
        public short Destination { get; private set; }

        /// <summary>
        /// Creates a new Jump or Call instruction
        /// </summary>
        /// <param name="isCall">true if this is a call instruction</param>
        /// <param name="dest">destination address</param>
        /// <param name="condition">the condition to execute this instruction on</param>
        public JumpCall(bool isCall,
                        short dest,
                        ConditionType condition = ConditionType.Unconditional)
            : base(condition)
        {
            this.IsCall = isCall;
            this.Destination = dest;
        }

        /// <summary>
        /// Creates a new Jump or Call instruction with an empty destination
        /// </summary>
        /// <param name="isCall">true if this is a call instruction</param>
        /// <param name="condition">the condition to execute this instruction on</param>
        /// <remarks>
        /// This is designed for instructions which will be fixed up later
        /// </remarks>
        public JumpCall(bool isCall, ConditionType condition = ConditionType.Unconditional)
            : this(isCall, 0, condition)
        {
        }

        /// <summary>
        /// Creates a new Jump or Call instruction with a new destination
        /// </summary>
        /// <param name="old">instruction to base this new one on</param>
        /// <param name="dest">destination address</param>
        /// <remarks>
        /// This is designed for completing a fixup
        /// </remarks>
        public JumpCall(JumpCall old, short dest)
            : this(old.IsCall, dest, old.Condition)
        {
        }

        public override void Accept(IInstructionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            string instruction = IsCall ? "Call" : "Jump";
            return string.Format("{0}{1} {2:X4}", instruction, ConditionStr, Destination);
        }
    }
}
