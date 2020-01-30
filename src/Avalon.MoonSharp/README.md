# Avalon.MoonSharp

Lua Support for the Avalon Mud Client.

- This is a .NET Standard 2.0/.NET Core 3 port of the Moonsharp library (which can be used with UWP and .NET Core 3).
- This is an unofficial release with minimal changes to the source outside of compilation targets.  There is again traction on the main project, hopefully the need for this package will cease to exist in the future and it can be deprecated.
- License is inherited from the official Moonsharp project.

### Project Description

An interpreter for the Lua language, written entirely in C# for the .NET, Mono, Xamarin and Unity3D platforms, including handy remote debugger facilities. 

### Key Differences

Pull request 246 (https://github.com/moonsharp-devs/moonsharp/pull/246) from the official project was merged into this fork that allows for async execution control (e.g. the ability to stop a script, pause a script, run a script in the background while continuing the normal flow of an application).

### The Official MoonSharp Repository and Resources:

- https://github.com/moonsharp-devs/moonsharp
- http://www.moonsharp.org/
- https://groups.google.com/forum/#!forum/moonsharp
- https://discordapp.com/invite/gEEHs6F

### Final Important Note

When/if the official MoonSharp project releases a new version targetting at least the .NET Standard I will mark this package as deprecated and remove it from the search index on NuGet.