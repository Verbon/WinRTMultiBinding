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


        private static readonly IDictionary<MultiBindingTargetInfo, MultiBinding> MultiBindings;


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
            MultiBindings = new Dictionary<MultiBindingTargetInfo, MultiBinding>();
        }


        static internal bool TryGetMultiBindingFor(FrameworkElement frameworkElement, DependencyProperty dependencyProperty, out MultiBinding multiBinding)
        {
            var dependencyPropertyInfo = new MultiBindingTargetInfo(frameworkElement, dependencyProperty);

            return MultiBindings.TryGetValue(dependencyPropertyInfo, out multiBinding);
        }


        private static void OnMultiBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var associatedObject = (FrameworkElement) d;
            var multiBinding = (MultiBinding) e.NewValue;
            var targetDependencyPropertyName = multiBinding.BindingPropertyPath.Path + DependecyPropertySuffix;
            var targetDependencyProperty = ExtractDependencyProperty(associatedObject, targetDependencyPropertyName);
            var multiBindingTargetInfo = new MultiBindingTargetInfo(associatedObject, targetDependencyProperty);

            AddToInnerDictionary(multiBindingTargetInfo, multiBinding);
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

        private static void AddToInnerDictionary(MultiBindingTargetInfo multiBindingTargetInfo, MultiBinding multiBinding)
        {
            var frameworkElement = multiBindingTargetInfo.FrameworkElement;

            RoutedEventHandler unloadedEventHandler = null;
            unloadedEventHandler += (sender, args) =>
                {
                    frameworkElement.Unloaded -= unloadedEventHandler;

                    MultiBindings.Remove(multiBindingTargetInfo);
                };
            frameworkElement.Unloaded += unloadedEventHandler;

            MultiBindings[multiBindingTargetInfo] = multiBinding;
        }



        private class MultiBindingTargetInfo : IEquatable<MultiBindingTargetInfo>
        {
            public FrameworkElement FrameworkElement { get; }

            public DependencyProperty DependencyProperty { get; }


            public MultiBindingTargetInfo(FrameworkElement frameworkElement, DependencyProperty dependencyProperty)
            {
                FrameworkElement = frameworkElement;
                DependencyProperty = dependencyProperty;
            }


            public override bool Equals(object obj)
            {
                if (obj is MultiBindingTargetInfo)
                {
                    return Equals((MultiBindingTargetInfo)obj);
                }

                return false;
            }

            public bool Equals(MultiBindingTargetInfo other)
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