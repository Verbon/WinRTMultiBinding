using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using WinRTMultibinding.Extensions;
using WinRTMultibinding.Interfaces;

namespace WinRTMultibinding
{
    [ContentProperty(Name = nameof(Bindings))]
    public class MultiBinding : BindingBase
    {
        private static readonly DependencyProperty TargetPropertyValueProperty = DependencyProperty.Register("TargetPropertyValue", typeof (object), typeof (MultiBinding), new PropertyMetadata(default(object), OnTargetPropertyValueChanged));


        private static readonly DisableablePropertyChangedCallback DisableableTargetPropertyValueChangedCallback;
        private PropertyInfo _targetPropertyInfo;
        private FrameworkElement _associatedObject;


        private IEnumerable<IMultibindingItem> MultibindingItems => Bindings;

        private IEnumerable<IOneWayMultibindingItem> OneWayMultibindingItems => Bindings;

        private IEnumerable<IOneWayToSourceMultibindingItem> OneWayToSourceMultibindingItems => Bindings; 

        public PropertyPath BindingPropertyPath { get; set; }

        public BindingMode Mode { get; set; }

        public IMultiValueConverter Converter { get; set; }

        public object ConverterParameter { get; set; }

        public string ConverterLanguage { get; set; }

        public List<Binding> Bindings { get; }


        static MultiBinding()
        {
            DisableableTargetPropertyValueChangedCallback = new DisableablePropertyChangedCallback(NotifyOnTargetPropertyValueChanged);
        }

        public MultiBinding()
        {
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


        private void Initialize()
        {
            _targetPropertyInfo = _associatedObject.GetType().GetRuntimeProperty(BindingPropertyPath.Path);

            if (!CheckIfCanApplyBinding(_targetPropertyInfo, Mode))
            {
                throw new InvalidOperationException($"Unable to attach binding to {_targetPropertyInfo.Name} property using {Mode} mode.");
            }

            Bindings.Where(binding => binding.Mode == default(BindingMode) || binding.Mode > Mode)
                .ForEach(binding => binding.Mode = Mode);
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
            using (DisableableTargetPropertyValueChangedCallback.Disable())
            {
                var binding = new Windows.UI.Xaml.Data.Binding { Source = _associatedObject, Path = BindingPropertyPath };
                BindingOperations.SetBinding(this, TargetPropertyValueProperty, binding);
            }
        }

        private void TriggerInitialOneWayBinding()
        {
            OneWayMultibindingItemOnSourcePropertyValueChanged(this, EventArgs.Empty);
        }

        private void OneWayMultibindingItemOnSourcePropertyValueChanged(object sender, EventArgs e)
        {
            var values = OneWayMultibindingItems.Select(item => item.SourcePropertyValue).ToArray();
            var convertedValue = Converter.Convert(values, _targetPropertyInfo.PropertyType, ConverterParameter, ConverterLanguage);
            convertedValue = ChangeType(convertedValue, _targetPropertyInfo.PropertyType);

            SetTargetPropertyValue(convertedValue);
        }

        private static bool CheckIfCanApplyBinding(PropertyInfo targetProperty, BindingMode mode)
        {
            switch (mode)
            {
                case BindingMode.OneTime:
                case BindingMode.OneWay:
                    return targetProperty.CanWrite();
                case BindingMode.TwoWay:
                    return targetProperty.CanRead() && targetProperty.CanWrite();
            }

            throw new ArgumentException("Unknown binding mode.", "mode");
        }

        private static object ChangeType(object value, Type conversionType)
        {
            var valueType = value.GetType();
            var valueTypeInfo = valueType.GetTypeInfo();
            var isCompatible = valueType == conversionType || valueTypeInfo.IsSubclassOf(conversionType);

            return isCompatible ? value : Convert.ChangeType(value, conversionType);
        }

        private void SetTargetPropertyValue(object targetPropertyValue)
        {
            using (DisableableTargetPropertyValueChangedCallback.Disable())
            {
                _targetPropertyInfo.SetValue(_associatedObject, targetPropertyValue);
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

        private void AssociatedObjectOnTargetPropertyValueChanged()
        {
            var value = _targetPropertyInfo.GetValue(_associatedObject);
            var targetTypes = OneWayToSourceMultibindingItems.Select(item => item.SourcePropertyType).ToArray();
            var values = Converter.ConvertBack(value, targetTypes, ConverterParameter, ConverterLanguage);

            OneWayToSourceMultibindingItems.ForEach((item, index) =>
                {
                    if (item.Mode > BindingMode.OneWay)
                    {
                        item.OnTargetPropertyValueChanged(values[index]);
                    }
                });
        }
    }
}