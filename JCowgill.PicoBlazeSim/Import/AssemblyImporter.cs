using System.IO;

namespace JCowgill.PicoBlazeSim.Import
{
    public class AssemblyImporter : AbstractImporter
    {
        /// <summary>
        /// Creates a new AssemblyImporter
        /// </summary>
        /// <param name="processor">processor this importer uses</param>
        /// <param name="keepDebugInfo">true to keep debug information</param>
        public AssemblyImporter(Processor processor, bool keepDebugInfo = true)
            : base(processor, keepDebugInfo)
        {
        }

        protected override ProgramBuilder ImportBuilder(TextReader input, ImportErrorList errors)
        {
            // Create new importer state and forward to that
            var state = new AssemblyImporterState(input, errors);
            state.ParseTopLevel();
            return state.Builder;
        }
    }
}
