---
description: "Use when creating views or view models. Covers project-specific file organisation conventions for views."
applyTo: ["src/**/Views/**/*.tsx", "src/**/Views/**/*VM.ts"]
---
# Project View Conventions

## Related Skills

- [frontend-create-view](../../skills/frontend-create-view/SKILL.md) — Create a new View + ViewModel

## File Organisation

Views are grouped by domain area, with child components in a `Components/` subfolder.

```
src/Domain/Views/
└── {DomainArea}/
    └── {SubArea}/
        ├── {Entity}View.tsx         → Main view
        ├── {Entity}VM.ts            → Main view model
        └── Components/              → Child components
            ├── {Name}Component.tsx
            └── {Name}ComponentVM.ts
```

Match the folder depth and naming already used in this project — check existing views for the established pattern.

## Naming Conventions

See `frontend-views.instructions.md` for View and ViewModel naming conventions.

- Component Views: `{Name}Component.tsx`
- Component ViewModels: `{Name}ComponentVM.ts`
