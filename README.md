# WinRTMultiBinding
Provides MultiBinding functionality for Windows 8.1, Windows Phone 8.1, Windows 10 UWP projects.

WinRT MultiBinding supports most of WPF MultiBinding's features.

### Installation via NuGet package

This portable library is available as NuGet package for Windows 8.1, Windows Phone 8.1, Windows 10 UWP projects:

https://www.nuget.org/packages/WinRTMultibinding.Universal/1.0.0

Use Package Manager to install package or type the following into the Package Manager Console:

```
Install-Package WinRTMultibinding.Universal
```

#### Installing obsolete packages

This library is also available as obsolete NuGet packages for Windows and Windows Phone projects:

https://www.nuget.org/packages/WinRT-Multibinding-Windows/

https://www.nuget.org/packages/WinRT-Multibinding-WindowsPhone/

Use Package Manager to install packages or type the following into the Package Manager Console:

```
Install-Package WinRT-Multibinding-Windows

Install-Package WinRT-Multibinding-WindowsPhone
```

<b>NOTE:</b> As these packages are obsolete, they are no longer supported. So, you should better install portable library version described above for use in your Windows 8.1/Windows Phone 8.1 projects.

### General
This library provides you <b>MultiBindingHelper.MultiBindings</b> attached property which you should initialize with a <b>MultiBindingCollection</b> instance which you should populate with <b>MultiBinding</b> items. Instance of <b>MultiBinding</b> class hosts <b>Binding</b> items.

```xaml
<Page xmlns:m="using:WinRTMultibinding.Foundation.Data">
  <Page.Resources>
    <MyMultiValueConverter x:Key="MyMultiValueConverter" />
    <MyConverter x:Key="MyConverter" />
    <SomeDataProvider x:Key="SomeDataProvider" />
  </Page.Resources>
  
  <Button x:Name="MyButton" Content="Button content" />
  
  <TextBlock>
    <m:MultiBindingHelper.MultiBindings>
      <m:MultiBindingCollection>
        <m:MultiBinding TargetProperty="Text" Converter="{StaticResource MyMultiValueConverter}">
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

### Features
  - Various Sources (ElementName, Source, RelativeSource, DataContext)
  - StringFormat
  - Converter
  - TargetNullValue
  - FallbackValue
  - Different Modes (OneTime, OneWay, TwoWay)
  - UpdateSourceTrigger
  - Attached properties binding

### Restrictions

1)  <strong>RelativeSource.TemplatedParent</strong> and <strong>RelativeSouce.None</strong> are <strong>not</strong> yet <strong>supported</strong>.

2) <strong>UpdateSourceTrigger</strong> - There are only PropertyChanged and Explicit modes. <strong>PropertyChanged</strong> is set by <strong>default</strong>.
  To update source explicitly use <strong>GetMultiBindingExpression()</strong> extension method. It returns <strong>MultiBindingExpression</strong> object which has <strong>UpdateSource()</strong> method.
  
3) As you can see, you do not set directly MultinBinding instance to target property, but use attached property instead. So, there are no restrictions to set built-in single Binding to the same property, <b>but</b> this binding will be ignored. For example if you do this:
```xaml
<TextBlock Text="{Binding Name}">
  <m:MultiBindingHelper.MultiBindings>
    <m:MultiBindingCollection>
      <m:MultiBinding TargetProperty="Text" Converter="{StaticResourc MyMultiValueConverter}">
        <m:Binding Path="First" />
        <m:Binding Path="Second" />
      </m:MultiBinding>
    </m:MultiBindingCollection>
  </m:MultiBindingHelper.MultiBindings>
</TextBlock>
```

In this case `Text="{Binding Name}"` binding will be ignored.

### Examples

##### Sources
As you've seen above WinRTMultiBinding supports different binding sources. <b>But</b>: RelativeSource supports only Self mode.

```xaml
<TextBlock>
    <m:MultiBindingHelper.MultiBindings>
      <m:MultiBindingCollection>
        <m:MultiBinding TargetProperty="Text" Converter="{StaticResource MyMultiValueConverter}">
          <m:Binding Path="First" Converter="{StaticResource MyConverter}" />
          <m:Binding Path="Second" Source="{StaticResource SomeDataProvider}" />
          <m:Binding Path="Foreground" RelativeSource="{RelativeSource Self}" />
          <m:Binding Path="Content" ElementName="MyButton" />
        </m:MultiBinding>
      </m:MultiBindingCollection>
    </m:MultiBindingHelper.MultiBindings>
  </TextBlock>
