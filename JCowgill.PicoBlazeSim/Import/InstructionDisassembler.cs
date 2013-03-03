using JCowgill.PicoBlazeSim.Instructions;
using System;

namespace JCowgill.PicoBlazeSim.Import
{
    /// <summary>
    /// Assembles instructions into their binary format
    /// </summary>
    /// <remarks>
    /// Unlike the other importers, the results of this will vary widely between processors
    /// </remarks>
    public abstract class InstructionDisassembler
    {
        /// <summary>
        /// Gets the processor used by this disassembler
        /// </summary>
        public abstract Processor Processor { get; }

        /// <summary>
        /// Creates a new InstructionDisassembler
        /// </summary>
        protected InstructionDisassembler()
        {
        }

        /// <summary>
        /// Creates a new InstructionDisassembler for the given processor
        /// </summary>
        /// <param name="processor">processor to disassemble for (only built-in processors)</param>
        /// <returns>the new disassembler</returns>
        public static InstructionDisassembler Create(Processor processor)
        {
            // Return relevant processor class
            if (processor == PicoBlazeSim.Processor.PicoBlazeCpld)
                return new PicoBlazeCpld();
            else if (processor == PicoBlazeSim.Processor.PicoBlaze)
                return new PicoBlaze();
            else if (processor == PicoBlazeSim.Processor.PicoBlazeII)
                return new PicoBlazeII();
            else if (processor == PicoBlazeSim.Processor.PicoBlaze3)
                return new PicoBlaze3();
            else if (processor == PicoBlazeSim.Processor.PicoBlaze6)
                return new PicoBlaze6();
            else
                throw new ArgumentException("Only built-in processors allowed", "processor");
        }

        /// <summary>
        /// Disassembles the given instruction
        /// </summary>
        /// <param name="instruction">instruction to disassemble</param>
        /// <returns>the internal instruction for the current processor</returns>
        /// <exception cref="ImportException">the given instruction is not legal on this processor</exception>
        public abstract IInstruction Disassemble(int instruction);

        /// <summary>
        /// Gets the first register in the instruction
        /// </summary>
        /// <param name="instruction">instruction to get register from</param>
        /// <returns>the first register number</returns>
        protected abstract byte GetRegister1(int instruction);

        /// <summary>
        /// Gets the second register in the instruction
        /// </summary>
        /// <param name="instruction">instruction to get register from</param>
        /// <returns>the second register number</returns>
        protected abstract byte GetRegister2(int instruction);

        /// <summary>
        /// Returns the bits used to evaluate a condition
        /// </summary>
        /// <param name="instruction">instruction to get bits from</param>
        /// <returns>number whose lower 3 bits contain the condition</returns>
        protected abstract int GetConditionBits(int instruction);

        /// <summary>
        /// Makes a BinaryConstant instruction
        /// </summary>
        /// <param name="instruction">instruction to make from</param>
        /// <param name="type">the type of instruction</param>
        /// <returns>the new BinaryConstant</returns>
        /// <remarks>
        /// Assumes the constant is stored in the lower 8 bits
        /// </remarks>
        protected BinaryConstant MakeBinaryConstant(int instruction, BinaryType type)
        {
            return new BinaryConstant(type, GetRegister1(instruction), (byte) instruction);
        }

        /// <summary>
        /// Makes a BinaryRegister instruction
        /// </summary>
        /// <param name="instruction">instruction to make from</param>
        /// <param name="type">the type of instruction</param>
        /// <returns>the new BinaryRegister</returns>
        protected BinaryRegister MakeBinaryRegister(int instruction, BinaryType type)
        {
            return new BinaryRegister(type, GetRegister1(instruction), GetRegister2(instruction));
        }

        /// <summary>
        /// Makes a Shift instruction
        /// </summary>
        /// <param name="instruction">instruction to make from</param>
        /// <returns>the new Shift</returns>
        /// <remarks>
        /// Assumes the shift type is stored in the lower 4 bits
        /// </remarks>
        protected Shift MakeShift(int instruction)
        {
            // Get and validate shift type
            int shiftType = instruction & 4;

            if ((shiftType & 1) == 1 && (shiftType & 7) != 6)
                throw new ImportException(string.Format("Illegal shift type: {0:X}", shiftType));

            // Create shift
            return new Shift((ShiftType) shiftType, GetRegister1(instruction));
        }

