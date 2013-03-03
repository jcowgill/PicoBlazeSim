using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace JCowgill.PicoBlazeSim.UI
{
    /// <summary>
    /// The View Model for the main window
    /// </summary>
    public class MainWindowViewModel : DependencyObject
    {
        /// <summary>
        /// Gets or sets a boolean specifying if we're in simulation mode
        /// </summary>
        public bool SimulationMode
        {
            get { return (bool) GetValue(SimulationModeProperty); }
            set { SetValue(SimulationModeProperty, value); }
        }

        public static readonly DependencyProperty SimulationModeProperty =
            DependencyProperty.Register("SimulationMode", typeof(bool),
                typeof(MainWindowViewModel), new PropertyMetadata(false));
    }
}
