using System.Collections.Generic;
using System.IO;

namespace JCowgill.PicoBlazeSim.Export
{
    /// <summary>
    /// Exporter which outputs in KCPSM assembly format
    /// </summary>
    /// <remarks>
    /// <para>This exporter is designed to be compatible with the assembly importer</para>
    /// <para>Any labels provided are used, but any jump destination without a label will have
    /// one generated for it</para>
    /// </remarks>
    public class AssemblyExporter : TextExporter
    {
        /// <summary>
        /// Generates the list of labels for the program
        /// </summary>
        /// <param name="program">program to generate labels from</param>
        /// <returns>the list of labels</returns>
        private static Dictionary<short, string> GenerateLabels(Program program)
        {
            // Get labels dictionary
            Dictionary<short, string> labels;

            if (program.DebugInfo == null)
            {
                labels = new Dictionary<short, string>();

                // Add some basic labels
                labels[0] = "Start";
                labels[program.Processor.InterruptVector] = "InterruptVector";
            }
            else
            {
                // Get predefined labels
                labels = new Dictionary<short, string>(program.DebugInfo.Labels);
            }

            // Add jump destinations
            new LabelsVisitor(labels).VisitAll(program);
            return labels;
        }

        /// <summary>
        /// Exports the program to the given text writer
        /// </summary>
        /// <param name="program">program to export</param>
        /// <param name="writer">text writer to export to</param>
        public override void Export(Program program, TextWriter writer)
        {
            // Get labels dictionary
            Dictionary<short, string> labels = GenerateLabels(program);

            // Process each instruction
            bool hasGap = false;
            WritingVisitor visitor = new WritingVisitor(labels, writer);

            for (short i = 0; i < program.Instructions.Count; i++)
            {
                var instruction = program.Instructions[i];

                // Valid instruction?
                if (instruction != null)
                {
                    bool spacerUsed = false;

                    // Add address directive if there was a gap
                    if (hasGap)
                    {
                        writer.WriteLine();
                        if (i > 255)
                            writer.WriteLine("ADDRESS {0:X4}", i);
                        else
                            writer.WriteLine("ADDRESS {0:X2}", i);

                        hasGap = false;
                        spacerUsed = true;
                    }

                    // Label here?
                    string label;

                    if (labels.TryGetValue(i, out label))
                    {
                        if (!spacerUsed)
                            writer.WriteLine();

                        writer.WriteLine(label + ":");
                    }

                    // Write instruction
                    instruction.Accept(visitor);
                }
                else
                {
                    // Ignore and mark gap
                    hasGap = true;
                }
            }
        }

        /// <summary>
        /// Visitor which writes instructions to the output stream
        /// </summary>
        private class WritingVisitor : IInstructionVisitor
        {
            private readonly Dictionary<short, string> labels;
            private readonly TextWriter writer;

            /// <summary>
            /// Creates a new WritingVisitor
            /// </summary>
            /// <param name="labels">list of labels</param>
            /// <param name="writer">writer to write instructions to</param>
            public WritingVisitor(Dictionary<short, string> labels, TextWriter writer)
            {
                this.labels = labels;
                this.writer = writer;
            }

            /// <summary>
            /// Gets the conditional string for a conditional instruction
            /// </summary>
            /// <param name="conditional">conditional instruction</param>
            /// <param name="withComma">true to include the comma</param>
            /// <returns>the conditional string</returns>
            private static string GetConditionalStr(Instructions.Conditional conditional,
                                                    bool withComma)
            {
                string conditionalStr;

                // Write main string first
                switch (conditional.Condition)
                {
                    case Instructions.ConditionType.Unconditional:
                        // No condition string here
                        return "";

                    case Instructions.ConditionType.Zero:       conditionalStr = " Z";  break;
                    case Instructions.ConditionType.Carry:      conditionalStr = " C";  break;
                    case Instructions.ConditionType.NotZero:    conditionalStr = " NZ"; break;
                    case Instructions.ConditionType.NotCarry:   conditionalStr = " NC"; break;

                    default:
                        throw new ExportException("Instruction has invalid ConditionType");
                }

                // Add comma if needed
                if (withComma)
                    conditionalStr += ",";

                return conditionalStr;
            }

