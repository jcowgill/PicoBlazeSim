
namespace JCowgill.PicoBlazeSim.Instructions
{
    /// <summary>
    /// Instruction which sets the current register bank
    /// </summary>
    public class SetRegisterBank : IInstruction
    {
        /// <summary>
        /// True to use the alternate register bank
        /// </summary>
        public bool AlternateBank { get; private set; }

        /// <summary>
        /// Creates a new SetRegisterBank instruction
        /// </summary>
        /// <param name="alternateBank">true to use tha alternate register bank</param>
        public SetRegisterBank(bool alternateBank)
        {
            this.AlternateBank = alternateBank;
        }

        public void Accept(IInstructionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            char bankChar = AlternateBank ? 'B' : 'A';
            return string.Format("RegisterBank {0}", bankChar);
        }
    }
}
