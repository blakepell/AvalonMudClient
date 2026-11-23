# Using application and .NET objects

[Table of contents](README.md)

The embedding application is the **host**. It can expose objects written in C#, functions, collections, and .NET types to your script. Their available members are part of the application's scripting API. These examples show how to consume that API; they do not require you to configure the engine or write C#.

## Read and update application data

Suppose your application provides `model` with a writable `Title`, `Enabled`, and `Total`, plus a `Recalculate()` method:

```javascript
// Requires the model API described above.
model.Title = 'Monthly report';
model.Enabled = true;
model.Recalculate();
let total = model.Total;
```

Use the names and capitalization in the application's reference. Assignments and method calls can affect the live application object immediately; they are not automatically staged until the script finishes. Read-only properties and blocked members cannot be changed. An unavailable member may return a missing value or cause an error, depending on the operation and application settings.

If the application exposes a function instead of an object, call it directly, for example `log('Finished')`. Names like `log` are conventions chosen by the application.

## Construct imported types

`new` constructs an exposed .NET type:

```javascript
include System.Text;

let builder = new StringBuilder('Report');
builder.AppendLine();
builder.Append('Ready');
let text = builder.ToString();
```

This is .NET construction. `new` is not a general JavaScript prototype-based constructor mechanism. Use functions that return object literals for your own script-created records.

For generic .NET types, pass the generic type arguments first, followed by any constructor arguments. JavaScript angle-bracket syntax such as `new List<String>()` is not supported.

```javascript
include System;
include System.Collections.Generic;

let names = new List(String);
names.Add('Ada');
names.Add('Grace');

let quantities = new Dictionary(String, Int32);
quantities.Add('notebooks', 3);
quantities['notebooks'] = 4;
let quantity = quantities['notebooks']; // 4
```

`String` and `Int32` here are .NET types imported from `System`. Lowercase aliases such as `string` and `int` work only if your application exposes them.

## Collection types

| Value | Size | Add an item | Visit values |
| --- | --- | --- | --- |
| Script array `[]` | `items.length` | `items.push(value)` | `for (const item of items)` |
| .NET array, including LINQ `ToArray()` | `items.Length` | Fixed size; create another collection | `for (const item of items)` |
| .NET `List` | `items.Count` | `items.Add(value)` | `for (const item of items)` |
| .NET dictionary | `items.Count` | `items.Add(key, value)` or index assignment | Enumerate `items.Keys` or `items.Values` |
| Deferred LINQ sequence | `items.Count()` with LINQ available | Change the source collection | `for (const item of items)` |

Script arrays, .NET arrays, and lists support numeric indexing. Dictionaries use their key type. A dictionary can be traversed without relying on the representation of its key/value entry objects:

```javascript
include System;
include System.Collections.Generic;

let quantities = new Dictionary(String, Int32);
quantities.Add('notebooks', 3);
quantities.Add('pens', 5);

let lines = [];
for (const key of quantities.Keys) {
    lines.push(`${key}: ${quantities[key]}`);
}

let hasPens = quantities.ContainsKey('pens'); // true
```

Do not change a collection's membership while enumerating it unless its API explicitly allows that operation. Materialize a copy first when needed.

## Method arguments and return values

Mosaic selects .NET overloads and converts arguments automatically where possible. A script function can be converted into a callback delegate. This is how array callbacks and LINQ selectors work. Match the documented argument count and types; ambiguous or incompatible overloads can fail.

For generic methods, use [GenericArguments](LINQ.md#explicit-generic-arguments) if inference does not produce the types you need. Preserve typed values returned by the application when possible instead of converting them to text and back.

.NET strings use members such as `Length`, `Trim()`, and `Substring(start, length)`. Static APIs use a type name, for example `Math.Sqrt(9)` after `include System;`. That `Math` is .NET's type with PascalCase methods, not JavaScript's built-in `Math` object.

## Application-specific limits

Importing a namespace does not grant every possible application permission. Access to members, reflection, files, network operations, or UI updates depends on the host's API and policies. Follow the application's threading and lifetime rules for the objects it exposes. If it supplies a disposable resource and asks scripts to own it, release it in `finally`; do not dispose objects the application owns.
