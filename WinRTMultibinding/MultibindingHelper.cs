using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using WinRTMultibinding.Extensions;
using WinRTMultibinding.Reflection;

namespace WinRTMultibinding
{
    public static class MultiBindingHelper
    {
        public static readonly DependencyProperty MultiBindingsProperty = DependencyProperty.RegisterAttached("MultiBindings", typeof (MultiBindingCollection), typeof (MultiBindingHelper), new PropertyMetadata(default(MultiBindingCollection), OnMultiBindingsChanged));

        public static void SetMultiBindings(DependencyObject element, MultiBindingCollection value)
        {
            element.SetValue(MultiBindingsProperty, value);
        }

        public static MultiBindingCollection GetMultiBindings(DependencyObject element)
        {
            return (MultiBindingCollection)element.GetValue(MultiBindingsProperty);
        }


        private const string DependecyPropertySuffix = "Property";


        private static readonly IDictionary<MultiBindingTargetInfo, MultiBinding> MultiBindings;


        static MultiBindingHelper()
        {
            MultiBindings = new Dictionary<MultiBindingTargetInfo, MultiBinding>();
        }


        internal static bool TryGetMultiBindingFor(FrameworkElement frameworkElement, DependencyProperty dependencyProperty, out MultiBinding multiBinding)
        {
            var dependencyPropertyInfo = new MultiBindingTargetInfo(frameworkElement, dependencyProperty);

            return MultiBindings.TryGetValue(dependencyPropertyInfo, out multiBinding);
        }


        private static void OnMultiBindingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var associatedObject = (FrameworkElement) d;
            var multiBindings = (MultiBindingCollection) e.NewValue;

            multiBindings.ForEach(multiBinding =>
                {
                    var targetDependencyPropertyName = multiBinding.TargetPropertyPath.Path + DependecyPropertySuffix;
                    var targetDependencyProperty = ExtractDependencyProperty(associatedObject, targetDependencyPropertyName);
                    if (targetDependencyProperty == null)
                    {
                        throw new InvalidOperationException($"{multiBinding.TargetPropertyPath.Path} is not a DependencyProperty.");
                    }
                    var multiBindingTargetInfo = new MultiBindingTargetInfo(associatedObject, targetDependencyProperty);

                    AddToInnerDictionary(multiBindingTargetInfo, multiBinding);
                    multiBinding.OnAttached(associatedObject);
                });
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
                return FrameworkElement.GetHashCode() ^ DependencyProperty.GetHashCode();
            }
        }
    }
}