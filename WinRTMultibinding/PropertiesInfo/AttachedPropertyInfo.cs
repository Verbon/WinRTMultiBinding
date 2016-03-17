using System;
using System.Reflection;
using Windows.UI.Xaml;
using WinRTMultibinding.Interfaces;

namespace WinRTMultibinding.PropertiesInfo
{
    internal class AttachedPropertyInfo : IBindablePropertyInfo
    {
        private const string Get = "Get";
        private const string Set = "Set";

        private readonly MethodInfo _getMethod;
        private readonly MethodInfo _setMethod;


        public string Name { get; }

        public Type PropertyType { get; }

        public bool CanRead => _getMethod != null;

        public bool CanWrite => _setMethod != null;


        public AttachedPropertyInfo(Type propertyOwnerType, string targetPropertyPath)
        {
            var propertyOwnerTypeInfo = propertyOwnerType.GetTypeInfo();
            _getMethod = propertyOwnerTypeInfo.GetDeclaredMethod(Get + targetPropertyPath);
            _setMethod = propertyOwnerTypeInfo.GetDeclaredMethod(Set + targetPropertyPath);

            Name = $"{propertyOwnerType.Name}.{targetPropertyPath}";
            if (_getMethod != null)
            {
                PropertyType = _getMethod.ReturnType;
            }
            else if (_setMethod != null)
            {
                var parameters = _setMethod.GetParameters();
                PropertyType = parameters[1].ParameterType;
            }
            else
            {
                throw new InvalidOperationException($"Attached property {Name} doesn't have neither getter nor setter.");
            }
        }


        public object GetValue(FrameworkElement frameworkElement)
            => _getMethod.Invoke(null, new object[]{ frameworkElement });

        public void SetValue(FrameworkElement frameworkElement, object value)
            => _setMethod.Invoke(null, new[] { frameworkElement, value });
    }
}