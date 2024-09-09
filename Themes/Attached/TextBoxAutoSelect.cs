using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Theme.WPF.Themes.Attached
{
    public static class TextBoxAutoSelect
    {
        private static readonly RoutedEventHandler Handler = ControlOnLoaded;

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(TextBoxAutoSelect), new PropertyMetadata(false, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is System.Windows.Controls.Control control)
            {
                control.Loaded += Handler;
            }
        }

        private static void ControlOnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Control control)
            {
                control.Focus();
                if (control is System.Windows.Controls.Primitives.TextBoxBase textbox)
                {
                    textbox.SelectAll();
                }

                control.Loaded -= Handler;
            }
        }

        public static void SetIsEnabled(DependencyObject element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(DependencyObject element)
        {
            return (bool) element.GetValue(IsEnabledProperty);
        }
    }
}