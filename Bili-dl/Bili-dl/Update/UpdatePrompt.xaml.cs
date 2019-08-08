using JsonUtil;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Bili_dl
{
    /// <summary>
    /// UpdatePrompt.xaml 的交互逻辑
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public partial class UpdatePrompt : UserControl
    {
        /// <summary>
        /// Confirmed delegate.
        /// </summary>
        /// <param name="IsUpdate">Is update</param>
        public delegate void ConfirmedHandler(bool IsUpdate);
        /// <summary>
        /// Occurs when user comfirmd a sellection.
        /// </summary>
        public event ConfirmedHandler Confirmed;

        public UpdatePrompt(string description)
        {
            InitializeComponent();
            InfoBox.Text = description;
        }

        private void LaterBtn_Click(object sender, RoutedEventArgs e)
        {
            ((Grid)this.Parent).Children.Remove(this);
            Confirmed?.Invoke(false);
        }

        private void NowBtn_Click(object sender, RoutedEventArgs e)
        {
            ((Grid)this.Parent).Children.Remove(this);
            UpdateUtil.RunUpdate();
            Confirmed?.Invoke(true);
        }
    }
}
