# Avalon Mud Client

## Quick Tips

### How to Update

1. `Help->Update Client` will check for a new release.  If a new release exists it will download the installer from GitHub and start the installer for you.  It will know the correct 32-bit/64-bit one based off of what you previously installed.  **NOTE**: Once the installer has finished downloading it will close the client.  If you are currently in a game be aware of this.

### Input Box

1. The backslash key `\` will select the entire input box if you press it.
2. The `up` and `down` arrow keys will cycle through the history of your commands.
3. Typing `#` will pull up a list of hash commands to choose from, continuing to type will filter the list.
4. Typing `#go` will pull up built in directions that are stored in the Directions tab.
5. Pressing `Page Up` or `Page Down` will show you the back buffer panel.  `Escape` will hide it (or if you page all the way to the bottom)
6. Pressing `Escape` will clear the input box of any text in it.
7. `Control+I` anywhere while on the main window will put the focus on the main input box.

### General Tips

2. The client will save your settings when you exit the client.  It does not auto-save currently.  If you want to exit the client without saving you can use `File->Exit Without Save`.
3. In the `Settings->Client Settings` you can change the `SaveDirectory` setting to change where profiles save.  I've found it handy to have my profile save into a cloud storage folder (e.g. Dropbox, Box, OneDrive) folder which is then available on all of my computers.  This has the added benefit of acting as a backup so you can restore old versions of your profile.
4. If you download any shared triggers and aliases and make changes to them you can use the `Lock` feature to lock them.  You will still be able to manually update them but auto-updates will skip these.  This means you can alter the stock triggers and make sure your changes are the ones that stay (but also be able to update the rest of them).
5. To add a new record in the `Aliases`, `Triggers` or `Directions` tab scroll to the bottom of the grid and begin adding in the last row.  Note: In the future we will also have custom entry forms that will be more robust.
6. If you have a large amount of text to to the game, like a note, you can use the `Tools->Send Text to Game`.  This will send your text line by line with a small pause in between each line to prevent disconnections in the case where some muds disconnect you for spamming too many commands at once (which is common Diku muds).
7. If the `#echo` hash command is used to echo to a terminal, it can be coupled with supported [color codes](./MudColorCodes.md) in Lopes format.

### Built in Hot Keys

1. `Control+Alt+A` will open the edit aliases window.
2. `Control+Alt+T` will open the edit triggers window.
3. `Control+Alt+M` will open the edit macros window.
4. `Control+Alt+D` will open the edit directions window.
5. `Control+Alt+V` will open the edit variables window.
6. `Control+D` will open a special search window with directions.  This direction list will only list directions that work from the current room you're in. The `Room` variable having to be set in some capacity.  In typical Diku/Merc/Smaug/Room muds this can either be put into the prompt of your game or scraped from the in game input with a trigger.
7. `Control+I` will put the focus on the input box.
8. `Control+Tab` will switch between the main tabs at the top.
9. `Control+F` will put the focus on the search text box on supported windows.
10. `Control+1`, `Control+2`, `Control+3` will jump to each of the custom tabs.
11. `Page Up` and `Page Down` will activate and scroll through the back buffer when the input box is active.
12. `Escape` will clear the input box as well as close the back buffer.
13. `Control+Alt+L` will open the editor for the last trigger or alias you edited.
14. `Control++` Control and the plus key will make the terminal font size larger.
15. `Control+-` Control and the minus key will make the terminal font size larger.