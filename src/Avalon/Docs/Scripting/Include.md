# The include statement

[Table of contents](README.md)

## Import a namespace

`include` is Mosaic's language extension for using .NET namespaces. After an import, public types in that namespace can be used by their short names.

```javascript
include System.Text;

let builder = new StringBuilder();
builder.Append('Hello');
builder.Append(' from Mosaic');
let message = builder.ToString(); // "Hello from Mosaic"
```

Write an unquoted dotted namespace name after `include`, on the same line. A terminating semicolon is recommended. Use one statement per namespace:

```javascript
include System;
include System.Collections.Generic;

let names = new List(String);
names.Add('Ada');
names.Add('Grace');
let count = names.Count; // 2
```

`include System;` imports types directly in `System`; it does not also import `System.Text` or `System.Collections.Generic`. Import each namespace you use.

## What include does

An include makes matching public types from the application's loaded .NET assemblies available for short-name lookup. It also makes extension methods declared by public static classes in that namespace available on matching objects.

It does not load a JavaScript file, download a package, load an assembly from a path, create a namespace object named `System`, or execute another script. File paths, quoted namespace strings, wildcard imports, comma-separated namespace lists, and JavaScript module syntax are not forms of this statement.

Use your application's documented script-loading feature if you need to share code between files. ECMAScript `import`, `export`, `require`, and npm modules are not supplied by Mosaic.

## Extension methods and LINQ

An extension method is called as though it belongs to the value you are working with. Importing `System.Linq` enables methods such as `Where`, `Select`, and `ToArray` on compatible collections:

```javascript
include System.Linq;

let numbers = Enumerable.Range(1, 6);
let squares = numbers
    .Where(value => value % 2 == 0)
    .Select(value => value * value)
    .ToArray();
// squares is a .NET array containing 4, 16, 36.
```

An application can expose extension methods itself, so some methods may already be available without an include. Custom extension methods use the same member-call syntax. See [LINQ queries](LINQ.md).

## Name resolution and lifetime

Variables and application-provided global names take priority over imported type names. Avoid naming a variable `StringBuilder`, `List`, or another type you intend to construct.

If two imported namespaces contain the same short type name, the namespace imported first wins. Imports the application established before your script also participate in that order. Ask for an application-provided alias when names conflict. A fully qualified name such as `System.Text.StringBuilder` works only if the application also exposes that namespace path; an `include` alone does not create it.

Includes take effect when their statements execute. They can appear inside a function or block, but their effect belongs to the scripting engine, not just that lexical block. If the application reuses the engine, imported namespaces can remain available in later runs until the application clears them. Put includes at the top of a script to make its dependencies clear. Repeating an unchanged import does not add another copy or change its original precedence.

## Application restrictions

The application may reject a namespace, restrict the types available through imports, or disable includes altogether. Only loaded libraries can supply types. An accepted include can still leave a particular type unavailable; accepting the namespace is not a guarantee that every type you expect exists.

Reflection access has additional policy restrictions. Use the APIs the application documents. If an import fails, check the namespace spelling and the application's supported namespaces rather than changing the statement to a file path.

`include` is contextual: `include System.Text;` is an import statement, while an ordinary function call named `include(...)` remains a function call. Calling `include('System.Text')` does not invoke the namespace-import feature.
