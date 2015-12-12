using Windows.UI.Xaml;

namespace WinRTMultibinding
{
    public static class MultibindingHelper
    {
        public static readonly DependencyProperty MultibindingProperty = DependencyProperty.RegisterAttached("Multibinding", typeof (Multibinding), typeof (MultibindingHelper), new PropertyMetadata(default(Multibinding), OnMultibindingChanged));


        public static void SetMultibinding(DependencyObject element, Multibinding value)
        {
            element.SetValue(MultibindingProperty, value);
        }

        public static Multibinding GetMultibinding(DependencyObject element)
        {
            return (Multibinding) element.GetValue(MultibindingProperty);
        }


        private static void OnMultibindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var associatedObject = (FrameworkElement) d;
            var multibinding = (Multibinding) e.NewValue;

            multibinding.OnAttached(associatedObject);
        }
    }
}