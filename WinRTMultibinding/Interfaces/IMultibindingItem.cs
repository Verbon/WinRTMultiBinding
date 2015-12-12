using System;
using Windows.UI.Xaml;

namespace WinRTMultibinding.Interfaces
{
    internal interface IMultibindingItem
    {
        object ComputedValue { get; set; }


        event EventHandler ComputedValueChanged;


        void Initialize(FrameworkElement targetElement);
    }
}