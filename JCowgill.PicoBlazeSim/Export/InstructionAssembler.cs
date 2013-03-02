using System;

namespace JCowgill.PicoBlazeSim.Export
{
    /// <summary>
    /// Assembles instructions into their binary format
    /// </summary>
    /// <remarks>
    /// Unlike the text exporters, the results of this will vary widely between processors
    /// </remarks>
    public class InstructionAssembler
    {
        private readonly Visitor visitor;
        private readonly int processorNumber;   // Processor number used for lookup tables

        /// <summary>
        /// Gets the last instruction assembled
        /// </summary>
        public int LastInstruction { get; private set; }

        /// <summary>
        /// Gets the processor used by this assembler
        /// </summary>
        public Processor Processor { get; private set; }

        /// <summary>
        /// True if the assembler is producing 18-bit instructions (instead of 16-bit)
        /// </summary>
        public bool WideInstructions
        {
            get
            {
                return Processor == PicoBlazeSim.Processor.PicoBlazeII ||
                       Processor == PicoBlazeSim.Processor.PicoBlaze3 ||
                       Processor == PicoBlazeSim.Processor.PicoBlaze6;
            }
        }

        /// <summary>
        /// Creates a new InstructionAssembler for the given processor
        /// </summary>
        /// <param name="processor">processor to assemble for (only built-in processors)</param>
        public InstructionAssembler(Processor processor)
        {
            // Find processor number
            if (processor == PicoBlazeSim.Processor.PicoBlazeCpld)
                processorNumber = 0;
            else if (processor == PicoBlazeSim.Processor.PicoBlaze)
                processorNumber = 1;
            else if (processor == PicoBlazeSim.Processor.PicoBlazeII)
                processorNumber = 2;
            else if (processor == PicoBlazeSim.Processor.PicoBlaze3)
                processorNumber = 3;
            else if (processor == PicoBlazeSim.Processor.PicoBlaze6)
                processorNumber = 4;
            else
                throw new ArgumentException("Only built-in processors allowed", "processor");

            // Save properties
            this.Processor = processor;
            this.visitor = new Visitor(this);
        }

        /// <summary>
        /// Assembles the given instruction
        /// </summary>
        /// <param name="instruction">instruction to assemble</param>
        /// <remarks>
        /// The assembler assumes the instructions provided are valid on the current processor
        /// </remarks>
        /// <returns>the binary version of that instruction on the current processor</returns>
        public int Assemble(IInstruction instruction)
        {
            // Assemble and return last instruction
            instruction.Accept(visitor);
            return LastInstruction;
        }

        /// <summary>
        /// Visitor used to assemble instructions
        /// </summary>
        private class Visitor : IInstructionVisitor
        {
            private readonly InstructionAssembler parent;

            /// <summary>
            /// Lookup table for the shift opcode
            /// </summary>
            private static readonly int[] ShiftOpcode = new int[]
            {
                0x0A000, 0x0D000, 0x28000, 0x20000, 0x14000
            };

            /// <summary>
            /// Lookup table for binary constant opcodes
            /// </summary>
            private static readonly int[] BinaryConstantOpcode = new int[]
            {
                0x00000, 0x00000, 0x00000, 0x00000, 0x01000,    // Load
                0x00000, 0x00000, 0x00000, 0x00000, 0x21000,    // LoadReturn
                0x00800, 0x01000, 0x02000, 0x0A000, 0x03000,    // And
                0x01000, 0x02000, 0x04000, 0x0C000, 0x05000,    // Or
                0x01800, 0x03000, 0x06000, 0x0E000, 0x07000,    // Xor
                0x02000, 0x04000, 0x08000, 0x18000, 0x11000,    // Add
                0x02800, 0x05000, 0x0A000, 0x1A000, 0x13000,    // AddCarry
                0x03000, 0x06000, 0x0C000, 0x1C000, 0x19000,    // Sub
                0x03800, 0x07000, 0x0E000, 0x1E000, 0x1B000,    // SubCarry
                0x00000, 0x00000, 0x00000, 0x12000, 0x0D000,    // Test
                0x00000, 0x00000, 0x00000, 0x14000, 0x1D000,    // Compare
                0x00000, 0x00000, 0x00000, 0x00000, 0x0F000,    // TestCarry
                0x00000, 0x00000, 0x00000, 0x00000, 0x1F000,    // CompareCarry
                0x08000, 0x0A000, 0x20000, 0x04000, 0x09000,    // Input
                0x08800, 0x0E000, 0x28000, 0x2C000, 0x2D000,    // Output
                0x00000, 0x00000, 0x00000, 0x06000, 0x0B000,    // Fetch
                0x00000, 0x00000, 0x00000, 0x2E000, 0x2F000,    // Store
                0x00000, 0x00000, 0x00000, 0x00000, 0x00000,    // Star
            };

