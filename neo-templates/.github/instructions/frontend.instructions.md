---
applyTo: "src/**/*.ts, src/**/*.tsx"
---

# Project WebApp

## Tech Stack

- **React 19** with TypeScript 5.9
- **Neo React Framework** (@singularsystems/neo-*) for UI components and patterns
- **Axios** for HTTP requests
- **MobX** for state management
- **Inversify** for dependency injection
- **Yarn 4** (with Plug'n'Play enabled)
- **CRACO** for build configuration
- **SASS** for styling with **Bootstrap 5.3**
- **Highcharts** for data visualization
- **TSLint** for code linting

## Architecture

The app uses an MVVM pattern built on the Neo React framework. See `frontend-views.instructions.md` for View/ViewModel conventions and `frontend-di-modules.instructions.md` for dependency injection patterns.
