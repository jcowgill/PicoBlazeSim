
namespace JCowgill.PicoBlazeSim
{
    /// <summary>
    /// Interface which allows instructions to be visited
    /// </summary>
    public interface IInstruction
    {
        /// <summary>
        /// Should call the correct method in the <see cref="IInstructionVisitor"/> class
        /// </summary>
        /// <param name="visitor">the visitor to call</param>
        void Accept(IInstructionVisitor visitor);
    }
}
