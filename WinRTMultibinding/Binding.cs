using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using WinRTMultibinding.Extensions;
using WinRTMultibinding.Interfaces;

namespace WinRTMultibinding
{
    public class Binding : Windows.UI.Xaml.Data.Binding, IMultibindingItem
    {
        private static readonly DependencyProperty ComputedValueProperty = DependencyProperty.Register("ComputedValue", typeof (object), typeof (Binding), new PropertyMetadata(default(object), OnComputedValueChanged));

        private EventHandler _computedValueChanged;


        private object ComputedValue
        {
            get
            {
                return GetValue(ComputedValueProperty);
            }
            set
            {
                SetValue(ComputedValueProperty, value);
            }
        }

        object IMultibindingItem.ComputedValue => ComputedValue;


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


        void IMultibindingItem.Initialize(FrameworkElement targetElement)
        {
            if (!String.IsNullOrEmpty(ElementName))
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


        private static void OnComputedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var binding = (Binding) d;
            binding.OnComputedValueChanged();
        }

        private void OnComputedValueChanged()
        {
            _computedValueChanged.RaiseEvent(this);
        }

        private void BindToElement(FrameworkElement targetElement)
        {
            Action targetElementOnLoadedEventHandler = () =>
                {
                    var element = targetElement.FindName(ElementName) as FrameworkElement;

                    if (element == null)
                    {
                        throw new ArgumentException("Element with the specified name not found.");
                    }

                    Source = element;
                    SetBinding();
                };

            SubscribeToLoadedEvent(targetElement, targetElementOnLoadedEventHandler);
        }

        private void BindToRelativeSource(FrameworkElement targetElement)
        {
            Action targetElementOnLoadedEventHandler = () =>
                {
                    switch (RelativeSource.Mode)
                    {
                        case RelativeSourceMode.Self:
                            Source = targetElement;
                            break;
                        default:
                            throw new ArgumentException("Unable to bind to this kind of RelativeSource.");
                    }

                    SetBinding();
                };

            SubscribeToLoadedEvent(targetElement, targetElementOnLoadedEventHandler);
        }

        private void BindToDataContext(FrameworkElement targetElement)
        {
            Action targetElementOnLoadedEventHandler = () =>
                {
                    Source = targetElement.DataContext;
                    targetElement.DataContextChanged += (sender, e) => Source = targetElement.DataContext;
                    SetBinding();
                };

            SubscribeToLoadedEvent(targetElement, targetElementOnLoadedEventHandler);
        }

        private void SetBinding()
        {
            BindingOperations.SetBinding(this, ComputedValueProperty, this);
        }

        private static void SubscribeToLoadedEvent(FrameworkElement targetElement, Action loadedEventHandler)
        {
            RoutedEventHandler targetElementOnLoadedEventHandler = null;

            targetElementOnLoadedEventHandler += (sender, e) =>
                {
                    targetElement.Loaded -= targetElementOnLoadedEventHandler;
                    loadedEventHandler();
                };

            targetElement.Loaded += targetElementOnLoadedEventHandler;
        }
    }
}