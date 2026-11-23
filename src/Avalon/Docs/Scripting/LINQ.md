# LINQ queries

[Table of contents](README.md)

LINQ provides .NET methods for querying sequences. In Mosaic you call those methods from JavaScript and pass script functions as predicates and selectors. Use method chains such as `.Where(...).Select(...)`; C# query syntax (`from ... where ... select ...`) is not part of the language.

## Filter and project

This example requires `System.Linq` imports to be allowed. It creates all its own data:

```javascript
include System.Linq;

const products = [
    { name: 'Notebook', price: 8, inStock: true },
    { name: 'Pen', price: 3, inStock: true },
    { name: 'Folder', price: 5, inStock: false }
];

let names = products
    .Where(product => product.inStock)
    .OrderBy(product => product.price)
    .Select(product => product.name)
    .ToArray();
// names: .NET array containing "Pen", "Notebook"
```

`Where` keeps items whose predicate returns true. `OrderBy` sorts by a selected key. `Select` transforms each item into a result. `ToArray` runs the query and stores its results in a .NET array.

Use the correct capitalization: LINQ methods are `Where` and `Select`; script array methods are `filter` and `map`. Script arrays and compatible application-provided .NET collections can both be query sources.

## Iterate or materialize

Many LINQ operations are deferred: creating the query does not yet visit its items. Iterating it or calling a terminal operation evaluates it. Repeated enumeration can run callbacks again and can observe changes in the source.

```javascript
include System.Linq;

let query = Enumerable.Range(1, 5).Where(value => value > 2);
let total = 0;
for (const value of query) {
    total += value;
}
// total: 12

let snapshot = query.ToArray(); // .NET array: 3, 4, 5
let count = snapshot.Length;   // 3
let scriptArray = [...snapshot];
scriptArray.push(6);           // Script array methods are now available.
```

`ToArray()` creates a fixed-size .NET array with `Length`. `ToList()` creates a .NET list with `Count` and methods such as `Add`. Neither automatically becomes a script array with `push` and lowercase `length`. Materialization copies the sequence, not the nested objects inside it.

## Common operations

These are commonly useful LINQ methods, not an exhaustive list of .NET overloads. Availability depends on imported or application-registered extensions and compatible argument types.

| Task | Method pattern |
| --- | --- |
| Keep matching items | `Where(item => condition)` |
| Transform items | `Select(item => value)` |
| Use the zero-based position | `Select((item, index) => value)` or `Where((item, index) => condition)` |
| Sort ascending / descending | `OrderBy(item => key)` / `OrderByDescending(item => key)` |
| Break sorting ties | `ThenBy(item => key)` / `ThenByDescending(item => key)` |
| Skip or limit results | `Skip(count)` / `Take(count)` |
| Group items | `GroupBy(item => key)` |
| Count items | `Count()` / `Count(item => condition)` |
| Test for matches | `Any()` / `Any(item => condition)` / `All(item => condition)` |
| Select one item | `First()`, `FirstOrDefault()`, `Single()`, `SingleOrDefault()`; predicate overloads also exist |
| Numeric summaries | `Sum(item => number)`, `Average(item => number)`, `Min(item => value)`, `Max(item => value)` |
| Remove duplicates | `Distinct()` |
| Store results | `ToArray()`, `ToList()`, `ToDictionary(keySelector, valueSelector)` |

`First` throws for an empty result. `Single` requires exactly one item; `SingleOrDefault` still throws for multiple items. `OrDefault` methods return the .NET element type's default on an empty result, which can be `0` or `false` rather than `null`. Empty numeric aggregates have method-specific behavior. Use `Any` to check for data when necessary.

`Distinct`, grouping, and dictionary keys use .NET equality. Objects with identical properties are not automatically equal by content. Select a scalar key when grouping by a property.

## Indexed projections and paging

```javascript
include System.Linq;

let page = Enumerable.Range(1, 20)
    .Skip(5)
    .Take(3)
    .Select((value, index) => `${index + 1}: ${value}`)
    .ToArray();
// page: "1: 6", "2: 7", "3: 8"
```

The index belongs to the sequence entering that operation, so this `Select` starts at zero after `Skip` and `Take`.

## Grouping and summaries

```javascript
include System.Linq;

const sales = [
    { category: 'Books', amount: 12 },
    { category: 'Games', amount: 30 },
    { category: 'Books', amount: 8 }
];

let summary = sales
    .GroupBy(sale => sale.category)
    .Select(group => ({
        category: group.Key,
        count: group.Count(),
        total: group.Sum(sale => sale.amount)
    }))
    .OrderBy(row => row.category)
    .ToArray();
// Books: count 2, total 20; Games: count 1, total 30.
```

Each group has a `Key` property and is itself enumerable. You can loop over it or apply another LINQ operation.

## Dictionaries

```javascript
include System.Linq;

const products = [
    { code: 'NB', name: 'Notebook' },
    { code: 'PN', name: 'Pen' }
];

let byCode = products.ToDictionary(product => product.code, product => product.name);
let name = byCode['NB']; // "Notebook"
```

Keys must be unique and valid for the dictionary type. Duplicate keys cause an error. Use grouping if multiple items legitimately share a key. `ContainsKey(key)` checks before accessing an optional key.

## Explicit generic arguments

Many .NET methods use generic types. Mosaic normally chooses `Object` for unspecified generic arguments; for extension methods it can infer the first generic type from the source collection. This is less extensive than C# compiler inference. Overloaded numeric methods and typed results may require explicit types.

`GenericArguments(...)` supplies types for the next method call only. It does not change all later operations in the chain. The arguments are type names, not strings or example values.

```javascript
include System;
include System.Linq;

const products = [
    { code: 'NB', name: 'Notebook' },
    { code: 'PN', name: 'Pen' }
];

let names = products
    .GenericArguments(Object, String)
    .Select(product => product.name)
    .GenericArguments(String)
    .ToArray();
// names is a .NET String array.

let byCode = products
    .GenericArguments(Object, String, String)
    .ToDictionary(product => product.code, product => product.name);
// Source item type, key type, value type: Object, String, String.
```

Without explicit arguments, a two-selector `ToDictionary` commonly produces a dictionary whose key and value types are both `Object`. This works for many script-only tasks. Specify types when an application API expects a particular .NET collection type. The application can document or expose any custom types you need.

## Static calls

The imported `Enumerable` type also exposes LINQ as static methods:

```javascript
include System;
include System.Linq;

let source = Enumerable.Range(1, 5);
let filtered = Enumerable.GenericArguments(Int32).Where(source, value => value > 3);
let result = Enumerable.GenericArguments(Int32).ToArray(filtered); // 4, 5
```

Static generic calls can use `Enumerable.GenericArguments(...)` before the method. Here the integer source needs explicit `Int32` arguments because static calls do not have extension receiver type inference. Extension syntax is usually easier to read because the source is already on the left of the dot.
