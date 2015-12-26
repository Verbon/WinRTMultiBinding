﻿using System;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using WinRTMultibinding.Extensions;
using WinRTMultibinding.Interfaces;

namespace WinRTMultibinding
{
    public class Binding : Windows.UI.Xaml.Data.Binding, IOneWayMultibindingItem, IOneWayToSourceMultibindingItem
    {
        private static readonly DependencyProperty BindingValueProperty = DependencyProperty.Register("BindingValue", typeof (object), typeof (Binding), new PropertyMetadata(default(object), OnBindingValueChanged));


        private static readonly DisableablePropertyChangedCallback DisableableBindingValueChangedCallback;
        private EventHandler _bindingValueChanged;


        object IOneWayMultibindingItem.SourcePropertyValue => GetValue(BindingValueProperty);

        Type IOneWayToSourceMultibindingItem.SourcePropertyType => GetValue(BindingValueProperty).GetType();


        event EventHandler IOneWayMultibindingItem.SourcePropertyValueChanged
        {
            add
            {
                _bindingValueChanged += value;
            }
            remove
            {
                _bindingValueChanged -= value;
            }
        }


        static Binding()
        {
            DisableableBindingValueChangedCallback = new DisableablePropertyChangedCallback(NotifyOnBindingValueChanged);
        }

        public Binding()
        {
            Mode = default(BindingMode);
        }


        void IMultibindingItem.Initialize(FrameworkElement targetElement)
        {
            if (Source != null)
            {
                SetBinding(() => Source);
            }
            else if (!String.IsNullOrWhiteSpace(ElementName))
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

        void IOneWayToSourceMultibindingItem.OnTargetPropertyValueChanged(object newSourcePropertyValue)
        {
            SetBindingValue(newSourcePropertyValue);
        }


        private void BindToElement(FrameworkElement targetElement)
        {
            Func<object> sourceFactory = () =>
                {
                    var element = targetElement.FindName(ElementName) as FrameworkElement;

                    if (element == null)
                    {
                        throw new ArgumentException("Element with the specified name not found.", ElementName);
                    }

                    return element;
                };

            SetBinding(sourceFactory);
        }

        private void BindToRelativeSource(FrameworkElement targetElement)
        {
            Func<object> sourceFactory = () =>
                {
                    switch (RelativeSource.Mode)
                    {
                        case RelativeSourceMode.Self:
                            return targetElement;
                        default:
                            throw new NotSupportedException($"{RelativeSource.Mode} RelativeSource mode is not supported.");
                    }
                };

            SetBinding(sourceFactory);
        }

        private void BindToDataContext(FrameworkElement targetElement)
        {
            Func<object> sourceFactory = () =>
                {
                    targetElement.DataContextChanged += (sender, e) => Source = targetElement.DataContext;

                    return targetElement.DataContext;
                };

            SetBinding(sourceFactory);
        }

        private void SetBinding(Func<object> sourceFactory)
        {
            Source = sourceFactory();

            if (!CheckIfBindingModeIsValid(Mode))
            {
                throw new InvalidOperationException($"Unable to attach binding to {Path.Path} property using {Mode} mode.");
            }

            SetBinding();
        }

        private void SetBinding()
        {
            using (DisableableBindingValueChangedCallback.Disable())
            {
                BindingOperations.SetBinding(this, BindingValueProperty, this);
            }
        }

        private bool CheckIfBindingModeIsValid(BindingMode mode)
        {
            var sourceProperty = Source.GetType().GetRuntimeProperty(Path.Path);

            switch (mode)
            {
                case BindingMode.OneTime:
                case BindingMode.OneWay:
                    return sourceProperty.CanRead();
                case BindingMode.TwoWay:
                    return sourceProperty.CanRead() && sourceProperty.CanWrite();
                default:
                    throw new ArgumentException("Unknown binding mode.", "mode");
            }
        }

        private void SetBindingValue(object bindingValue)
        {
            using (DisableableBindingValueChangedCallback.Disable())
            {
                SetValue(BindingValueProperty, bindingValue);
            }
        }

        private static void OnBindingValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisableableBindingValueChangedCallback.OnPropertyChanged(d, e);
        }

        private static void NotifyOnBindingValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var binding = (Binding)d;
            binding.OnBindingValueChanged();
        }

        private void OnBindingValueChanged()
        {
            _bindingValueChanged.RaiseEvent(this);
        }
    }
}