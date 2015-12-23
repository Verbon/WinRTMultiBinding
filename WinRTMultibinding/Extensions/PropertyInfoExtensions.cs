using System.Reflection;

namespace WinRTMultibinding.Extensions
{
    internal static class PropertyInfoExtensions
    {
        public static bool CanRead(this PropertyInfo propertyInfo)
            => propertyInfo.CanRead && !propertyInfo.GetMethod.IsFamily;

        public static bool CanWrite(this PropertyInfo propertyInfo)
            => propertyInfo.CanWrite && !propertyInfo.SetMethod.IsFamily;
    }
}