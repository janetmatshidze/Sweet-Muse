---
description: "Use when building UI with Neo React components. Covers Neo.Card, Neo.Button, Neo.Form, Neo.FormGroup, Neo.Modal, NeoGrid, Neo.Pager, Neo.TransitionContainer, Neo.TabContainer, dropdowns, editors, validation, alerts, and all other Neo UI components."
applyTo: "src/**/*.tsx"
---
# Neo UI Components

- Neo components use bootstrap classes for layout and styling.
    - E.g. a Neo.Button will render a button with class "btn btn-primary", and support bootstrap variants and sizes.

## Layout Components

### Neo.Card

Content panel with title and optional icon.

```tsx
<Neo.Card title="Section Title" icon="search"
    headerElements={<Neo.Button icon="add" text="Add" onClick={() => ...} />}
    className="custom-class">
    {/* Card content */}
</Neo.Card>
```

### Neo.GridLayout

Responsive grid. Each direct child becomes a column.

GridLayout should only be used where the columns have equal spacing. For custom spacing, just use divs with bootstrap classes.

```tsx
<Neo.GridLayout md={2} lg={3} xl={4}>
    <div>Item 1</div>
    <div>Item 2</div>
    <div>Item 3</div>
</Neo.GridLayout>

// With shared form group props (aligns labels across inline form groups)
<Neo.GridLayout formGroupProps={{ xs: 4, md: 12, lg: 4 }}>
    <div>
        <Neo.FormGroupInline bind={model.meta.firstName} />
        <Neo.FormGroupInline bind={model.meta.lastName} />
    </div>
</Neo.GridLayout>
```

## Form Components

### Native HTML → Neo component mapping

Never use native `<input>`, `<select>`, or `<textarea>` directly. Always use the Neo equivalent:

| Native element | Neo equivalent |
|---|---|
| `<input type="text">` / `<textarea>` | `<Neo.FormGroup bind={model.meta.field} />` (use `editorProps={{ rows: 3 }}` for multiline) |
| `<input type="number">` | `<Neo.FormGroup bind={model.meta.field} />` (Neo detects numeric type automatically) |
| `<input type="date">` | `<Neo.FormGroup bind={model.meta.field} />` |
| `<input type="datetime-local">` | `<Neo.FormGroup bind={model.meta.field} dateProps={{ formatString: "dd MMM yyyy HH:mm" }} />` |
| `<input type="password">` | `<Neo.FormGroup bind={model.meta.field} input={{ type: "password" }} />` |
| `<input type="checkbox">` for boolean | `<Neo.FormGroup bind={model.meta.field} />` |
| `<input type="checkbox">` toggle/switch | `<Neo.FormGroup bind={model.meta.field} input={{ type: "switch" }} />` |
| Yes/No radio pair | `<Neo.RadioList bind={model.meta.field} radioList={{ items: [{ id: true, text: "Yes" }, { id: false, text: "No" }], inline: true }} />` |
| Radio group from enum | `<Neo.RadioList bind={model.meta.field} radioList={{ enumType: MyEnum }} />` |
| `<select>` with static options | `<Neo.DropDown bind={model.meta.field} select={{ items: [...], displayMember: "...", valueMember: "..." }} />` |
| `<select>` from data source | `<Neo.FormGroup bind={model.meta.field} select={{ itemSource: dataSource, displayMember: "..." }} />` |
| `<input>` with label | `<Neo.FormGroup bind={model.meta.field} />` (handles label, editor, and validation) |
| `<input>` with prepend text | `<Neo.FormGroup bind={model.meta.field} prependText="R" />` |
| `<input>` with append button | `<Neo.FormGroup bind={model.meta.field} append={<Neo.Button .../>} />` |
| Long list `<select>` / search-as-you-type | `<Neo.AutoCompleteDropDown bind={model.meta.field} bindDisplay={model.meta.displayField} itemSource={...} />` |

