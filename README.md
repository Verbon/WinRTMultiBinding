# WinRTMultiBinding
Provides MultiBinding functionality for Windows 8.1 and Windows Phone 8.1 projects.

WinRT MultiBinding supports most of WPF MultiBinding's features.

###General
This library provides you <b>MultiBindingHelper.MultiBindings</b> attached property which you should initialize with a <b>MultiBindingCollection</b> instance which you should populate with <b>MultiBinding</b> items. Instance of <b>MultiBinding</b> class hosts <b>Binding</b> items.

```xaml
<Page xmlns:m="using:WinRTMultibinding">
  <Page.Resources>
    <SomeDataProvider x:Key="SomeDataProvider" />
  </Page.Resources>
  
  <Button x:Name="MyButton" Content="Button content" />
  
  <TextBlock>
    <m:MultiBindingHelper.MultiBindings>
      <m:MultiBindingCollection>
        <m:MultiBinding TargetPropertyPath="Text" Converter="{StaticResource MyMultiValueConverter}">
          <m:Binding Path="First" Converter="{StaticResource MyConverter}" />
          <m:Binding Path="Second" Source="{StaticResource SomeDataProvider}" />
          <m:Binding Path="Foreground" RelativeSource="{RelativeSource Self}" />
          <m:Binding Path="Content" ElementName="MyButton" />
        </m:MultiBinding>
      </m:MultiBindingCollection>
    </m:MultiBindingHelper.MultiBindings>
  </TextBlock>
</Page>
```

###Features
  - Various Sources (ElementName, Source, RelativeSource, DataContext)
  - StringFormat
  - Converter
  - TargetNullValue
  - FallbackValue
  - Different Modes (OneTime, OneWay, TwoWay)
  - UpdateSourceTrigger

###Restrictions

1)  <strong>RelativeSource.TemplatedParent</strong> and <strong>RelativeSouce.None</strong> are <strong>not</strong> yet <strong>supported</strong>.

2) <strong>UpdateSourceTrigger</strong> - There are only PropertyChanged and Explicit modes. <strong>PropertyChanged</strong> is set by <strong>default</strong>.
  To update source explicitly use <strong>GetMultiBindingExpression()</strong> extension method. It returns <strong>MultiBindingExpression</strong> object which has <strong>UpdateSource()</strong> method.

### Examples

######Sources
As you've seen above WinRTMultiBinding supports different binding sources. But: in <b>RelativeSource</b> only <b>Self</b> mode is <b>supported</b>.

```xaml
<TextBlock>
    <m:MultiBindingHelper.MultiBindings>
      <m:MultiBindingCollection>
        <m:MultiBinding TargetPropertyPath="Text" Converter="{StaticResource MyMultiValueConverter}">
          <m:Binding Path="First" Converter="{StaticResource MyConverter}" />
          <m:Binding Path="Second" Source="{StaticResource SomeDataProvider}" />
          <m:Binding Path="Foreground" RelativeSource="{RelativeSource Self}" />
          <m:Binding Path="Content" ElementName="MyButton" />
        </m:MultiBinding>
      </m:MultiBindingCollection>
    </m:MultiBindingHelper.MultiBindings>
  </TextBlock>
```

######Using StringFormat
```xaml
<TextBlock>
  <m:MultiBindingHelper.MultiBindings>
    <m:MultiBindingCollection>
      <m:MultiBinding TargetPropertyPath="Text" StringFormat="{}{0} - {1}">
        <m:Binding Path="First" />
        <m:Binding Path="Second" />
      </m:MultiBinding>
    </m:MultiBindingCollection>
  </m:MultiBindingHelper.MultiBindings>
</TextBlock>
```

######Using Converter
Custom converter must implement <b>IMultiValueConverter</b> interface.
```csharp
public interface IMultiValueConverter
{
    object Convert(object[] values, Type targetType, object parameter, string language);

    object[] ConvertBack(object value, Type[] targetTypes, object parameter, string language);
}
```

```xaml
<TextBlock>
  <m:MultiBindingHelper.MultiBindings>
    <m:MultiBindingCollection>
      <m:MultiBinding TargetPropertyPath="Text" Converter="{StaticResource MyMultiValueConverter}">
        <m:Binding Path="First" />
        <m:Binding Path="Second" />
        <m:Binding Path="Third" />
      </m:MultiBinding>
    </m:MultiBindingCollection>
  </m:MultiBindingHelper.MultiBindings>
</TextBlock>
```

