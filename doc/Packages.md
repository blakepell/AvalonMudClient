# Avalon Mud Client

## Packages

A package is a simple JSON file that includes the contents of one or more triggers, aliases, settings, directions or variables.  The schema of this file will match the schema of the profile's settings file.  The purpose of a package is to be able to combine all of the assets needed to import a desired set of functionality.

### Example:

In the following example the JSON package includes an alias and a trigger that work in tandem to create and enable a `shop-inspect` alias that allows a user to look through potentially hundreds of items in a merchant shop and report back with the quality of those items.  In many Diku muds items can be accessed like `2.potion` or `3.sword` to indicate you want the 2nd potion or 3rd sword.  In a shop with hundreds of similiar items it becomes important to know which sword you want to purchase if they have the same name but are of different qualities.

The following gist contains the JSON package that could be imported as well as the friendly formatted versions of the Lua for the alias and trigger as well as the regular expression pattern for the trigger.

[shop-inspect gist](https://gist.github.com/blakepell/0af993ecad9c832bd34f9d1eab7e32fa)