All of these honour `bind={model.meta.property}` for two-way binding, labels, and inline validation. Never wire up `value`/`onChange` manually.

### Neo.Form

Model-bound form with submit handler and validation.

```tsx
<Neo.Form model={viewModel.model}
    onSubmit={() => viewModel.save()}
    showSummaryModal >
    <Neo.FormGroup bind={viewModel.model.meta.firstName} />
    <Neo.FormGroup bind={viewModel.model.meta.lastName} />
    <Neo.Button isSubmit variant="primary">Save</Neo.Button>
</Neo.Form>
```

`Neo.Form` can display a summary of validation errors by including the `showSummaryModal` prop. This will show a modal with the list of errors when the user tries to submit an invalid form.

#### Validation Display Mode

Control when validation errors appear:

```tsx
<Neo.Form model={model} validationDisplayMode={Validation.DisplayMode.AfterBlur}>

// Options:
// Validation.DisplayMode.AfterBlur   - Show after field loses focus (default)
// Validation.DisplayMode.Always      - Show immediately
// Validation.DisplayMode.AfterSubmit - Show only after first submit attempt
// Validation.DisplayMode.Never       - Never show inline
```

### Neo.FormGroup

Field editor bound to a model property via `bind`. Automatically renders the appropriate editor based on property type and calculates the label text.

```tsx
// Text input (default for string properties)
<Neo.FormGroup bind={model.meta.name} />

// With label override
<Neo.FormGroup bind={model.meta.name} label="Custom Label" />

// Read-only display
<Neo.FormGroup display={model.meta.name} />

// Disabled / Read-only
<Neo.FormGroup bind={model.meta.name} isDisabled={true} />
<Neo.FormGroup bind={model.meta.name} isReadOnly={true} />

// Dropdown select from data source
<Neo.FormGroup bind={model.meta.cropTypeId}
    select={{ itemSource: dataCache.cropTypes, displayMember: "cropTypeName" }} />

// Dropdown select from enum
<Neo.FormGroup bind={model.meta.triggerTypeId}
    select={{ items: EnumHelper.asList(TriggerType), displayMember: "typeName" }} />

// Radio list
<Neo.FormGroup bind={model.meta.isActive}
    radioList={{ items: [{ text: "Active", value: true }, { text: "Inactive", value: false }], inline: true, valueMember: "value" }} />

// Multi-line text editor
<Neo.FormGroup bind={model.meta.description} editorProps={{ rows: 3 }} />

// With prepend/append
<Neo.FormGroup bind={model.meta.email} prependText="fa-at" />
```

### Neo.FormGroupInline

Inline layout variant (label and editor side by side).

```tsx
<Neo.FormGroupInline xs={4} md={12} lg={4} bind={model.meta.fieldName} />
```

### Neo.FormGroupFloating

Floating label (label as placeholder, floats on focus).

```tsx
<Neo.FormGroupFloating bind={model.meta.name} />
```

### Neo.ValidationSummary

Shows validation errors for a model. Only use if there is a specific reason to show the model errors, otherwise leave the form to handle validation display.

```tsx
<Neo.ValidationSummary model={model} />
```

## Editors & Input Types

Neo determines the editor type from the bound property's data type. Override with the `input` prop.

