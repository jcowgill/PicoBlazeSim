
namespace JCowgill.PicoBlazeSim.Instructions
{
    /// <summary>
    /// Types of binary instruction
    /// </summary>
    public enum BinaryType
    {
        /// <summary>
        /// Loads a new value into a register
        /// </summary>
        Load,

        /// <summary>
        /// Combination of Load and unconditional Return instructions
        /// </summary>
        LoadReturn,

        /// <summary>
        /// Performs a bitwize and
        /// </summary>
        And,

        /// <summary>
        /// Performs a bitwize or
        /// </summary>
        Or,

        /// <summary>
        /// Performs a bitwize xor
        /// </summary>
        Xor,

        /// <summary>
        /// Performs an addition
        /// </summary>
        Add,

        /// <summary>
        /// Performs an addition with carry
        /// </summary>
        AddCarry,

        /// <summary>
        /// Performs an subtraction
        /// </summary>
        Sub,

        /// <summary>
        /// Performs an subtraction with carry / borrow
        /// </summary>
        SubCarry,

        /// <summary>
        /// Performs a bitwize and / xor without saving the result (just updates registers)
        /// </summary>
        Test,

        /// <summary>
        /// Performs a subtraction without saving the result (just updates registers)
        /// </summary>
        Compare,

        /// <summary>
        /// Same as Test but results change due to flags
        /// </summary>
        /// <remarks>
        /// <para>If ZERO == 0, do not modify ZERO flag</para>
        /// <para>If CARRY == 1, test for even parity instead of odd</para>
        /// </remarks>
        TestCy,

        /// <summary>
        /// Same as Compare but uses a SubCy instruction
        /// </summary>
        /// <remarks>
        /// <para>If ZERO == 0, do not modify ZERO flag</para>
        /// </remarks>
        CompareCy,

        /// <summary>
        /// Read from input port
        /// </summary>
        Input,

        /// <summary>
        /// Write to input port
        /// </summary>
        Output,

        /// <summary>
        /// Read from scratchpad ram
        /// </summary>
        Fetch,

        /// <summary>
        /// Write to scratchpad ram
        /// </summary>
        Store,

        /// <summary>
        /// Copies a value into a register in another register bank
        /// </summary>
        Star,
    }
}