######Modes
MultiBinding's BindingMode(<b>OneWay</b> by default) used as the default value for all the bindings in the collection unless an individual binding overrides this property. For example, if the Mode property on the MultiBinding object is set to TwoWay, then all the bindings in the collection are considered TwoWay unless you set a different Mode value on one of the bindings explicitly. Child bindings can only limit parent Mode, but not vice versa(it's okay to have MultiBinding's Mode set to TwoWay and one of the Bindings Mode set to OneWay, but <b>NOT</b> MultiBinding's Mode set to OneWay and one of the Bindings Mode set to TwoWay).

```xaml
<TextBox>
    <m:MultiBindingHelper.MultiBindings>
        <m:MultiBindingCollection>
            <m:MultiBinding TargetPropertyPath="Text" Mode="TwoWay" Converter="{StaticResource MyMultiValueConverter}">
                <m:Binding Path="First" />
                <m:Binding Path="Second" Mode="OneWay" />
                <m:Binding Path="Third" />
            </m:MultiBinding>
        </m:MultiBindingCollection>
    </m:MultiBindingHelper.MultiBindings>
</TextBox>
```

######TargetNullValue
If you specify TargetNullValue it's returned when your Converter returns null.

```xaml
<TextBlock>
    <m:MultiBindingHelper.MultiBindings>
        <m:MultiBindingCollection>
            <m:MultiBinding TargetPropertyPath="Text" TargetNullValue="Null value" Converter="{StaticResource MyMultiValueConverter}">
                <m:Binding Path="First" />
                <m:Binding Path="Second" />
            </m:MultiBinding>
        </m:MultiBindingCollection>
    </m:MultiBindingHelper.MultiBindings>
</TextBlock>
```

######FallbackValue
If you specify FallbackValue it's returned when:
  - you specified both <b>StringFormat</b> and <b>Converter</b> to target <b>string</b> property
  - your Converter returned null, but TargetNullValue is not specified
  - your Converter returned DependencyProperty.UnsetValue

If you did not specify FallbackValue it contains target property type's default value. So, it's always initialized.

```xaml
<TextBlock>
    <m:MultiBindingHelper.MultiBindings>
        <m:MultiBindingCollection>
            <m:MultiBinding TargetPropertyPath="Text" FallbackValue="Fallback value" Converter="{StaticResource MyMultiValueConverter}">
                <m:Binding Path="First" />
                <m:Binding Path="Second" />
            </m:MultiBinding>
        </m:MultiBindingCollection>
    </m:MultiBindingHelper.MultiBindings>
</TextBlock>
```

######UpdateSourceTrigger
If you chose Default or PropertyChanged, your source value is updated every time target property changes.
If you chose Explicit:

```xaml
<TextBox>
    <m:MultiBindingHelper.MultiBindings>
        <m:MultiBindingCollection>
            <m:MultiBinding TargetPropertyPath="Text" Mode="TwoWay" Converter="{StaticResource MyMultiValueConverter}"
                            UpdateSourceTrigger="Explicit">
                <m:Binding Path="First" />
                <m:Binding Path="Second" />
            </m:MultiBinding>
        </m:MultiBindingCollection>
    </m:MultiBindingHelper.MultiBindings>
</TextBox>
```

Do this:

```csharp
var multiBindingExpression = MyTextBox.GetMultiBindingExpression(TextBox.TextProperty);
multiBindingExpression.UpdateSource();
```

######Multiple properties binding
Simply add several <b>MultiBinding</b> items to <b>MultiBindingCollection</b>.

```xaml
<TextBox>
    <m:MultiBindingHelper.MultiBindings>
        <m:MultiBindingCollection>
            <m:MultiBinding TargetPropertyPath="Text" Converter="{StaticResource MyMultiValueConverter}">
              <m:Binding Path="First" />
              <m:Binding Path="Second" />
            </m:MultiBinding>
            <m:MultiBinding TargetPropertyPath="FontSize" Converter="{StaticResource MyAnotherMultiValueConverter}">
              <m:Binding Path="Third" />
              <m:Binding Path="Fourth" />
        </m:MultiBindingCollection>
    </m:MultiBindingHelper.MultiBindings>
</TextBox>
```