```

##### Using StringFormat
```xaml
<TextBlock>
  <m:MultiBindingHelper.MultiBindings>
    <m:MultiBindingCollection>
      <m:MultiBinding TargetProperty="Text" StringFormat="{}{0} - {1}">
        <m:Binding Path="First" />
        <m:Binding Path="Second" />
      </m:MultiBinding>
    </m:MultiBindingCollection>
  </m:MultiBindingHelper.MultiBindings>
</TextBlock>
```

##### Using Converter
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
      <m:MultiBinding TargetProperty="Text" Converter="{StaticResource MyMultiValueConverter}">
        <m:Binding Path="First" />
        <m:Binding Path="Second" />
        <m:Binding Path="Third" />
      </m:MultiBinding>
    </m:MultiBindingCollection>
  </m:MultiBindingHelper.MultiBindings>
</TextBlock>
```

##### Modes
MultiBinding's BindingMode(<b>OneWay</b> by default) used as the default value for all the bindings in the collection unless an individual binding overrides this property. For example, if the Mode property on the MultiBinding object is set to TwoWay, then all the bindings in the collection are considered TwoWay unless you set a different Mode value on one of the bindings explicitly. Child bindings can only limit parent Mode, but not vice versa(it's okay to have MultiBinding's Mode set to TwoWay and one of the Bindings Mode set to OneWay, but <b>NOT</b> MultiBinding's Mode set to OneWay and one of the Bindings Mode set to TwoWay).

```xaml
<TextBox>
    <m:MultiBindingHelper.MultiBindings>
        <m:MultiBindingCollection>
            <m:MultiBinding TargetProperty="Text" Mode="TwoWay" Converter="{StaticResource MyMultiValueConverter}">
                <m:Binding Path="First" />
                <m:Binding Path="Second" Mode="OneWay" />
                <m:Binding Path="Third" />
            </m:MultiBinding>
        </m:MultiBindingCollection>
    </m:MultiBindingHelper.MultiBindings>
</TextBox>
```

##### TargetNullValue
If you specify TargetNullValue it's returned when your Converter returns null.

```xaml
<TextBlock>
    <m:MultiBindingHelper.MultiBindings>
        <m:MultiBindingCollection>
            <m:MultiBinding TargetProperty="Text" TargetNullValue="Null value" Converter="{StaticResource MyMultiValueConverter}">
                <m:Binding Path="First" />
                <m:Binding Path="Second" />
            </m:MultiBinding>
        </m:MultiBindingCollection>
    </m:MultiBindingHelper.MultiBindings>
</TextBlock>
```

##### FallbackValue
If you specify FallbackValue it's returned when:
  - you specified both <b>StringFormat</b> and <b>Converter</b> to target <b>string</b> property
  - your Converter returned null, but TargetNullValue is not specified
  - your Converter returned <b>DependencyProperty.UnsetValue</b>

If you did not specify FallbackValue it contains target property type's default value. So, it's always initialized.

```xaml
<TextBlock>
    <m:MultiBindingHelper.MultiBindings>
        <m:MultiBindingCollection>
            <m:MultiBinding TargetProperty="Text" FallbackValue="Fallback value" Converter="{StaticResource MyMultiValueConverter}">
                <m:Binding Path="First" />
                <m:Binding Path="Second" />
            </m:MultiBinding>
        </m:MultiBindingCollection>
    </m:MultiBindingHelper.MultiBindings>
</TextBlock>
```

##### UpdateSourceTrigger
If you chose Default or PropertyChanged, your source value is updated every time target property changes.
If you chose Explicit:

```xaml
<TextBox x:Name="MyTextBox">
    <m:MultiBindingHelper.MultiBindings>
        <m:MultiBindingCollection>
            <m:MultiBinding TargetProperty="Text" Mode="TwoWay" Converter="{StaticResource MyMultiValueConverter}"
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

##### Attached properties binding
To bind to attached property you should specify ```AttachedPropertyOwnerTypeProvider```, and set ```TargetProperty``` to target property name <b>without</b> attached property owner type name. So, binding to ```Grid.Row``` attached property will look as folowing:

```xaml
<Page xmlns:m="using:WinRTMultibinding.Foundation.Data">
  <Page.Resources>
    <MyMultiValueConverter x:Key="MyMultiValueConverter" />
    <GridTypeProvider x:Key="GridTypeProvider" />
  </Page.Resources>
  
  <Grid>
    <Grid.RowDefinitions>
     <RowDefinition />
     <RowDefinition />
     <RowDefinition />
    </Grid.RowDefinitions>
    
    <TextBox Text="Binding to Grid.Row attached property">
      <m:MultiBindingHelper.MultiBindings>
        <m:MultiBindingCollection>
          <m:MultiBinding AttachedPropertyOwnerTypeProvider="{StaticResource GridTypeProvider}" TargetProperty="Row"
                          Converter="{StaticResource MyMultivalueConverter}" Mode="OneWay">
            <m:Binding Path="First" />
            <m:Binding Path="Second" />
            <m:Binding Path="Third" />
          </m:MultiBinding>
        </m:MultiBindingCollection>
      </m:MultiBindingHelper.MultiBindings>
    </TextBox>
  </Grid>
