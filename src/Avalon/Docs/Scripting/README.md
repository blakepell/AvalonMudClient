# Mosaic JavaScript language reference

This guide is for people writing scripts inside an application that uses Mosaic UI Scripting. It explains the language, working with data, and calling the features your application exposes. You do not need to write C# to use these examples.

Mosaic runs a JavaScript-style language on .NET. It supports familiar statements, functions, arrays, and objects, plus .NET namespace imports through `include` and collection queries through LINQ. It is not a complete browser or Node.js JavaScript runtime. The [compatibility reference](Compatibility.md) describes the differences that matter when writing scripts.

## Table of contents

| Page | What you will learn |
| --- | --- |
| [Getting started](Getting-Started.md) | Your first script, application-provided names, comments, and output |
| [Variables, values, and operators](Variables-and-Operators.md) | `let`, `const`, `var`, strings, numbers, scope, comparisons, and defaults |
| [Conditions and loops](Control-Flow.md) | `if`, `switch`, `for`, foreach-style `for...of`, `for...in`, `while`, `do...while`, `break`, and `continue` |
| [Functions](Functions.md) | Declarations, arrows, callbacks, default/rest parameters, closures, and returning values |
| [Arrays and objects](Arrays-and-Objects.md) | Properties, indexing, array methods, spread, destructuring, and JSON |
| [The include statement](Include.md) | Importing .NET namespaces, extension methods, name resolution, and restrictions |
| [LINQ queries](LINQ.md) | Filtering, projecting, sorting, grouping, aggregation, materializing results, and generic type arguments |
| [Using application and .NET objects](Application-Objects.md) | Reading and changing host data, calling methods, creating objects, lists, and dictionaries |
| [Errors and asynchronous work](Errors-and-Async.md) | `throw`, `try`/`catch`/`finally`, .NET errors, `async`, `await`, and cancellation |
| [Recipes](Recipes.md) | Complete scripts for reports, validation, grouping, and lookup tables |
| [Compatibility and troubleshooting](Compatibility.md) | Supported features, differences from standard JavaScript, and common problems |

## Reading the examples

Examples use standard JavaScript code fences, including Mosaic's custom `include` syntax. Each code block is a separate example unless the text says to continue a previous one. Comments show results where useful. Run a block in a fresh script scope when experimenting: your application may retain variables between runs.

Most examples create their own data. A block that uses an application object states what the application must provide. Names such as `model`, `app`, `Console`, and `JSON` are not automatically supplied by the language. Namespace examples require the application to allow the named `include` statements and have the corresponding .NET libraries loaded.

The behavior described here follows the current Mosaic implementation. Applications can choose different rules for missing values, variable scope, and access to .NET. Start with your application's scripting help for its exposed names and restrictions.