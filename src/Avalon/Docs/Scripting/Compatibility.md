# Compatibility and troubleshooting

[Table of contents](README.md)

Mosaic implements a useful subset of JavaScript syntax with a .NET-oriented runtime. A statement being recognized by the parser does not guarantee the runtime implements it. This page records current behavior so you can write scripts that work predictably.

## Supported syntax at a glance

| Feature | Status and guidance |
| --- | --- |
| Comments, blocks, literals, expressions | Supported for ordinary scripting |
| `var`, `let`, `const` | Supported; some scope and missing-value behavior is configurable |
| `if` / `else`, conditional `?:` | Supported; only the selected branch runs |
| `switch` | Supported; keep `default` last and use explicit `break` |
| `for`, `for...of`, `for...in`, `while`, `do...while` | Supported; `for...of` and `for...in` need declared loop variables |
| `break`, `continue`, `return` | Supported; no labeled statements |
| Functions and arrows | Supported; declare before use and use explicit returns in block bodies |
| Default and rest parameters | Supported with default-evaluation differences |
| Arrays, object literals, spread | Array/call spread supported; object spread is not standard property copying |
| Destructuring | Partial; object aliases/rest and array function parameters have limitations |
| Template literals and tagged templates | Implemented; interpolation uses `${expression}` |
| Optional member/call syntax | Implemented with evaluation differences described below |
| `try`, `catch`, `finally`, `throw` | Supported with .NET exception objects and finalizer differences |
| `async`, `await` | Supported for .NET awaitable values; execution mode is host-controlled |
| `include Namespace.Name` | Mosaic extension importing types and extension methods |
| `new` | Constructs exposed .NET types |
| JavaScript classes, `extends`, `super`, `this` | Not implemented by the execution runtime |
| Generators and `yield` | Not implemented; do not assume async iteration protocols are supported either |
| ECMAScript modules and dynamic `import()` | Not implemented |
| `debugger`, `with`, labels | Not implemented by the execution runtime |
| Regular-expression literals | Do not produce a usable JavaScript RegExp runtime value; use an exposed .NET regex API if available |

## Standard globals are not automatic

A fresh engine does not install a browser or Node.js global environment. Do not assume `console`, `window`, `document`, `fetch`, timers, `require`, `Promise`, `Date`, `RegExp`, `Map`, `Set`, `Object.keys`, or `Array.isArray` exist. Likewise, `JSON` and `globalThis` require the application to expose them.

Importing `System` may make names such as `Math`, `String`, `Object`, and `Array` resolve to .NET types. These are .NET APIs, not JavaScript built-ins. For example, .NET uses `Math.Sqrt` rather than `Math.sqrt`.

Literal arrays and objects work without global `Array` and `Object` constructors. Use `[]`, `{}`, `for...in`, and the documented array methods.

## Evaluation differences

| Area | Current Mosaic behavior | Writing guidance |
| --- | --- | --- |
| `&&`, `\|\|`, `??` | Both operands are evaluated | Use `if` or `?:` to guard calls or expensive work |
| `&&=`, `\|\|=`, `??=` | Right side is evaluated before deciding the assignment | Keep right sides free of unintended side effects |
| Optional calls and computed access | Arguments and computed-key expressions can be evaluated even when the receiver/callee is absent | Use `if` when skipping those expressions matters; repeat `?.` at nullable member boundaries |
| Parameter and destructuring defaults | Default expressions can run even when a value exists; omission and explicit null/undefined are not interchangeable | Use simple defaults or explicit checks in a function body |
| Declarations | Executed in source order | Declare before use; do not depend on hoisting |
| `switch` | Default body is handled after other cases; later case tests can still be evaluated during fall-through | Put `default` last, use constant case values, and break explicitly |
| `finally` | Its returned control-flow value does not replace the earlier one | Put cleanup there, not a replacement return value |

## Values and object differences

