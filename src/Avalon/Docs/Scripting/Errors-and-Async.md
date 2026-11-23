# Errors and asynchronous work

[Table of contents](README.md)

## throw and catch

Use `throw` to stop an operation with an explanation. Catch errors around work you know how to handle.

```javascript
function requirePositive(value) {
    if (value <= 0) {
        throw 'Quantity must be greater than zero.';
    }
    return value;
}

let message = '';
try {
    requirePositive(0);
} catch (error) {
    message = error.Message;
}
// message: "Quantity must be greater than zero."
```

Mosaic turns a thrown non-exception value into a .NET exception using its text. `catch (error)` receives an exception object, including when you originally threw a string. Use `error.Message` or `error.ToString()`, rather than assuming JavaScript's lowercase `error.message`. Use a named catch parameter.

You can rethrow the caught exception with `throw error;`. Avoid `throw null`, since converting it to a message is not supported. A JavaScript `Error` constructor is not built in; an exposed .NET exception type can be constructed if your application permits it.

## finally

`finally` runs after the `try`/`catch` path and is useful for ordinary cleanup:

```javascript
let finished = false;
let message = '';
try {
    throw 'Cannot process this item.';
} catch (error) {
    message = error.Message;
} finally {
    finished = true;
}
// finished: true
```

Keep `finally` focused on cleanup. A `return`, `break`, or `continue` from a finalizer does not override the earlier control flow in the same way as standard JavaScript. Cancellation can interrupt finalizer work, so essential resource ownership remains part of the application's contract.

Syntax errors occur while the script is parsed, before its `try` blocks execute. Fix those in the source. Runtime errors include invalid member access, unsupported syntax that the parser accepted, incompatible method arguments, and errors thrown by application methods. Exceptions from reflected .NET calls may be wrapped; the detailed error text can include an inner exception.

## async and await

Use `await` with an asynchronous application method to obtain its completed result. Mosaic can await .NET `Task`, `Task<T>`, `ValueTask`, and `ValueTask<T>` values; applications may also provide custom await behavior. Awaiting an ordinary value returns that value.

This example requires the application to expose `service.LoadNameAsync(id)`, an asynchronous operation returning a name:

```javascript
// Requires service.LoadNameAsync(id).
async function loadGreeting(id) {
    let name = await service.LoadNameAsync(id);
    return `Hello, ${name}`;
}

let greeting = await loadGreeting(42);
```

Top-level `await` is enabled by the default parser configuration. The application can change that setting. The `async` and `await` syntax does not provide the browser Promise API or an event loop. The host's execution mode determines whether waiting suspends asynchronously or blocks its executing thread. Use the workflow the application's documentation recommends.

## Awaiting operations in order

Use a loop when each asynchronous operation must finish before the next starts:

```javascript
// Requires service.LoadNameAsync(id).
let names = [];
for (const id of [1, 2, 3]) {
    let name = await service.LoadNameAsync(id);
    names.push(name);
}
```

Do not use an async `forEach` callback or a LINQ predicate as a replacement for this sequence. These synchronous collection APIs do not provide an await-each-callback contract. Parallel work requires an appropriate application API and attention to shared state; it is not implied by `async`.

Wrap awaited work in `try`/`catch` to handle failures just as you would a synchronous call.

## Cancellation

The application can cancel script execution, for example when the user presses Stop or a time limit expires. Mosaic checks for cancellation during operations such as loop iterations and function calls. Cancellation associated with that execution is propagated past script `catch` blocks; scripts cannot reliably swallow it and keep running. Write bounded loops and let cancellation return control to the application.
