# Avalon Mud Client

## Packages

A package is a simple JSON file that includes the contents of one or more triggers, aliases, settings, directions or variables.  The schema of this file will match the schema of the profile's settings file.  The purpose of a package is to be able to combine all of the aliases and triggers needed to import a desired set of functionality.

The benefit of a package is that a user can import the package manually or in the future download them via the mud client for usage.  Some packaged items will be tagged with a unique identifier so that if they are re-imported the user can be asked to overwrite the previous version.  This will allow a user to export a set of directions from the client and send them to someone else who can then import them.

### Example:

In the following example the JSON package includes and alias and a trigger that work in tandem to create and enable a `shop-inspect` alias that allow a user to look through potentially hundreds of items in a merchant shop and report back with the quality of those items.  It many Diku muds items can be accessed like `2.potion` or `3.sword` to indicate you want the 2nd potion or 3rd sword.  In a shop with hundreds of similiar items it becomes important to know which sword you want to purchase.

The following gist contains the JSON package that could be imported as well as the fiendly versions of the Lua for the alias and the trigger and the regular expression pattern for the trigger.

[shop-inspect gist](https://gist.github.com/blakepell/0af993ecad9c832bd34f9d1eab7e32fa)
