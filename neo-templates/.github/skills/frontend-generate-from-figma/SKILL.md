---
name: frontend-generate-from-figma
description: Implement a front-end Figma design as a Neo MVVM View/ViewModel pair
argument-hint: "<FigmaUrl> [AdditionalStateUrls...] <EntityName> <DomainArea>"
---

Use this skill when implementing a page or screen from a Figma design. It drives the Figma MCP server to extract design context, reconciles multiple state frames into a single Neo MVVM View/ViewModel pair, and applies all project coding, styling, and component standards.

**Two modes** — choose one before starting:

| Mode | When to use |
|---|---|
| **Single view** | One screen, 1–N Figma frames each representing a different UI state (empty, loaded, error, modal open, etc.) |
| **Multi-view batch** | Multiple distinct screens — run the single-view flow once per screen |

---

## Placeholders

| Placeholder | Meaning | Example |
|---|---|---|
| `{EntityName}` | PascalCase name of the entity or page | `ApprovalRequest`, `UserAdmin` |
| `{DomainArea}` | Folder path under `src/Domain/Views/` | `Approvals`, `Admin/Users` |
| `{DisplayName}` | Human-readable page title | `Approval Requests`, `User Administration` |

---

### How to collect it

1. Firstly, ask the developer for `EntityName` and `DomainArea`.
2. Secondly, ask the developer for additional context using the wording below:
> "Is there any additional context I should use? For example: existing models or API clients to wire up, business rules, field constraints, navigation behaviour, or anything the design does not capture."
3. Lastly, after receiving a reply (or a skip), ask for the Figma URLs.


### How to apply it

| Context type | Where it applies |
|---|---|
| Existing model or API client | Step 5 (ViewModel) — inject the client; use the model's properties for binding instead of ad-hoc strings |
| Field constraints or validation rules | Step 5 (ViewModel) — add the constraint to the model or VM property |
| Business rules (e.g. conditional visibility logic) | Step 3 (state analysis) + Step 5 (ViewModel) — add the rule as a computed property |
| Navigation behaviour (e.g. redirect after save) | Step 5 (ViewModel) — implement in the relevant action method |
| Specific component to use | Step 6 (View) — use that component instead of what Figma reference code suggests |
| Naming preferences | Apply throughout all generated files |
| Anything the design does not capture | Add it where it logically belongs; note it in a `// TODO:` comment if uncertain |

> **Rule:** Never discard developer context in favour of what the design shows. If they conflict, follow the developer context and note the discrepancy with a `// Note:` comment in the generated code.

---

## Single-view workflow

### Step 1 — Parse Figma URLs

Extract `fileKey` and `nodeId` from each Figma URL provided:

```
https://www.figma.com/design/:fileKey/:fileName?node-id=:nodeId
```

Convert `-` to `:` in `nodeId` (e.g. `123-456` → `123:456`).

Each URL represents one **state** of the same view (e.g. default, loading, populated, empty, error, modal open). Collect them all before proceeding.

---

### Step 2 — Fetch design context for every state frame

Call `get_design_context` for each state frame in parallel, passing `fileKey` and `nodeId`.

> The MCP tool returns reference code (React + Tailwind), a screenshot, and contextual hints (Code Connect mappings, design tokens, annotations).

Honor hints using the priority order defined in the Figma MCP instructions. When mapping design tokens, use the project's Bootstrap variables or SCSS tokens as the equivalent — not raw CSS variable names from the MCP output.

---

### Step 3 — Analyse states and plan VM properties

Compare all state frames side by side. For each visual difference, decide:

| Difference observed | Implementation |
|---|---|
| Content visible vs hidden | Boolean VM property (e.g. `isLoading`, `hasResults`) |
| Different data shown | Observable collection or model property on the VM |
| Modal or panel open/closed | Boolean VM property (e.g. `isDetailPanelOpen`) |
| Selected item highlighted | `selectedItem` property on the VM |
| Error message shown | String or null VM property |
| Tab or step change | Numeric/enum VM property |

> **Rule:** Anything that changes between states becomes a VM property. Anything static (labels, layout structure, icons) stays in the View.

---

### Step 4 — Translate Figma reference code to Neo MVVM

The MCP output uses React functional components with Tailwind. Translate it to this project's stack using the mapping table below.

#### Component & pattern mapping

| Figma MCP output | This project's equivalent |
|---|---|
| Functional component (`const X = () => ...`) | Class-based `@observer` — top-level views extend `Views.ViewBase<VM>`; child components extend `React.Component<IProps>` |
| `useState(...)` | Observable property on the ViewModel |
| `useEffect(...)` | `initialise()` method on the ViewModel |
| Tailwind classes (`text-blue-500`, `p-4`) | Bootstrap utilities (`.text-primary`, `.p-3`) or Neo component props |
| `<button>` / `<input>` | `<Neo.Button>` / `<Neo.FormGroup bind={...}>` |
| Generic container `<div>` with card-like styling | `<Neo.Card title="...">` |
| `<form>` | `<Neo.Form model={...} onSubmit={...}>` |
| Grid/flex container with equal columns | `<Neo.GridLayout md={2}>` |
| Data table | `<NeoGrid.Grid items={...}>` |
| Loading spinner / skeleton | Handled automatically by `taskRunner` — no manual spinner needed |
| Code Connect component hint | Use the mapped codebase component as-is |

