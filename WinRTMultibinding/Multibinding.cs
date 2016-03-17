﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using WinRTMultibinding.Extensions;
using WinRTMultibinding.Interfaces;
using WinRTMultibinding.PropertiesInfo;

namespace WinRTMultibinding
{
    [ContentProperty(Name = nameof(Bindings))]
    public class MultiBinding : BindingBase
    {
        private static readonly DependencyProperty TargetPropertyValueProperty = DependencyProperty.Register("TargetPropertyValue", typeof (object), typeof (MultiBinding), new PropertyMetadata(default(object), OnTargetPropertyValueChanged));


        private static readonly DisableablePropertyChangedCallback DisableableTargetPropertyValueChangedCallback;
        private IBindablePropertyInfo _bindablePropertyInfo;
        private FrameworkElement _associatedObject;


        private IReadOnlyList<IMultibindingItem> MultibindingItems => Bindings;

        private IReadOnlyList<IOneWayMultibindingItem> OneWayMultibindingItems => Bindings;

        private IReadOnlyList<IOneWayToSourceMultibindingItem> OneWayToSourceMultibindingItems => Bindings;

        private bool CanUseStringFormat => StringFormat != null && _bindablePropertyInfo.PropertyType == typeof (String);

        private bool CanUseConverter => Converter != null;


        public PropertyPath TargetPropertyPath { get; set; }

        public BindingMode Mode { get; set; }

        public UpdateSourceTrigger UpdateSourceTrigger { get; set; }

        public string StringFormat { get; set; }

        public IMultiValueConverter Converter { get; set; }

        public object ConverterParameter { get; set; }

        public string ConverterLanguage { get; set; }

        public object TargetNullValue { get; set; }

        public object FallbackValue { get; set; }

        public ITypeProvider AttachedPropertyOwnerTypeProvider { get; set; }

        public List<Binding> Bindings { get; }


        static MultiBinding()
        {
            DisableableTargetPropertyValueChangedCallback = new DisableablePropertyChangedCallback(NotifyOnTargetPropertyValueChanged);
        }

        public MultiBinding()
        {
            Mode = BindingMode.OneWay;
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Bindings = new List<Binding>();
        }


        internal void OnAttached(FrameworkElement associatedObject)
        {
            if (_associatedObject != null)
            {
                throw new InvalidOperationException("There is already object associated with this Multibinding.");
            }

            _associatedObject = associatedObject;
            _associatedObject.Loaded += (s, e) => Initialize();
        }

        internal MultiBindingExpression GetExpression()
            => MultiBindingExpression.CreateFrom(this, AssociatedObjectOnTargetPropertyValueChanged);


        private void Initialize()
        {
            _bindablePropertyInfo = GetBindablePropertyInfo();
            if (TargetNullValue != null)
            {
                TargetNullValue = ChangeType(TargetNullValue, _bindablePropertyInfo.PropertyType);
            }
            FallbackValue = FallbackValue != null ? ChangeType(FallbackValue, _bindablePropertyInfo.PropertyType) : GetDefaultValueForTargetProperty();

            if (!CanUseStringFormat && !CanUseConverter)
            {
                throw new InvalidOperationException("Unable to attach binding. Please specify StringFormat or Converter.");
            }
            if (!CheckIfBindingModeIsValid(Mode))
            {
                throw new InvalidOperationException($"Unable to attach binding to {_bindablePropertyInfo.Name} property using {Mode} mode.");
            }

            Bindings.Where(binding => binding.Mode == default(BindingMode) || binding.Mode > Mode)
                .ForEach(binding => binding.Mode = Mode);
            Bindings.ForEach(binding => binding.UpdateSourceTrigger = UpdateSourceTrigger);
            MultibindingItems.ForEach(item => item.Initialize(_associatedObject));

            switch (Mode)
            {
                case BindingMode.OneTime:
                case BindingMode.OneWay:
                    CreateOneWayBinding();
                    break;
                case BindingMode.TwoWay:
                    CreateOneWayBinding();
                    CreateOneWayToSourceBinding();
                    break;
            }

            TriggerInitialOneWayBinding();
        }

        private void CreateOneWayBinding()
        {
            OneWayMultibindingItems.ForEach(item => item.SourcePropertyValueChanged += OneWayMultibindingItemOnSourcePropertyValueChanged);
        }

        private void CreateOneWayToSourceBinding()
        {
            switch (UpdateSourceTrigger)
            {
                case UpdateSourceTrigger.Default:
                case UpdateSourceTrigger.PropertyChanged:
                    using (DisableableTargetPropertyValueChangedCallback.Disable())
                    {
                        var binding = new Windows.UI.Xaml.Data.Binding { Source = _associatedObject, Path = TargetPropertyPath };
                        BindingOperations.SetBinding(this, TargetPropertyValueProperty, binding);
                    }
                    break;
                case UpdateSourceTrigger.Explicit:
                    break;
                default:
                    throw new InvalidOperationException("Unknown UpdateSourceTrigger mode.");
            }
        }

