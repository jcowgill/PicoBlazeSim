using System;

namespace JCowgill.PicoBlazeSim
{
    /// <summary>
    /// Flags attached to a processor
    /// </summary>
    [Flags]
    public enum ProcessorFlags
    {
        /// <summary>
        /// Processor has the Store and Fetch instructions
        /// </summary>
        HasStoreFetch   = 1,

        /// <summary>
        /// Processor has the Test and Compare instructions
        /// </summary>
        HasTestCompare  = 2,

        /// <summary>
        /// Processor has an alternate register bank (with RegBank and Star instructions)
        /// </summary>
        HasAlternateBank = 4,

        /// <summary>
        /// Processor has the HwBuild instruction
        /// </summary>
        HasHwBuild = 8,

        /// <summary>
        /// Processor has the LoadReturn instruction
        /// </summary>
        HasLoadReturn = 16,

        /// <summary>
        /// Processor has the TestCy and CompareCy instructions
        /// </summary>
        HasTestCompareCarry = 32,

        /// <summary>
        /// Process has indirect jumps and calls
        /// </summary>
        HasIndirectJumps = 64,

        /// <summary>
        /// Processor has the OutputConstant instruction
        /// </summary>
        HasOutputConstant = 128,

        /// <summary>
        /// Flags available on the PicoBlaze3
        /// </summary>
        PicoBlaze3Flags = HasStoreFetch | HasTestCompare,

        /// <summary>
        /// Flags available on the PicoBlaze6
        /// </summary>
        PicoBlaze6Flags = PicoBlaze3Flags | HasAlternateBank | HasHwBuild |
                          HasLoadReturn   | HasTestCompareCarry | HasIndirectJumps |
                          HasOutputConstant,
    }
}
