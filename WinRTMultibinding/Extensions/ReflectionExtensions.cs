using System.Reflection;

namespace WinRTMultibinding.Extensions
{
    internal static class ReflectionExtensions
    {
        public static bool CanRead(this PropertyInfo propertyInfo)
            => propertyInfo.CanRead && !propertyInfo.GetMethod.IsPrivate;

        public static bool CanWrite(this PropertyInfo propertyInfo)
            => propertyInfo.CanWrite && !propertyInfo.SetMethod.IsPrivate;
    }
}