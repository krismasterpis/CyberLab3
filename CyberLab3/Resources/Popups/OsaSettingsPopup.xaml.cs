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
            SmoothComboBox.SelectedItem = osa.SMOOTHING_ENABLED;
            foreach (ComboBoxItem item in SpComboBox.Items)
            {
                if (item.Content.ToString() == osa.SAMPLING_POINTS_COUNT.ToString())
                {
                    SpComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void applyButt_Click(object sender, RoutedEventArgs e)
        {
            osa.RESOLUTION_SETTING = (OSA.ResolutionSetting)ResComboBox.SelectedIndex;
            osa.VBW_SETTING = (OSA.VbwSetting)VbwComboBox.SelectedIndex;
            int.TryParse(PaTextBox.Text, out var paInt);
            osa.POINT_AVERAGE_COUNT = paInt;
            int.TryParse(SaTextBox.Text, out var saInt);
            osa.SWEEP_AVERAGE_COUNT = saInt;
            osa.SMOOTHING_ENABLED = SmoothComboBox.Text;
            string str = string.Empty;
            switch(osa.RESOLUTION_SETTING.ToString())
            {
                case "Res_0_03_nm":
                    {
                        str = "0.03";
                        break;
                    }
                case "Res_0_05_nm":
                    {
                        str = "0.05";
                        break;
                    }
                case "Res_0_07_nm":
                    {
                        str = "0.07";
                        break;
                    }
                case "Res_0_10_nm":
                    {
                        str = "0.1";
                        break;
                    }
                case "Res_0_20_nm":
                    {
                        str = "0.2";
                        break;
                    }
                case "Res_0_50_nm":
                    {
                        str = "0.5";
                        break;
                    }
                case "Res_1_00_nm":
                    {
                        str = "1.0";
                        break;
                    }
            }
            osa.Write(AnritsuMs9740AScpiCommands.RESOLUTION + " " + str);
            switch (osa.VBW_SETTING.ToString())
            {
                case "VBW_10_Hz":
                    {
                        str = "10";
                        break;
                    }
                case "VBW_100_Hz":
                    {
                        str = "100";
                        break;
                    }
                case "VBW_200_Hz":
                    {
                        str = "200";
                        break;
                    }
                case "VBW_1_kHz":
                    {
                        str = "1000";
                        break;
                    }
                case "VBW_2_kHz":
                    {
                        str = "2000";
                        break;
                    }
                case "VBW_10_kHz":
                    {
                        str = "10000";
                        break;
                    }
                case "VBW_100_kHz":
                    {
                        str = "100000";
                        break;
                    }
                case "VBW_1_MHz":
                    {
                        str = "1000000";
                        break;
                    }
            }
            osa.Write(AnritsuMs9740AScpiCommands.VIDEO_BANDWIDTH + " " + str);
            if(osa.POINT_AVERAGE_COUNT > 0) osa.Write(AnritsuMs9740AScpiCommands.POINT_AVERAGE + " " + osa.POINT_AVERAGE_COUNT.ToString());
            else osa.Write(AnritsuMs9740AScpiCommands.POINT_AVERAGE + " OFF");
            osa.Write(AnritsuMs9740AScpiCommands.SWEEP_AVERAGE + " " + osa.SWEEP_AVERAGE_COUNT.ToString());
            osa.Write(AnritsuMs9740AScpiCommands.SMOOTHING + " " + osa.SMOOTHING_ENABLED.ToString());
            osa.Write(AnritsuMs9740AScpiCommands.SAMPLING_POINTS + " " + osa.SAMPLING_POINTS_COUNT.ToString());        }
    }
}
