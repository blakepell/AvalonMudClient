# Avalon Mud Client

A Windows MUD (multi-user dimension) client that allows you to connect to and play any number of text based online multi user games.

[Screenshots](doc/Screenshots.md)

### Documentation

- [Aliases](doc/Aliases.md)
- [Hash Commands](doc/HashCommands.md)
- [Lua](doc/Lua.md)
  - [Lua Overview](doc/Lua.md)
  - [Lua Extensions](doc/LuaExtensions.md)
  - [Lua Examples](doc/LuaExamples.md)
- [Macros](doc/Macros.md)
- [Packages](doc/Packages.md)
- [Plugins](doc/Plugins.md)
- [Triggers](doc/Triggers.md)
- [Variables](doc/Variables.md)

### Info

 - Language: C# / WPF for .Net Core 3.1
 - OS Support: Windows 7, 8.1, 10 (1607+)

### Key Features
 
 - Aliases (simple and regular expression)
 - Triggers (simple and regular expression)
 - Macros
 - LUA (LUA can be inlined as alias or trigger commands with extended UI commands exposed)
 - Colored syntax editor for LUA
 - 4K monitor support / responsive UI design.
 - Touch screen friendly.
 - SQLite Database Builtin with each profile.
 - Profiles can be used for multiple characters (any trigger or alias can be set to only run for certain characters)
 - Directions support
 - Global variable support in and outside of LUA that persists beyond mud sessions (Avalon also has temp variable support). 
 - Plugin ability (extend Avalon by writing your own C# or Lua plugins)
 - Opinionated UI with terminals for the main content as well as different communication types.
 - Custom scraping that can be easily turned on and off via hash commands/LUA and then piped to variables (imagine an alias that scraped notes into a database for posterity, etc.).

### Open Source Packages

 - [AvalonEdit](https://github.com/icsharpcode/AvalonEdit) (MIT)
 - [ModernWpfUI](https://github.com/Kinnara/ModernWpf) (MIT)
 - [MoonSharp](https://github.com/moonsharp-devs/moonsharp) (Custom License, see project)
 - [Command Line Parser](https://github.com/commandlineparser/commandline) (MIT)
 - [Argus Framework](https://www.github.com/blakepell/ArgusFramework) (MIT)
 - [WPF-AutoComplete-TextBox](https://github.com/quicoli/WPF-AutoComplete-TextBox) (MIT)
 - [TentacleSoftware.Telnet](https://github.com/Spksh/TentacleSoftware.Telnet) (Apache v2)
 
### Short Term Road-map

 - Finish SQLite DB Support
 - Plugin support will change a bit over the short term.  Plugins will support JSON for Triggers/Aliases/Lua and C# for things that need UI components or a robust programming model/framework.  The ability to add a user control from a plugin straight into the UI will be a priority.  I think this will be incredibly easy to use and a powerful to extend the UI outside of my realm.
 - The ability to easy search for community based triggers, aliases and scripts for import.
 - Additional Hash Commands and Lua extensions.
 - Documentation
 - Publish binary for initial alpha release (self contained EXE)
 - Ensure touch screen scrolling is smooth on all termianls and controls.
 - Figure out the best deployment mechanism or subsets of deployment mechanisms.
 - Plugin support for a Dark and Shattered Lands (dsl-mud.org) with a deployment that is completely setup to play from the first run (a custom tailored deployment that just works without a cumbersome setup process for new players).
 - Timers (there is a tick timer built in and some hash commands like alias support a delay flag which is effectivly a one time timer to fire a command after a specified amount of time.

### Long Term Road-map

 - The UI is already 4K/high resolution friendly.  Provide additional layouts for high resolution monitors that surface more content areas the user can pipe data or import plugins into.
 - Publish this app on the Windows Store free of charge.
 - Use the shared components to start a Xamarin forms project that can be deployed to Android/iOS as well as Avalonia for an OSX deployment.
 - Any controls are of general use and are stable will be abstracted will be housed as their own project so that they can be submitted to NuGet.  An example of this is the SQLite query control which is in development and can be provided as a general WPF control.  The control offers the ability to open a sqlite database, see it's schema (tables/views) and then query the data with syntax highlighting for the SQL and a DataGrid for the output (which will have exporting and importing abilities that can be turned on and off).
 
### License
 
 The Avalon Mud Client is being released under a modified MIT license with an additional clause
 requiring credit to the original author (me).  E.g. this means the license should be flexible enough
 to do what you need to do with it.