            /// <summary>
            /// Writes a binary instruction
            /// </summary>
            /// <param name="instruction">instruction to write</param>
            /// <param name="isReg">true if right side is a register</param>
            private void WriteBinary(Instructions.Binary instruction, bool isReg)
            {
                // Get instruction name
                bool indirectRightReg = false;
                string instructionName;

                switch (instruction.Type)
                {
                    case Instructions.BinaryType.Load:
                        instructionName = "LOAD"; break;
                    case Instructions.BinaryType.LoadReturn:
                        instructionName = "LOAD&RETURN"; break;
                    case Instructions.BinaryType.And:
                        instructionName = "AND"; break;
                    case Instructions.BinaryType.Or:
                        instructionName = "OR"; break;
                    case Instructions.BinaryType.Xor:
                        instructionName = "XOR"; break;
                    case Instructions.BinaryType.Add:
                        instructionName = "ADD"; break;
                    case Instructions.BinaryType.AddCarry:
                        instructionName = "ADDCY"; break;
                    case Instructions.BinaryType.Sub:
                        instructionName = "SUB"; break;
                    case Instructions.BinaryType.SubCarry:
                        instructionName = "SUBCY"; break;
                    case Instructions.BinaryType.Test:
                        instructionName = "TEST"; break;
                    case Instructions.BinaryType.Compare:
                        instructionName = "COMPARE"; break;
                    case Instructions.BinaryType.TestCarry:
                        instructionName = "TESTCY"; break;
                    case Instructions.BinaryType.CompareCarry:
                        instructionName = "COMPARECY"; break;
                    case Instructions.BinaryType.Star:
                        instructionName = "STAR"; break;

                    case Instructions.BinaryType.Input:
                        instructionName = "INPUT"; indirectRightReg = true; break;
                    case Instructions.BinaryType.Output:
                        instructionName = "OUTPUT"; indirectRightReg = true; break;
                    case Instructions.BinaryType.Fetch:
                        instructionName = "FETCH"; indirectRightReg = true; break;
                    case Instructions.BinaryType.Store:
                        instructionName = "STORE"; indirectRightReg = true; break;

                    default:
                        throw new ExportException("Instruction has invalid BinaryType");
                }

                // Get correct pattern
                string pattern;

                if (!isReg)
                    pattern = "\t{0} s{1:X}, {2:X}";
                else if (!indirectRightReg)
                    pattern = "\t{0} s{1:X}, s{2:X}";
                else
                    pattern = "\t{0} s{1:X}, (s{2:X})";

                // Write result
                writer.WriteLine(pattern, instructionName, instruction.Left, instruction.Right);
            }

            public void Visit(Instructions.BinaryConstant instruction)
            {
                WriteBinary(instruction, false);
            }

            public void Visit(Instructions.BinaryRegister instruction)
            {
                WriteBinary(instruction, true);
            }

            public void Visit(Instructions.Shift instruction)
            {
                // Get instruction name
                string instructionName;

                switch (instruction.Type)
                {
                    case Instructions.ShiftType.Sla: instructionName = "SLA"; break;
                    case Instructions.ShiftType.Rl:  instructionName = "RL";  break;
                    case Instructions.ShiftType.Slx: instructionName = "SLX"; break;
                    case Instructions.ShiftType.Sl0: instructionName = "SL0"; break;
                    case Instructions.ShiftType.Sl1: instructionName = "SL1"; break;
                    case Instructions.ShiftType.Sra: instructionName = "SRA"; break;
                    case Instructions.ShiftType.Srx: instructionName = "SRX"; break;
                    case Instructions.ShiftType.Rr:  instructionName = "RR";  break;
                    case Instructions.ShiftType.Sr0: instructionName = "SR0"; break;
                    case Instructions.ShiftType.Sr1: instructionName = "SR1"; break;

                    default:
                        throw new ExportException("Instruction has invalid ShiftType");
                }

                // Write result
                writer.WriteLine("\t{0} s{1:X}", instructionName, instruction.Register);
            }