```tsx
// Text input (default for string)
<Neo.Input bind={model.meta.name} placeholder="Enter name..." />

// Password
<Neo.Input bind={model.meta.password} input={{ type: "password" }} />

// Multiline text
<Neo.Input bind={model.meta.description} input={{ rows: 3 }} />

// Checkbox (automatic for boolean)
<Neo.FormGroup bind={model.meta.isActive} />

// Switch toggle
<Neo.FormGroup bind={model.meta.isActive} input={{ type: "switch" }} />

// Tri-state checkbox (requires @Attributes.NullableBoolean() on property)
<Neo.FormGroup bind={model.meta.approvalStatus} />

// Append / Prepend icons and components
<Neo.Input bind={model.meta.search} prependText="fa-search"
    append={<Neo.Button variant="secondary" isOutline onClick={onClear}>Clear</Neo.Button>} />

// Date picker (date only)
<Neo.DatePicker bind={model.meta.startDate} dateProps={{ formatString: "dd MMM yyyy" }} />

// Date + time picker
<Neo.DatePicker bind={model.meta.startDate} dateProps={{ formatString: "dd MMM yyyy HH:mm" }} />

// Time only
<Neo.DatePicker bind={model.meta.startTime} dateProps={{ formatString: "HH:mm" }} />
```

- These editors are rarely used on their own, they are rendered as children of `Neo.FormGroup` or `NeoGrid.Column` which handle labels, validation, and layout.

## Dropdown Selects

### Basic Dropdown (Neo.FormGroup with `select`)

For short, static lists.

```tsx
// Simple items array (first numeric prop = value, first string prop = display)
<Neo.FormGroup bind={model.meta.statusId} select={{ items: statusItems }} />

// With explicit members
<Neo.FormGroup bind={model.meta.countryId}
    select={{ items: countries, displayMember: "name", valueMember: "countryId" }} />

// From enum
<Neo.DropDown bind={model.meta.priority} select={{ items: EnumHelper.asList(Priority) }} />

// Allow null selection
<Neo.DropDown bind={model.meta.statusId} select={{ items: statusItems, allowNulls: true }} />

// Async data source (shows loading spinner until loaded)
// In VM: countryDataSource = new Data.ApiClientDataSource(apiClient.getCountries);
<Neo.FormGroup bind={model.meta.countryId} select={{ itemSource: viewModel.countryDataSource }} />
```

### Auto-Complete Dropdown (Neo.AutoCompleteDropDown)

For long lists, async search, and multi-select.

```tsx
// Async search
<Neo.AutoCompleteDropDown
    itemSource={viewModel.apiClient.searchCountries}
    bind={model.meta.countryId}
    bindDisplay={model.meta.countryName} />

// With default items that show before searching
<Neo.AutoCompleteDropDown
    itemSource={viewModel.apiClient.searchCountries}
    bind={model.meta.countryId}
    bindDisplay={model.meta.countryName}
    items={viewModel.popularCountries} />

// Multi-select
<Neo.AutoCompleteDropDown
    itemSource={viewModel.apiClient.searchCountries}
    bindItems={model.meta.selectedItems}
    bindIds={model.selectedIds} />

// Wrap in FormGroup for label + validation
<Neo.FormGroup>
    <Neo.AutoCompleteDropDown
        itemSource={viewModel.apiClient.searchCountries}
        bind={model.meta.countryId}
        bindDisplay={model.meta.countryName} />
</Neo.FormGroup>
```

**Model pattern for async dropdowns:**

```ts
@Rules.Required()
selectedCountryId: number | null = null;

// Display name is client-only, decorated with NoTracking so it's not serialised
@Attributes.NoTracking(Misc.SerialiseType.FullOnly)
selectedCountryName: string | null = null;
```

### Radio List

```tsx
// From enum
<Neo.RadioList bind={model.meta.priority} radioList={{ enumType: Priority }} />

// Inline (horizontal)
<Neo.FormGroup bind={model.meta.priority} radioList={{ enumType: Priority, inline: true }} />

// Boolean radio list
<Neo.RadioList bind={model.meta.isSell}
    radioList={{ items: [{ id: false, text: "Buy" }, { id: true, text: "Sell" }], inline: true }} />
```

## Data Grid (NeoGrid)

Display tabular data with sorting, buttons, and custom rendering.

