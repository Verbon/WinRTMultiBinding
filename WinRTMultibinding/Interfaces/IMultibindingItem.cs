using Windows.UI.Xaml;

namespace WinRTMultibinding.Interfaces
{
    internal interface IMultibindingItem
    {
        void Initialize(FrameworkElement targetElement);
    }
}