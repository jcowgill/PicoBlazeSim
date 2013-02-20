using System;

namespace JCowgill.PicoBlazeSim.Simulation
{
    /// <summary>
    /// Exception thrown when there is an error during program simulation
    /// </summary>
    [Serializable]
    public class SimulationException : Exception
    {
        public SimulationException(string msg)
            : base(msg)
        {
        }

        public SimulationException(string msg, Exception inner)
            : base(msg, inner)
        {
        }
    }
}
