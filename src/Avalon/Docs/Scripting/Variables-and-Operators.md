# Variables, values, and operators

[Table of contents](README.md)

## Declaring variables

```javascript
let count = 2;
count = count + 1;

const title = 'Inventory';
var legacyTotal = 0;
```

| Declaration | Use |
| --- | --- |
| `let` | A value you will reassign; visible within its block |
| `const` | A binding you will not reassign; visible within its block |
| `var` | Normally visible throughout the enclosing function or global scope; the application can change this rule |

A `const` object can still have its properties changed, and a `const` array can still grow. Only reassignment of the variable is prohibited.

```javascript
const settings = { enabled: false };
settings.enabled = true;

const names = [];
names.push('Ada');
```

Declare variables and functions before using them. Mosaic executes declarations in statement order; do not rely on JavaScript hoisting or temporal-dead-zone behavior. Always declare variables explicitly. Some application configurations allow assignment to an undeclared name, but its scope depends on application settings.

## Values and numbers

Values include numbers, strings, booleans, `null`, arrays, objects, functions, and objects supplied by .NET.

```javascript
let whole = 42;
let fraction = 3.5;
let enabled = true;
let missing = null;
let labels = ['one', 'two'];
let record = { name: 'Ada', score: 42 };
let kind = typeof record; // "object"
```

By default, number literals can become .NET `Int32`, `Int64`, or `Double` values. Arithmetic normally converts these numbers to `Double`; the application can select different numeric rules. This matters when passing values to overloaded .NET methods or looking them up in typed collections. `typeof` reports these numeric types as `"number"`.

Avoid assuming all numbers use browser JavaScript's exact coercion rules. Use consistent numeric values for comparisons and the application's conversion APIs when a specific type is required. For example, with `include System;`, `Convert.ToDouble('12.5')` converts text using .NET's current culture; accepted decimal separators depend on the application environment.

## Strings

```javascript
let name = 'Ada';
let greeting = "Hello, " + name;
let summary = `${name} has ${2 + 3} items.`;
let twoLines = 'First line\nSecond line';
// summary: "Ada has 5 items."
```

`${...}` is required for interpolation; `{name}` alone is literal text. Escape a quote or backslash with `\`. Strings expose .NET members, such as `Length`, `Substring`, `Replace`, and `ToUpper`:

```javascript
let text = '  mosaic  ';
let cleaned = text.Trim().ToUpper(); // "MOSAIC"
let size = cleaned.Length;          // 6
let prefix = cleaned.Substring(0, 3); // "MOS"
```

Use those names and their capitalization. A standard JavaScript string prototype, including lowercase `toUpperCase()` and `length`, is not supplied.

## Missing values

The default configuration is forgiving: unresolved names and many missing accesses produce `null`, and the usual undefined value is collapsed to `null`. An application can instead preserve `undefined` or throw when an undeclared name or missing reference is accessed.

Use declared variables, explicit `null` values, and the application's documented property names. Optional member access is available:

```javascript
let customer = null;
let city = customer?.Address?.City;
let caption = city ?? 'Unknown';
// caption: "Unknown"
```

Some array methods return a distinct undefined value even in the default configuration. Do not assume every missing-value path produces precisely the same value. See [compatibility](Compatibility.md).

## Operators

| Purpose | Operators |
| --- | --- |
| Arithmetic | `+`, `-`, `*`, `/`, `%`, `**` |
| Assignment | `=`, `+=`, `-=`, `*=`, `/=`, `%=`, `**=` |
| Increment/decrement | `++value`, `value++`, `--value`, `value--` |
| Comparison | `==`, `!=`, `===`, `!==`, `<`, `<=`, `>`, `>=` |
| Logical and defaults | `!`, `&&`, `\|\|`, `??` |
| Conditional expression | `condition ? whenTrue : whenFalse` |
| Bitwise | `&`, `\|`, `^`, `~`, `<<`, `>>`, `>>>` and their compound assignments |
| Other | `typeof`, `void`, comma expressions, `in`, `instanceof`, `delete` |

Parentheses make evaluation order explicit. `+` also joins strings. Comparison, bitwise operations, `in`, `instanceof`, and `delete` have .NET-related differences described in [compatibility](Compatibility.md); their presence does not imply full ECMAScript semantics.

```javascript
let count = 4;
let total = (count + 2) * 3;          // 18
let category = total >= 10 ? 'large' : 'small';
let remainder = total % 5;           // 3
let squared = count ** 2;            // 16
```

`false`, `null`, undefined values, zero, the empty string, and a `Double` NaN are false in conditions. Arrays and objects, including empty ones, are true.

## Logical expressions evaluate both sides

In Mosaic, `&&`, `||`, and `??` evaluate both operands. Their compound assignments (`&&=`, `||=`, `??=`) also evaluate the right-hand expression before deciding what to assign. Do not use them to guard a method call or to delay expensive work.

Use `if` or a conditional expression when only one branch should execute:

```javascript
function describe(customer) {
    if (customer != null) {
        return customer.Name;
    }
    return 'Unknown';
}

let caption = describe(null); // "Unknown"
```

`value ?? 'fallback'` is convenient when the fallback is a simple value. Be especially careful with `value ?? makeFallback()`: `makeFallback()` runs even when `value` is already present.