- **Numbers:** literal values can retain .NET numeric types; arithmetic normally uses doubles. Applications can change both rules. Equality and numeric conversions are not a full implementation of JavaScript coercion. Keep operands of comparable types.
- **Bitwise operations:** implementation width and conversion depend on the underlying numeric types, often using 64-bit values. Do not assume JavaScript's uniform 32-bit behavior, particularly for negative values and `>>>`.
- **Missing values:** the default friendly configuration usually maps missing values to `null`; stricter configurations can throw or preserve undefined. Array methods such as `find` and empty `pop` can still return an undefined sentinel. Do not treat a missing-value test as proof that a property exists.
- **Strings:** use .NET members such as `Length`, `Trim`, `Substring`, and `ToUpper`.
- **Objects:** there is no complete JavaScript prototype system. `this`, class-based methods, and prototype-based constructors are unavailable. Use object literals and functions receiving explicit objects.
- **`delete`:** assigns a missing value to the reference instead of removing the property. The property can remain enumerable. Use an application's removal API, or construct a new object containing the properties you want.
- **`in`:** checks keys on supported .NET `IDictionary` values; it is not a general JavaScript property/prototype test. Do not assume it works for every script object implementation. Use documented lookup methods or iterate keys.
- **`instanceof`:** uses .NET runtime-type-name comparison, not JavaScript prototype inheritance or general .NET assignability. Do not write `value instanceof SomeType` expecting browser behavior; use an application-provided type check when needed.
- **Destructuring:** object alias patterns do not bind aliases normally, object rest does not collect remaining fields, and array patterns are not implemented as function parameters. Destructuring declarations also have initializer-resolution differences in asynchronous execution; prefer separate declarations and assignment or explicit member access. Array rest produces a .NET list and short source sequences can leave bindings unassigned. See [arrays and objects](Arrays-and-Objects.md#destructuring).
- **Object spread:** `{ ...source }` enumerates into numbered keys rather than generally copying named properties; asynchronous execution has additional source-resolution limitations. Copy properties explicitly.
- **Array methods:** index handling, equality, sorting, and scalar `concat` differ from standard JavaScript. See the [array method reference](Arrays-and-Objects.md#array-method-reference).

## Application configuration

The default engine uses a friendly configuration, but an embedding application can change it. Relevant choices include whether undeclared names throw, whether missing values are null or undefined, whether null member access is tolerated, where `var` and implicit assignments are scoped, numeric handling, availability of `arguments`, top-level `await`, permitted imports, and member access.

A preset described as ECMAScript-style changes selected options; it does not turn Mosaic into a fully conforming ECMAScript engine. For portable scripts, declare names, handle missing data explicitly, avoid implicit conversions, and use this reference's supported patterns.

## Troubleshooting

| Symptom | What to check |
| --- | --- |
| A type name is missing or cannot be constructed | Import its exact namespace, check that the application loaded the library, and check for a variable shadowing the type |
| `include` is rejected | Namespace spelling and application permissions; use an unquoted dotted name on the same line |
| `System.Text.StringBuilder` is unavailable after an include | Use `StringBuilder`; fully qualified paths need separate exposure by the application |
| `.Where(...)` cannot be called | Include `System.Linq`, verify the source is enumerable, and check callback arguments |
| `.push(...)` or `.length` fails on a query result | `ToArray()` returns a .NET array; use `Length` or convert with `[...result]` |
| A string method is unavailable | Use the .NET member name and capitalization, such as `ToUpper()` |
| A method overload cannot be selected | Check argument types/count, callback parameter count, and explicit `GenericArguments` |
| `JSON` or `globalThis` is missing | These require application-provided globals |
| A guarded expression still calls a function | Logical operators evaluate both sides; use `if` or `?:` |
| A function or variable used earlier is missing | Move its declaration before first use |
| A rerun says a name is already declared | The application may preserve the scope; use its reset/new-scope workflow or its recommended function entry point |
| Error details appear empty | Caught values are .NET exceptions; use `Message` or `ToString()` |
| A script stops despite `catch` | Execution cancellation propagates to the application |
| Browser or npm code parses but fails at runtime | Check the feature table and available globals; use the application's supported APIs |
