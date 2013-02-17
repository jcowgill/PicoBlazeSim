
namespace JCowgill.PicoBlazeSim
{
    /// <summary>
    /// Represents a type of PicoBlaze processor
    /// </summary>
    public class Processor
    {
        public static readonly Processor PicoBlazeCpld  = new Processor( 256,  8,  4, 0, 0);
        public static readonly Processor PicoBlaze      = new Processor( 256, 16, 15, 0, 0);
        public static readonly Processor PicoBlazeII    = new Processor(1024, 32, 31, 0, 0);
        public static readonly Processor PicoBlaze3 =
            new Processor(1024, 16, 31, 64, ProcessorFlags.HasStoreFetch |
                                            ProcessorFlags.HasTestCompare);

        /// <summary>
        /// Gets the size of the instruction ROM
        /// </summary>
        public int RomSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of registers
        /// </summary>
        public int RegisterCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the size of the call stack
        /// </summary>
        public int StackSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the size of scratchpad ram
        /// </summary>
        public int ScratchpadSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the flags for this processor
        /// </summary>
        public ProcessorFlags Flags
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new ProcessorOptions class
        /// </summary>
        public Processor(int romSize,
                                int registerCount,
                                int stackSize,
                                int scratchpadSize,
                                ProcessorFlags flags)
        {
            this.RomSize = romSize;
            this.RegisterCount = registerCount;
            this.StackSize = stackSize;
            this.ScratchpadSize = scratchpadSize;
            this.Flags = flags;
        }
    }
}