        /// <summary>
        /// Makes a ReturnInterrupt instruction using the LSB as the flag
        /// </summary>
        /// <param name="instruction">instruction to make from</param>
        /// <returns>the new ReturnInterrupt</returns>
        protected static ReturnInterrupt MakeReturnInterrupt(int instruction)
        {
            return new ReturnInterrupt((instruction & 1) == 1);
        }

        /// <summary>
        /// Makes a SetInterruptFlag instruction using the LSB as the flag
        /// </summary>
        /// <param name="instruction">instruction to make from</param>
        /// <returns>the new SetInterruptFlag</returns>
        protected static SetInterruptFlag MakeSetInterruptFlag(int instruction)
        {
            return new SetInterruptFlag((instruction & 1) == 1);
        }

        /// <summary>
        /// Gets the condition used by the instruction
        /// </summary>
        /// <param name="instruction">instruction to check</param>
        /// <returns>its condition type</returns>
        protected ConditionType GetCondition(int instruction)
        {
            // Get bits and return new result
            switch (GetConditionBits(instruction) & 7)
            {
                case 4: return ConditionType.Zero;
                case 5: return ConditionType.NotZero;
                case 6: return ConditionType.Carry;
                case 7: return ConditionType.NotCarry;

                default: return ConditionType.Unconditional;
            }
        }

        /// <summary>
        /// The PicoBlazeCPLD disassembler
        /// </summary>
        private class PicoBlazeCpld : InstructionDisassembler
        {
            public override Processor Processor
            {
                get { return PicoBlazeSim.Processor.PicoBlazeCpld; }
            }

            public override IInstruction Disassemble(int i)
            {
                // Get opcode and do stuff
                switch (i >> 11)
                {
                    case 0x00: /* 00 */ return MakeBinaryConstant(i, BinaryType.Load);
                    case 0x01: /* 08 */ return MakeBinaryConstant(i, BinaryType.And);
                    case 0x02: /* 10 */ return MakeBinaryConstant(i, BinaryType.Or);
                    case 0x03: /* 18 */ return MakeBinaryConstant(i, BinaryType.Xor);
                    case 0x04: /* 20 */ return MakeBinaryConstant(i, BinaryType.Add);
                    case 0x05: /* 28 */ return MakeBinaryConstant(i, BinaryType.AddCarry);
                    case 0x06: /* 30 */ return MakeBinaryConstant(i, BinaryType.Sub);
                    case 0x07: /* 38 */ return MakeBinaryConstant(i, BinaryType.SubCarry);
                    case 0x08: /* 40 */ return MakeBinaryRegister(i, BinaryType.Load);
                    case 0x09: /* 48 */ return MakeBinaryRegister(i, BinaryType.And);
                    case 0x0A: /* 50 */ return MakeBinaryRegister(i, BinaryType.Or);
                    case 0x0B: /* 58 */ return MakeBinaryRegister(i, BinaryType.Xor);
                    case 0x0C: /* 60 */ return MakeBinaryRegister(i, BinaryType.Add);
                    case 0x0D: /* 68 */ return MakeBinaryRegister(i, BinaryType.AddCarry);
                    case 0x0E: /* 70 */ return MakeBinaryRegister(i, BinaryType.Sub);
                    case 0x0F: /* 78 */ return MakeBinaryRegister(i, BinaryType.SubCarry);
                    case 0x10: /* 80 */ return MakeBinaryConstant(i, BinaryType.Input);
                    case 0x11: /* 88 */ return MakeBinaryConstant(i, BinaryType.Output);
                    case 0x12: /* 90 */ return new Return(GetCondition(i));

                    case 0x14: /* A0 */ return MakeShift(i);

                    case 0x18: /* C0 */ return MakeBinaryRegister(i, BinaryType.Input);
                    case 0x19: /* C8 */ return MakeBinaryRegister(i, BinaryType.Output);
                    case 0x1A: /* D0 */ return new JumpCall(false, (byte) i, GetCondition(i));
                    case 0x1B: /* D8 */ return new JumpCall(true, (byte) i, GetCondition(i));
                    case 0x1C: /* E0 */ return MakeReturnInterrupt(i);

                    case 0x1E: /* F0 */ return MakeSetInterruptFlag(i);

                    default:
                        throw new ImportException(
                            string.Format("Illegal opcode: {0:X2}", i >> 8 & 0xF8));
                }
            }

