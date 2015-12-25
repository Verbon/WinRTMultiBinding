using Windows.UI.Xaml;

namespace WinRTMultibinding.Extensions
{
    public static class MultiBindingExtensions
    {
        public static MultiBindingExpression GetMultiBindingExpression(this FrameworkElement frameworkElement, DependencyProperty dependencyProperty)
        {
            MultiBinding multiBinding;

            return MultiBindingHelper.TryGetMultiBindingFor(frameworkElement, dependencyProperty, out multiBinding) ? multiBinding.GetMultiBindingExpression() : null;
        }
    }
}