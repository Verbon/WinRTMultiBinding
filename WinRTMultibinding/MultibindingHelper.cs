using Windows.UI.Xaml;

namespace WinRTMultibinding
{
    public static class MultiBindingHelper
    {
        public static readonly DependencyProperty MultiBindingProperty = DependencyProperty.RegisterAttached("MultiBinding", typeof (MultiBinding), typeof (MultiBindingHelper), new PropertyMetadata(default(MultiBinding), OnMultiBindingChanged));


        public static void SetMultiBinding(DependencyObject element, MultiBinding value)
        {
            element.SetValue(MultiBindingProperty, value);
        }

        public static MultiBinding GetMultiBinding(DependencyObject element)
        {
            return (MultiBinding) element.GetValue(MultiBindingProperty);
        }


        private static void OnMultiBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var associatedObject = (FrameworkElement) d;
            var multiBinding = (MultiBinding) e.NewValue;

            multiBinding.OnAttached(associatedObject);
        }
    }
}