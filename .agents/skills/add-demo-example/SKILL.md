---
name: add-demo-example
description: "Add a new control example to MosaicWpfDemo. Use when: adding an example, creating a demo, adding a side menu entry, adding a new example view, registering a control demo, adding LinkedSource entries to the csproj."
argument-hint: "Name of the control to add an example for (e.g. 'WindowTitleBar')"
---

# Add Demo Example

Adds a fully wired-up control example to the `MosaicWpfDemo` project. This involves four coordinated changes:

1. Create `{Name}Example.xaml` + `{Name}Example.xaml.cs` in `src/MosaicWpfDemo/Views/Examples/`
2. Register two `EmbeddedResource` LinkedSource entries in `MosaicWpfDemo.csproj`
3. Add a `<mosaic:SideMenuItem>` block in `src/MosaicWpfDemo/MainWindow.xaml`
4. Verify the project builds with 0 errors

---

## Step 1 — Gather Information

Before writing any code, collect:

- **Control name** — the argument passed to the skill, or ask the user (e.g. `Avatar`, `WindowTitleBar`).
- **Display label** — the menu text (defaults to the control name; may contain spaces, e.g. `Side Menu`).
- **Icon** — check `src/MosaicWpfDemo/Assets/` for an existing `*-48.png` that fits. If none fits, use `collage-48.png` as a fallback. Ask the user if they want a specific icon.
- **Fully-qualified control type** — needed for `DocumentationType`, e.g. `mosaic:Avatar`. Inspect `src/Mosaic.UI.Wpf/Controls/` to find the class and its namespace.
- **Example content** — if the user specifies behaviour or wants a working demo, inspect the control's source and existing examples (e.g. `BadgeExample.xaml`) to understand patterns before writing XAML. If no content is specified, produce an empty scaffold.

---

## Step 2 — Create the Example UserControl

### `{Name}Example.xaml`
Path: `src/MosaicWpfDemo/Views/Examples/{Name}Example.xaml`

```xml
<UserControl
    x:Class="MosaicWpfDemo.Views.Examples.{Name}Example"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:controls="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:local="clr-namespace:MosaicWpfDemo.Views.Examples"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    d:DesignHeight="450"
    d:DesignWidth="800"
    mc:Ignorable="d">
    <!-- Example content here -->
</UserControl>
```

### `{Name}Example.xaml.cs`
Path: `src/MosaicWpfDemo/Views/Examples/{Name}Example.xaml.cs`

```csharp
/*
 * Mosaic UI for WPF
 *
 * @project lead      : Blake Pell
 * @website           : https://www.blakepell.com
 * @website           : https://www.apexgate.net
 * @copyright         : Copyright (c), 2023-2025 All rights reserved.
 * @license           : MIT - https://opensource.org/license/mit/
 */

namespace MosaicWpfDemo.Views.Examples
{
    public partial class {Name}Example
    {
        public {Name}Example()
        {
            InitializeComponent();
        }
    }
}
```

> If the user requests a working example with behaviour, add the necessary event handlers and using directives to the code-behind. Keep the file header.

---

## Step 3 — Register LinkedSources in the csproj

File: `src/MosaicWpfDemo/MosaicWpfDemo.csproj`

Add the following two `EmbeddedResource` items inside the existing `<!-- Embedded Resources and Linked Embedded Resources -->` `<ItemGroup>`. Insert them in alphabetical order by control name to keep the file tidy.

```xml
<EmbeddedResource Include="Views\Examples\{Name}Example.xaml">
    <Link>LinkedSources\{Name}Example.xaml</Link>
</EmbeddedResource>
<EmbeddedResource Include="Views\Examples\{Name}Example.xaml.cs">
    <Link>LinkedSources\{Name}Example.xaml.cs</Link>
</EmbeddedResource>
```

The `Link` path must begin with `LinkedSources\` and match the `XamlFile` / `CodeFile` parameter values used in the next step (`MosaicWpfDemo.LinkedSources.{Name}Example.xaml`).

---

## Step 4 — Add the SideMenuItem to MainWindow.xaml

File: `src/MosaicWpfDemo/MainWindow.xaml`

Insert the following block inside `<mosaic:SideMenu.MenuItems>`. Keep the entries in roughly the same alphabetical order as the existing items.

```xml
<!--  {DisplayLabel}  -->
<mosaic:SideMenuItem
    ContentType="{x:Type views:ShellView}"
    ContentTypeIsSingleton="True"
    ImageSource="/Assets/{icon}-48.png"
    Text="{DisplayLabel}">
    <mosaic:SideMenuItem.ParameterCollection>
        <mosaic:SideMenuParameterCollection>
            <mosaic:SideMenuParameter Key="Title" Value="{DisplayLabel}" />
            <mosaic:SideMenuParameter Key="XamlFile" Value="MosaicWpfDemo.LinkedSources.{Name}Example.xaml" />
            <mosaic:SideMenuParameter Key="CodeFile" Value="MosaicWpfDemo.LinkedSources.{Name}Example.xaml.cs" />
            <mosaic:SideMenuParameter Key="DocumentationType" Value="{x:Type mosaic:{ControlType}}" />
            <mosaic:SideMenuParameter Key="ExampleType" Value="{x:Type example:{Name}Example}" />
            <mosaic:SideMenuParameter Key="ImageSource" Value="/Assets/{icon}-48.png" />
        </mosaic:SideMenuParameterCollection>
    </mosaic:SideMenuItem.ParameterCollection>
</mosaic:SideMenuItem>
```

**Parameter reference:**

| Placeholder | Example value | Notes |
|---|---|---|
| `{Name}` | `Avatar` | PascalCase, no spaces, used in all file/type references |
| `{DisplayLabel}` | `Avatar` or `Side Menu` | The text shown in the sidebar |
| `{icon}` | `avatar` | Filename stem of the 48 px PNG in `Assets/` (without `-48.png`) |
| `{ControlType}` | `Avatar` | The short type name used with the `mosaic:` XML namespace |

> `DocumentationType` drives the auto-generated XML documentation shown in the Shell view. Set it to the primary control being demonstrated. If more than one control is shown, use the most representative one.

---

## Step 5 — Verify

Run:
```
dotnet build src/MosaicWpfDemo/MosaicWpfDemo.csproj -c Debug
```

Confirm **0 errors**. Common issues:

| Symptom | Fix |
|---|---|
| `{Name}Example` type not found | Confirm the `x:Class` in the XAML matches the namespace/class in the code-behind |
| `LinkedSources` resource not found at runtime | Check the `<Link>` value uses backslashes and the `Key` values in the `SideMenuParameter` use dots |
| `DocumentationType` XAML parse error | Ensure the control class is public and the `mosaic:` namespace is declared in `MainWindow.xaml` |
| Build warning about duplicate resources | The same file was added twice in the csproj — remove the duplicate |

---

## Quick Checklist

- [ ] `{Name}Example.xaml` created with correct `x:Class`
- [ ] `{Name}Example.xaml.cs` created with matching namespace and class name
- [ ] Two `EmbeddedResource` entries added to `MosaicWpfDemo.csproj`
- [ ] `<mosaic:SideMenuItem>` block added to `MainWindow.xaml`
- [ ] All six `SideMenuParameter` keys present (`Title`, `XamlFile`, `CodeFile`, `DocumentationType`, `ExampleType`, `ImageSource`)
- [ ] Build passes with 0 errors
