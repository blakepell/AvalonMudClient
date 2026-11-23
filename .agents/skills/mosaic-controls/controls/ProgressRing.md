# ProgressRing

**Base class:** `Control`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/ProgressRing/ProgressRing.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/ProgressRingExample.xaml`

## Description

An indeterminate animated ring/spinner for long-running work. The template uses calculated `TemplateSettings` to size and position animated ellipses, switching between small and large visual states based on control size.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `IsActive` | `bool` | `false` | Starts/stops animation and active visual state. |
| `TemplateSettings` | `ProgressRingTemplateSettings` | read-only | Calculated ellipse diameter, offset, and max side length for the template. |

```xml
<mosaic:ProgressRing
    Width="48"
    Height="48"
    IsActive="{Binding IsBusy}" />
```
