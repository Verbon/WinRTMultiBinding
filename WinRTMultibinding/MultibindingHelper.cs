using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using WinRTMultibinding.Reflection;

namespace WinRTMultibinding
{
    public static class MultiBindingHelper
    {
        private const string DependecyPropertySuffix = "Property";


        public static readonly DependencyProperty MultiBindingProperty = DependencyProperty.RegisterAttached("MultiBinding", typeof (MultiBinding), typeof (MultiBindingHelper), new PropertyMetadata(default(MultiBinding), OnMultiBindingChanged));


        private static readonly IDictionary<DependencyPropertyInfo, MultiBinding> _multiBindings;


        public static void SetMultiBinding(DependencyObject element, MultiBinding value)
        {
            element.SetValue(MultiBindingProperty, value);
        }

        public static MultiBinding GetMultiBinding(DependencyObject element)
        {
            return (MultiBinding) element.GetValue(MultiBindingProperty);
        }


        static MultiBindingHelper()
        {
            _multiBindings = new Dictionary<DependencyPropertyInfo, MultiBinding>();
        }


        static internal bool TryGetMultiBindingFor(FrameworkElement frameworkElement, DependencyProperty dependencyProperty, out MultiBinding multiBinding)
        {
            var dependencyPropertyInfo = new DependencyPropertyInfo(frameworkElement, dependencyProperty);

            return _multiBindings.TryGetValue(dependencyPropertyInfo, out multiBinding);
        }


        private static void OnMultiBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var associatedObject = (FrameworkElement) d;
            var multiBinding = (MultiBinding) e.NewValue;
            var targetDependencyPropertyName = multiBinding.BindingPropertyPath.Path + DependecyPropertySuffix;
            var targetDependencyProperty = ExtractDependencyProperty(associatedObject, targetDependencyPropertyName);
            var dependencyPropertyInfo = new DependencyPropertyInfo(associatedObject, targetDependencyProperty);

            AddToInnerDictionary(dependencyPropertyInfo, multiBinding);
            multiBinding.OnAttached(associatedObject);
        }

        private static DependencyProperty ExtractDependencyProperty(DependencyObject dependencyObject, string dependencyPropertyName)
        {
            var dependencyProperty = ExtractDependenctyPropertyFromProperty(dependencyObject, dependencyPropertyName) ??
                                     ExtractDependencyPropertyFromField(dependencyObject, dependencyPropertyName);

            return dependencyProperty;
        }

        private static DependencyProperty ExtractDependenctyPropertyFromProperty(DependencyObject dependencyObject, string dependencyPropertyName)
        {
            var propertyInfo = Reflector.ScanHierarchyForMember(dependencyObject.GetType(), typeInfo => typeInfo.GetDeclaredProperty(dependencyPropertyName));

            return (DependencyProperty) propertyInfo?.GetValue(dependencyObject);
        }

        private static DependencyProperty ExtractDependencyPropertyFromField(DependencyObject dependencyObject, string dependencyPropertyName)
        {
            var fieldInfo = Reflector.ScanHierarchyForMember(dependencyObject.GetType(), typeInfo => typeInfo.GetDeclaredField(dependencyPropertyName));

            return (DependencyProperty) fieldInfo?.GetValue(dependencyObject);
        }

        private static void AddToInnerDictionary(DependencyPropertyInfo dependencyPropertyInfo, MultiBinding multiBinding)
        {
            var frameworkElement = dependencyPropertyInfo.FrameworkElement;

            RoutedEventHandler unloadedEventHandler = null;
            unloadedEventHandler += (sender, args) =>
                {
                    frameworkElement.Unloaded -= unloadedEventHandler;

                    _multiBindings.Remove(dependencyPropertyInfo);
                };
            frameworkElement.Unloaded += unloadedEventHandler;

            _multiBindings[dependencyPropertyInfo] = multiBinding;
        }



        private class DependencyPropertyInfo : IEquatable<DependencyPropertyInfo>
        {
            public FrameworkElement FrameworkElement { get; }

            public DependencyProperty DependencyProperty { get; }


            public DependencyPropertyInfo(FrameworkElement frameworkElement, DependencyProperty dependencyProperty)
            {
                FrameworkElement = frameworkElement;
                DependencyProperty = dependencyProperty;
            }


            public override bool Equals(object obj)
            {
                if (obj is DependencyPropertyInfo)
                {
                    return Equals((DependencyPropertyInfo)obj);
                }

                return false;
            }

            public bool Equals(DependencyPropertyInfo other)
            {
                return FrameworkElement.Equals(other.FrameworkElement) &&
                       DependencyProperty.Equals(other.DependencyProperty);
            }

            public override int GetHashCode()
            {
                return FrameworkElement.GetHashCode() * DependencyProperty.GetHashCode();
            }
        }
    }
}