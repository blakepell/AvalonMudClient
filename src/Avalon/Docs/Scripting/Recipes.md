# Recipes

[Table of contents](README.md)

Each recipe is a complete, independent script using sample data. Replace the sample data with your application's documented collection when ready. Results are stored in variables; display or save them through your application's output API.

## Build a report with a foreach-style loop

```javascript
const products = [
    { name: 'Notebook', quantity: 3, price: 8 },
    { name: 'Pen', quantity: 5, price: 3 }
];

let lines = [];
let grandTotal = 0;
for (const product of products) {
    let total = product.quantity * product.price;
    grandTotal += total;
    lines.push(`${product.name}: ${product.quantity} x ${product.price} = ${total}`);
}
lines.push(`Total: ${grandTotal}`);
let report = lines.join('\n');
// Notebook: 3 x 8 = 24
// Pen: 5 x 3 = 15
// Total: 39
```

## Validate records and continue after a bad item

```javascript
const rows = [
    { name: 'Notebook', quantity: 3 },
    { name: '', quantity: 2 },
    { name: 'Pen', quantity: 0 }
];

function validate(row) {
    if (row.name == '') {
        throw 'Name is required.';
    }
    if (row.quantity <= 0) {
        throw 'Quantity must be positive.';
    }
    return row;
}

let accepted = [];
let errors = [];
for (let i = 0; i < rows.length; i++) {
    try {
        accepted.push(validate(rows[i]));
    } catch (error) {
        errors.push(`Row ${i + 1}: ${error.Message}`);
    }
}
// accepted.length: 1
// errors: ["Row 2: Name is required.", "Row 3: Quantity must be positive."]
```

## Find an item and stop immediately

```javascript
const rows = [
    { code: 'NB', quantity: 3 },
    { code: 'PN', quantity: 0 },
    { code: 'FD', quantity: 2 }
];

function firstOutOfStock(items) {
    for (const item of items) {
        if (item.quantity == 0) {
            return item;
        }
    }
    return null;
}

let found = firstOutOfStock(rows);
let message = found != null ? `Restock ${found.code}` : 'All items in stock';
// message: "Restock PN"
```

## Query, sort, and number a report

Requires `include System.Linq` to be allowed.

```javascript
include System.Linq;

const products = [
    { name: 'Notebook', quantity: 3, price: 8 },
    { name: 'Pen', quantity: 5, price: 3 },
    { name: 'Folder', quantity: 0, price: 5 }
];

let rows = products
    .Where(product => product.quantity > 0)
    .OrderByDescending(product => product.quantity * product.price)
    .Select((product, index) =>
        `${index + 1}. ${product.name}: ${product.quantity * product.price}`)
    .ToArray();

let report = [...rows].join('\n');
// 1. Notebook: 24
// 2. Pen: 15
```

## Group work by owner

Requires `include System.Linq` to be allowed.

```javascript
include System.Linq;

const tasks = [
    { owner: 'Ada', hours: 2 },
    { owner: 'Grace', hours: 3 },
    { owner: 'Ada', hours: 4 }
];

let totals = tasks
    .GroupBy(task => task.owner)
    .Select(group => ({
        owner: group.Key,
        tasks: group.Count(),
        hours: group.Sum(task => task.hours)
    }))
    .OrderBy(row => row.owner)
    .ToArray();

let lines = [];
for (const row of totals) {
    lines.push(`${row.owner}: ${row.tasks} tasks, ${row.hours} hours`);
}
let report = lines.join('\n');
// Ada: 2 tasks, 6 hours
// Grace: 1 tasks, 3 hours
```

## Build a typed lookup

Requires `System`, `System.Collections.Generic`, and `System.Linq` imports to be allowed.

```javascript
include System;
include System.Collections.Generic;
include System.Linq;

let quantities = new Dictionary(String, Int32);
quantities.Add('NB', 3);
quantities.Add('PN', 5);

let codes = quantities.Keys.OrderBy(code => code).ToArray();
let lines = [];
for (const code of codes) {
    lines.push(`${code}=${quantities[code]}`);
}
let report = lines.join(', '); // "NB=3, PN=5"
```

## Display the name and row counts of all database tables

```javascript
let sql = `
SELECT name
FROM sqlite_schema
WHERE type = 'table' AND name NOT LIKE 'sqlite_%';
`;

let results = db.Select(sql);

terminal.Clear("Output")
terminal.WriteLine("Output", `${"Table Name".PadRight(35)} ${"Rows".PadLeft(10)}`, false);
terminal.WriteLine("Output", "-".Repeat(46), false);

foreach (var item in results) {
    let rows = db.SelectValue(`SELECT COUNT(*) FROM "${item.name.Replace('"', '""')}"`);
    let msg = `${item.name.PadRight(35)} ${rows.ToString().PadLeft(10)}`;
    terminal.WriteLine("Output", msg, false);
}
```