            protected override byte GetRegister1(int instruction)
            {
                return (byte) (instruction >> 8 & 7);
            }

            protected override byte GetRegister2(int instruction)
            {
                return (byte) (instruction >> 5 & 7);
            }

            protected override int GetConditionBits(int instruction)
            {
                return instruction >> 8;
            }
        }

        /// <summary>
        /// The PicoBlaze disassembler
        /// </summary>
        private class PicoBlaze : InstructionDisassembler
        {
            /// <summary>
            /// Table of BinaryType values
            /// </summary>
            private readonly BinaryType[] BinaryTypeTable =
            {
                BinaryType.Load,
                BinaryType.And,
                BinaryType.Or,
                BinaryType.Xor,
                BinaryType.Add,
                BinaryType.AddCarry,
                BinaryType.Sub,
                BinaryType.SubCarry,
            };

            public override Processor Processor
            {
                get { return PicoBlazeSim.Processor.PicoBlaze; }
            }

            public override IInstruction Disassemble(int i)
            {
                int opcode = i >> 12;

                // Get opcode and do stuff
                switch (opcode)
                {
                    case 0x8:
                    case 0x9: return MakeSpecialInstruction(i);

                    case 0xA: return MakeBinaryConstant(i, BinaryType.Input);
                    case 0xB: return MakeBinaryRegister(i, BinaryType.Input);
                    case 0xC: return MakeBinaryRegister(i, BinaryTypeTable[i & 7]);
                    case 0xD: return MakeShift(i);
                    case 0xE: return MakeBinaryConstant(i, BinaryType.Output);
                    case 0xF: return MakeBinaryRegister(i, BinaryType.Output);

                    default:
                        // Binary Constant?
                        if (opcode < 8)
                            return MakeBinaryConstant(i, BinaryTypeTable[opcode]);

                        throw new ImportException(string.Format("Illegal opcode: {0:X}", i >> 12));
                }
            }

            protected override byte GetRegister1(int instruction)
            {
                return (byte) (instruction >> 8 & 0xF);
            }

            protected override byte GetRegister2(int instruction)
            {
                return (byte) (instruction >> 4 & 0xF);
            }

            protected override int GetConditionBits(int instruction)
            {
                return instruction >> 10;
            }

            private IInstruction MakeSpecialInstruction(int i)
            {
                // Handle jumps and calls
                switch ((i >> 8) & 0x3)
                {
                    case 0:
                        // Return and interrupt instructions
                        switch ((i >> 4) & 0xF)
                        {
                            case 0x1: return new SetInterruptFlag(false);
                            case 0x3: return new SetInterruptFlag(true);
                            case 0x8: return new Return(GetCondition(i));
                            case 0xD: return new ReturnInterrupt(false);
                            case 0xF: return new ReturnInterrupt(true);
                        }

                        break;

                    case 1:
                        return new JumpCall(false, (byte) i, GetCondition(i));

                    case 3:
                        return new JumpCall(true, (byte) i, GetCondition(i));
                }

                throw new ImportException(string.Format("Illegal instruction: {0:X4}", i));
            }
        }

        /// <summary>
        /// The PicoBlazeII disassembler
        /// </summary>
        private class PicoBlazeII : InstructionDisassembler
        {
            public override Processor Processor
            {
                get { return PicoBlazeSim.Processor.PicoBlazeII; }
            }

