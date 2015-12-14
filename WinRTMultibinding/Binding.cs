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
                SubscribeToLoadedEvent(targetElement, SetBinding);
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

        private void OnComputedValueChanged()
        {
            _computedValueChanged.RaiseEvent(this);
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