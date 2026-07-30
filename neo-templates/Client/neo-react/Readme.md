# Neo-react yeoman generator

The neo-react template creates a react app as an enhanced create-react-app project. It includes the neo dependencies, and optionally adds demo pages for reference.

The 2 main types of projects which can be created are:

* Basic
* Modular app 

## Command line

To generate a project without any user input, you can specify the project name after the generator name:

```
yo neo-react Tuckshop
```

*This will generate a modular app with the default module (Domain).*

To specify custom modules, add additional arguments after the name:

```
yo neo-react Tuckshop Customers Orders Payments
```

*This will create 3 modules. Customers Orders, and Payments*

By default, neo services (Reporting, Notifications) **will** be included. To change this, include `-n` in the command:

```
yo neo-react Tuckshop -n
```

## Template development

The code which is called by yeoman to generate the project files is in `.\generators\app\index.js`. 

The template files are in `.\generators\app\templates`. These files are copied into different locations in the generated project, depending on whether the *basic* or *modular app* project type is selected.

Because files may be in different relative locations in the final project, the import paths in the files need to be dynamic. Use template variables for these paths. E.g. `AppService.get(<%= typesPath %>.Config)`.

## Variable names

The following variable names are available:

### Common:
* `projectName`: Name the user types in when creating the project.
* `npmName`: Project name with spaces replaced with dashes.
* `codeName`: Project name with spaces removed.
* `examples`: Boolean indicating whether demo pages must be included.
* `hasModules`: Boolean indicating whether this is a modular app project.

### Path names:
See code.

## Testing

To test your template, run the following command in the project root folder (where this readme file is):

```
npm link
```

Then create a folder somewhere, and run `yo neo-react` as normal.

*Hint:* If you need to make a change and re-generate, remove everything except the *node_modules* folder.