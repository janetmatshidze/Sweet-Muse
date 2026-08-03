var Generator = require('yeoman-generator');
var mkdirp = require('mkdirp');
var chalk = require('chalk');

module.exports = class extends Generator {

  modules = [];
  serverScope = " {Add Service Scopes here}";
  includeNeoServices = false;
  isScripted = false;

  constructor(args, opts) {
    super(args, opts, { customInstallTask: true });

    if (args.length > 0) {
      this.isScripted = true;

      this.answers = {
        name: args[0],
      }
      
      this.includeNeoServices = !opts.n;

      if (args.length > 1) {
        // Arguments after the first will be custom module names.
        this.modules.push(...args.slice(1).map(name => new Module(name)));
      } else {
        // Default domain service
        this.modules.push(new Module("Domain"));
        this.serverScope = ` ${args[0]}.Domain`;
      }
    }
  }

  async prompting() {

    this.log(chalk.blue(" ▄███") + chalk.cyan("████") + " ▄████▄  ▄███▄");
    this.log(chalk.blue(" ██    ") + chalk.cyan("██") + " ██     ██   ██");
    this.log(chalk.blue(" ██    ") + chalk.cyan("██") + " ████   ██   ██");
    this.log(chalk.blue(" ██    ") + chalk.cyan("██") + " ██     ██   ██");
    this.log(chalk.blue(" █▀    ") + chalk.cyan("█▀") + " ▀████▀  ▀███▀   React project generator");
    this.log("▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀")

    if (!this.answers) {
      this.answers = await this.prompt([
        {
          type: "input",
          name: "name",
          message: "What is the name of your project?",
          default: this.appname // Default to current folder name
        },
        {
          type: "list",
          name: "appType",
          message: "What type of app are you building?",
          choices: [ { value: "modular", name: "Modular app" }, { value: "basic", name: "Basic" }]
        }
      ]);
  
      if (this.answers.appType === "modular") {
        const modularAnswers = (await this.prompt([
          {
            type: "list",
            name: "neoServices",
            message: "Do you want to include neo services (Identity, Notifications, Reporting)?",
            choices: [ { value: "Yes", name: "Yes, I want them" }, { value: "No", name: "No thanks"}]
          },
          {
            type: "list",
            name: "modularType",
            message: "Do you want to specify module names?",
            choices: [ { value: "default", name: "No, use defaults" }, { value: "specify", name: "Yes"}]
          }]));
  
        this.includeNeoServices = modularAnswers.neoServices === "Yes";
  
        if (modularAnswers.modularType === "specify") {
          let moduleName = "-";
  
          while (this.answers.appType === "modular" && moduleName) {
            moduleName = (await this.prompt([{
              type: "input",
              name: "module",
              message: "Name of module (leave blank to stop adding modules)"
            }])).module;
      
            if (moduleName) {
              this.modules.push(new Module(moduleName));
            }
          }
        } else {
  
          const serverName = (await this.prompt([{
            type: "input",
            name: "serverName",
            message: "What was the name given to your server project?",
          }])).serverName;
  
          this.serverScope = ` ${serverName}.Domain`;
  
          this.modules.push(new Module("Domain"));
        }
      }
    }

    this.codeName = this.answers.name.replace(/ /g, "");

    this.templateVars = {
      projectName: this.answers.name,
      npmName: this.answers.name.toLowerCase().replace(/ /g, "-") + "-react",
      codeName: this.codeName,
      serverScope: this.serverScope,
      hasModules: this.modules.length > 0,
      includeNeoServices: this.includeNeoServices,
      srcRelativeToIndex: this.modules.length === 0 ? "." : "./App",
      moduleName: "App",
      moduleTypeImports: "",
      moduleTypeExports: "",
      moduleSetupImports: "",
      moduleRegistration: "",
      moduleRouteImports: "",
      modulePureRoutes: "",
      moduleMenuRoutes: "",
      appIndexImports: "",
      appIndexExports: "",
      typesPath: this.modules.length === 0 ? "Types" : "Types.App"
    };

    this._createModuleText();
  }

  writing() {
    this.log("app name", this.answers.name);

    let srcFolder = this.modules.length === 0 ? "src" : "src/App";

    this._copyDirectory(".");
    this._copyDirectory("_vscode", ".vscode");
    this._copyDirectory("public");
    this._copyDirectory("deploy_config");
    
    this._copyDirectory("SrcRoot", "src");
    this._copyDirectory("src", srcFolder);

    this._copyFile("Other/gitignore", ".gitignore");
    this._copyFile("Other/NeoTemplate.code-workspace", this.codeName + ".code-workspace");

    if (this.modules.length === 0) {
      this._copyFile("Conditional/AuthorisationService.ts", srcFolder + "/Services/AuthorisationService.ts");
      this._copyFile("Conditional/DataCache.ts", srcFolder + "/Services/AppDataCache.ts");
    } 

    if (this.templateVars.includeNeoServices) {
      this._copyDirectoryRaw("Reporting/**", "src/Reporting");
      this._copyDirectoryRaw("Identity/**", "src/Identity");
    }
     
    this._copyDirectoryRaw("src/assets/img", srcFolder + "/assets/img");
    this._copyDirectory("src/Components", srcFolder + "/Components");

    //If models are added to the src/Models directory, then change this.
    this._copyDirectory("src/Models", srcFolder + "/Models");
    this._copyDirectory("src/Models/Security", srcFolder + "/Models/Security");

    this._copyDirectory("src/Services", srcFolder + "/Services");
    this._copyDirectory("src/Styles", srcFolder + "/Styles");
    this._copyDirectory("src/Styles/Components", srcFolder + "/Styles/Components");
    this._copyDirectory("src/Views", srcFolder + "/Views");
    this._copyDirectory("src/Views/Security", srcFolder + "/Views/Security");

    this._createModules();
  }

  _createModuleText() {
    var sortedModules = this.modules.sort((a, b) => (a.codeName < b.codeName) ? -1 : (a.codeName > b.codeName ? 1 : 0));

    for (let module of sortedModules) {
      if (module) {

        this.templateVars.moduleTypeImports += `import { ${module.codeName}ExportedTypes } from '../${module.codeName}/${module.codeName}ExportedTypes';\n`;
        this.templateVars.moduleTypeExports += `\t${module.codeName}: ${module.codeName}ExportedTypes,\n`;

        this.templateVars.moduleSetupImports += `import { ${module.codeName}AppModule } from '../${module.codeName}/${module.codeName}Module';\n`;
        this.templateVars.moduleRegistration += `appService.registerModule(${module.codeName}AppModule);\n`;

        this.templateVars.moduleRouteImports += `import * as ${module.codeName}Routes from '../../${module.codeName}/${module.codeName}Routes';\n`;
        this.templateVars.modulePureRoutes += `\t\t\t...${module.codeName}Routes.PureRoutes,\n`;
        this.templateVars.moduleMenuRoutes += `\t\t\t...${module.codeName}Routes.MenuRoutes,\n`;
      }
    }

    function trimEnd(str) {
      return str.substring(0, str.length - 1) 
    }

    if (this.modules.length > 0) {
      // Remove last newline.
      this.templateVars.moduleTypeExports = trimEnd(this.templateVars.moduleTypeExports);
      this.templateVars.moduleRegistration = '\n' + trimEnd(this.templateVars.moduleRegistration);
      this.templateVars.appIndexExports = trimEnd(this.templateVars.appIndexExports);
    }
  }

  _createModules() {

    for (let module of this.modules) {
      if (module.codeName === "Domain") {
        this._createDomainModule(module);
      } else {
        this._createModule(module);
      }
    }
  }

  _createModule(module) {

    if (module) {
      let moduleRoot = "src/" + module.codeName;
      this.templateVars.moduleName = module.codeName;
            
      this._copyFile("ModuleRoot/index.ts", moduleRoot + "/index.ts");
      this._copyFile("ModuleRoot/Module.ts", moduleRoot + "/" + module.codeName + "Module.ts");
      this._copyFile("ModuleRoot/Routes.ts", moduleRoot + "/" + module.codeName + "Routes.ts");
      this._copyFile("ModuleRoot/Types.ts", moduleRoot + "/" + module.codeName + "Types.ts");
      this._copyFile("ModuleRoot/ExportedTypes.ts", moduleRoot + "/" + module.codeName + "ExportedTypes.ts");
    
      mkdirp.sync(this.destinationPath(moduleRoot));
      mkdirp.sync(this.destinationPath(moduleRoot + "/ApiClients"));
      mkdirp.sync(this.destinationPath(moduleRoot + "/Models"));
      mkdirp.sync(this.destinationPath(moduleRoot + "/Services"));
      mkdirp.sync(this.destinationPath(moduleRoot + "/Views"));

      this._copyFile("Conditional/DataCache.ts", moduleRoot + "/Services/" + module.codeName + "DataCache.ts");
    }
  }

  _createDomainModule(module) {
      let moduleRoot = "src/Domain";
      this.templateVars.moduleName = module.codeName;
            
      this._copyFile("ModuleRoot/index.ts", moduleRoot + "/index.ts");
      this._copyFile("Domain/DomainModule.ts", moduleRoot + "/" + module.codeName + "Module.ts");
      this._copyFile("Domain/DomainRoutes.ts", moduleRoot + "/" + module.codeName + "Routes.ts");
      this._copyFile("Domain/DomainTypes.ts", moduleRoot + "/" + module.codeName + "Types.ts");
      this._copyFile("ModuleRoot/ExportedTypes.ts", moduleRoot + "/" + module.codeName + "ExportedTypes.ts");

      this._copyFile("Domain/ApiClients/CatalogueApiClient.ts", moduleRoot + "/ApiClients/CatalogueApiClient.ts");
      this._copyFile("Domain/Models/Security/CatalogueRoles.ts", moduleRoot + "/Models/Security/CatalogueRoles.ts");
      this._copyFile("Domain/Services/CatalogueEditService.ts", moduleRoot + "/Services/CatalogueEditService.ts");
      this._copyFile("Domain/Views/Catalogue/CatalogueEntry.ts", moduleRoot + "/Views/Catalogue/CatalogueEntry.ts");
      this._copyFile("Domain/Views/Catalogue/CatalogueRoutes.ts", moduleRoot + "/Views/Catalogue/CatalogueRoutes.ts");
      this._copyFile("Domain/Views/Catalogue/CatalogueView.tsx", moduleRoot + "/Views/Catalogue/CatalogueView.tsx");

      this._copyFile("Conditional/DataCache.ts", moduleRoot + "/Services/" + module.codeName + "DataCache.ts");
  }

  install() {

  }

  end() {

    this.log(chalk.green("----------------"));
    this.log(chalk.green("Congratulations!"));
    this.log(this.answers.name + " is almost ready.");

    if (!this.isScripted) {
      this.log("vs-code should open automatically. If it doesn't, double click on the " + chalk.cyan(this.codeName + ".code-workspace") + " file and follow the instructions in " + chalk.cyan("readMe.md") + ".");
      this.log("----------------");
  
      this.spawnCommand("code", [this.codeName + ".code-workspace", "-g README.md"]);
    }
  }

  _copyFile(fileName, destinationName) {
    this.fs.copyTpl(
      this.templatePath(fileName),
      this.destinationPath(destinationName),
      this.templateVars, 
      undefined, 
      { globOptions: { dot: true } });
  }

  _copyDirectory(folderName, destinationName) {
    return this.fs.copyTpl(
      this.templatePath(folderName + '/*'),
      this.destinationPath(destinationName || folderName),
      this.templateVars, 
      undefined, 
      { globOptions: { dot: true } });
  }

  _copyDirectoryRaw(folderName, destinationName) {
    this.fs.copy(
      this.templatePath(folderName + '/*'),
      this.destinationPath(destinationName || folderName));
  }
};

class Module {

  constructor(name) {
    this.name = name;
    this.codeName = name.replace(/ /g, "");
    this.camelCaseName = this.codeName[0].toLowerCase() + this.codeName.substr(1);
  }

  name = "";

  /**
   * Name with spaces removed.
   */
  codeName = "";

  camelCaseName = "";
}