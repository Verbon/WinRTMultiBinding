using System;
using System.Reflection;
using Windows.UI.Xaml;
using WinRTMultibinding.Extensions;
using WinRTMultibinding.Interfaces;

namespace WinRTMultibinding.PropertiesInfo
{
    internal class DependencyPropertyInfo : IBindablePropertyInfo
    {
        private readonly PropertyInfo _targetPropertyInfo;


        public string Name { get; }

        public Type PropertyType { get; }

        public bool CanRead => _targetPropertyInfo.CanRead();

        public bool CanWrite => _targetPropertyInfo.CanWrite();


        public DependencyPropertyInfo(Type frameworkElementObjectType, string targetPropertyPath)
        {
            _targetPropertyInfo = frameworkElementObjectType.GetRuntimeProperty(targetPropertyPath);

            Name = _targetPropertyInfo.Name;
            PropertyType = _targetPropertyInfo.PropertyType;
        }


        public object GetValue(FrameworkElement frameworkElement)
            => _targetPropertyInfo.GetValue(frameworkElement);

        public void SetValue(FrameworkElement frameworkElement, object value)
            => _targetPropertyInfo.SetValue(frameworkElement, value);
    }
}