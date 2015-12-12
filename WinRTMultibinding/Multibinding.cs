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
    public class Multibinding : DependencyObject
    {
        private static readonly DependencyProperty TargetPropertyValueProperty = DependencyProperty.Register("TargetPropertyValue", typeof (object), typeof (Multibinding), new PropertyMetadata(default(object), OnTargetPropertyValueChanged));


        private PropertyInfo _targetPropertyInfo;
        private FrameworkElement _associatedObject;


        private IEnumerable<IMultibindingItem> MultibindingItems => Bindings;

        public PropertyPath BindingPropertyPath { get; set; }

        public BindingMode Mode { get; set; }

        public IMultivalueConverter Converter { get; set; }

        public object ConverterParameter { get; set; }

        public string ConverterLanguage { get; set; }

        public List<Binding> Bindings { get; set; }


        public Multibinding()
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
            Initialize();
        }


        private void Initialize()
        {
            _targetPropertyInfo = _associatedObject.GetType().GetRuntimeProperty(BindingPropertyPath.Path);
            Bindings.ForEach(binding => binding.Mode = Mode);
            MultibindingItems.ForEach(item => item.Initialize(_associatedObject));

            switch (Mode)
            {
                case BindingMode.OneTime:
                case BindingMode.OneWay:
                    MakeOneWayBinding();
                    break;
                case BindingMode.TwoWay:
                    MakeOneWayBinding();
                    MakeOneWayToSourceBinding();
                    break;
            }
        }

        private void MakeOneWayBinding()
        {
            MultibindingItems.ForEach(item => item.ComputedValueChanged += MultibindingItemOnComputedValueChanged);
        }

        private void MakeOneWayToSourceBinding()
        {
            var binding = new Windows.UI.Xaml.Data.Binding { Source = _associatedObject, Path = BindingPropertyPath };
            BindingOperations.SetBinding(this, TargetPropertyValueProperty, binding);
        }

        private void MultibindingItemOnComputedValueChanged(object sender, EventArgs e)
        {
            var values = MultibindingItems.Select(item => item.ComputedValue).ToArray();
            var convertedValue = Converter.Convert(values, _targetPropertyInfo.PropertyType, ConverterParameter, ConverterLanguage);
            convertedValue = ChangeType(convertedValue, _targetPropertyInfo.PropertyType);

            _targetPropertyInfo.SetValue(_associatedObject, convertedValue);
        }

        private static void OnTargetPropertyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var multibinding = (Multibinding) d;
            multibinding.AssociatedObjectOnTargetPropertyChanged();
        }

        private void AssociatedObjectOnTargetPropertyChanged()
        {
            if (MultibindingItems.Any(item => item.ComputedValue == null))
            {
                return;
            }

            var value = _targetPropertyInfo.GetValue(_associatedObject);
            var targetTypes = MultibindingItems.Select(item => item.ComputedValue.GetType()).ToArray();
            var values = Converter.ConvertBack(value, targetTypes, ConverterParameter, ConverterLanguage);

            MultibindingItems.ForEach((item, index) => item.ComputedValue = values[index]);
        }

        private static object ChangeType(object value, Type type)
        {
            var valueType = value.GetType().GetTypeInfo();
            var isCompatible = valueType.IsSubclassOf(type);

            return isCompatible ? value : Convert.ChangeType(value, type);
        }
    }
}