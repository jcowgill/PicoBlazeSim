using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace JCowgill.PicoBlazeSim.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Register Syntax Highlighter
            RegisterHighlighter();
        }

        /// <summary>
        /// Loads and registers the PicoBlaze highlighter
        /// </summary>
        private void RegisterHighlighter()
        {
            // Get global highlighting managr
            HighlightingManager manager = HighlightingManager.Instance;
            IHighlightingDefinition definition;

            // Open XML stream
            Uri streamLoc = new Uri("/PicoBlaze.xshd", UriKind.Relative);
            Stream stream = Application.GetResourceStream(streamLoc).Stream;

            try
            {
                using (XmlReader xmlReader = new XmlTextReader(stream))
                {
                    stream = null;

                    // Load highlighter
                    definition = HighlightingLoader.Load(xmlReader, manager);
                }
            }
            finally
            {
                if (stream != null)
                    stream.Dispose();
            }

            // Register highlighter
            manager.RegisterHighlighting("PicoBlaze", new string[] { ".psm" }, definition);
        }
    }
}
