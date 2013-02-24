using System;

namespace JCowgill.PicoBlazeSim
{
    /// <summary>
    /// Represents a type of PicoBlaze processor
    /// </summary>
    public class Processor
    {
        #region Builtin Processors

        public static readonly Processor PicoBlazeCpld = new Builder()
        {
            RomSize = 256,
            InterruptVector = 0xFF,
            RegisterCount = 8,
            StackSize = 4,

        }.Build();

        public static readonly Processor PicoBlaze = new Builder()
        {
            RomSize = 256,
            InterruptVector = 0xFF,
            RegisterCount = 16,
            StackSize = 15,

        }.Build();

        public static readonly Processor PicoBlazeII = new Builder()
        {
            RomSize = 256,
            InterruptVector = 0xFF,
            RegisterCount = 32,
            StackSize = 31,

        }.Build();

        public static readonly Processor PicoBlaze3 = new Builder()
        {
            RomSize = 1024,
            InterruptVector = 0x3FF,
            RegisterCount = 16,
            StackSize = 31,
            ScratchpadSize = 64,
            Flags = ProcessorFlags.PicoBlaze3Flags,

        }.Build();

        public static readonly Processor PicoBlaze6 = new Builder()
        {
            RomSize = 4096,
            InterruptVector = 0xFFF,
            RegisterCount = 16,
            StackSize = 31,
            ScratchpadSize = 256,
            Flags = ProcessorFlags.PicoBlaze6Flags,

        }.Build();

        #endregion

        /// <summary>
        /// Gets the size of the instruction ROM
        /// </summary>
        public short RomSize { get; private set; }

        /// <summary>
        /// The address to jump to when an interrupt occurs
        /// </summary>
        public short InterruptVector { get; private set; }

        /// <summary>
        /// Gets the number of registers (single bank)
        /// </summary>
        public short RegisterCount { get; private set; }

        /// <summary>
        /// Gets the size of the call stack
        /// </summary>
        public short StackSize { get; private set; }

        /// <summary>
        /// Gets the size of scratchpad ram
        /// </summary>
        public short ScratchpadSize { get; private set; }

        /// <summary>
        /// Gets the flags for this processor
        /// </summary>
        public ProcessorFlags Flags { get; private set; }

        /// <summary>
        /// Creates a new processor from a builder
        /// </summary>
        /// <param name="builder">builder to create from</param>
        public Processor(Builder builder)
        {
            // Copy properties over
            this.RomSize            = builder.RomSize;
            this.InterruptVector    = builder.InterruptVector;
            this.RegisterCount      = builder.RegisterCount;
            this.StackSize          = builder.StackSize;
            this.ScratchpadSize     = builder.ScratchpadSize;
            this.Flags              = builder.Flags;
        }

        /// <summary>
        /// Helper class for building processors
        /// </summary>
        public class Builder
        {
            public short RomSize { get; set; }
            public short InterruptVector { get; set; }
            public short RegisterCount { get; set; }
            public short StackSize { get; set; }
            public short ScratchpadSize { get; set; }
            public ProcessorFlags Flags { get; set; }

            /// <summary>
            /// Creates an empty Builder
            /// </summary>
            public Builder()
            {
            }

            /// <summary>
            /// Creates a builder based on another processor
            /// </summary>
            /// <param name="processor">processor to base on</param>
            public Builder(Processor processor)
            {
                this.RomSize            = processor.RomSize;
                this.InterruptVector    = processor.InterruptVector;
                this.RegisterCount      = processor.RegisterCount;
                this.StackSize          = processor.StackSize;
                this.ScratchpadSize     = processor.ScratchpadSize;
                this.Flags              = processor.Flags;
            }

            /// <summary>
            /// Creates a new Processor from this builder
            /// </summary>
            /// <returns>the new Processor</returns>
            public Processor Build()
            {
                return new Processor(this);
            }
        }
    }
}
