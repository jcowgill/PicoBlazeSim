
namespace JCowgill.PicoBlazeSim.Instructions
{
    /// <summary>
    /// A return from interrupt instruction
    /// </summary>
    public class ReturnInterrupt : IInstruction
    {
        /// <summary>
        /// True if interrupts are re-enabled after this instruction
        /// </summary>
        public bool EnableInterrupts
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new ReturnInterrupt instruction
        /// </summary>
        /// <param name="enableInterrupts">true to re-enable interrupts</param>
        public ReturnInterrupt(bool enableInterrupts)
        {
            this.EnableInterrupts = enableInterrupts;
        }

        public void Accept(IInstructionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
