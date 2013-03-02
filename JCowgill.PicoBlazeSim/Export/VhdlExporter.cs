using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JCowgill.PicoBlazeSim.Export
{
    /// <summary>
    /// Exporter which outputs a VHDL file containing the program ROM
    /// </summary>
    public class VhdlExporter : IExporter
    {
        /// <summary>
        /// Gets the name of the entity to use
        /// </summary>
        public string EntityName { get; private set; }

        /// <summary>
        /// Creates a new VhdlExporter with the given entity name
        /// </summary>
        /// <param name="name">entity name</param>
        public VhdlExporter(string name)
        {
            this.EntityName = name;
        }

        public void Export(Program program, TextWriter writer)
        {
            // Only PicoBlaze currently supported
            if (program.Processor != Processor.PicoBlaze)
                throw new ExportException("VhdlExporter currently only supports the PicoBlaze 1");

            // Create new compiler
            InstructionAssembler compiler = new InstructionAssembler(program.Processor);

            // Create hex lines
            StringBuilder[] hexLines = new StringBuilder[16];

            for (int i = 0; i < 16; i++)
            {
                hexLines[i] = new StringBuilder(64);
            }

            // Compile each instruction into the hex line
            //  This is done in reverse so the highest addresses appear first
            for (int i = program.Instructions.Count - 1; i >= 0; i--)
            {
                int result;

                // Compile instruction
                if (program.Instructions[i] == null)
                    result = 0;
                else
                    result = compiler.Assemble(program.Instructions[i]);

                // Write result
                hexLines[i / 16].Append(result.ToString("X4"));
            }

            // Write VHDL to writer
            writer.WriteLine(VhdlSkeleton1, EntityName);

            for (int i = 0; i < 16; i++)
                writer.WriteLine(VhdlSkeletonLine1, i, hexLines[i]);

            writer.WriteLine(VhdlSkeleton2);

            for (int i = 0; i < 16; i++)
                writer.WriteLine(VhdlSkeletonLine2, i, hexLines[i]);

            writer.WriteLine(VhdlSkeleton3);
        }

        #region Vhdl Skeleton

        private static readonly string VhdlSkeleton1 =
@"--
LIBRARY IEEE;
USE IEEE.STD_LOGIC_1164.ALL;
--
LIBRARY unisim;
USE unisim.vcomponents.ALL;
--
ENTITY {0} IS
PORT (
    clk_i : IN STD_LOGIC;
    rst_i : IN STD_LOGIC;
    port_a_adr_i : IN STD_LOGIC_VECTOR ( 7 DOWNTO 0 );
    port_a_dat_o : OUT STD_LOGIC_VECTOR ( 15 DOWNTO 0 );
    port_a_stb_i : IN STD_LOGIC;
    port_a_ack_o : OUT STD_LOGIC );
END vector;
--
ARCHITECTURE rom_data_arch OF {0} IS
--
    attribute INIT_00 : string;
    attribute INIT_01 : string;
    attribute INIT_02 : string;
    attribute INIT_03 : string;
    attribute INIT_04 : string;
    attribute INIT_05 : string;
    attribute INIT_06 : string;
    attribute INIT_07 : string;
    attribute INIT_08 : string;
    attribute INIT_09 : string;
    attribute INIT_0A : string;
    attribute INIT_0B : string;
    attribute INIT_0C : string;
    attribute INIT_0D : string;
    attribute INIT_0E : string;
    attribute INIT_0F : string;
--";

        private static readonly string VhdlSkeletonLine1 =
            "    attribute INIT_{0:X2} OF ram_256_x_16 : label IS  \"{1}\";";

        private static readonly string VhdlSkeleton2 =
@"--
BEGIN
--
    ram_256_x_16: RAMB4_S16_S16
    --translate_off
    GENERIC MAP (";

        private static readonly string VhdlSkeletonLine2 =
            "        INIT_{0:X2} => X\"{1}\"";

        private static readonly string VhdlSkeleton3 =
@"    --translate_on
    PORT MAP(
        dia => ""0000000000000000"",
        ena => '1',
        wea => '0',
        rsta => '0',
        clka => clk_i,
        addra => ""00000000"",
        doa => OPEN,
        dib => ""0000000000000000"",
        enb => '1',
        web => '0',
        rstb => '0',
        clkb => clk_i,
        addrb => port_a_adr_i,
        dob => port_a_dat_o );
--
    port_a_ack_o <= port_a_stb_i;
--
END rom_data_arch;";

        #endregion
    }
}
