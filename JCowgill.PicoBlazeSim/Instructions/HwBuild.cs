
namespace JCowgill.PicoBlazeSim.Instructions
{
    /// <summary>
    /// Reads the HwBuild value into a register
    /// </summary>
    public class HwBuild : IInstruction
    {
        /// <summary>
        /// True if this instruction is a call
        /// </summary>
        public byte Register { get; private set; }

        /// <summary>
        /// Creates a new HwBuild instruction
        /// </summary>
        /// <param name="reg">register to read into</param>
        public HwBuild(byte reg)
        {
            this.Register = reg;
        }

        public void Accept(IInstructionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return string.Format("HwBuild s{0}", Register);
        }
    }
}
