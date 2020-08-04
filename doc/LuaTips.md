# Avalon Mud Client

## Lua Tips

### Echos

When writing more than one line to the mud client with commands like `lua.Echo` it is much more efficient to create your
entire string in Lua and then make one `lua.Echo` call.  The reason behind this is that each call gets marshalled to the
UI thread which is an expensive operation.  Note that when echoing multiline strings your colors will need to be started
 on each line.  Lines in the string maybe seperated with the C style carriage return/line feed `\r\n`.

#### Lua example

This script will show the performance of writing 10,000 numbers out to the terminal one write at at time vs. creating
a string and concating all of the numbers onto it one line at at time, then only writing it to the terminal at the end.

```lua
-- Lua script to show performance difference in echoing 10,000 echos (crudely
-- timed by just showing the time).

local testStart1
local testEnd1
local testStart2
local testEnd2

lua.Echo("10000 individual echos")
lua.Echo("--------------------------------------------------------------")
testStart1 = lua.GetTime()

for i = 1, 10000, 1 do
    -- Write the number to the terminal on a line.
    lua.Echo(i)
end

testEnd1 = lua.GetTime()

lua.Echo("10000 string concats, 1 echo")
lua.Echo("--------------------------------------------------------------")
testStart2 = lua.GetTime()

local buf = ""

for i = 1, 10000, 1 do
    -- Concat the number onto the string and then also the newline to
    -- simulate test 1.
    buf = buf .. i .. "\r\n"
end

-- Write the entire contents once.
lua.Echo(buf)
testEnd2 = lua.GetTime()

lua.Echo("Test 1: {y" .. testStart1 .. " {xto {y" .. testEnd1 .. "{x")
lua.Echo("Test 2: {y" .. testStart2 .. " {xto {y" .. testEnd2 .. "{x")
```

The end result of this crude test on a 9th gen Core i-9 9900 with an NVIDIA GeForce RTX 2060 SUPER video card was:

|Test #|Start Time|End Time|
|------|----------|--------|
|Test 1|10:44:47|10:45:05|
|Test 2|10:45:05|10:45:05|

As you can see, the first test took 18 seconds to complete while the second test
completed on the same second.

### Mud Client Variables

Although Avalon Lua supports globals, Lua can also reference variables which the mud client sets that are stored on the
C# side.  Variables though simple are one of the more powerful constructs you'll use when writing scripts.  Imagine a
trigger that sets a variable with a creatures name you see in a game.  A long running lua script may then use that variable
as it becomes available effectivly letting the script react to in game content.

### Pausing your script

One of the common things Lua coders need is a way to pause their script in a way that doesn't block the mud client.  To
make this easy for you `lua.Sleep` has been provided as a way for you to pause your script without blocking the execution
of anything else in the mud client.  `lua.Sleep` accepts milliseconds for the parameter.  This is handy for cases when
you're running a looped action that must continue for a long time but might be waiting for input from the game.  In order
to interact with the loop you can use Lua global variable that are set from other commands.

### Debugging Information

The `#lua-debug` has command has been provided as a way to obtain info about the state of the current Lua environment.  It can provide stats on the number of active scripts running, the total number of scripts run and a current list of all of the global variables that are currently set.

The output of `#lua-debug` looks something like:

```
Lua Environment Info:
---------------------------------------------------------------------
  * Active Lua Scripts Running: 0
  * Total Lua Scripts Run: 13
  * Global Variable Count: 1


Lua Global Variables:
---------------------------------------------------------------------
  * myName: Blake
```