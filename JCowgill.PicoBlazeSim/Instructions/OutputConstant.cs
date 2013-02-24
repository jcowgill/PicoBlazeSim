
namespace JCowgill.PicoBlazeSim.Instructions
{
    /// <summary>
    /// Instruction which outputs a constant value
    /// </summary>
    public class OutputConstant : IInstruction
    {
        /// <summary>
        /// Gets the constant to output
        /// </summary>
        public byte Constant { get; private set; }

        /// <summary>
        /// Gets the IO port to output to
        /// </summary>
        public byte Port { get; private set; }

        /// <summary>
        /// Creates a new OutputConstant instruction
        /// </summary>
        /// <param name="constant">the constant to output</param>
        /// <param name="port">the IO port to output to</param>
        public OutputConstant(byte constant, byte port)
        {
            this.Constant = constant;
            this.Port = port;
        }

        public void Accept(IInstructionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return string.Format("Output {0:X2}, {1:X2}", Constant, Port);
        }
    }
}
