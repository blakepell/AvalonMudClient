# Avalon Mud Client

## Packages

A package is a JSON file that includes the contents of one or more triggers, aliases, settings, directions or variables or setup commands that can change the behavior or settings of the mud client.  The purpose of a package is to be able to combine all of the assets needed to import a desired set of functionality.

A package may not contain any features other than a `SetupCommand` which can be used to tailor behavior of the client.  An example
would be executing a command that changes the setting `SearchBarCommand` to search a wiki for your mud.

To create a basic package you can navigate to `File->Create Package` and choose any combination of aliases, triggers and directions. Hold down the control key to select multiple items. When done you can select the option to create the package and the mud client will prompt you to save the file. Additional JSON properties include `Category`, `Version`, `SetupCommand` and `SetupCommandLua`.

Here is an example package that will work with any ROM 2.4 based mud that implements Wiznet with a `-->` prefix.  The package creates a trigger that moves any Wiznet item into the 3rd terminal window.  Additionally it executes a command that names the tab and makes it visible.

```
{
  "Id": "a3738921-fdd7-4ea2-84b0-a9bc8c6862db",
  "Name": "Wiznet",
  "Description": "A trigger that will move wiznet info to terminal 3 without gagging it from the main game terminal.",
  "Author": "Rhien",
  "GameAddress": "dsl-mud.org",
  "Category": "Immortals",
  "Version": 4,
  "SetupCommand": "#setting -c -s \"CustomTab3Label\" -v \"Wiznet\";#setting -c -s \"CustomTab3Visible\" -v \"true\"",
  "AliasList": [],
  "TriggerList": [
    {
      "Command": "#echo -d -t Terminal3 \" %0\"",
      "Pattern": "^-->",
      "Character": "",
      "Group": "Immortal",
      "IsSilent": false,
      "IsLua": false,
      "Plugin": false,
      "DisableAfterTriggered": false,
      "Lock": false,
      "LastMatched": "0001-01-01T00:00:00",
      "VariableReplacement": false,
      "Enabled": true,
      "Gag": false,
      "MoveTo": 0,
      "HighlightLine": false,
      "Count": 0,
      "Priority": 10000,
      "StopProcessing": false,
      "Identifier": "1eb065f2-834b-473d-a4a2-3ffcb4a36641",
      "PackageId": "",
      "SystemTrigger": false
    }
  ],
  "DirectionList": []
}
```

## Important Note

It is important that you only import packages from trusted sources.  This mud client runs much like a programming IDE and as a result running untrusted
code via packages (or in general) can cause problems including security problems.  As with anything, always be aware of what you install.

## Example:

In the following example the JSON package includes an alias and a trigger that work in tandem to create and enable a `shop-inspect` alias that allows a user to look through potentially hundreds of items in a merchant shop and report back with the quality of those items.  In many Diku muds items can be accessed like `2.potion` or `3.sword` to indicate you want the 2nd potion or 3rd sword.  In a shop with hundreds of similiar items it becomes important to know which sword you want to purchase if they have the same name but are of different qualities.

The following gist contains the JSON package that could be imported as well as the friendly formatted versions of the Lua for the alias and trigger as well as the regular expression pattern for the trigger.

[shop-inspect gist](https://gist.github.com/blakepell/0af993ecad9c832bd34f9d1eab7e32fa)
