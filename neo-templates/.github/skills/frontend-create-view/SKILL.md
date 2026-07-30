---
name: frontend-create-view
description: Create a new Neo MVVM View and ViewModel pair for a page or dialog
argument-hint: "<EntityName> [domain area e.g. CropCycles/Config]"
---

Use this skill when adding a new page or screen that needs its own route and state. It covers creating both the `.ts` ViewModel and the `.tsx` View in the correct location, wiring up the MVVM lifecycle, and injecting services.

## Steps

### 1. Determine file location

Place files under `src/Domain/Views/{DomainArea}/`:

```
src/Domain/Views/CropCycles/Config/
├── CropCycleConfigView.tsx
├── CropCycleConfigVM.ts
└── Components/         ← child components go here
```

### 2. Create the ViewModel (`{EntityName}VM.ts`)

```ts
import { Views } from '@singularsystems/neo-react';
import { AppService, Types } from '../../Services/AppService';

export default class {EntityName}VM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private queryApiClient = AppService.get(Types.Domain.ApiClients.{EntityName}QueryApiClient),
        private commandApiClient = AppService.get(Types.Domain.ApiClients.{EntityName}CommandApiClient)) {

        super(taskRunner);
        this.makeObservable();
    }

    // Observable state
    public selectedItem: {EntityName}Lookup | null = null;

    // Lifecycle — called by the framework after the view mounts
    public async initialise() {
        const data = await this.taskRunner.waitForData(this.queryApiClient.getItems());
        this.items.set(data);
    }

    // Clean up reactions/subscriptions registered outside autoDispose
    public dispose() {
        super.dispose();
    }
}
```

**ViewModel rules:**
- First constructor param is always `taskRunner = AppService.get(Types.Neo.TaskRunner)`.
- Call `super(taskRunner)` then `this.makeObservable()`.
- Use `this.taskRunner.waitForData()` to load data; use `this.taskRunner.run()` for commands.
- Register child VMs with `this.registerViewModel(ChildVM, { initialise: true })`.
- Use `this.autoDispose()` for reactions so they are cleaned up automatically.

### 3. Create the View (`{EntityName}View.tsx`)

```tsx
import React from 'react';
import { Neo, Views } from '@singularsystems/neo-react';
import { observer } from 'mobx-react';
import {EntityName}VM from './{EntityName}VM';

@observer
export default class {EntityName}View extends Views.ViewBase<{EntityName}VM> {

    constructor(props: unknown) {
        super("{Display Name}", {EntityName}VM, props);
    }

    public render() {
        const vm = this.viewModel;

        return (
            <div>
                <Neo.Card title="{Display Name}">
                    {/* content */}
                </Neo.Card>
            </div>
        );
    }
}
```

**View rules:**
- Class-based component with `@observer` decorator.
- `super("Display Name", VMClass, props)` — first arg is the page title.
- Access state via `this.viewModel` only. Never use React state.
- Do not override React lifecycle methods; use `initialise()` in the VM.
- Default export, one View per file.

### 4. Add URL parameters (optional)

If the view needs to reflect state in the URL (e.g. selected entity, active tab):

```ts
class {EntityName}Params {
    public entityId = { required: true };
    public tab = { isQuery: true };
}

export default class {EntityName}View extends Views.ViewBase<{EntityName}VM, {EntityName}Params> {
    public static params = new {EntityName}Params();

    protected viewParamsUpdated() {
        this.viewModel.loadEntity(this.viewParams.entityId.asNullableInt());
    }
}
```

### 5. Register the route

See [frontend-add-route](../frontend-add-route/SKILL.md).

## Quality checklist

- [ ] No React state in View (`useState`, `setState`) — use VM properties
- [ ] No React lifecycle overrides in View — use VM `initialise()` / `dispose()`
- [ ] `makeObservable()` called in VM constructor
- [ ] `dispose()` calls `super.dispose()` and cleans up manual subscriptions
- [ ] `@observer` decorator present on the View class
- [ ] Both files use default export; one class per file
- [ ] Unit tests cover `initialise()` logic and key commands
