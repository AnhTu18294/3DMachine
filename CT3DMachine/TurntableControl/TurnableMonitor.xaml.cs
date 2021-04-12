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
using System.Text.RegularExpressions;
using NLog;

namespace CT3DMachine.TurntableControl
{

    /// <summary>
    /// Interaction logic for TurnableMonitor.xaml
    /// </summary>
    public partial class TurnableMonitor : UserControl
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        public enum SampleType : byte
        {
            SMALL = 0x01,
            MIDDLE = 0x02,
            BIG = 0x03
        }

        private SampleType mSampleType = SampleType.SMALL;

        public double getStep()
        {
            double step = -1.0;
            try
            {
                step = Convert.ToDouble(this.tbStep.Text);
            }catch(Exception e)
            {
                Logger.Error(e.ToString());
            }
            return step;
        }

        public double getXRayZPos()
        {
            double xRayZPos = -1.0;
            try
            {
                xRayZPos = Convert.ToDouble(this.tbXRayZPos.Text);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            return xRayZPos;
        }

        public double getDetYPos()
        {
            double detYPos = -1.0;
            try
            {
                detYPos = Convert.ToDouble(this.tbDetYPos.Text);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            return detYPos;
        }

        public double getDetZPos()
        {
            double detZPos = -1.0;
            try
            {
                detZPos = Convert.ToDouble(this.tbDetZPos.Text);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            return detZPos;
        }

        public double getTotalRotation()
        {
            double totalRotation = -1.0;
            try
            {
                totalRotation = Convert.ToDouble(this.tbTotalRotation.Text);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            return totalRotation;
        }

        public double getCurrentRotC()
        {
            double currRotC = -1.0;
            try
            {
                currRotC = Convert.ToDouble(this.tbCurrentRotC.Text);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            return currRotC;
        }

        public SampleType getSampleType()
        {
            return this.mSampleType;
        }

        public void enableAllControl()
        {
            this.tbStep.IsEnabled = true;
            this.tbXRayZPos.IsEnabled = true;
            this.tbDetYPos.IsEnabled = true;
            this.tbDetZPos.IsEnabled = true;
            this.tbCurrentRotC.IsEnabled = true;
            this.tbTotalRotation.IsEnabled = true;
            this.rbSmallSample.IsEnabled = true;
            this.rbBigSample.IsEnabled = true;
            this.rbMiddleSample.IsEnabled = true;
        }

        public void disableAllControl()
        {
            this.tbStep.IsEnabled = false;
            this.tbXRayZPos.IsEnabled = false;
            this.tbDetYPos.IsEnabled = false;
            this.tbDetZPos.IsEnabled = false;
            this.tbCurrentRotC.IsEnabled = false;
            this.tbTotalRotation.IsEnabled = false;
            this.rbSmallSample.IsEnabled = false;
            this.rbBigSample.IsEnabled = false;
            this.rbMiddleSample.IsEnabled = false;
        }

        public void setCurrentRotC(double _currentRotC)
        {
            this.tbCurrentRotC.Text = _currentRotC.ToString();
        }

        public TurnableMonitor()
        {
            InitializeComponent();
            Console.WriteLine("===> Step Text = " + this.tbStep.Text);
        }

        private void onRBSmallSampleChecked(object sender, RoutedEventArgs e)
        {
            this.mSampleType = SampleType.SMALL;
        }

        private void onRBMiddleSampleChecked(object sender, RoutedEventArgs e)
        {
            this.mSampleType = SampleType.MIDDLE;
        }

        private void onRBBigSampleChecked(object sender, RoutedEventArgs e)
        {
            this.mSampleType = SampleType.BIG;
        }

        private void NumberValidationStep(object sender, TextCompositionEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = (System.Windows.Controls.TextBox)sender;
            string regexStr = @"^[0-9]{0,3}\.*?$";
            string previewStr = textBox.Text;
            if (previewStr.Contains(".")) { regexStr = @"^[0-9]{0,3}\.*?[0-9]{1,2}$"; }
            previewStr += e.Text;
            Regex regex = new Regex(regexStr);
            e.Handled = !regex.IsMatch(previewStr);
        }               

        private void NumberValidationTotalRotation(object sender, TextCompositionEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = (System.Windows.Controls.TextBox)sender;
            string regexStr = @"^[0-9]{0,3}\.*?$";
            string previewStr = textBox.Text;
            if (previewStr.Contains(".")) { regexStr = @"^[0-9]{0,3}\.*?[0-9]{1,2}$"; }
            previewStr += e.Text;
            Regex regex = new Regex(regexStr);
            e.Handled = !regex.IsMatch(previewStr);
        }

        private void NumberValidationXRayZPos(object sender, TextCompositionEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = (System.Windows.Controls.TextBox)sender;
            string regexStr = @"^[0-9]{0,5}\.*?$";
            string previewStr = textBox.Text;
            if (previewStr.Contains(".")) { regexStr = @"^[0-9]{0,5}\.*?[0-9]{1,2}$"; }
            previewStr += e.Text;
            Regex regex = new Regex(regexStr);
            e.Handled = !regex.IsMatch(previewStr);
        }

        private void NumberValidationDetZPos(object sender, TextCompositionEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = (System.Windows.Controls.TextBox)sender;
            string regexStr = @"^[0-9]{0,5}\.*?$";
            string previewStr = textBox.Text;
            if (previewStr.Contains(".")) { regexStr = @"^[0-9]{0,5}\.*?[0-9]{1,2}$"; }
            previewStr += e.Text;
            Regex regex = new Regex(regexStr);
            e.Handled = !regex.IsMatch(previewStr);
        }

        private void NumberValidationDetYPos(object sender, TextCompositionEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = (System.Windows.Controls.TextBox)sender;
            string regexStr = @"^[0-9]{0,5}\.*?$";
            string previewStr = textBox.Text;
            if (previewStr.Contains(".")) { regexStr = @"^[0-9]{0,5}\.*?[0-9]{1,2}$"; }
            previewStr += e.Text;
            Regex regex = new Regex(regexStr);
            e.Handled = !regex.IsMatch(previewStr);
        }
    }
}
