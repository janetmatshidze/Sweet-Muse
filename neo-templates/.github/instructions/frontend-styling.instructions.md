---
description: "Use when implementing or reviewing UI styling. Covers Bootstrap usage, SCSS standards, CSS scoping, icons, theming, accessibility, and naming conventions."
applyTo: ["src/**/*.scss", "src/**/*.tsx", "src/**/*.ts"]
---
# UI Styling Guidelines

## Purpose

This document defines the UI and styling standards for this web application to ensure:

* Consistency
* Maintainability
* Accessibility
* Alignment with Bootstrap best practices

---

## Scope & Assumptions

* The app uses Bootstrap.
* Bootstrap is included by importing `bootstrap.scss` from the Bootstrap npm package.
* Bootstrap variables are overridden in the `Variables-Bootstrap.scss` file, which is imported before the main Bootstrap build.
* Bootstrap variables are overridden by specifying Sass variables, not CSS variables.
* The primary color of Bootstrap is overridden to match the app’s branding, using the `$main-color` variable.
* The app may introduce additional CSS variables for concepts not represented in Bootstrap, but should prefer Bootstrap variables when applicable.
* The app may use CSS variables for theming and design tokens, but should not use CSS variables to override Bootstrap semantics that are already represented by Bootstrap variables.

## Styling Decision Guide

When implementing UI:

* Use Bootstrap utilities for adjusting spacing, layout, alignment, typography
* In most cases, custom styling or components should not be necessary.
* Use the components provided by Neo.
* Use existing project components where possible.
* Create new components only when the UI is reused or expected to be reused, and follow the guidelines below.

### Use **custom CSS / SCSS** when:

* Bootstrap cannot achieve the requirement
* The design is component-specific or complex

### Style overrides

* Only target nested elements using class names or by id. 
* Do not use `:nth-child()` or other structural selectors to override styles.

---

## Bootstrap Usage Standards

Always check Bootstrap utilities first.

**Examples:**

* `.d-flex`, `.align-items-center`
* `.mt-3`, `.p-2`
* `.row`, `.col`

> **Rule:** If Bootstrap solves it, do not recreate it.

### Responsive Design

* Bootstrap breakpoints (`sm`, `md`, `lg`, `xl`, `xxl`)
* Responsive utility classes

### Override Only the Delta

When modifying Bootstrap components:

* Only override what differs
* Do not restate defaults
* Remove redundant legacy styles

### Z-Index Discipline

* Use Bootstrap’s defined z-index scale (`$zindex-*` variables)
* If a new layer is required:
  * Extend the scale in Variables-Bootstrap.scss
  * Do not hardcode values in components
* Do not introduce arbitrary values (e.g. `9999`)

---

## No Inline Styles

Inline styles are allowed ONLY when:
* Values are runtime-dynamic (e.g. calculated widths, positions)
* The value cannot be represented using:
  * Bootstrap utilities
  * CSS variables
* The style will not be reused

---

## SCSS Standards

* Do not create mixins.
* Using existing mixins is allowed if the mixin is intended for the specific use case.
* Do not create custom functions. 

### Imports

* Do not import `scss` files into views or components. This gives the impression that styles are isolated when they are not.
* All `scss` files must be imported into `App.scss`, or into a module index file that is imported into `App.scss`.
---

## CSS Modules

CSS Modules are **not used** in this project. Component-scoped styles are achieved through:

* Adding a top level class to the component root element (e.g. `.employee-card`, `.task-list`)
* Nesting classes within that class.

---

# Component Standards

* This project does not have component isolation. All scss files are global and styles are scoped via class naming conventions and nesting.
* Do not isolate a new component at a page level without explicit instruction.

---

## Refactoring Rule

When modifying a component:

* Remove unused styles
* Replace custom CSS with utilities where possible
* Eliminate duplication

---

## Icon Usage

Use Neo icon APIs first.

Preferred patterns:

* Use the `icon` prop where available (e.g. Button, Card, Tab).
* Use `Neo.Icon` when a component does not expose an icon prop.
* Use material symbols naming to specify icon names.

**Rules:**

* Must prefer icon props over manual icon markup
* Must use `Neo.Icon` when no icon prop is available
* Do not use raw Material Symbols elements
* Do not use Font Awesome elements or shims
* Do not simulate old icon systems

---

# Theming & Dark Mode

If theming is implemented:

* Use CSS variables (`:root`, `[data-theme="dark"]`)
* Override Bootstrap variables where appropriate
* Do not duplicate theme tokens unnecessarily

### Design Tokens:
* CSS variables should represent:
  * Colors
  * Spacing (if dynamic)
  * Typography scales (if extended)
* Do not create tokens for:
  * One-off values
  * Component-specific styling

All themes must:

* Maintain accessibility contrast
* Be consistent across components

---

# Accessibility Standards

Must follow WCAG 2.1 AA.

**Requirements:**

* Use semantic HTML
* Ensure keyboard accessibility
* Maintain contrast ratio ≥ 4.5:1
* Provide `aria-label` for icon-only controls

* All interactive elements must:
  * Have visible focus states
  * Be reachable via keyboard (Tab navigation)
* Avoid:
  * Using color alone to convey meaning
  * Non-semantic clickable elements (e.g. divs without roles)

---

# Naming Conventions

Use **kebab-case** for CSS class names:

* `.project-card`
* `.task-list-item`

Use **PascalCase** for SCSS filenames:

* `TaskList.scss`
* `ProjectCard.scss`

Do not use camelCase.