            /// <summary>
            /// Lookup table for binary register opcodes
            /// </summary>
            private static readonly int[] BinaryRegisterOpcode = new int[]
            {
                0x04000, 0x0C000, 0x10000, 0x01000, 0x00000,    // Load
                0x00000, 0x00000, 0x00000, 0x00000, 0x20000,    // LoadReturn
                0x04800, 0x0C001, 0x12000, 0x0B000, 0x02000,    // And
                0x05000, 0x0C002, 0x14000, 0x0D000, 0x04000,    // Or
                0x05800, 0x0C003, 0x16000, 0x0F000, 0x06000,    // Xor
                0x06000, 0x0C004, 0x18000, 0x19000, 0x10000,    // Add
                0x06800, 0x0C005, 0x1A000, 0x1B000, 0x12000,    // AddCarry
                0x07000, 0x0C006, 0x1C000, 0x1D000, 0x18000,    // Sub
                0x07800, 0x0C007, 0x1E000, 0x1F000, 0x1A000,    // SubCarry
                0x00000, 0x00000, 0x00000, 0x13000, 0x0C000,    // Test
                0x00000, 0x00000, 0x00000, 0x15000, 0x1C000,    // Compare
                0x00000, 0x00000, 0x00000, 0x00000, 0x0E000,    // TestCarry
                0x00000, 0x00000, 0x00000, 0x00000, 0x1E000,    // CompareCarry
                0x0C000, 0x0B000, 0x30000, 0x05000, 0x08000,    // Input
                0x0C800, 0x0F000, 0x38000, 0x2D000, 0x2E000,    // Output
                0x00000, 0x00000, 0x00000, 0x07000, 0x0C000,    // Fetch
                0x00000, 0x00000, 0x00000, 0x2F000, 0x2E000,    // Store
                0x00000, 0x00000, 0x00000, 0x00000, 0x16000,    // Star
            };

            /// <summary>
            /// Offset for calculating the register version from the BinaryOpcode version
            /// </summary>
            /// <remarks>
            /// Does not apply to PicoBlaze1 (which is strange)
            /// </remarks>
            private static readonly int[] BinaryRegisterOffset = new int[]
            {
                0x040, 0x000, 0x100, 0x010, -0x010
            };

            /// <summary>
            /// Lookup table for offsets for conditionals
            /// </summary>
            private static readonly int[] ConditionalOffset = new int[]
            {
                0x00000, 0x00000, 0x00000, 0x00000, 0x00000,    // Unconditional
                0x00400, 0x01000, 0x01000, 0x01000, 0x0C000,    // Zero
                0x00600, 0x01800, 0x01800, 0x01800, 0x14000,    // Carry
                0x00500, 0x01400, 0x01400, 0x01400, 0x10000,    // NotZero
                0x00700, 0x01C00, 0x01C00, 0x01C00, 0x18000,    // NotCarry
            };