#### Naming rules

- Derive semantic names from function, not from Figma layer names. Ignore names like `Frame 120` or `Group 4`.
- A frame containing a list of approval items → `ApprovalItemList`.
- A frame containing a form → `{EntityName}Form`.

---

### Step 5 — Create the ViewModel

Follow [frontend-create-view-skill](../frontend-create-view/SKILL.md) Step 2.

After creating the base ViewModel, add one observable property for each state variation identified in Step 3. Do not add properties that are not backed by a state difference observed across the Figma frames.

---

### Step 6 — Create the View

Follow [frontend-create-view-skill](../frontend-create-view/SKILL.md) Step 3.

#### Component breakdown rules

- Extract every distinct visual section of the design into its own component under `Components/{SectionName}.tsx` by default.
- Only keep markup inline in the top-level View if the section is trivially simple (a single line or a static label with no logic, conditional rendering is allowed if the aforementioned requirements are still met).
- Co-locate extracted components under `src/Domain/Views/{DomainArea}/Components/`.
- Unless the view is trivially simple or contains trivially simple sections, the top-level View must serve as a container only — it delegates rendering to child components via `viewModel` props.
- For components with 1–2 props, pass individual values/callbacks directly.
- For components that need 3 or more things from the same VM, define a typed interface (`IComponentVM`) that declares exactly the slice needed, then pass `vm` as that interface. The child stays decoupled from the concrete VM class; the parent avoids prop drilling.

```tsx
// Child declares its interface contract
interface IQuestionnairePanel {
    visibleQuestions: QuestionLookup[];
    canGoNext: boolean;
    getAnswer(id: number): QuestionAnswer;
    goToNextStep(): void;
    cancelRequest(): void;
}

// Parent passes the whole VM — QuestionnaireVM satisfies the interface
<QuestionnairePanel viewModel={vm} />
```

- If the child has its own state or async logic, extract a dedicated child ViewModel owned by the parent VM instead.

> **Tailwind rule:** The MCP output contains Tailwind classes — replace every one with a Bootstrap utility or `Neo.*` component prop. No Tailwind class should appear in the committed output.

#### Form input rules

Never use native HTML `<input>`, `<select>`, or `<textarea>` elements. Always use the Neo equivalents by following [frontend-components.instructions](../../instructions/frontend-components.instructions.md).

#### Styling rules

> **No inline styles.** The `style={{...}}` prop is **forbidden** except when the value is truly runtime-dynamic (e.g. a width calculated in JavaScript at render time). Static design values — colours, font sizes, spacing, borders — must always go into SCSS classes.

When the design introduces styles that cannot be expressed with Bootstrap utilities:
1. Create `src/Domain/Styles/Domain.scss` (or the equivalent module SCSS file) if it does not already exist.
2. Add a top-level kebab-case class to the component root element (e.g. `.questionnaire-view`, `.approval-routing-strip`).
3. Keep nested selectors kebab-case as well (e.g. `& .questionnaire-view-header`, `& .questionnaire-view-section`) and avoid underscore-based naming patterns.
4. Import the SCSS file into `App.scss` (or the module index SCSS), **not** into the component file.
5. Reference the classes via `className` in JSX — never via `style={{}}`.

When mapping Figma design tokens to CSS:
- Prefer Bootstrap utility classes (`.bg-white`, `.border-bottom`, `fs-5`, etc.) over custom SCSS wherever a Bootstrap equivalent exists.
- Only write custom SCSS for values that Bootstrap cannot represent.

---

### Step 7 — Register the route

Follow [frontend-add-route-skill](../frontend-add-route/SKILL.md).

---

## Quality checklist

### MVVM
Apply the quality checklist from [frontend-create-view-skill](../frontend-create-view/SKILL.md).

### Figma fidelity
- [ ] All visual states identified in Step 3 are covered by VM properties
- [ ] Screenshots reviewed to confirm layout intent is captured
- [ ] Code Connect hints (if any) used directly — no re-implementation

### Figma translation
- [ ] No Tailwind classes in output
- [ ] No inline `style={{...}}` props — all static styles in SCSS classes imported via `App.scss`
- [ ] CSS/SCSS class names use kebab-case only (no underscore-based class names)
- [ ] Semantic names used throughout (no `Frame120`, `Group4`, etc.)
- [ ] Route registered per [frontend-add-route-skill](../frontend-add-route/SKILL.md)
- [ ] No `.toArray()` calls on `List` properties — `List<T>` extends `Array<T>` and is already iterable directly (see `frontend-models.instructions.md`)
- [ ] All import paths verified against the actual folder depth of the generated file — count the `../` segments and confirm they resolve to the correct location

### Developer context
- [ ] Developer was asked for additional context before generation started
- [ ] All supplied context has been applied (models wired, rules implemented, naming followed)
- [ ] Any conflict between context and design is noted with a `// Note:` comment in the generated code