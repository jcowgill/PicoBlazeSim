
namespace JCowgill.PicoBlazeSim.Simulation
{
    /// <summary>
    /// The reason why a break event was raised
    /// </summary>
    public enum BreakEventReason
    {
        /// <summary>
        /// Event was caused by use of the <see cref="SimulationManager.Break"/> method
        /// </summary>
        BreakMethod,

        /// <summary>
        /// Event was caused by hitting a breakpoint
        /// </summary>
        Breakpoint,

        /// <summary>
        /// Event was caused by completing a step
        /// </summary>
        Step,

        /// <summary>
        /// A simulation exception was thrown
        /// </summary>
        SimulationException,
    }
}
