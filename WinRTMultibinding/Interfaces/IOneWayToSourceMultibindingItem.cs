using System;

namespace WinRTMultibinding.Interfaces
{
    internal interface IOneWayToSourceMultibindingItem : IMultibindingItem
    {
        Type SourcePropertyType { get; }


        void OnTargetPropertyValueChanged(object newSourcePropertyValue);
    }
}