```tsx
import { NeoGrid } from '@singularsystems/neo-react';

<NeoGrid.Grid items={viewModel.items}>
    {(item, meta) => (
        <NeoGrid.Row>
            <NeoGrid.Column display={meta.name} />
            <NeoGrid.Column display={meta.startDate} dateProps={{ formatString: "dd MMM yyyy" }} />
            <NeoGrid.Column display={meta.status} label="Status" />
            <NeoGrid.Column label="Custom">
                {item.isActive ? "Active" : "Inactive"}
            </NeoGrid.Column>
            <NeoGrid.Column display={meta.someField} hideBelow="xl" width={120} />
            <NeoGrid.ButtonColumn>
                <Neo.Button size="sm" icon="edit" isOutline onClick={() => editItem(item)} />
                <Neo.Button size="sm" icon="delete" isOutline variant="danger" onClick={() => deleteItem(item)} />
            </NeoGrid.ButtonColumn>
        </NeoGrid.Row>
    )}
</NeoGrid.Grid>
```

### Editable Data Grid

Use `bind` instead of `display` for editable columns.

```tsx
<NeoGrid.Grid items={viewModel.items}>
    {(item, meta) => (
        <NeoGrid.Row>
            <NeoGrid.Column bind={meta.name} />
            <NeoGrid.Column bind={meta.quantity} />
            <NeoGrid.Column bind={meta.statusId}
                select={{ items: viewModel.statuses }} />
        </NeoGrid.Row>
    )}
</NeoGrid.Grid>
```

### Grid with Pager

```tsx
<Neo.Pager pageManager={viewModel.pageManager} pageControls="top">
    <NeoGrid.Grid items={viewModel.pageManager}>
        {(item, meta) => (
            <NeoGrid.Row>
                <NeoGrid.Column display={meta.name} sort />
                <NeoGrid.Column display={meta.status} sort />
            </NeoGrid.Row>
        )}
    </NeoGrid.Grid>
</Neo.Pager>
```

## Buttons

```tsx
// Standard button
<Neo.Button text="Save" icon="save" onClick={() => viewModel.save()} />

// Outline variant
<Neo.Button text="Edit" icon="edit" isOutline onClick={() => ...} />

// Small size
<Neo.Button size="sm" icon="delete" isOutline variant="danger" onClick={() => ...} />

// Submit button (inside Neo.Form)
<Neo.Button icon="search" text="Search" isSubmit />

// With tooltip
<Neo.Button icon="info" tooltip="More information" onClick={() => ...} />

// Variant (danger, etc.)
<Neo.Button text="Delete" variant="danger" onClick={() => ...} />

// Pulse animation
<Neo.Button text="Action" pulse={model.isDirty} onClick={() => ...} />

// Dropdown menu button
<Neo.Button text="Actions"
    menuItems={[
        { text: "Edit", data: "edit" },
        { text: "Delete", data: "delete" }
    ]}
    onClick={(e, data) => viewModel.handleAction(data)} />
```

## Modals

### ModalUtils (Convenience Methods)

```ts
// Simple message
ModalUtils.showMessage("Title", "Your changes have been saved.");

// Yes/No confirmation
const confirmed = await ModalUtils.showYesNo("Confirm", "Are you sure you want to delete?");

// Yes/No/Cancel
const result = await ModalUtils.showYesNoCancel("Save", "Save changes before leaving?");
```

**Note:** The body parameter can accept a component instead of a string.

### Neo.Modal (Custom Modal)

#### Bound to observable property

```tsx
<Neo.Modal title="Edit Item"
    bindModel={viewModel.meta.selectedItem}
    acceptButton={{ text: "Save", onClick: () => viewModel.save() }}
    closeButton={false}>
    {(item: SomeModel) => (
        <div>
            <Neo.FormGroup bind={item.meta.name} />
            <Neo.FormGroup bind={item.meta.description} />
        </div>
    )}
</Neo.Modal>
```

#### Bound to boolean property.

