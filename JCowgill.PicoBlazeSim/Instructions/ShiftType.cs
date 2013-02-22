
namespace JCowgill.PicoBlazeSim.Instructions
{
    /// <summary>
    /// Types of shift instruction
    /// </summary>
    public enum ShiftType
    {
        /// <summary>
        /// Shift left with carry
        /// </summary>
        Sla = 0x0,

        /// <summary>
        /// Left rotate
        /// </summary>
        Rl  = 0x2,

        /// <summary>
        /// Shift left, extend bit 0
        /// </summary>
        Slx = 0x4,

        /// <summary>
        /// Shift left, extend 0
        /// </summary>
        Sl0 = 0x6,

        /// <summary>
        /// Shift left, extend 1
        /// </summary>
        Sl1 = 0x7,

        /// <summary>
        /// Shift right with carry
        /// </summary>
        Sra = 0x8,

        /// <summary>
        /// Shift right sign extend
        /// </summary>
        Srx = 0xA,

        /// <summary>
        /// Right rotate
        /// </summary>
        Rr  = 0xC,

        /// <summary>
        /// Shift right, extend 0
        /// </summary>
        Sr0 = 0xE,

        /// <summary>
        /// Shift right, extend 1
        /// </summary>
        Sr1 = 0xF,
    }
}
