using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Theme.WPF.Themes.Attached
{
    /// <summary>
    /// An attached property class for showing a hint when a text box is empty. Set the text box's Tag property to the text
    /// </summary>
    public static class TextHinting
    {
        public static readonly DependencyProperty ShowWhenFocusedProperty =
            DependencyProperty.RegisterAttached(
                "ShowWhenFocused",
                typeof(bool),
                typeof(TextHinting),
                new FrameworkPropertyMetadata(false));

        public static void SetShowWhenFocused(System.Windows.Controls.Control control, bool value)
        {
            if (control is System.Windows.Controls.Primitives.TextBoxBase || control is PasswordBox)
            {
                control.SetValue(ShowWhenFocusedProperty, value);
            }

            throw new ArgumentException("Control was not a textbox", nameof(control));
        }

        public static bool GetShowWhenFocused(System.Windows.Controls.Control control)
        {
            if (control is System.Windows.Controls.Primitives.TextBoxBase || control is PasswordBox)
            {
                return (bool) control.GetValue(ShowWhenFocusedProperty);
            }

            throw new ArgumentException("Control was not a textbox", nameof(control));
        }
    }
}