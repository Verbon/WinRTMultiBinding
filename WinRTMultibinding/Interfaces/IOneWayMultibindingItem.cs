using System;

namespace WinRTMultibinding.Interfaces
{
    internal interface IOneWayMultibindingItem : IMultibindingItem
    {
        object SourcePropertyValue { get; }


        event EventHandler SourcePropertyValueChanged;
    }
}