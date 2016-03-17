using System;
using Windows.UI.Xaml;

namespace WinRTMultibinding.Interfaces
{
    internal interface IBindablePropertyInfo
    {
        string Name { get; }

        Type PropertyType { get; }

        bool CanRead { get; }

        bool CanWrite { get; }


        object GetValue(FrameworkElement frameworkElement);

        void SetValue(FrameworkElement frameworkElement, object value);
    }
}