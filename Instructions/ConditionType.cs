
namespace JCowgill.PicoBlazeSim.Instructions
{
    /// <summary>
    /// Types of conditions allowed for conditional instructions
    /// </summary>
    public enum ConditionType
    {
        /// <summary>
        /// Instruction is unconditional
        /// </summary>
        Unconditional,

        /// <summary>
        /// Conditional on the zero flag being set
        /// </summary>
        Zero,

        /// <summary>
        /// Conditional on the cary flag being set
        /// </summary>
        Carry,

        /// <summary>
        /// Conditional on the zero flag not being set
        /// </summary>
        NotZero,

        /// <summary>
        /// Conditional on the carry flag not being set
        /// </summary>
        NotCarry,
    }
}
