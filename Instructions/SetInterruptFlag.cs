
namespace JCowgill.PicoBlazeSim.Instructions
{
    /// <summary>
    /// Instruction which sets the interrupt flag
    /// </summary>
    public class SetInterruptFlag
    {
        /// <summary>
        /// True if interrupts are enabled after this instruction
        /// </summary>
        public bool EnableInterrupts
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new SetInterruptFlag instruction
        /// </summary>
        /// <param name="enableInterrupts">true to re-enable interrupts</param>
        public SetInterruptFlag(bool enableInterrupts)
        {
            this.EnableInterrupts = enableInterrupts;
        }

        public void Accept(IInstructionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