        private void TriggerInitialOneWayBinding()
        {
            OneWayMultibindingItemOnSourcePropertyValueChanged(this, EventArgs.Empty);
        }

        private void OneWayMultibindingItemOnSourcePropertyValueChanged(object sender, EventArgs e)
        {
            if (CanUseStringFormat && CanUseConverter)
            {
                SetTargetPropertyValue(FallbackValue);
            }
            else
            {
                var values = OneWayMultibindingItems.Select(item => item.SourcePropertyValue).ToArray();

                if (CanUseStringFormat)
                {
                    SetTargetPropertyValueUsingStringFormat(values);
                }
                else
                {
                    SetTargetPropertyValueUsingConverter(values);
                }
            }
        }

        private void AssociatedObjectOnTargetPropertyValueChanged()
        {
            if (CanUseConverter)
            {
                var value = _bindablePropertyInfo.GetValue(_associatedObject);
                var targetTypes = OneWayToSourceMultibindingItems.Select(item => item.SourcePropertyType).ToArray();
                var values = Converter.ConvertBack(value, targetTypes, ConverterParameter, ConverterLanguage);

                values.ForEach((v, index) =>
                    {
                        var item = OneWayToSourceMultibindingItems[index];

                        if (item.Mode > BindingMode.OneWay)
                        {
                            item.OnTargetPropertyValueChanged(v);
                        }
                    });
            }
        }

        private void SetTargetPropertyValueUsingStringFormat(object[] args)
        {
            var formattedValue = String.Format(StringFormat, args);

            SetTargetPropertyValue(formattedValue);
        }

        private void SetTargetPropertyValueUsingConverter(object[] values)
        {
            var convertedValue = Converter.Convert(values, _bindablePropertyInfo.PropertyType, ConverterParameter, ConverterLanguage);

            if (convertedValue == null)
            {
                convertedValue = TargetNullValue ?? FallbackValue;
            }
            else if (convertedValue == DependencyProperty.UnsetValue)
            {
                convertedValue = FallbackValue;
            }
            else
            {
                convertedValue = ChangeType(convertedValue, _bindablePropertyInfo.PropertyType);
            }

            SetTargetPropertyValue(convertedValue);
        }

        private void SetTargetPropertyValue(object targetPropertyValue)
        {
            using (DisableableTargetPropertyValueChangedCallback.Disable())
            {
                _bindablePropertyInfo.SetValue(_associatedObject, targetPropertyValue);
            }
        }

        private static object ChangeType(object value, Type conversionType)
        {
            var valueType = value.GetType();
            var valueTypeInfo = valueType.GetTypeInfo();
            var isCompatible = valueType == conversionType || valueTypeInfo.IsSubclassOf(conversionType);

            return isCompatible ? value
                : (conversionType == typeof (String) ? value.ToString() : Convert.ChangeType(value, conversionType));
        }

        private object GetDefaultValueForTargetProperty()
        {
            var propertyType = _bindablePropertyInfo.PropertyType;
            var propertyTypeInfo = propertyType.GetTypeInfo();

            return propertyTypeInfo.IsValueType
                ? Activator.CreateInstance(propertyType)
                : (propertyType == typeof(String) ? String.Empty : null);
        }

        private bool CheckIfBindingModeIsValid(BindingMode mode)
        {
            switch (mode)
            {
                case BindingMode.OneTime:
                case BindingMode.OneWay:
                    return _bindablePropertyInfo.CanWrite;
                case BindingMode.TwoWay:
                    return _bindablePropertyInfo.CanRead && _bindablePropertyInfo.CanWrite;
                default:
                    throw new ArgumentException("Unknown binding mode.", "mode");
            }
        }

        private static void OnTargetPropertyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisableableTargetPropertyValueChangedCallback.OnPropertyChanged(d, e);
        }

        private static void NotifyOnTargetPropertyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var multiBinding = (MultiBinding)d;
            multiBinding.AssociatedObjectOnTargetPropertyValueChanged();
        }

        private IBindablePropertyInfo GetBindablePropertyInfo()
        {
            if (AttachedPropertyOwnerTypeProvider == null)
            {
                return new DependencyPropertyInfo(_associatedObject.GetType(), TargetPropertyPath.Path);
            }
            else
            {
                return new AttachedPropertyInfo(AttachedPropertyOwnerTypeProvider.GetType(), TargetPropertyPath.Path);
            }
        }
    }
}