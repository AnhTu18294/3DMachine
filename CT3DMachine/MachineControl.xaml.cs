using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CT3DMachine
{
    /// <summary>
    /// Interaction logic for MachineControl.xaml
    /// </summary>

    public delegate void EventStartHandler();
    public delegate void EventExecuteHandler();
    public delegate void EventStopHandler();

    public partial class MachineControl : UserControl
    {
        public event EventStartHandler EventStart;
        public event EventExecuteHandler EventExecute;
        public event EventStopHandler EventStop;

        protected void OnEventStart()
        {
            if (this.EventStart != null)
                this.EventStart();
        }

        protected void OnEventExecute()
        {
            if (this.EventExecute != null)
                this.EventExecute();
        }

        protected void OnEventStop()
        {
            if(this.EventStop != null)
            {
                this.EventStop();
            }
        }

        public MachineControl()
        {
            InitializeComponent();           
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            // Send event to MainWindow
            this.OnEventStart();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            // Send event to MainWindow
            this.OnEventStop();
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            // Send event to MainWindow
            this.OnEventExecute();
        }
    }
}