            public override IInstruction Disassemble(int i)
            {
                // Get opcode and do stuff
                switch (i >> 13)
                {
                    case 0x00: /* 00 */ return MakeBinaryConstant(i, BinaryType.Load);
                    case 0x01: /* 02 */ return MakeBinaryConstant(i, BinaryType.And);
                    case 0x02: /* 04 */ return MakeBinaryConstant(i, BinaryType.Or);
                    case 0x03: /* 06 */ return MakeBinaryConstant(i, BinaryType.Xor);
                    case 0x04: /* 08 */ return MakeBinaryConstant(i, BinaryType.Add);
                    case 0x05: /* 0A */ return MakeBinaryConstant(i, BinaryType.AddCarry);
                    case 0x06: /* 0C */ return MakeBinaryConstant(i, BinaryType.Sub);
                    case 0x07: /* 0E */ return MakeBinaryConstant(i, BinaryType.SubCarry);
                    case 0x08: /* 10 */ return MakeBinaryRegister(i, BinaryType.Load);
                    case 0x09: /* 12 */ return MakeBinaryRegister(i, BinaryType.And);
                    case 0x0A: /* 14 */ return MakeBinaryRegister(i, BinaryType.Or);
                    case 0x0B: /* 16 */ return MakeBinaryRegister(i, BinaryType.Xor);
                    case 0x0C: /* 18 */ return MakeBinaryRegister(i, BinaryType.Add);
                    case 0x0D: /* 1A */ return MakeBinaryRegister(i, BinaryType.AddCarry);
                    case 0x0E: /* 1C */ return MakeBinaryRegister(i, BinaryType.Sub);
                    case 0x0F: /* 1E */ return MakeBinaryRegister(i, BinaryType.SubCarry);

                    case 0x10: /* 20 */ return MakeBinaryConstant(i, BinaryType.Input);
                    case 0x12: /* 24 */ return new Return(GetCondition(i));
                    case 0x14: /* 28 */ return MakeShift(i);
                    case 0x16: /* 2C */ return MakeReturnInterrupt(i);
                    case 0x18: /* 30 */ return MakeBinaryRegister(i, BinaryType.Input);
                    case 0x1A: /* 34 */ return new JumpCall(false, (byte) i, GetCondition(i));
                    case 0x1B: /* 36 */ return new JumpCall(true, (byte) i, GetCondition(i));
                    case 0x1C: /* 38 */ return MakeBinaryRegister(i, BinaryType.Output);
                    case 0x1F: /* 3C */ return MakeSetInterruptFlag(i);

                    default:
                        throw new ImportException(
                            string.Format("Illegal opcode: {0:X2}", i >> 12 & 0x3F));
                }
            }

            protected override byte GetRegister1(int instruction)
            {
                return (byte) (instruction >> 8 & 0x1F);
            }

            protected override byte GetRegister2(int instruction)
            {
                return (byte) (instruction >> 3 & 0x1F);
            }

            protected override int GetConditionBits(int instruction)
            {
                return instruction >> 10;
            }
        }

        /// <summary>
        /// The PicoBlaze3 disassembler
        /// </summary>
        private class PicoBlaze3 : InstructionDisassembler
        {
            public override Processor Processor
            {
                get { return PicoBlazeSim.Processor.PicoBlaze3; }
            }

