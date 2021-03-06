# Avalon Mud Client

## Frequently Asked Questions

- **RAM usage seems to grow a bit, is this normal**?
  - Yes, it is normal.  The Avalon Mud Client leverges a UI framework called WPF and an editor called AvalonEdit to create the ANSI terminal.  Many programming objects are used to create and track the colored coded ANSI text.  A few tips to lower usage are 1.) Set the back buffer to only keep a set number of lines. 2.) You can clear the content of the game and back buffer terminal with the `#clear` hash command followed by a `#gc` hash command that will force the garbage collector to reclaim that memory.  Generally the garbage collector knows to clear it's contents when memory pressure becomes high.  It's typically best to rely on the garbage collector but this can be done if you desire.  The main source of memory usage is from the structures used in keeping the terminal lines (for most computers, this isn't an issue).  Although I'm conscience of memory management Avalon's focus is bringing powerful features.
  - A second tip if to use the x86 (32-bit version) which will take less memory to run (although runs into a memory cap).  That said, I have never ran into a problem where that kind of RAM was being used.  General usage for 64-bit is between 130MB-250MB and for 32-bit 70MB-120MB.

- **An update occured or I've messed up by panel layout, how can I fix this?**
  - There are times when an update occurs and new panels or UI features are added that might mess your layout up.  The layout can be reset to the default by using the `View->Reset Layout` menu.

- **How do I connect/disconnect from my game?**
  - After you have set the IP address of the game you want to connect to in settings if it is not the default, click on the menu `File->Connect` or use the connect button in the window title bar (which is red when disconnected, green when connected).  There is a setting to auto connect also when a profile is opened located at: `Settings->Profile Settings->Network->AutoConnect`.