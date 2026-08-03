---
description: "Use when writing or reviewing TypeScript/React code. Covers project coding standards including formatting, naming conventions, import order, TypeScript configuration, error handling, and React best practices."
applyTo: ["src/**/*.ts", "src/**/*.tsx"]
---
# Frontend Coding Standards

## File Organization

- **One class/component per file**
- **File naming**: PascalCase for all files (e.g., `CropCycleConfigView.tsx`, `AppService.ts`, `ClientUtils.ts`)

## Import Organization

Imports should be organized in this order:

1. External React libraries
2. Neo framework imports
3. Third-party libraries
4. Local types and models
5. Local components
6. Local services
7. Local styles

```ts
import React from 'react';
import { observer } from 'mobx-react';
import { Neo, NeoGrid } from '@singularsystems/neo-react';
import { Model, Validation } from '@singularsystems/neo-core';
import { CatalogueColumn } from '@singularsystems/neo-react-services';
import MyEntity from '../../../Models/MyArea/MyEntity';
import MyEntityVM from '../MyEntityVM';
import SharedModals from '../../Modals/SharedModals';
```

## Formatting

- **Indentation**: 4 spaces (configured in the .code-workspace file)
- **Line breaks**: Single blank lines between logical sections
- **No trailing whitespace**
- **End files with a newline**

### JSX Formatting

- Self-closing tags for components without children: `<Component />`
- Multi-line JSX wrapped in parentheses
- Properly indent nested elements

### Comments

- JSDoc comments for public methods and classes
- Inline comments for complex logic explanation
- TODO comments should include context

### Strings

- **Double quotes** for strings in JSX attributes
- Omit parentheses for string JSX attribute values
- **Template literals** for string interpolation

## TypeScript Configuration

The project uses strict TypeScript settings:

- `strict: true` — Enable all strict type checking options
- `noImplicitAny: true` — Raise error on expressions with implied 'any' type
- `noImplicitThis: true` — Raise error on 'this' with implied 'any' type
- `noImplicitReturns: true` — Report error when not all code paths return a value
- `strictNullChecks: true` — Enable strict null checks
- `forceConsistentCasingInFileNames: true` — Enforce consistent file name casing

## Naming Conventions

- **Variables**: camelCase (e.g., `selectedItem`, `isActive`)
- **Functions/Methods**: camelCase (e.g., `saveEntity()`, `loadData()`)
- **Classes**: PascalCase (e.g., `CropCycleConfig`, `AppService`)
- **Constants**: camelCase or UPPER_SNAKE_CASE for true constants
- **Interfaces**: PascalCase with `I` prefix (e.g., `IComponentProps`)
- **Type aliases**: PascalCase
- **Enums**: PascalCase for name, PascalCase for members

## Variable Declarations

- **`const`** for immutable bindings (preferred)
- **`let`** when mutation is needed
- **Never use `var`**

## Loops

Use `for...of` loops instead of `.forEach()` callback style.

## Boolean Logic

- Use explicit boolean checks for clarity when appropriate
- Truthiness checks acceptable for null/undefined checking

```ts
if (!isTerminalSelected) { }
if (dealer?.isInactive) { }
```

## Null/Undefined Handling

- Use optional chaining `?.` for potentially undefined values
- Use nullish coalescing `??` for default values

## Type Safety

- No explicit `any` unless absolutely necessary
- Use generic types for reusable components
- Define proper return types for functions
- Use union types for multiple possible types
- Use type guards for runtime type checking
- Explicit type annotations for class properties; type inference acceptable for simple assignments

## Access Modifiers

- `public` for component methods and properties (can be explicit or implicit)
- `private` for internal state and helpers
- `protected` for methods intended for override in subclasses

## Async/Await

Prefer async/await over promise chains.

## Arrow Functions vs Regular Functions

- Arrow functions for callbacks and short inline functions
- Regular functions for component methods
- Avoid arrow functions in JSX when configured

## Error Handling

- **Do not** use try-catch blocks in domain code
- **Do not** use `console.log`

## API Calling Rules

- All HTTP calls must use axios
- All HTTP calls must be in methods of an `ApiClient` class

## React Best Practices

- Extract complex JSX into separate components or methods
- Class-based components extending `React.Component` or `Views.ViewBase`
- Props interfaces prefixed with `I` (e.g., `IMyComponentProps`)
- Prefer functional composition over inheritance where appropriate

## DRY Principle

Do not duplicate code, styles, or patterns. Before writing new code, check if an equivalent implementation already exists. If deviating from DRY is genuinely warranted (readability, tight coupling risk), raise it with the user first.

## Module System

- ES6 modules: use `import`/`export`
- Module resolution: Node-style (`moduleResolution: "node"`)
- Default exports for models and main components
- Named exports for utilities and services
- Absolute imports for cross-module dependencies
- Relative imports for local files

## Code Comments and Documentation

- Document public APIs with JSDoc comments
- Add inline comments for complex business logic
- Keep comments up-to-date with code changes
