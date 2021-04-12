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
using NLog;
using CT3DMachine.Helper;
using CT3DMachine.Service;
using CT3DMachine.Connector;
using CT3DMachine.Codec;
using CT3DMachine.Model;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace CT3DMachine
{
    /// <summary>
    /// Interaction logic for Scan3DMachineControllerView.xaml
    /// </summary>
    public partial class CT3DControlView : System.Windows.Controls.UserControl
    {
        public CT3DControlView()
        {
            InitializeComponent();
        }
    }
}
