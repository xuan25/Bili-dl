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

namespace PrintingPreview
{
    /// <summary>
    /// Interaction logic for NumberBox.xaml
    /// </summary>
    public partial class NumberBox : UserControl
    {
        public delegate void ValueChangedHandler(NumberBox sender, double newValue);
        public event ValueChangedHandler ValueChanged;

        private double value;
        public double Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                InputBox.Text = value.ToString("0.##");
                ValueChanged?.Invoke(this, Value);
            }
        }

        public NumberBox()
        {
            InitializeComponent();

            InputBox.KeyDown += InputBox_KeyDown;
            InputBox.LostKeyboardFocus += InputBox_LostKeyboardFocus;
        }

        private void InputBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                Value = double.Parse(((TextBox)sender).Text);
            }
            catch (Exception)
            {
                ((TextBox)sender).Text = Value.ToString();
            }
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
                Mouse.Capture(null);
            }
        }

        private void IncreaseBtn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StepperButtonDown(sender, 0.1);
        }

        private void DecreaseBtn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StepperButtonDown(sender, -0.1);
        }

        private void StepperButtonDown(object sender, double step)
        {
            Button button = (Button)sender;
            bool flag = true;
            void lostMouseCaptureAction(object senderLmc, MouseEventArgs eLmc)
            {
                flag = false;
                button.LostMouseCapture -= lostMouseCaptureAction;
            }
            button.LostMouseCapture += lostMouseCaptureAction;

            Value += step;
            new Task(() =>
            {
                System.Threading.Thread.Sleep(500);
                while (flag)
                {
                    Dispatcher.Invoke(() => Value += step);
                    System.Threading.Thread.Sleep(50);
                }
            }).Start();
        }
    }
}