```tsx
<Neo.Modal title="Edit Entity"
    bind={viewModel.meta.showEditModal}
    acceptButton={{ text: "Save", onClick: () => viewModel.save() }}>
    <Neo.FormGroup bind={viewModel.meta.name} />
</Neo.Modal>
```

### Key Props

- `bindModel` or `bind` — observable that controls open/close (non-null = open)
- `show` — boolean to control visibility
- `title` — modal header
- `size` — `"sm"`, `"lg"`, `"xl"`
- `acceptButton` — `{ text, onClick }` for primary action
- `closeButton` — `false` to hide, or `{ text }` to customise

### Rules:

- Avoid adding additional properties to the view model to control modal visibility. Rather rely on a property being null, or not. E.g. bindModel={this.viewModel.meta.editingEntity}

## Transition Panels

Animated panel switching — shows one panel at a time.

```tsx
<Neo.TransitionContainer>
    <Neo.TransitionPanel isVisible={!viewModel.selectedItem}>
        {/* List view */}
    </Neo.TransitionPanel>
    <Neo.TransitionPanel isVisible={!!viewModel.selectedItem}>
        {selectedItem => (
            /* Detail view - selectedItem is non-null here */
            <div>
                <Neo.Button onClick={() => viewModel.selectedItem = null} icon="arrow-left">Back</Neo.Button>
                <Neo.FormGroup display={selectedItem.meta.name} />
            </div>
        )}
    </Neo.TransitionPanel>
</Neo.TransitionContainer>
```

## Tabs

```tsx
<Neo.TabContainer>
    <Neo.Tab header="General">
        {/* Tab 1 content */}
    </Neo.Tab>
    <Neo.Tab header="Settings">
        {/* Tab 2 content */}
    </Neo.Tab>
</Neo.TabContainer>
```

With icons and lazy loading:

```tsx
<Neo.Tabs>
    <Neo.Tab header="Details" icon="info">
        <Neo.FormGroup bind={model.meta.name} />
    </Neo.Tab>
    <Neo.Tab header="History" icon="history">
        <NeoGrid.Grid items={viewModel.historyItems}>
            {/* grid content */}
        </NeoGrid.Grid>
    </Neo.Tab>
    <Neo.Tab header="Settings" icon="settings" onInitialise={() => viewModel.loadSettings()}>
        {/* lazily loaded content */}
    </Neo.Tab>
</Neo.Tabs>
```

## Alerts & Notifications

```tsx
// Toast notifications via global notifications service
viewModel.notifications.addSuccess("Record saved successfully.");
viewModel.notifications.addDanger("Failed to save record.");
viewModel.notifications.addWarning("Some fields were not filled in.");
viewModel.notifications.addInfo("Data has been refreshed.");

// Inline alert in JSX
<Neo.Alert variant="info">This record is read-only because it has been completed.</Neo.Alert>
```

## Loading & TaskRunner

```tsx
// Show loading bar bound to the view's task runner
<Neo.Loader task={viewModel.taskRunner} />
```

## Global Components (in Layout.tsx)

These are mounted once in the app layout and should NOT be added to individual views:

```tsx
<Neo.ModalContainer />
<Neo.ToastContainer notificationStore={NotifyUtils.store} />
<Neo.TooltipProvider />
<Neo.ContextMenuContainer />
<Neo.Loader task={routing.taskRunner} />
```

## Other Components

| Component    | Purpose |
| ---          | ---     |
| `Neo.Loader` | Loading indicator bound to a `task` (TaskRunner) |
| `Neo.Alert`  | Styled alert with `variant` prop |
| `Neo.Icon`   | Icon with `name` and optional `solid` prop |
| `Neo.Link`   | Router-aware link |

## Form Binding Pattern

All form binding goes through the model's `.meta` property:

```tsx
bind={model.meta.propertyName}
```

The `meta` object is auto-generated by Neo's observability system and provides the binding context that `Neo.FormGroup` uses to determine editor type, validation state, and labels.