            public void Visit(Instructions.Return instruction)
            {
                writer.WriteLine("\tRETURN{0}", GetConditionalStr(instruction, false));
            }

            public void Visit(Instructions.ReturnInterrupt instruction)
            {
                if (instruction.EnableInterrupts)
                    writer.WriteLine("\tRETURNI ENABLE");
                else
                    writer.WriteLine("\tRETURNI DISABLE");
            }

            public void Visit(Instructions.SetInterruptFlag instruction)
            {
                if (instruction.EnableInterrupts)
                    writer.WriteLine("\tENABLE INTERRUPT");
                else
                    writer.WriteLine("\tDISABLE INTERRUPT");
            }

            public void Visit(Instructions.SetRegisterBank instruction)
            {
                if (instruction.AlternateBank)
                    writer.WriteLine("\tREGBANK B");
                else
                    writer.WriteLine("\tREGBANK A");
            }

            public void Visit(Instructions.JumpCall instruction)
            {
                string jumpCall = instruction.IsCall ? "CALL" : "JUMP";

                writer.WriteLine("\t{0}{1} {2}",
                                 jumpCall,
                                 GetConditionalStr(instruction, true),
                                 labels[instruction.Destination]);
            }

            public void Visit(Instructions.JumpCallIndirect instruction)
            {
                string jumpCall = instruction.IsCall ? "CALL" : "JUMP";

                writer.WriteLine("\t{0}@ (s{1:X}, s{2:X})", jumpCall,
                                 instruction.Register1, instruction.Register2);
            }

            public void Visit(Instructions.HwBuild instruction)
            {
                writer.WriteLine("\tHWBUILD s{0:X}", instruction.Register);
            }

            public void Visit(Instructions.OutputConstant instruction)
            {
                writer.WriteLine("\tOUTPUTK {0:X}, {1:X}", instruction.Constant, instruction.Port);
            }
        }

        /// <summary>
        /// Visitor which adds any generated labels to the labels list
        /// </summary>
        private class LabelsVisitor : IInstructionVisitor
        {
            private readonly Dictionary<short, string> labels;

            /// <summary>
            /// Creates a new LabelsVisitor
            /// </summary>
            /// <param name="labels">labels dictionary to add to</param>
            public LabelsVisitor(Dictionary<short, string> labels)
            {
                this.labels = labels;
            }

            /// <summary>
            /// Visits all the labels
            /// </summary>
            public void VisitAll(Program program)
            {
                for (short i = 0; i < program.Instructions.Count; i++)
                {
                    if (program.Instructions[i] != null)
                    {
                        program.Instructions[i].Accept(this);
                    }
                }
            }

            public void Visit(Instructions.JumpCall instruction)
            {
                // Add label here if there isn't one
                if (!labels.ContainsKey(instruction.Destination))
                    labels[instruction.Destination] = "Label_" + instruction.Destination.ToString("X4");
            }

            #region Ignored Instructions

            public void Visit(Instructions.BinaryConstant instruction)
            {
            }

            public void Visit(Instructions.BinaryRegister instruction)
            {
            }

            public void Visit(Instructions.Shift instruction)
            {
            }

            public void Visit(Instructions.Return instruction)
            {
            }

            public void Visit(Instructions.ReturnInterrupt instruction)
            {
            }

            public void Visit(Instructions.SetInterruptFlag instruction)
            {
            }

            public void Visit(Instructions.SetRegisterBank instruction)
            {
            }

            public void Visit(Instructions.JumpCallIndirect instruction)
            {
            }

            public void Visit(Instructions.HwBuild instruction)
            {
            }

            public void Visit(Instructions.OutputConstant instruction)
            {
            }

            #endregion
        }
    }
}
