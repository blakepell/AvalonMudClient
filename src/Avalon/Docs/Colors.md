# Supported ANSI Colors and Styles

The table below lists the available ANSI colors and style codes used by the client. The values are directly extracted from the individual files implementing `AnsiColor`.

| Name         | Ansi Code           | Mud Color Code |
|--------------|---------------------|----------------|
| Clear        | `\x1B[0m`           | `{x`           |
| Default      | `\x1B[0;37m`        | `{w`           |
| Background   | `\x1B[40m`          | `{Z`           |
| Reverse      | `\x1B[7m`           | `{&`           |
| Underline    | `\x1B[4m`           | `{_`           |
| Red          | `\x1B[1;31m`        | `{R`           |
| DarkRed      | `\x1B[0;31m`        | `{r`           |
| Black        | `\x1B[0;30m`        | `{k`           |
| DarkGray     | `\x1B[1;30m`        | `{D`           |
| LightGray    | `\x1B[0;37m`        | `{w`           |
| Green        | `\x1B[1;32m`        | `{G`           |
| DarkGreen    | `\x1B[0;32m`        | `{g`           |
| Yellow       | `\x1B[1;33m`        | `{Y`           |
| DarkYellow   | `\x1B[0;33m`        | `{y`           |
| Blue         | `\x1B[1;34m`        | `{B`           |
| DarkBlue     | `\x1B[0;34m`        | `{b`           |
| Purple       | `\x1B[1;35m`        | `{m`           |
| DarkPurple   | `\x1B[0;35m`        | `{M`           |
| Cyan         | `\x1B[1;36m`        | `{C`           |
| DarkCyan     | `\x1B[0;36m`        | `{c`           |
| White        | `\x1B[1;37m`        | `{W`           |
| Orange       | `\x1B[38;5;214m`    | `{o`           |
| Pink         | `\x1B[38;5;213m`    | `{p`           |
| Brown        | `\x1B[38;5;130m`    | `{n`           |
| Magenta      | `\x1B[1;35m`        | `{u`           |

---

### Notes:
- The `AnsiCode` are provided for reference.
- The `MudColorCode` can be used when writing content to the terminals in order to color or style text.
- When colors are interpreted locally with a function like `terminal.EchoLine` they are not sent to the game server.  Game servers often have their own color code system.
- This style of color codes is based off of `Lopes Colors` which was a common color snippet used in many `DikuMud`/`Merc`/`ROM` based code bases.

### Custom Colors:

To input your own colors from the Ribbon select `Theme -> Edit Terminal Colors` and you'll be presented with colors that you can update via a color picker or by the colors hex value. 

[Return to Table of Contents](Index.md)