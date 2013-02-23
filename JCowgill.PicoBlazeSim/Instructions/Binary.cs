
namespace JCowgill.PicoBlazeSim.Instructions
{
    /// <summary>
    /// Base class for binary operators
    /// </summary>
    public abstract class Binary : IInstruction
    {
        /// <summary>
        /// Gets the type of this binary instruction
        /// </summary>
        public BinaryType Type { get; private set; }

        /// <summary>
        /// Gets the left register in this instruction
        /// </summary>
        public byte Left { get; private set; }

        /// <summary>
        /// Gets the right register / constant in this instruction
        /// </summary>
        public byte Right { get; private set; }

        /// <summary>
        /// Creates a new Binary instruction
        /// </summary>
        /// <param name="type">the type of the instruction</param>
        /// <param name="left">the left register (must be less than 16)</param>
        /// <param name="right">the right register / constant</param>
        protected Binary(BinaryType type, byte left, byte right)
        {
            this.Type = type;
            this.Left = left;
            this.Right = right;
        }

        public abstract void Accept(IInstructionVisitor visitor);
    }
}
