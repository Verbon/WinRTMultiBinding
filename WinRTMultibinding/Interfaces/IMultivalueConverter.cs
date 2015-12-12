using System;

namespace WinRTMultibinding.Interfaces
{
    public interface IMultivalueConverter
    {
        object Convert(object[] values, Type targetType, object parameter, string language);

        object[] ConvertBack(object value, Type[] targetTypes, object parameter, string language);
    }
}