</Page>
```

###### Introducing Type Providers
In the previous example ```GridTypeProvider``` is a class derived from ```TypeProvider<T>``` abstract class. This class has method ```GetType``` which returns the type of its generic type parameter (explicit implementation of ```ITypeProvider``` interface). The only purpose of this class, as you can see, is to provide attached property owner type. You only need to inherit from ```TypeProvider<T>``` with an attached property owner type specified as a generic type parameter, so that later you can use it in XAML. ```GridTypeProvider``` might look like this:

```csharp
public class GridTypeProvider : TypeProvider<Grid>
{

}
```

Then you should instantiate it in XAML and pass as a value for ```AttachedPropertyOwnerTypeProvider``` property, as shown in the example above.

###### Type Providers based solution overview
This awkward and clumsy interface based on type providers might not seem clear at a first glance. But it has the greatest advantage: it's type and assembly independent. This means it allows you to bind not only to built-in controls (such as Grid, Scrollviewer) which have attached properties, but also to custom attached properties hosted by custom types.

For example, if we would like to bind to ```MyAttachedProperty``` property which is hosted by ```MyAttachedPropertyOwner``` class, we should simply derive from ```TypeProvider<T>``` in the following manner:

```csharp
public class MyAttachedPropertyOwnerTypeProvider : TypeProvider<MyAttachedPropertyOwner>
{

}
```

and set it as ```AttachedPropertyOwnerTypeProvider``` for ```MultiBinding``` item:

```xaml
<Page xmlns:m="using:WinRTMultibinding.Foundation.Data">
  <Page.Resources>
    <MyMultiValueConverter x:Key="MyMultiValueConverter" />
    <MyAttachedPropertyOwnerTypeProvider x:Key="MyAttachedPropertyOwnerTypeProvider" />
  </Page.Resources>
  
  <Grid>
    <Grid.RowDefinitions>
     <RowDefinition />
     <RowDefinition />
     <RowDefinition />
    </Grid.RowDefinitions>
    
    <TextBox Text="Binding to Grid.Row attached property">
      <m:MultiBindingHelper.MultiBindings>
        <m:MultiBindingCollection>
          <m:MultiBinding AttachedPropertyOwnerTypeProvider="{StaticResource MyAttachedPropertyOwnerTypeProvider}"
                          TargetProperty="MyAttachedProperty" Converter="{StaticResource MyMultivalueConverter}" Mode="OneWay">
            <m:Binding Path="First" />
            <m:Binding Path="Second" />
            <m:Binding Path="Third" />
          </m:MultiBinding>
        </m:MultiBindingCollection>
      </m:MultiBindingHelper.MultiBindings>
    </TextBox>
  </Grid>
</Page>
```
So this weird interface is payment for allowing you binding to any attached property you want.

Pros:
  - possibility to bind to any attached property hosted by any type

Cons:
  - cumbersome, clumsy interface (which is fixable by good code styles, naming conventions etc.)

##### Multiple properties binding
Simply add several <b>MultiBinding</b> items to <b>MultiBindingCollection</b>.

```xaml
<TextBox>
    <m:MultiBindingHelper.MultiBindings>
        <m:MultiBindingCollection>
            <m:MultiBinding TargetProperty="Text" Converter="{StaticResource MyMultiValueConverter}">
              <m:Binding Path="First" />
              <m:Binding Path="Second" />
            </m:MultiBinding>
            <m:MultiBinding TargetProperty="FontSize" Converter="{StaticResource MyAnotherMultiValueConverter}">
              <m:Binding Path="Third" />
              <m:Binding Path="Fourth" />
        </m:MultiBindingCollection>
    </m:MultiBindingHelper.MultiBindings>
</TextBox>
```

#### Support
If you do have a contribution for the package feel free to put up a Pull Request or open Issue.
