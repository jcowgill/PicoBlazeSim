
namespace JCowgill.PicoBlazeSim.Instructions
{
    /// <summary>
    /// A binary instruction whose right side is a register
    /// </summary>
    public class BinaryRegister : Binary
    {
        /// <summary>
        /// Creates a new BinaryRegister
        /// </summary>
        /// <param name="type">the type of the operator</param>
        /// <param name="left">the left register (must be less than 16)</param>
        /// <param name="right">the right register (must be less than 16)</param>
        public BinaryRegister(BinaryType type, byte left, byte right)
            : base(type, left, right)
        {
        }

        public override void Accept(IInstructionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
