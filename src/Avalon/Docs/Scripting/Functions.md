# Functions

[Table of contents](README.md)

## Named functions and return values

Declare a function before calling it. Use `return` explicitly in a block-bodied function when its result matters.

```javascript
function lineTotal(quantity, price) {
    return quantity * price;
}

let total = lineTotal(3, 12.5); // 37.5
```

Parameters are local to the call. Functions can call other functions, call themselves recursively, and return functions or objects. Application code can also call a script function if the application supports that workflow.

## Function expressions and arrow functions

```javascript
const double = function (value) {
    return value * 2;
};

const square = value => value * value;
const add = (left, right) => left + right;
const describe = value => {
    return `Value: ${value}`;
};

let result = add(double(3), square(2)); // 10
```

An expression-bodied arrow returns its expression. For an object literal, use parentheses: `const make = name => ({ name: name });`.

Mosaic does not implement JavaScript `this`. Pass the object you need as a parameter, or refer to a variable captured by the function. Do not depend on the browser distinction between arrow-function `this` and ordinary-function `this`.

## Default and rest parameters

```javascript
function greet(name = 'Guest') {
    return `Hello, ${name}`;
}

function sum(...values) {
    return values.reduce((total, value) => total + value, 0);
}

let welcome = greet();            // "Hello, Guest"
let total = sum(2, 3, 4);         // 9
let spreadTotal = sum(...[5, 6]); // 11
```

Default parameters supply a value when the argument is omitted. In this implementation the default expression is evaluated even when an argument was supplied; keep defaults free of side effects. Passing `null` or an explicit undefined value is not the same as omitting an argument.

A final `...values` parameter gathers extra arguments into a script array. Prefer it to `arguments`, which is available only if the application enables that special object.

Object destructuring parameters are supported for simple matching property names:

```javascript
function area({ width, height }) {
    return width * height;
}
let result = area({ width: 4, height: 3 }); // 12
```

Array destructuring parameters are not implemented by the function runtime. Pass an array normally and destructure it inside the function instead. See [destructuring limitations](Arrays-and-Objects.md#destructuring).

## Callbacks

A callback is a function passed to another operation. Array methods, LINQ methods, and application APIs can accept them.

```javascript
const scores = [40, 75, 90];
let passed = scores.filter(score => score >= 60);
let labels = passed.map((score, index) => `${index + 1}: ${score}`);
// labels: ["1: 75", "2: 90"]
```

Use a predicate returning `true` or `false` for LINQ `Where`, a value-returning function for `Select`, and a block callback for several statements. Match the callback's parameter count to the overload you intend to call. For example, `(item, index)` selects indexed forms of `Select` and `Where`.

## Closures

A function can retain access to values in its enclosing scope:

```javascript
function makeAdder(amount) {
    return value => value + amount;
}

const addFive = makeAdder(5);
let result = addFive(7); // 12
```

Declare captured values before creating the function. If your application calls scripts or callbacks concurrently, avoid assuming that a shared counter or object update is atomic. A thread-safe scripting environment does not make a multi-step operation such as `total += value` atomic.

For asynchronous functions, see [errors and asynchronous work](Errors-and-Async.md).
