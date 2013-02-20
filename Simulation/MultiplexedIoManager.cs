using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCowgill.PicoBlazeSim.Simulation
{
    /// <summary>
    /// An IO manager which forwards requests on different ports to other io managers
    /// </summary>
    public class MultiplexedIoManager : IInputOutputManager
    {
        private PortTranslation[] translations = new PortTranslation[256];

        /// <summary>
        /// Adds an IO manager to the multiplexer
        /// </summary>
        /// <param name="ioManager">io manager to add</param>
        /// <param name="multiplexPort">start of the range of multiplexer ports to allocate</param>
        /// <param name="destPort">start of the range of destination ports to use</param>
        /// <param name="length">number of ports to allocate to this io manager</param>
        /// <remarks>
        /// Any existing io managers at these locations will be overwritten
        /// </remarks>
        public void AddIoManager(IInputOutputManager ioManager, byte multiplexPort, byte destPort,
                                    byte length)
        {
            // Validate length
            if (multiplexPort + length > 256 || destPort + length > 256)
                throw new ArgumentOutOfRangeException("length");

            // Add the nessesary translations
            for (int i = 0; i < length; i++)
            {
                translations[multiplexPort + i] =
                    new PortTranslation(ioManager, (byte) (destPort + i));
            }
        }

        /// <summary>
        /// Removes all the port translations in the given range
        /// </summary>
        /// <param name="multiplexPort">starting of range of ports to remove</param>
        /// <param name="length">length of range</param>
        public void RemoveIoManager(byte multiplexPort, byte length)
        {
            // Validate length
            if (multiplexPort + length > 256)
                throw new ArgumentOutOfRangeException("length");

            // Erase all ports in that range
            for (int i = multiplexPort; i < multiplexPort + length; i++)
                translations[i] = new PortTranslation();
        }

        /// <summary>
        /// Removes all the port translations belonging to the given io manager
        /// </summary>
        /// <param name="ioManager">io manager to remove translations for</param>
        public void RemoveIoManager(IInputOutputManager ioManager)
        {
            for (int i = 0; i < 256; i++)
            {
                if (translations[i].IoManager == ioManager)
                    translations[i] = new PortTranslation();
            }
        }

        public byte Input(byte port)
        {
            // Get translation
            PortTranslation translation = translations[port];

            // Forward if we can
            if (translation.IoManager != null)
                return translation.IoManager.Input(translation.Port);
            else
                return 0;
        }

        public void Output(byte port, byte data)
        {
            // Get translation
            PortTranslation translation = translations[port];

            // Forward if we can
            if (translation.IoManager != null)
                translation.IoManager.Output(translation.Port, data);
        }

        /// <summary>
        /// Stores the destination of a port in this io manager
        /// </summary>
        private struct PortTranslation
        {
            /// <summary>
            /// The destination io manager for this translation
            /// </summary>
            public readonly IInputOutputManager IoManager;

            /// <summary>
            /// The port this translation maps to
            /// </summary>
            public readonly byte Port;

            /// <summary>
            /// Creates a new port translation structure
            /// </summary>
            /// <param name="ioManager">destination io manager</param>
            /// <param name="port">port to map to</param>
            public PortTranslation(IInputOutputManager ioManager, byte port)
            {
                this.IoManager = ioManager;
                this.Port = port;
            }
        }
    }
}
