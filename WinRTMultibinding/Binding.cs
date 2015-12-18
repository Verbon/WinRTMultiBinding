using System;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using WinRTMultibinding.Extensions;
using WinRTMultibinding.Interfaces;

namespace WinRTMultibinding
{
    public class Binding : Windows.UI.Xaml.Data.Binding, IMultibindingItem
    {
        private static readonly DependencyProperty ComputedValueProperty = DependencyProperty.Register("ComputedValue", typeof (object), typeof (Binding), new PropertyMetadata(default(object), OnComputedValueChanged));


        private static readonly DisableablePropertyChangedCallback DisableableComputedValueChangedCallback;
        private EventHandler _computedValueChanged;


        object IMultibindingItem.ComputedValue
        {
            get
            {
                return GetValue(ComputedValueProperty);
            }
            set
            {
                using (DisableableComputedValueChangedCallback.Disable())
                {
                    SetValue(ComputedValueProperty, value);
                }
            }
        }


        event EventHandler IMultibindingItem.ComputedValueChanged
        {
            add
            {
                _computedValueChanged += value;
            }
            remove
            {
                _computedValueChanged -= value;
            }
        }


        static Binding()
        {
            DisableableComputedValueChangedCallback = new DisableablePropertyChangedCallback(NotifyOnComputedValueChanged);
        }


        void IMultibindingItem.Initialize(FrameworkElement targetElement)
        {
            if (Source != null)
            {
                SetBinding(targetElement);
            }
            else if (!String.IsNullOrEmpty(ElementName))
            {
                BindToElement(targetElement);
            }
            else if (RelativeSource != null)
            {
                BindToRelativeSource(targetElement);
            }
            else
            {
                BindToDataContext(targetElement);
            }
        }


        private void BindToElement(FrameworkElement targetElement)
        {
            Func<object> sourceSelector = () =>
                {
                    var element = targetElement.FindName(ElementName) as FrameworkElement;

                    if (element == null)
                    {
                        throw new ArgumentException("Element with the specified name not found.");
                    }

                    return element;
                };

            SetBinding(targetElement, sourceSelector);
        }

        private void BindToRelativeSource(FrameworkElement targetElement)
        {
            Func<object> sourceSelector = () =>
                {
                    switch (RelativeSource.Mode)
                    {
                        case RelativeSourceMode.Self:
                            return targetElement;
                        default:
                            throw new NotSupportedException("Unable to bind to this kind of RelativeSource.");
                    }
                };

            SetBinding(targetElement, sourceSelector);
        }

        private void BindToDataContext(FrameworkElement targetElement)
        {
            Func<object> sourceSelector = () =>
                {
                    targetElement.DataContextChanged += (sender, e) => Source = targetElement.DataContext;
                    return targetElement.DataContext;
                };

            SetBinding(targetElement, sourceSelector);
        }

        private void SetBinding(FrameworkElement targetElement)
        {
            SetBinding(targetElement, () => Source);
        }

        private void SetBinding(FrameworkElement targetElement, Func<object> sourceSelector)
        {
            RoutedEventHandler targetElementOnLoadedEventHandler = null;

            targetElementOnLoadedEventHandler += (sender, e) =>
                {
                    targetElement.Loaded -= targetElementOnLoadedEventHandler;
                    Source = sourceSelector();

                    if (!CheckIfCanApplyBinding(Source, Path.Path, Mode))
                    {
                        throw new InvalidOperationException($"Unable to attach binding to {Path.Path} property using {Mode} mode.");
                    }

                    SetBinding();
                };

            targetElement.Loaded += targetElementOnLoadedEventHandler;
        }

        private void SetBinding()
        {
            BindingOperations.SetBinding(this, ComputedValueProperty, this);
        }

        private void OnComputedValueChanged()
        {
            _computedValueChanged.RaiseEvent(this);
        }

        private static bool CheckIfCanApplyBinding(object source, string propertyPath, BindingMode mode)
        {
            var sourceProperty = source.GetType().GetRuntimeProperty(propertyPath);

            switch (mode)
            {
                case BindingMode.OneTime:
                case BindingMode.OneWay:
                    return sourceProperty.CanRead();
                case BindingMode.TwoWay:
                    return sourceProperty.CanRead() && sourceProperty.CanWrite();
            }

            throw new ArgumentException("Unknown binding mode.", "mode");
        }

        private static void OnComputedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisableableComputedValueChangedCallback.OnPropertyChanged(d, e);
        }

        private static void NotifyOnComputedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var binding = (Binding)d;
            binding.OnComputedValueChanged();
        }
    }
}