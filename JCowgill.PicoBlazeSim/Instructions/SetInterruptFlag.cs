
namespace JCowgill.PicoBlazeSim.Instructions
{
    /// <summary>
    /// Instruction which sets the interrupt flag
    /// </summary>
    public class SetInterruptFlag : IInstruction
    {
        /// <summary>
        /// True if interrupts are enabled after this instruction
        /// </summary>
        public bool EnableInterrupts { get; private set; }

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

        public override string ToString()
        {
            string enableStr = EnableInterrupts ? "Enable" : "Disable";
            return string.Format("SetInterruptFlag {0}", enableStr);
        }
    }
}
