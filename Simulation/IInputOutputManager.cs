
namespace JCowgill.PicoBlazeSim.Simulation
{
    /// <summary>
    /// Interface which handles input and output instructions
    /// </summary>
    public interface IInputOutputManager
    {
        /// <summary>
        /// Called when an INPUT instruction is executed to read data
        /// </summary>
        /// <param name="port">port to read from</param>
        /// <returns>data generated</returns>
        byte Input(byte port);

        /// <summary>
        /// Called when an OUTPUT instruction is executed to write data
        /// </summary>
        /// <param name="port">port to write to</param>
        /// <param name="data">data to write</param>
        void Output(byte port, byte data);
    }
}
