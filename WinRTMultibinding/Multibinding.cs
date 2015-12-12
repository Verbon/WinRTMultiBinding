using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using WinRTMultibinding.Extensions;
using WinRTMultibinding.Interfaces;

namespace WinRTMultibinding
{
    [ContentProperty(Name = nameof(Bindings))]
    public class Multibinding
    {
        private PropertyInfo _targetPropertyInfo;
        private FrameworkElement _associatedObject;


        private IEnumerable<IMultibindingItem> MultibindingItems => Bindings;

        public PropertyPath BindingPropertyPath { get; set; }

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
            MultibindingItems.ForEach(item => item.Initialize(_associatedObject));
            MultibindingItems.ForEach(item => item.ComputedValueChanged += MultibindingItemOnComputedValueChanged);
        }

        private void MultibindingItemOnComputedValueChanged(object sender, EventArgs e)
        {
            var values = MultibindingItems.Select(item => item.ComputedValue).ToArray();
            var convertedValue = Converter.Convert(values, _targetPropertyInfo.PropertyType, ConverterParameter, ConverterLanguage);

            TryChangeTypeTo(_targetPropertyInfo.PropertyType, ref convertedValue);
            _targetPropertyInfo.SetValue(_associatedObject, convertedValue);
        }

        private static bool TryChangeTypeTo(Type type, ref object value)
        {
            try
            {
                var convertedValue = Convert.ChangeType(value, type);
                value = convertedValue;

                return true;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
    }
}