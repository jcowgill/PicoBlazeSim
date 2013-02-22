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
    }
}
