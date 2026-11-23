# Themes
---

Themes let you customize the visual appearance of the client with different color schemes and styles for better readability.

There are two parts to the client's look, and they're set separately:

- The **application theme** controls the windows, menus, ribbon and tool windows around the game.
- The **terminal colors** control the game text itself, including how the colors a game sends are drawn on your screen.

## The Application Theme

Click **Theme** in the **UI** group on the **Game** tab of the ribbon to choose a theme. The available themes are:

| Group | Themes |
|-------|--------|
| **Standard Themes** | Black, Black (Lighter Borders), Blackest, Dark, Light, White |
| **Metro Themes** | Metro Black, Metro Dark, Metro Light, Metro White |
| **Office Colorful** | A light theme with a colored accent: Blue, Green, Indigo, Orange, Pink, Purple, Red, Teal or Yellow |
| **Office White** | A white theme with a colored accent: Blue, Green, Indigo, Orange, Pink, Purple, Red, Teal or Yellow |

The new theme is applied right away and remembered the next time the client starts.

The **Toggle Themes** button in the window's title bar switches quickly between the dark themes, in the order Dark, Metro Dark, Black and Metro Black.

Scripts can change the theme too, which is handy if you want a different look at night:

```javascript
ui.SetTheme("Light");
```

> **Tip:** The application theme doesn't change the terminal's background or text colors. Those are set with the terminal colors below, so you can use a light theme around a dark terminal or the other way around.

## Terminal Colors

Most games color their text by sending **ANSI color codes**, small instructions mixed into the text that say "the following text is bright red" or "go back to normal". There are only a limited number of these codes, and the game only names the color, not the exact shade. The client decides what each one looks like on your screen.

Terminal colors let you choose that shade for each color the game can send. For example, if the bright blue a game uses is hard to read on a black background, you can change **Blue** to a lighter blue and every piece of bright blue text from the game will use it.

### Changing the Colors

There are two ways to get to the terminal colors:

- Click **Theme** on the ribbon and choose **Edit Terminal Colors**. This opens the **Color Theme** tool window, which can be [docked](Layouts.md#the-docking-system) wherever you like while you experiment.
- Open the client settings (**Options** on the **File** menu, or **Client Settings** from the settings button in the window's title bar), find **Color Theme** in the **Terminal** category and expand it.

Select the color you want to change and choose a new one. Changes are applied as soon as you make them, so you can keep the game open next to the Color Theme window and see the result.

To go back to the original colors, click **Theme** on the ribbon and choose **Reset Terminal Colors**.

Terminal colors belong to the client rather than a [profile](Profiles.md), so they're used for every game you play. They're saved in `AppSettings.json` with the rest of your settings and are included in your [backups](Profiles.md#backups).

### The Terminal Color Settings

Each setting below is matched to an ANSI code. Many games also let you type colors with a short code in curly braces, such as `{R` for red. Avalon understands the same codes in text it writes itself, for example from `#echo` or `terminal.WriteLine` in a [script](Scripting.md).

| Setting | ANSI Code | Color Code | Default | Used For |
|---------|-----------|------------|---------|----------|
| **Background** | | | Black | The background of the game terminal and custom terminals. |
| **Default** | | | Light Gray | Text that has no color code at all. |
| **Clear** | `ESC[0m` | `{x` | Light Gray | Text after the game resets its colors. |
| **Red** | `ESC[1;31m` | `{R` | Red | Bright red. |
| **Dark Red** | `ESC[0;31m` | `{r` | Dark Red | Normal red. |
| **Green** | `ESC[1;32m` | `{G` | Lime | Bright green. |
| **Dark Green** | `ESC[0;32m` | `{g` | Green | Normal green. |
| **Yellow** | `ESC[1;33m` | `{Y` | Yellow | Bright yellow. |
| **Dark Yellow** | `ESC[0;33m` | `{y` | Gold | Normal yellow. |
| **Blue** | `ESC[1;34m` | `{B` | Blue | Bright blue. |
| **Dark Blue** | `ESC[0;34m` | `{b` | Dark Blue | Normal blue. |
| **Purple** | `ESC[1;35m` | `{M` | Magenta | Bright magenta. |
| **Dark Purple** | `ESC[0;35m` | `{m` | Dark Magenta | Normal magenta. |
| **Cyan** | `ESC[1;36m` | `{C` | Cyan | Bright cyan. |
| **Dark Cyan** | `ESC[0;36m` | `{c` | Dark Cyan | Normal cyan. |
| **White** | `ESC[1;37m` | `{W` | White | Bright white. |
| **Light Gray** | `ESC[0;37m` | `{w` | Light Gray | Normal white. |
| **Black** | `ESC[1;30m` | `{k` | Dark Gray | Bright black. This defaults to dark gray so the text isn't invisible on a black background. |
| **Dark Gray** | `ESC[1;30m` | `{D` | Dark Gray | Shares its code with **Black**, which takes priority. Change **Black** to change this color. |
| **Orange** | `ESC[38;5;166m` | `{o` | Orange | Extended color. |
| **Pink** | `ESC[38;5;205m` | `{p` | Hot Pink | Extended color. |
| **Brown** | `ESC[38;5;130m` | `{n` | Brown | Extended color. |
| **Magenta** | `ESC[38;5;61m` | `{u` | Medium Purple | Extended color. |

`ESC` is the escape character that starts every ANSI code. The "bright" colors are the ones games usually call bold. Note that the setting named **Purple** is used for ANSI magenta, while the setting named **Magenta** is an extended color.

### Info Bar Colors

The same Color Theme also sets the colors of the info bar (the stats bar below the terminal):

| Setting | Default | Used For |
|---------|---------|----------|
| **Info Bar Background** | `#171717` | The background of the info bar. |
| **Info Bar Foreground** | White | The labels and values on the info bar. |
| **Info Bar Warning** | Yellow | Values that are getting low. |
| **Info Bar Danger** | Red | Values that are very low, and the tick timer when the next tick is close. |
| **Info Bar Text Dark** | Dark Cyan | Labels, such as the word in front of the room name. |
| **Info Bar Text Light** | Cyan | Values, such as the room name itself. |

## Other Appearance Settings

A few other settings change how the client looks:

| Setting | Where | Description |
|---------|-------|-------------|
| **Font Size** | Client settings, **UI** category | The font size used by the terminal and the client's windows. |
| **Button Margin** | Client settings, **UI** category | The space around the buttons in the [Alias Button Actions and Movement](UI-Elements.md) windows. |
| **Tab color, command box colors and block caret** | Profile settings | Set per [profile](Profiles.md), which helps tell games apart when several are open. |

To make specific text stand out, such as your character's name or a rare item, use a [highlight trigger](HighlightTriggers.md) rather than changing the terminal colors.

[Return to Table of Contents](Index.md)
