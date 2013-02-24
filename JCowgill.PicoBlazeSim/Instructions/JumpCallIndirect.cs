
namespace JCowgill.PicoBlazeSim.Instructions
{
    /// <summary>
    /// An indirect jump or call instruction
    /// </summary>
    public class JumpCallIndirect : IInstruction
    {
        /// <summary>
        /// True if this instruction is a call
        /// </summary>
        public bool IsCall { get; private set; }

        /// <summary>
        /// The first register specifying the destination
        /// </summary>
        public byte Register1 { get; private set; }

        /// <summary>
        /// The second register specifying the destination
        /// </summary>
        public byte Register2 { get; private set; }

        /// <summary>
        /// Creates a new indirect Jump or Call instruction
        /// </summary>
        /// <param name="isCall">true if this is a call instruction</param>
        /// <param name="dest">destination address</param>
        /// <param name="condition">the condition to execute this instruction on</param>
        public JumpCallIndirect(bool isCall, byte reg1, byte reg2)
        {
            this.IsCall = isCall;
            this.Register1 = reg1;
            this.Register2 = reg2;
        }

        public void Accept(IInstructionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            string instruction = IsCall ? "Call@" : "Jump@";
            return string.Format("{0} (s{1}, s{2})", instruction, Register1, Register2);
        }
    }
}
