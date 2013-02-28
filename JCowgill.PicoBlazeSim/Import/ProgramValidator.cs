using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCowgill.PicoBlazeSim.Import
{
    /// <summary>
    /// Class which verifies that a program is compatible with the given processor
    /// </summary>
    /// <remarks>
    /// Currently checks:
    /// <list type="bullet">
    ///     <item><description>Only allowed instructions are used</description></item>
    ///     <item><description>Check maximum size of registers</description></item>
    ///     <item><description>Check jump destinations are within allowed range</description></item>
    ///     <item><description>Check instruction enumeration constants</description></item>
    /// </list>
    /// </remarks>
    public static class ProgramValidator
    {
        /// <summary>
        /// Validates the given program
        /// </summary>
        /// <param name="program">program to validate</param>
        /// <param name="errors">error list to add any errors to</param>
        public static void Validate(Program program, ImportErrorList errors)
        {
            // Visit all the instructions with the validator
            new Visitor(program, errors).VisitAll();
        }

        private class Visitor : IInstructionVisitor
        {
            private readonly Program program;
            private readonly ImportErrorList errors;

            private int? lineNumber;

            public Visitor(Program program, ImportErrorList errors)
            {
                this.program = program;
                this.errors = errors;
            }

            public void VisitAll()
            {
                IDictionary<short, int> lineNos;

                // Get line numbers
                if (program.DebugInfo == null)
                    lineNos = new Dictionary<short, int>();
                else
                    lineNos = program.DebugInfo.LineNumbers;

                // Iterate over all the instructions
                for (short i = 0; i < program.Instructions.Count; i++)
                {
                    // Skip null instructions
                    var instruction = program.Instructions[i];

                    if (instruction != null)
                    {
                        // Store line number
                        int lineNumberTmp;

                        if (lineNos.TryGetValue(i, out lineNumberTmp))
                            lineNumber = lineNumberTmp;
                        else
                            lineNumber = null;

                        // Visit instruction
                        instruction.Accept(this);
                    }
                }
            }

            /// <summary>
            /// Adds an error with the current line number
            /// </summary>
            /// <param name="msg">error message</param>
            private void AddErrorWithLine(string msg)
            {
                errors.AddError(msg, lineNumber);
            }

            /// <summary>
            /// Ensures that some flags has been set
            /// </summary>
            /// <param name="flag">required flags</param>
            private void EnsureFlag(ProcessorFlags flags)
            {
                if ((program.Processor.Flags & flags) != flags)
                    AddErrorWithLine("Invalid instruction for this processor");
            }

            /// <summary>
            /// Adds an error if the given register is invalid
            /// </summary>
            /// <param name="reg">register number</param>
            private void ValidateRegister(byte reg)
            {
                // Check register is in range
                if (reg >= program.Processor.RegisterCount)
                    AddErrorWithLine("Invalid register number for this processor: " + reg.ToString("X2"));
            }

            /// <summary>
            /// Validates the condition of a conditional instruction
            /// </summary>
            /// <param name="instruction">instruction to validate</param>
            private void ValidateConditional(Instructions.Conditional instruction)
            {
                switch (instruction.Condition)
                {
                    case Instructions.ConditionType.Unconditional:
                    case Instructions.ConditionType.Zero:
                    case Instructions.ConditionType.Carry:
                    case Instructions.ConditionType.NotZero:
                    case Instructions.ConditionType.NotCarry:
                        // OK
                        break;

                    default:
                        AddErrorWithLine("(Internal Error) Instruction has invalid ConditionType");
                        break;
                }
            }

            /// <summary>
            /// Validates the type and left side of a binary instruction
            /// </summary>
            /// <param name="instruction">instruction to validate</param>
            private void ValidateBinary(Instructions.Binary instruction)
            {
                ProcessorFlags reqdFlag = 0;

                // Check left
                ValidateRegister(instruction.Left);

                // Check opcode
                switch (instruction.Type)
                {
                    case Instructions.BinaryType.Load:
                    case Instructions.BinaryType.And:
                    case Instructions.BinaryType.Or:
                    case Instructions.BinaryType.Xor:
                    case Instructions.BinaryType.Add:
                    case Instructions.BinaryType.AddCarry:
                    case Instructions.BinaryType.Sub:
                    case Instructions.BinaryType.SubCarry:
                    case Instructions.BinaryType.Input:
                    case Instructions.BinaryType.Output:
                        // Always ok
                        break;

                    case Instructions.BinaryType.LoadReturn:
                        reqdFlag = ProcessorFlags.HasLoadReturn;
                        break;

                    case Instructions.BinaryType.Test:
                    case Instructions.BinaryType.Compare:
                        reqdFlag = ProcessorFlags.HasTestCompare;
                        break;

                    case Instructions.BinaryType.TestCarry:
                    case Instructions.BinaryType.CompareCarry:
                        reqdFlag = ProcessorFlags.HasTestCompareCarry;
                        break;

                    case Instructions.BinaryType.Fetch:
                    case Instructions.BinaryType.Store:
                        reqdFlag = ProcessorFlags.HasStoreFetch;
                        break;

                    case Instructions.BinaryType.Star:
                        reqdFlag = ProcessorFlags.HasAlternateBank;
                        break;

                    default:
                        AddErrorWithLine("(Internal Error) Instruction has invalid BinaryType");
                        return;
                }

                // Check flags
                EnsureFlag(reqdFlag);
            }

            public void Visit(Instructions.BinaryConstant instruction)
            {
                ValidateBinary(instruction);
            }

            public void Visit(Instructions.BinaryRegister instruction)
            {
                ValidateRegister(instruction.Right);
                ValidateBinary(instruction);
            }

            public void Visit(Instructions.Shift instruction)
            {
                // Check register
                ValidateRegister(instruction.Register);

                // Check opcode
                switch (instruction.Type)
                {
                    case Instructions.ShiftType.Sla:
                    case Instructions.ShiftType.Rl:
                    case Instructions.ShiftType.Slx:
                    case Instructions.ShiftType.Sl0:
                    case Instructions.ShiftType.Sl1:
                    case Instructions.ShiftType.Sra:
                    case Instructions.ShiftType.Srx:
                    case Instructions.ShiftType.Rr:
                    case Instructions.ShiftType.Sr0:
                    case Instructions.ShiftType.Sr1:
                        // OK
                        break;

                    default:
                        AddErrorWithLine("(Internal Error) Instruction has invalid ShiftType");
                        return;
                }
            }

            public void Visit(Instructions.Return instruction)
            {
                ValidateConditional(instruction);
            }

            public void Visit(Instructions.ReturnInterrupt instruction)
            {
            }

            public void Visit(Instructions.SetInterruptFlag instruction)
            {
            }

            public void Visit(Instructions.SetRegisterBank instruction)
            {
                EnsureFlag(ProcessorFlags.HasAlternateBank);
            }

            public void Visit(Instructions.JumpCall instruction)
            {
                ValidateConditional(instruction);
                if (instruction.Destination >= program.Processor.RomSize)
                    AddErrorWithLine("Destination of Jump / Call out of allowed range");
            }

            public void Visit(Instructions.JumpCallIndirect instruction)
            {
                EnsureFlag(ProcessorFlags.HasIndirectJumps);
                ValidateRegister(instruction.Register1);
                ValidateRegister(instruction.Register2);
            }

            public void Visit(Instructions.HwBuild instruction)
            {
                EnsureFlag(ProcessorFlags.HasHwBuild);
                ValidateRegister(instruction.Register);
            }

            public void Visit(Instructions.OutputConstant instruction)
            {
                EnsureFlag(ProcessorFlags.HasOutputConstant);
                if (instruction.Port >= 0x10)
                    AddErrorWithLine("OUTPUTK can only use ports from 0 - F");
            }
        }
    }
}
