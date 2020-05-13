# Avalon Mud Client

## Plugins

Plugins are similiar to packages but are more advanced.  Plugins are compiled DLL's that inherit from the IPlugin interface in `Avalon.Common`.  A plugin can contain Windows, Controls, Hash Commands, Triggers or Aliases.  All of these have the option to execute in advanced ways that normal triggers cannot.  As an example, a CLR trigger can execute not just commands or Lua but compiled C# code that have access to the full .NET Core framework.

A plugin is associated with the IP address of a given game and though loaded at runtime will not be activated until you connect to the specified game it's designed for.  This means upon connection you might see the UI of your client change (extra menu items added, tab labels changed, etc.).

### How do I create a plugin?

- Create a new .NET Core library project.
- Add a reference to `Avalon.Common`
- Add an interface and inherit from `Avalon.Common.Plugins.Plugin`
- Fill in the property for `IpAddress`
- Override and fill in your code for the `Initialize` and `Tick` methods.  The `Initialize` method will load and initialize all Triggers, Hash Commands and Menu items.  