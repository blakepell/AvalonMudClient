# DatePicker

**Base class:** `Control`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/DatePicker/DatePicker.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/DatePickerExample.xaml`

## Description

A Mosaic date picker with a toggle popup calendar. It displays a month grid, supports previous/next month navigation, and exposes the selected date as a nullable `DateTime`.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `SelectedDate` | `DateTime?` | `null` | Selected date, suitable for two-way binding. |
| `CurrentMonth` | `DateTime` | default | Month currently displayed by the popup calendar. |
| `KeepPopupOpen` | `bool` | `true` | Keeps the popup open after selecting a date. |

```xml
<mosaic:DatePicker
    SelectedDate="{Binding DueDate, Mode=TwoWay}"
    KeepPopupOpen="False" />
```

## Notes

Template parts include `PART_Popup`, `PART_Switch`, `PART_ListBox`, `PART_Left`, and `PART_Right`.
