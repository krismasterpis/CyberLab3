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

namespace CyberLab3.Resources.Controls
{
    [TemplatePart(Name = "OsaTraceControl_ComboBox", Type = typeof(ComboBox))]
    public class OsaTraceControl : Control
    {
        static OsaTraceControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OsaTraceControl), new FrameworkPropertyMetadata(typeof(OsaTraceControl)));
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(OsaTraceControl), new PropertyMetadata(string.Empty));
        public string SelectedComboBoxItemString
        {
            get { return (string)GetValue(SelectedComboBoxItemStringProperty); }
            set { SetValue(SelectedComboBoxItemStringProperty, value); }
        }

        public static readonly DependencyProperty SelectedComboBoxItemStringProperty = DependencyProperty.Register("SelectedComboBoxItemString", typeof(string), typeof(OsaTraceControl), new PropertyMetadata(string.Empty));

        public ComboBox comboBox;

        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged",RoutingStrategy.Bubble,typeof(SelectionChangedEventHandler),typeof(OsaTraceControl));

        public event SelectionChangedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            /*            _comboBox = GetTemplateChild("OsaTraceControl_ComboBox") as ComboBox;

                        if (_comboBox != null)
                        {
                            _comboBox.SelectionChanged += (s, e) =>
                            {
                                SelectedComboBoxItemString = _comboBox.SelectedItem.ToString();
                            };
                            if (_comboBox.SelectedItem != null)
                            {
                                SelectedComboBoxItemString = _comboBox.SelectedItem.ToString();
                            }
                        }*/
            if (GetTemplateChild("OsaTraceControl_ComboBox") is ComboBox innerComboBox)
            {
                innerComboBox.SelectionChanged += OsaTraceControl_SelectionChanged;
                comboBox = innerComboBox;
            }
        }

        private void OsaTraceControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RoutedEventArgs newEventArgs = new SelectionChangedEventArgs(SelectionChangedEvent, e.RemovedItems, e.AddedItems);
            if (GetTemplateChild("OsaTraceControl_ComboBox") is ComboBox innerComboBox)
            {
                var selectedItem = innerComboBox.SelectedItem as ComboBoxItem;
                SelectedComboBoxItemString = selectedItem?.Content.ToString();
            }
            RaiseEvent(newEventArgs);
        }
    }
}
