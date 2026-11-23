# Conditions and loops

[Table of contents](README.md)

## if and else

```javascript
let score = 82;
let grade;

if (score >= 90) {
    grade = 'A';
} else if (score >= 80) {
    grade = 'B';
} else {
    grade = 'Needs improvement';
}
// grade: "B"
```

Use a conditional expression for a small choice: `let label = score >= 80 ? 'Pass' : 'Review';`.

## switch

```javascript
let command = 'save';
let message = '';

switch (command) {
    case 'open':
        message = 'Opening';
        break;
    case 'save':
        message = 'Saving';
        break;
    default:
        message = 'Unknown command';
        break;
}
// message: "Saving"
```

Cases use Mosaic's strict equality comparison. Without `break`, execution continues into subsequent cases. Keep `default` last: Mosaic handles it after the other cases, regardless of where it appears in the source. Use simple case values without side effects.

## Counted for loops

The three parts are initialization, a condition checked before each iteration, and an update after each iteration.

```javascript
let squares = [];
for (let i = 0; i < 5; i++) {
    squares.push(i * i);
}
// squares: [0, 1, 4, 9, 16]
```

Index an array when you need both its position and value:

```javascript
const names = ['Ada', 'Linus', 'Grace'];
let lines = [];
for (let i = 0; i < names.length; i++) {
    lines.push(`${i + 1}. ${names[i]}`);
}
```

## Foreach-style iteration: for...of

Mosaic uses `for...of` to visit values. There is no `foreach (...)` language keyword.

```javascript
const prices = [12, 8, 5];
let total = 0;
for (const price of prices) {
    total += price;
}
// total: 25
```

The source can be a script array, a .NET collection, or a LINQ sequence. It must be enumerable. Include a declaration (`const`, `let`, or `var`) in the loop header. The runtime expects that declaration; use `for (const item of items)`, not assignment to a previously declared `item`.

Choose `const` when the iteration variable will not be reassigned. You can still change properties on each item if those properties are writable.

## Keys: for...in

`for...in` visits keys or member names, rather than values:

```javascript
const options = { theme: 'dark', size: 12 };
let entries = [];
for (const key in options) {
    entries.push(`${key}: ${options[key]}`);
}
// entries contains "theme: dark" and "size: 12"; do not depend on key order.
```

Always declare the loop variable here too. For script objects this yields keys; for .NET lists it yields indexes and for dictionaries it yields keys. Arbitrary .NET objects can expose public member names, including names other than data properties. Use `for...of` for collection items. Do not depend on browser prototype-enumeration rules or a particular key order.

## while

`while` checks the condition before running the body, so the body can run zero times.

```javascript
let remaining = 3;
let visits = [];
while (remaining > 0) {
    visits.push(remaining);
    remaining--;
}
// visits: [3, 2, 1]
```

## do...while

`do...while` runs the body once before checking the condition.

```javascript
let value = 1;
do {
    value *= 2;
} while (value < 10);
// value: 16
```

## break, continue, and return

`break` leaves the nearest loop or switch. `continue` skips to the next loop iteration. In a counted `for`, the update expression still runs after `continue`.

```javascript
let selected = [];
for (let i = 0; i < 10; i++) {
    if (i == 7) {
        break;
    }
    if (i % 2 == 0) {
        continue;
    }
    selected.push(i);
}
// selected: [1, 3, 5]
```

Inside a function, `return` exits the function, including any loop it is running. Labeled statements and labeled loop control are not supported. To leave multiple nested loops, return from a helper function or use a flag.

## Array forEach

Script arrays also support a callback method named `forEach`:

```javascript
let lines = [];
['red', 'green'].forEach((color, index) => {
    lines.push(`${index}: ${color}`);
});
```

This is a method call. You cannot use `break` or `continue` to control its iteration, and `return` only leaves the callback. Prefer `for...of` when you need early exit or sequential `await` operations.
