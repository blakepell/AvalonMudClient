# Avalon Mud Client

## Lua Overview

Lua is a general purpose scripting language primarily designed for use in embedded systems.  The Avalon Mud Client uses a C# adapation of Lua which is 99% compliant with Lua 5.2 (the only unsupported feature being weak table support).  In Avalon, Lua can be inlined into aliases and triggers without the need to setup additional files.  When you edit an alias or trigger that contains Lua you will be provided with a dialog that syntax highlights your code.

Avalon has built in additional support to the Lua environment to allow for ease of scripting and interaction with the mud clients UI.  Many commands which traditionally require Lua libraries have been provided (the ability to pause a script, the ability to download a file from the internet, string formatting functions, etc).

In Avalon Lua, each script is run in it's own container seperate from other Lua scripts that might be running.  The Lua script can interact with the general environment in a few ways.  First, a `globals` namespace has been provided where global Lua variables can be stored and shared between scripts.  These globals do not persist beyond the process (e.g. they are cleared when the mud client is shut down).  Second, Lua can make calls to store mud client global variables via `lua:SetVariable` and `lua:GetVariable`.  These variables DO persist beyond the process and will be available when the mud client next starts (they are saved in the profile's json file).  Finally, a `lua` namespace with helper functions have been provided.  These functions service two purposes, to make Lua scripting easier and more robust and to interact with the mud client's UI (to do this such as change the windows title, set and get variables, send commands or echo to the terminal window, make database calls, etc.).

In order to be most efficient, Avalon uses a memory pool of Lua environments that can run concurrently and share some state variable between them.  If an alias or trigger is set to run a Lua script it will load and cache those scripts as functions where any values from the trigger or alias as passed in as vararg parameters.  As a result, an alias that runs Lua would need to retrieve those variables which can be done via `select` or a custom `getarg` command that Avalon provides.  `getarg` has an additional benefit of starting at 1 for parameters and returning a blank string instead of a null if one isn't found.

### Example

This example is for an alias named `id` and accepts two parameters which are a start and an end index.  It uses the `getarg` function to retrieve the value and then converts those values to a number via `tonumber`.

```
    -- alias: id 5 10

    local startIndex = tonumber(getarg(1, ...))
    local endIndex = tonumber(getarg(2, ...))

    for i = startIndex, endIndex, 1
    do
        lua:LogInfo("Iteration: " .. i)
    end
```

Avalon Lua Tips:

- [Lua Tips](LuaTips.md)

Avalon Lua Extensions:

- [Lua Extensions](LuaExtensions.md)

MoonSharp Project Links:

- [Avalon MoonSharp Source Code (Avalon.MoonSharp)](https://github.com/blakepell/AvalonMudClient/tree/master/src/Avalon.MoonSharp)
- [MoonSharp Source Code](https://github.com/moonsharp-devs/moonsharp)
- [MoonSharp Community](https://www.moonsharp.org/)

## Screenshots

![alt text](images/LuaEditor.png "Lua Editor")
