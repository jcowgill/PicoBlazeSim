using System.IO;

namespace JCowgill.PicoBlazeSim.Import
{
    /// <summary>
    /// Performs program creation and validation for importers
    /// </summary>
    /// <remarks>
    /// If you're creating a new importer, you probably want to derive from this class
    /// </remarks>
    public abstract class AbstractImporter : IImporter
    {
        /// <summary>
        /// The processor this importer uses to create programs
        /// </summary>
        public Processor Processor { get; private set; }

        /// <summary>
        /// True if debug information is kept by this importer
        /// </summary>
        public bool KeepDebugInfo { get; private set; }

        /// <summary>
        /// Creates a new AbstractImporter
        /// </summary>
        /// <param name="processor">processor this importer uses</param>
        /// <param name="keepDebugInfo">true to keep debug information</param>
        protected AbstractImporter(Processor processor, bool keepDebugInfo = true)
        {
            this.Processor = processor;
            this.KeepDebugInfo = keepDebugInfo;
        }

        /// <summary>
        /// Imports a program from the given reader
        /// </summary>
        /// <param name="input">input reader</param>
        /// <param name="errors">error list to write any errors to</param>
        /// <returns>the program builder containing the program's data</returns>
        /// <exception cref="ImportException">if thrown will be converted to an ImportError</exception>
        /// <remarks>
        /// An import failiure is marked by returning null or having any errors in the list
        /// </remarks>
        protected abstract ProgramBuilder ImportBuilder(TextReader input, ImportErrorList errors);

        public Program Import(TextReader input, ImportErrorList errors)
        {
            try
            {
                // Do main import
                ProgramBuilder builder = ImportBuilder(input, errors);

                // Exit if there were errors
                if (errors.ErrorCount > 0)
                    return null;
                else if (builder == null)
                    throw new ImportException("Unknown import error");

                // Create program
                Program program = builder.CreateProgram(Processor, KeepDebugInfo);

                // Validate program
                ProgramValidator.Validate(program, errors);
                return (errors.ErrorCount == 0) ? program : null;
            }
            catch (ImportException e)
            {
                // Add to error list
                errors.AddError(e.Message);
                return null;
            }
        }
    }
}
