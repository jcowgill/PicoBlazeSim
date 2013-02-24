using JCowgill.PicoBlazeSim.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JCowgill.PicoBlazeSim.Import
{
    /// <summary>
    /// Helper class for building Program objects
    /// </summary>
    public class ProgramBuilder
    {
        /// <summary>
        /// Contains the instructions generated so far
        /// </summary>
        private List<IInstruction> store = new List<IInstruction>();

        /// <summary>
        /// Dictionary of line numbers
        /// </summary>
        private Dictionary<short, int> lineNumbers = new Dictionary<short, int>();

        /// <summary>
        /// Contains the list of fixups
        /// </summary>
        private List<Tuple<short, string>> fixups =
            new List<Tuple<short, string>>();

        /// <summary>
        /// Dictionary of marked labels
        /// </summary>
        private Dictionary<string, short> labels =
            new Dictionary<string, short>();

        /// <summary>
        /// Dictionary of marked labels stored by address
        /// </summary>
        private Dictionary<short, string> labelsReversed =
            new Dictionary<short, string>();

        /// <summary>
        /// Gets or sets the current write address
        /// </summary>
        public short Address { get; set; }

        /// <summary>
        /// Adds a new instruction to the program
        /// </summary>
        /// <param name="instruction">instruction to add</param>
        /// <param name="line">optional line number for this instruction</param>
        /// <remarks>
        /// <para>The instruction is inserted at <see cref="Address"/> and <see cref="Address"/> is
        /// then incremented.</para>
        /// <para>You are not allowed to "rewrite" instructions by decrementing Address and then
        /// adding another instruction</para>
        /// </remarks>
        /// <exception cref="ImportException">
        /// If you attempt to "rewrite" an instruction
        /// </exception>
        public void Add(IInstruction instruction, int? line = null)
        {
            // Ensure store is large enough
            if (Address >= store.Count)
            {
                store.AddRange(Enumerable.Repeat<IInstruction>(null, Address - store.Count + 1));
            }
            else if (store[Address] != null)
            {
                throw new ImportException("You cannot insert 2 instructions at the same address");
            }

            // Store line number
            if (line.HasValue)
                lineNumbers[Address] = line.Value;

            // Insert instruction
            store[Address] = instruction;
            Address++;
        }

        /// <summary>
        /// Adds a jump / call instruction with a label fixup
        /// </summary>
        /// <param name="instruction">instruction to add</param>
        /// <param name="label">label to fixup</param>
        /// <param name="line">optional line number for this instruction</param>
        /// <remarks>
        /// <para>This method allows you to defer the resolving of labels until after all the
        /// instructions have been generated.</para>
        /// <para>If a jump instruction does not need fixing up, you can just use
        /// <see cref="Add"/> instead</para>
        /// </remarks>
        public void AddWithFixup(JumpCall instruction, string label, int? line = null)
        {
            // Add instruction and fixup
            fixups.Add(Tuple.Create(this.Address, label));
            Add(instruction, line);
        }

        /// <summary>
        /// Marks a label at the current address
        /// </summary>
        /// <param name="label">label to mark</param>
        /// <exception cref="ImportException">Thrown if a duplicate label was marked</exception>
        public void MarkLabel(string label)
        {
            // Do not allow duplicate labels
            if (labels.ContainsKey(label))
                throw new ImportException("Duplicate label \"" + label + "\"");

            labels.Add(label, Address);
            labelsReversed[Address] = label;
        }

        /// <summary>
        /// Creates a program from the information in the program builder
        /// </summary>
        /// <param name="processor">processor to pass to the program</param>
        /// <param name="keepDebugInfo">true to add a ProgramDebugInfo class to the Program</param>
        /// <returns>the created program</returns>
        public Program CreateProgram(Processor processor, bool keepDebugInfo = true)
        {
            // Resolve all fixups
            foreach (Tuple<short, string> fixup in fixups)
            {
                // Get the JumpCall instruction
                JumpCall oldInstruction = (JumpCall) store[fixup.Item1];
                short destination;

                // Lookup label
                if (!labels.TryGetValue(fixup.Item2, out destination))
                    throw new ImportException("Label \"" + fixup.Item2 + "\" is not defined");

                // Create and store new instruction
                store[fixup.Item1] = new JumpCall(oldInstruction, destination);
            }

            // Create debug info
            ProgramDebugInfo debugInfo = null;
            if (keepDebugInfo)
                debugInfo = new ProgramDebugInfo(lineNumbers, labelsReversed);

            // Create program object
            return new Program(processor, store, debugInfo);
        }
    }
}
