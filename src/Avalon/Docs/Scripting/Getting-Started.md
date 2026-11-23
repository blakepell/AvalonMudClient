# Getting started

[Table of contents](README.md)

## A first script

This example needs no application-specific objects. It creates data, filters it with a loop, and builds a message.

```javascript
const tasks = [
    { name: 'Write report', complete: true },
    { name: 'Review results', complete: false },
    { name: 'Send summary', complete: false }
];

let remaining = [];
for (const task of tasks) {
    if (!task.complete) {
        remaining.push(task.name);
    }
}

let message = `${remaining.length} tasks remaining: ${remaining.join(', ')}`;
// message: "2 tasks remaining: Review results, Send summary"
```

Creating `message` does not itself display anything. Your application decides how script results are shown or used.

## Output and application data

An application can provide named objects, values, and functions. For example, if its documentation says it provides `app.WriteLine(text)`, you can call it like this:

```javascript
// Requires an application-provided app.WriteLine method.
app.WriteLine('Hello from Mosaic!');
```

If the application allows `System` imports, .NET console output is another option:

```javascript
include System;
Console.WriteLine('Hello from Mosaic!');
```

`Console.WriteLine` writes to the process console; a desktop application may not show that console. Use the application's documented output API for messages intended for its user interface. Browser `console.log`, `alert`, and `document` are not built in.

## Writing statements

Names are case-sensitive: `total` and `Total` are different. Use semicolons to end statements and braces to group them. Many line endings can replace semicolons, but explicit semicolons make scripts easier to read and combine.

```javascript
// A single-line comment.
let quantity = 3;

/* A comment can
   span several lines. */
let price = 12.5;
let total = quantity * price;

if (total > 30) {
    total -= 5;
}
// total: 32.5
```

Use single or double quotes for ordinary strings, and backticks with `${expression}` for interpolation. Put a returned expression on the same line as `return`.

## Three kinds of features

| Kind | Examples | Availability |
| --- | --- | --- |
| Language syntax and literal values | `if`, `for`, functions, `[]`, `{}` | Part of Mosaic |
| Imported .NET features | `StringBuilder`, `List`, LINQ `Where` | Available when imports and libraries permit them |
| Application features | `app`, `model`, a custom `log` function | Defined by the application |

The application also decides whether variables survive between runs, which operations are permitted, and how long a script may run. A script copied from another application may need different object and method names.

Continue with [variables and operators](Variables-and-Operators.md) or [conditions and loops](Control-Flow.md).
