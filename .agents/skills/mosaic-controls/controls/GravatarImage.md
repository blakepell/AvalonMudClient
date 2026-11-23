# GravatarImage

**Base class:** `Image`  
**Namespace:** `Mosaic.UI.Wpf.Controls`  
**Source:** `src/Mosaic.UI.Wpf/Controls/GravatarImage/GravatarImage.cs`  
**Example:** `src/MosaicWpfDemo/Views/Examples/GravatarImageExample.xaml`

## Description

Extends `Image` to automatically download and display a [Gravatar](https://gravatar.com) avatar given an email address. The image URL is built from the MD5 hash of the email and fetched from `secure.gravatar.com`.

## Key Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Email` | `string` | `null` | The email address whose Gravatar to load. Changing this reloads the image. |
| `Size` | `int` | `80` | Pixel size requested from Gravatar (1–2048). |
| `DefaultImage` | `string` | `"mp"` | Gravatar fallback type: `"mp"` (mystery person), `"identicon"`, `"monsterid"`, `"wavatar"`, `"retro"`, `"robohash"`, `"blank"`, or a URL. |
| `Rating` | `string` | `"g"` | Gravatar rating filter: `"g"`, `"pg"`, `"r"`, or `"x"`. |

## XAML Example

```xml
xmlns:mosaic="clr-namespace:Mosaic.UI.Wpf.Controls;assembly=Mosaic.UI.Wpf"

<mosaic:GravatarImage
    Width="64" Height="64"
    Email="{Binding UserEmail}"
    Size="128"
    DefaultImage="identicon" />
```

## Notes

- The image is fetched asynchronously; no loading placeholder is shown by default. Wrap in a `Grid` with a default image underneath if a placeholder is needed.
- The control uses `BitmapImage` with `BitmapCacheOption.OnLoad` to avoid holding the HTTP stream open.
- If `Email` is null or empty, the control uses the `DefaultImage` fallback directly.
