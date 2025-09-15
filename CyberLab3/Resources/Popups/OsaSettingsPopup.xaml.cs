using CyberLab3.Resources.Libraries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace CyberLab3.Resources.Popups
{
    /// <summary>
    /// Logika interakcji dla klasy OsaSettingsPopup.xaml
    /// </summary>
    public partial class OsaSettingsPopup : Window
    {
        OSA osa;
        public OsaSettingsPopup(OSA _Osa)
        {
            InitializeComponent();
            osa = _Osa;
            ResComboBox.SelectedIndex = osa.RESOLUTION_SETTING.GetHashCode();
            foreach (ComboBoxItem item in VbwComboBox.Items)
            {
                if (item.Content.ToString() == osa.VBW_SETTING.GetHashCode().ToString())
                {
                    VbwComboBox.SelectedItem = item;
                    break;
                }
            }
            PaTextBox.Text = osa.POINT_AVERAGE_COUNT.ToString();
            SaTextBox.Text = osa.SWEEP_AVERAGE_COUNT.ToString();
            if (osa.SMOOTHING_ENABLED == -1)
            {
                SmoothComboBox.SelectedIndex = 1;
            }
            else
            {
                SmoothComboBox.SelectedIndex = 0;
            }
            foreach (ComboBoxItem item in SpComboBox.Items)
            {
                if (item.Content.ToString() == osa.SAMPLING_POINTS_COUNT.ToString())
                {
                    SpComboBox.SelectedItem = item;
                    break;
                }
            }
        }
    }
}