            /// <summary>
            /// Miscellaneous Opcodes
            /// </summary>
            private static readonly int[] MiscOpcodes = new int[]
            {
                0x9000, 0x8080, 0x24000, 0x2A000, 0x25000,  // Return
                0xD800, 0x8300, 0x36000, 0x30000, 0x20000,  // Call
                0xD000, 0x8100, 0x34000, 0x34000, 0x22000,  // Jump
                0xE000, 0x80D0, 0x2C000, 0x38000, 0x29000,  // ReturnInterrupt Disable
                0xE001, 0x80F0, 0x2C001, 0x38001, 0x29001,  // ReturnInterrupt Enable
                0xF000, 0x8010, 0x3C000, 0x3C000, 0x28000,  // SetInterruptFlag Disable
                0xF001, 0x8030, 0x3C001, 0x3C001, 0x28001,  // SetInterruptFlag Enable
            };

            /// <summary>
            /// Creates a new Visitor for the InstructionAssembler
            /// </summary>
            /// <param name="parent">parent assembler</param>
            public Visitor(InstructionAssembler parent)
            {
                this.parent = parent;
            }

            /// <summary>
            /// Reads an opcode table
            /// </summary>
            /// <param name="array">table array</param>
            /// <param name="index">index to read</param>
            /// <returns>the data in the array</returns>
            private int ReadOpcodeTable(int[] array, int index)
            {
                return array[5 * index + parent.processorNumber];
            }

            public void Visit(Instructions.BinaryConstant instruction)
            {
                int opcode = ReadOpcodeTable(BinaryConstantOpcode, (int) instruction.Type);

                parent.LastInstruction = opcode | (int) (instruction.Left << 8)
                                                | (int) instruction.Right;
            }

            public void Visit(Instructions.BinaryRegister instruction)
            {
                int opcode = ReadOpcodeTable(BinaryRegisterOpcode, (int) instruction.Type);

                parent.LastInstruction = opcode | (int) (instruction.Left << 8)
                                                | (int) (instruction.Right << 4);
            }

            public void Visit(Instructions.Shift instruction)
            {
                // Or opcode with register, shift left and or with shift type
                parent.LastInstruction =
                    (int) ((ShiftOpcode[parent.processorNumber] | instruction.Register) << 8) |
                    (int) instruction.Type;
            }

            public void Visit(Instructions.Return instruction)
            {
                parent.LastInstruction =
                    ReadOpcodeTable(MiscOpcodes, 0) +
                    ReadOpcodeTable(ConditionalOffset, (int) instruction.Condition);
            }

            public void Visit(Instructions.ReturnInterrupt instruction)
            {
                int opcodeIndex = instruction.EnableInterrupts ? 4 : 3;
                parent.LastInstruction = ReadOpcodeTable(MiscOpcodes, opcodeIndex);
            }

            public void Visit(Instructions.SetInterruptFlag instruction)
            {
                int opcodeIndex = instruction.EnableInterrupts ? 6 : 5;
                parent.LastInstruction = ReadOpcodeTable(MiscOpcodes, opcodeIndex);
            }

            public void Visit(Instructions.JumpCall instruction)
            {
                int opcode = ReadOpcodeTable(MiscOpcodes, instruction.IsCall ? 1 : 2);

                opcode += ReadOpcodeTable(ConditionalOffset, (int) instruction.Condition);

                parent.LastInstruction = opcode | (ushort) instruction.Destination;
            }

            public void Visit(Instructions.SetRegisterBank instruction)
            {
                if (!instruction.AlternateBank)
                    parent.LastInstruction = 0x37000;
                else
                    parent.LastInstruction = 0x37001;
            }

            public void Visit(Instructions.JumpCallIndirect instruction)
            {
                int opcode = instruction.IsCall ? 0x24000 : 0x26000;

                parent.LastInstruction = opcode | (int) (instruction.Register1 << 8)
                                                | (int) (instruction.Register2 << 4);
            }

            public void Visit(Instructions.HwBuild instruction)
            {
                parent.LastInstruction = 0x14080 | (int) (instruction.Register << 8);
            }

            public void Visit(Instructions.OutputConstant instruction)
            {
                parent.LastInstruction = 0x2B000 | (int) (instruction.Constant << 4) |
                                                    instruction.Port;
            }
        }
    }
}
