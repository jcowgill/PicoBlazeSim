
namespace JCowgill.PicoBlazeSim.Instructions
{
    /// <summary>
    /// A shift or rotate instruction
    /// </summary>
    public class Shift : IInstruction
    {
        /// <summary>
        /// Gets the type of this shift instruction
        /// </summary>
        public ShiftType Type { get; private set; }

        /// <summary>
        /// Gets the register used in this instruction
        /// </summary>
        public byte Register { get; private set; }

        /// <summary>
        /// Creates a new Shift instruction
        /// </summary>
        /// <param name="type">the type of the instruction</param>
        /// <param name="reg">the register (must be less than 16)</param>
        public Shift(ShiftType type, byte reg)
        {
            this.Type = type;
            this.Register = reg;
        }

        public void Accept(IInstructionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return string.Format("{0} s{1}", Type, Register);
        }
    }
}