            public override IInstruction Disassemble(int i)
            {
                // Get opcode and do stuff
                switch (i >> 12)
                {
                    case 0x00: return MakeBinaryConstant(i, BinaryType.Load);
                    case 0x01: return MakeBinaryRegister(i, BinaryType.Load);
                    case 0x04: return MakeBinaryConstant(i, BinaryType.Input);
                    case 0x05: return MakeBinaryRegister(i, BinaryType.Input);
                    case 0x06: return MakeBinaryConstant(i, BinaryType.Fetch);
                    case 0x07: return MakeBinaryRegister(i, BinaryType.Fetch);
                    case 0x0A: return MakeBinaryConstant(i, BinaryType.And);
                    case 0x0B: return MakeBinaryRegister(i, BinaryType.And);
                    case 0x0C: return MakeBinaryConstant(i, BinaryType.Or);
                    case 0x0D: return MakeBinaryRegister(i, BinaryType.Or);
                    case 0x0E: return MakeBinaryConstant(i, BinaryType.Xor);
                    case 0x0F: return MakeBinaryRegister(i, BinaryType.Xor);

                    case 0x12: return MakeBinaryConstant(i, BinaryType.Test);
                    case 0x13: return MakeBinaryRegister(i, BinaryType.Test);
                    case 0x14: return MakeBinaryConstant(i, BinaryType.Compare);
                    case 0x15: return MakeBinaryRegister(i, BinaryType.Compare);
                    case 0x18: return MakeBinaryConstant(i, BinaryType.Add);
                    case 0x19: return MakeBinaryRegister(i, BinaryType.Add);
                    case 0x1A: return MakeBinaryConstant(i, BinaryType.AddCarry);
                    case 0x1B: return MakeBinaryRegister(i, BinaryType.AddCarry);
                    case 0x1C: return MakeBinaryConstant(i, BinaryType.Sub);
                    case 0x1D: return MakeBinaryRegister(i, BinaryType.Sub);
                    case 0x1E: return MakeBinaryConstant(i, BinaryType.SubCarry);
                    case 0x1F: return MakeBinaryRegister(i, BinaryType.SubCarry);

                    case 0x20: return MakeShift(i);
                    case 0x2A:
                    case 0x2B: return new Return(GetCondition(i));
                    case 0x2C: return MakeBinaryConstant(i, BinaryType.Output);
                    case 0x2D: return MakeBinaryRegister(i, BinaryType.Output);
                    case 0x2E: return MakeBinaryConstant(i, BinaryType.Store);
                    case 0x2F: return MakeBinaryRegister(i, BinaryType.Store);

                    case 0x30:
                    case 0x31: return new JumpCall(false, (short) (i & 0x3FF), GetCondition(i));
                    case 0x34:
                    case 0x35: return new JumpCall(true, (short) (i & 0x3FF), GetCondition(i));
                    case 0x38: return MakeReturnInterrupt(i);
                    case 0x3C: return MakeSetInterruptFlag(i);

                    default:
                        throw new ImportException(
                            string.Format("Illegal opcode: {0:X2}", i >> 12 & 0x3F));
                }
            }

            protected override byte GetRegister1(int instruction)
            {
                return (byte) (instruction >> 8 & 0xF);
            }

            protected override byte GetRegister2(int instruction)
            {
                return (byte) (instruction >> 4 & 0xF);
            }

            protected override int GetConditionBits(int instruction)
            {
                return instruction >> 10;
            }
        }

        /// <summary>
        /// The PicoBlaze6 disassembler
        /// </summary>
        private class PicoBlaze6 : InstructionDisassembler
        {
            public override Processor Processor
            {
                get { return PicoBlazeSim.Processor.PicoBlaze3; }
            }

