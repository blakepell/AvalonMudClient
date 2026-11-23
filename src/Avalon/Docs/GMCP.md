# GMCP
---

GMCP (Generic Mud Communication Protocol) lets a game send information to the client behind the scenes, separate from the text you see in the terminal.

Without GMCP, the only way for the client to know your health, the room you're in or the spells you're affected by is to read it out of the game's text with [triggers](Triggers.md). That works, but it breaks when the game changes how a line is worded, and it means the information has to be printed to your screen in the first place. With GMCP, the game sends that information directly as structured data, and the client hands it to your scripts without ever showing it in the terminal.

This page describes how GMCP works in Avalon for any game. For the specific events Dark & Shattered Lands sends and what's in each one, see the **GMCP Reference** in the **Documentation** group on the **DSL** tab of the ribbon.

## How GMCP Works

GMCP messages travel over the same connection as the rest of the game, wrapped in a telnet *subnegotiation* so the client can tell them apart from normal text. Each message is sent as:

```
IAC SB GMCP <message> IAC SE
```

| Byte | Value | Meaning |
|------|-------|---------|
| `IAC` | 255 | "Interpret as command." Marks the start of a telnet command. |
| `SB` | 250 | Start of a subnegotiation. |
| `GMCP` | 201 | The telnet option number for GMCP. |
| `<message>` | | The GMCP message itself (see below). |
| `IAC SE` | 255 240 | End of the subnegotiation. |

When Avalon sees this sequence it collects everything between the start and end markers as a GMCP message and leaves it out of the terminal. If the message itself needs to contain the byte 255, the game sends it twice (`IAC IAC`), as telnet requires.

### Turning GMCP On at the Game

Many games use telnet negotiation to agree on GMCP: the game announces `IAC WILL GMCP` and waits for the client to answer `IAC DO GMCP`. **Avalon doesn't answer this negotiation.** The game needs to start sending GMCP on its own or when the player asks for it with a command. On Dark & Shattered Lands that command is `gmcp on`, and the GMCP button on the status bar sends it for you (see below).

### The Message Format

Avalon expects each GMCP message to be an **event name**, a space, and a **JSON object**:

```
room_data {"room": "Temple Square", "area": "Midgaard", "exits": ["n", "s", "e"]}
```

| Part | Rules |
|------|-------|
| **Event name** | Letters, numbers and underscores only, such as `room_data` or `CharVitals`. The name is case sensitive. |
| **JSON** | A single JSON object, starting with `{` and ending with `}`, on one line. |

Messages that don't fit this format are ignored and a warning is written to the [Log Window](UI-Elements.md#log-window). Note that names containing dots, such as the `Char.Vitals` style used by some games, aren't currently recognized.

Avalon receives GMCP messages but doesn't send any to the game.

## Enabling GMCP in Avalon

GMCP is turned on or off for each [profile](Profiles.md) with the **GMCP Enable** setting in the profile's **Events** category. It's on by default. While it's off, GMCP messages from the game are still kept out of the terminal, but they're discarded rather than passed to your scripts.

The **GMCP** button on the status bar at the bottom of the window shows whether GMCP is on for the active profile. Clicking it switches the setting and also sends `gmcp on` or `gmcp off` to the game.

The setup wizard that runs when you first start Avalon has a **GMCP Events** switch. When it's on, the **GMCP** [package](Packages.md) is installed, which contains ready-made event scripts for Dark & Shattered Lands.

## GMCP Events

Every GMCP event is handled by a [script](Scripting.md) with the same name as the event. When a `room_data` message arrives, Avalon calls the JavaScript function `room_data` and passes it the message's JSON as a string. Event scripts are listed with the rest of your scripts under **Scripts** → **Edit Scripts** on the ribbon (`Ctrl+Alt+X`).

### Empty Events Are Created Automatically

You don't need to know ahead of time which events a game sends. When GMCP is enabled and a message arrives for an event that has no script yet, **Avalon automatically creates an empty event script for it in the Scripts list.** The new script is named after the event, has the description `GMCP Event`, and contains a function that parses the JSON and does nothing else:

```javascript
function room_data(json) {
    let obj = JSON.parse(json);
}
```

After you've played for a little while, your Scripts list will contain an entry for every event the game sends. Fill in the ones you care about and leave the rest empty; an empty event does no harm.

> **Tip:** Because event scripts are created with the exact name the game sends, a game that sends many different events will add many scripts. If you don't want them, turn off **GMCP Enable** for the profile before connecting.

### Writing an Event Script

Open the event's script and add whatever you'd like to happen when the event arrives. Most event scripts copy values into [variables](Variables.md), which can then be shown in the [Variables window](UI-Elements.md#variables) or used by your triggers and aliases:

```javascript
function room_data(json) {
    let obj = JSON.parse(json);

    terminal.SetVariable("Room", obj.room);
    terminal.SetVariable("Area", obj.area);
    terminal.SetVariable("Exits", obj.exits.join(" "));
}
```

Event scripts have the full [scripting](Scripting.md) environment available, so they can do much more than set variables. A few common uses:

| Use | Example |
|-----|---------|
| Keep your current room up to date for [directions](Directions.md). | `terminal.SetVariable("Room", obj.room)` |
| Fill in the [Affects](UI-Elements.md#affects) window. | `vm.Affects.AddOrUpdateAffect(af)` |
| Update the stats bar. | `vm.Hp = obj.hp` |
| Send a message to a [custom terminal](UI-Elements.md#new-terminal). | `terminal.WriteLine("GMCP", json)` |

## Troubleshooting

If GMCP doesn't seem to be doing anything:

1. Check that the **GMCP** button on the status bar shows GMCP as on.
2. Make sure the game is sending GMCP. Some games need a command or an option in your account settings.
3. Set **Log Level** to `Debug` in the client settings. Every GMCP message received is then written to the [Log Window](UI-Elements.md#log-window) as `GMCP: <message>`, so you can see exactly what the game sends.
4. Check the Log Window for errors. If an event script fails, the error is logged with the event's name.
5. Open **Scripts** → **Edit Scripts** and check that a script exists for the event and that the function inside it has the same name as the event.

[Return to Table of Contents](Index.md)
