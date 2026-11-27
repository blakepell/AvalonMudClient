# MCP Server
---

Avalon can run a **Model Context Protocol (MCP) server** so that AI assistants such as Claude Code or Codex can talk to the running client. Once connected, the assistant can use the tools Avalon provides to see and change things in the client.

The server is off by default and only accepts connections from programs on your own computer.

## Available Tools

Tools that work with a profile use the profile whose terminal is showing, unless the assistant names another one. Changes are saved as soon as they're made, and appear right away in the editors and in any open terminal.

### Profiles and Documentation

| Tool | Description |
|------|-------------|
| `list_profiles` | Lists your profiles, which one is showing, and how many aliases, triggers, variables and scripts each has. |
| `get_scripting_guide` | Gives the assistant Avalon's scripting documentation: a primer on the scripting language, how scripts are scoped and the most used objects, plus every page of the [Scripting](Scripting.md) and [language](Scripting/README.md) reference. |

### Aliases, Triggers, Variables and Scripts

| Tool | Description |
|------|-------------|
| `list_aliases`, `add_alias`, `update_alias`, `remove_alias` | List, add, change and remove [aliases](Aliases.md). |
| `list_triggers`, `add_trigger`, `update_trigger`, `remove_trigger` | List, add, change and remove [triggers](Triggers.md). |
| `test_trigger_pattern` | Tries a trigger pattern on sample lines without changing anything, showing what `%0`–`%9` would hold. |
| `list_variables`, `set_variable`, `remove_variable` | List, create or change, and remove [variables](Variables.md). Setting a variable this way doesn't run its **On Change Event**. |
| `list_scripts`, `get_script`, `add_script`, `update_script`, `remove_script` | List, read, add, change and remove the scripts in the Scripts list. |
| `validate_javascript` | Checks that JavaScript has no syntax errors without saving or running it. |

JavaScript and trigger patterns are checked before they're saved. If the code has a syntax error or the pattern isn't a valid regular expression, nothing is changed and the assistant is told why. The check only covers syntax, so it's still worth trying a new script in the game.

### Status Bar

| Tool | Description |
|------|-------------|
| `get_status_text` | Gets the text shown on the left side of the status bar. |
| `set_status_text` | Sets the text shown on the left side of the status bar. |

## Turning the Server On

1. Open **Client Settings** from the Settings (cog) button, or **Options** in the backstage (File menu).
2. In the **MCP Server** section, set **Enabled** to true.
3. Optionally change **Port**. The default is `7425`.

The server starts as soon as **Enabled** is turned on and stops as soon as it's turned off. If you change the port while it's running, the server restarts on the new port.

You can also turn the server on and off by clicking **MCP** on the status bar. The icon next to it shows a green check when the server is running and a red X when it's off. Hover over it to see the address the server is listening on, or the reason it couldn't start (for example, another program already using the port).

With the default port, the server's address is:

```
http://localhost:7425/mcp
```

## Registering from the Developer Tab

Open **Developer → AI** to register or unregister Avalon with Codex or Claude Code. The menu uses your current MCP port and checks each client's user configuration to enable the applicable action. The corresponding CLI must be available on PATH. If you later change Avalon's port, unregister and register again to update the saved address.

If you register from a terminal and changed the port, use your port in place of `7425` in the commands below.

## Adding Avalon to Claude Code

Run this in a terminal:

```
claude mcp add --transport http avalon http://localhost:7425/mcp
```

This adds the server for the project folder you run it in. To make Avalon available in every folder, add `--scope user`:

```
claude mcp add --transport http --scope user avalon http://localhost:7425/mcp
```

To check that Claude Code can reach Avalon, make sure the server is turned on in Avalon and run:

```
claude mcp list
```

You can also type `/mcp` inside a Claude Code session to see the server's status and its tools. To remove it later, run `claude mcp remove avalon`.

## Adding Avalon to Codex

Run this in a terminal:

```
codex mcp add avalon --url http://localhost:7425/mcp
```

This saves the server to `~/.codex/config.toml`, so it's available in every folder. If you'd rather edit that file yourself, add:

```toml
[mcp_servers.avalon]
url = "http://localhost:7425/mcp"
```

To check that it was added, run `codex mcp list`, or type `/mcp` inside a Codex session. To remove it later, run `codex mcp remove avalon`.

## Trying It Out

With Avalon running and the server turned on, start a new Claude Code or Codex session and ask something like:

> Set the Avalon status bar text to "Hello from Claude".

The text on the left side of Avalon's status bar should change.

## Troubleshooting

| Problem | What to check |
|---------|---------------|
| The assistant says it can't connect to Avalon. | Avalon has to be running with the MCP server turned on **before** the assistant starts its session. Turn the server on, then start a new session. |
| The status bar icon stays on a red X. | Hover over **MCP** on the status bar to see the error. Usually another program is using the port, so pick a different **Port** in Client Settings and update the address you gave the assistant. |
| The tools don't show up. | Make sure the address ends in `/mcp` and that the port matches the one in Client Settings. |

## Security

The server only listens on your own computer (`localhost`), so other computers on your network can't reach it. It doesn't ask for a password, though, so any program running on your computer can use it while it's on. Turn it off when you aren't using it.

[Return to Table of Contents](Index.md)