            public override IInstruction Disassemble(int i)
            {
                // Get opcode and do stuff
                switch (i >> 12)
                {
                    case 0x00: return MakeBinaryRegister(i, BinaryType.Load);
                    case 0x01: return MakeBinaryConstant(i, BinaryType.Load);
                    case 0x02: return MakeBinaryRegister(i, BinaryType.And);
                    case 0x03: return MakeBinaryConstant(i, BinaryType.And);
                    case 0x04: return MakeBinaryRegister(i, BinaryType.Or);
                    case 0x05: return MakeBinaryConstant(i, BinaryType.Or);
                    case 0x06: return MakeBinaryRegister(i, BinaryType.Xor);
                    case 0x07: return MakeBinaryConstant(i, BinaryType.Xor);
                    case 0x08: return MakeBinaryRegister(i, BinaryType.Input);
                    case 0x09: return MakeBinaryConstant(i, BinaryType.Input);
                    case 0x0A: return MakeBinaryRegister(i, BinaryType.Fetch);
                    case 0x0B: return MakeBinaryConstant(i, BinaryType.Fetch);
                    case 0x0C: return MakeBinaryRegister(i, BinaryType.Test);
                    case 0x0D: return MakeBinaryConstant(i, BinaryType.Test);
                    case 0x0E: return MakeBinaryRegister(i, BinaryType.TestCarry);
                    case 0x0F: return MakeBinaryConstant(i, BinaryType.TestCarry);

                    case 0x10: return MakeBinaryRegister(i, BinaryType.Add);
                    case 0x11: return MakeBinaryConstant(i, BinaryType.Add);
                    case 0x12: return MakeBinaryRegister(i, BinaryType.AddCarry);
                    case 0x13: return MakeBinaryConstant(i, BinaryType.AddCarry);
                    case 0x14: return MakeShiftOrHwBuild(i);
                    case 0x16: return MakeBinaryRegister(i, BinaryType.Star);
                    case 0x18: return MakeBinaryRegister(i, BinaryType.Sub);
                    case 0x19: return MakeBinaryConstant(i, BinaryType.Sub);
                    case 0x1A: return MakeBinaryRegister(i, BinaryType.SubCarry);
                    case 0x1B: return MakeBinaryConstant(i, BinaryType.SubCarry);
                    case 0x1C: return MakeBinaryRegister(i, BinaryType.Compare);
                    case 0x1D: return MakeBinaryConstant(i, BinaryType.Compare);
                    case 0x1E: return MakeBinaryRegister(i, BinaryType.CompareCarry);
                    case 0x1F: return MakeBinaryConstant(i, BinaryType.CompareCarry);

                    case 0x21: return MakeBinaryConstant(i, BinaryType.LoadReturn);
                    case 0x24: return MakeJumpCallIndirect(i, true);
                    case 0x26: return MakeJumpCallIndirect(i, false);
                    case 0x28: return MakeSetInterruptFlag(i);
                    case 0x29: return MakeReturnInterrupt(i);
                    case 0x2B: return MakeOutputConstant(i);
                    case 0x2C: return MakeBinaryRegister(i, BinaryType.Output);
                    case 0x2D: return MakeBinaryConstant(i, BinaryType.Output);
                    case 0x2E: return MakeBinaryRegister(i, BinaryType.Store);
                    case 0x2F: return MakeBinaryConstant(i, BinaryType.Store);

                    case 0x37: return MakeRegBank(i);

                    case 0x20:
                    case 0x30:
                    case 0x34:
                    case 0x38:
                        return new JumpCall(true, (short) (i & 0xFFF), GetCondition(i));

                    case 0x22:
                    case 0x32:
                    case 0x36:
                    case 0x3A:
                        return new JumpCall(false, (short) (i & 0xFFF), GetCondition(i));

                    case 0x25:
                    case 0x31:
                    case 0x35:
                    case 0x39:
                        return new Return(GetCondition(i));

                    default:
                        throw new ImportException(
                            string.Format("Illegal opcode: {0:X2}", i >> 12 & 0x3F));
                }
            }

            protected override byte GetRegister1(int instruction)
            {
                return (byte) (instruction >> 8 & 0xF);
            }

            protected override byte GetRegister2(int instruction)
            {
                return (byte) (instruction >> 4 & 0xF);
            }

            protected override int GetConditionBits(int instruction)
            {
                return instruction >> 14;
            }

            private IInstruction MakeShiftOrHwBuild(int instruction)
            {
                if ((instruction & 0x80) != 0)
                    return new HwBuild(GetRegister1(instruction));
                else
                    return MakeShift(instruction);
            }

            private static SetRegisterBank MakeRegBank(int instruction)
            {
                return new SetRegisterBank((instruction & 1) == 1);
            }

            private JumpCallIndirect MakeJumpCallIndirect(int instruction, bool isCall)
            {
                return new JumpCallIndirect(isCall, GetRegister1(instruction),
                                                    GetRegister2(instruction));
            }

            private static OutputConstant MakeOutputConstant(int instruction)
            {
                return new OutputConstant((byte) (instruction >> 4), (byte) (instruction & 0xF));
            }
        }
    }
}
