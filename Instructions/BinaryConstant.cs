
namespace JCowgill.PicoBlazeSim.Instructions
{
    /// <summary>
    /// A binary instruction whose right side is a constant
    /// </summary>
    public class BinaryConstant : Binary
    {
        /// <summary>
        /// Creates a new BinaryConstant
        /// </summary>
        /// <param name="type">the type of the operator</param>
        /// <param name="left">the left register (must be less than 16)</param>
        /// <param name="right">the right constant</param>
        public BinaryConstant(BinaryType type, byte left, byte right)
            : base(type, left, right)
        {
        }

        public override void Accept(IInstructionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
