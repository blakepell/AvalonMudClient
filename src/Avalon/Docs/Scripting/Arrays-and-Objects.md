# Arrays and objects

[Table of contents](README.md)

## Objects and properties

```javascript
let person = { name: 'Ada', active: true };
person.score = 90;
person['display-name'] = 'Ada L.';

let property = 'score';
let score = person[property]; // 90
```

Use dot notation for a known property name and brackets for a computed key or a name containing punctuation. Property names are case-sensitive. Object literals can use variable-name shorthand and computed keys:

```javascript
const name = 'Ada';
const key = 'score';
let person = { name, [key]: 90 };
```

Objects supplied by the application may have fixed, read-only, or restricted properties. A script object is flexible; an arbitrary C# object does not acquire new C# properties when you assign an unknown name.

## Arrays

Script arrays use zero-based indexes and lowercase `length`:

```javascript
let colors = ['red', 'green'];
colors.push('blue');
colors[0] = 'orange';

let first = colors[0];       // "orange"
let last = colors[colors.length - 1]; // "blue"
let count = colors.length;  // 3
```

`length` is writable: reducing it truncates the array; increasing it creates empty slots represented by undefined values. Prefer dense arrays and valid indexes when sharing scripts between applications.

### Array method reference

These methods belong to script arrays made with `[]`. A .NET array or list has its own API; see [collection types](Application-Objects.md#collection-types).

| Method | Result or effect |
| --- | --- |
| `at(index)` | Item at a nonnegative index; use `array[array.length - 1]` for the last item |
| `push(value, ...)` | Append values; return the new length |
| `pop()` | Remove and return the last value |
| `shift()` | Remove and return the first value |
| `unshift(value, ...)` | Prepend values; return the new length |
| `concat(otherArray, ...)` | New array containing the original and other enumerable sequences |
| `slice(start, end)` | New array from `start` up to, but excluding, `end`; either argument can be omitted |
| `splice(start, deleteCount, ...items)` | Change the original and return removed items; omit `deleteCount` to remove the rest |
| `join(separator)` | Joined text; default separator is a comma |
| `indexOf(value, fromIndex)` | First matching index, or `-1`; `fromIndex` is optional |
| `lastIndexOf(value, fromIndex)` | Last matching index, or `-1`; `fromIndex` is optional |
| `includes(value, fromIndex)` | Whether a matching value exists; `fromIndex` is optional |
| `forEach(callback)` | Run a callback for each item |
| `map(callback)` | New array of callback results |
| `filter(callback)` | New array containing items for which the callback is true |
| `find(callback)` | First matching value; undefined when there is no match |
| `findIndex(callback)` | First matching index, or `-1` |
| `some(callback)` / `every(callback)` | Whether any / all items pass the callback |
| `reduce(callback, initial)` | Accumulate from left to right |
| `reduceRight(callback, initial)` | Accumulate from right to left |
| `reverse()` | Reverse the original array and return it |
| `sort(compare)` | Sort the original array and return it; comparator is optional |
| `fill(value, start, end)` | Fill a range in the original; bounds are optional and `end` is exclusive |
| `copyWithin(target, start, end)` | Copy a range within the original; `start` and `end` are optional |
| `flat(depth)` | New array flattening nested script arrays; default depth is one |
| `flatMap(callback)` | Map and flatten one level of returned script arrays |
| `keys()` / `values()` / `entries()` | Enumerable indexes, values, or `[index, value]` pairs |
| `toString()` / `toLocaleString()` | Text representation; locale form delegates to the same implementation |

Most callbacks accept `(value)`, `(value, index)`, or `(value, index, array)`. Reduction callbacks accept `(accumulator, value)`, with optional index and array parameters. No JavaScript `thisArg` overload is provided.

Use valid bounds: methods do not uniformly apply all JavaScript index-clamping rules. Negative `at` indexes currently use the object's property count rather than its array length and can return undefined unexpectedly; use explicit indexing instead. `concat` expects enumerable arguments, not arbitrary scalar values. Searches use .NET equality, so numerically equivalent values with different underlying numeric types may not match. Default `sort()` uses .NET comparison, not JavaScript's default string ordering. Supply a comparator for predictable numeric sorting.

```javascript
let values = [9, 2, 10, 4];
let ordered = values.slice().sort((a, b) => a - b);
let doubled = ordered.map(value => value * 2);
let total = doubled.reduce((sum, value) => sum + value, 0);
// ordered: [2, 4, 9, 10]; total: 50; values is unchanged.
```

Give `reduce` an initial value when the array may be empty. Without one, the first value is the starting accumulator and an empty array throws. `pop()` and `shift()` on an empty array return undefined.

## Spread

Spread expands a sequence into an array or a function call.

```javascript
const first = [1, 2];
let combined = [...first, 3, 4];

const defaults = { enabled: true, title: 'Default' };
let custom = { enabled: defaults.enabled, title: 'Custom' };

function add(a, b) {
    return a + b;
}
let total = add(...first); // 3
```

Copies are shallow: nested objects are still shared. Array and call spread can also consume .NET enumerable values. Object-literal spread is not a general property-copy operation in Mosaic: its implementation enumerates the source into numbered keys, and behavior also differs by execution mode. Copy named properties explicitly, as above.

## Destructuring

Array destructuring extracts positions; object destructuring extracts properties with matching names. This example uses assignment to declared variables:

```javascript
const values = [10, 20, 30, 40];
let first, second, remaining;
[first, second, ...remaining] = values;

const options = { title: 'Report' };
let title, enabled;
({ title, enabled = true } = options);
// first: 10; second: 20; remaining: [30, 40]
// title: "Report"; enabled: true
```

Destructuring declarations such as `let [a, b] = values;` and `let { title } = options;` also exist. In the application's asynchronous execution mode, declaration initializers are not consistently resolved to their values: destructuring from a variable can fail or bind incorrect values. Prefer separate declarations and destructuring assignment as above, or ordinary property/index access. This limitation depends on how the application runs the script, even when the script contains no `await`.

This is not a complete implementation of standard JavaScript destructuring:

- Avoid object renaming patterns such as `{ name: displayName }`; the runtime does not bind the alias as standard JavaScript would. Write `let displayName = person.name;`.
- Object rest `{ name, ...rest }` does not collect the remaining properties. Copy needed properties explicitly.
- Default expressions may run even when a value exists. Keep them free of side effects, and do not assume an explicit `null` or undefined member triggers the default.
- Array patterns work in variable declarations and assignments; use a normal parameter for arrays passed to functions.

An array rest binding such as `remaining` above is a .NET list with `Count`, not a script array with `length`. Convert it with `[...remaining]` for script array methods. When the source is exhausted, the pattern processor can stop without assigning remaining bindings; do not rely on standard JavaScript defaults or an automatically created empty rest array for short inputs.

## JSON

The application must expose Mosaic's `JSON` object for these examples. `JSON` is not automatically installed.

```javascript
// Requires the application-provided Mosaic JSON object.
let text = JSON.stringify({ name: 'Ada', scores: [8, 9] });
let restored = JSON.parse(text);
let name = restored.name; // "Ada"
let pretty = JSON.stringify(restored, null, 2);
```

Mosaic's JSON API uses .NET serialization. `parse` accepts a JSON string; no reviver parameter is implemented. The `stringify` replacer parameter is ignored. Zero `space` selects compact output; a nonzero integer selects indented output, rather than setting an exact indentation width. Default settings allow comments and trailing commas and use camel-case naming for .NET properties. The application can customize these settings.
