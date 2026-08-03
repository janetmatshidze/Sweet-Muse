import { AppService } from './Services/AppService';
import { AppModule } from './AppModule';
import { AppServices } from '@singularsystems/neo-core';
import { NeoReactModule } from '@singularsystems/neo-react';
<%_ if(includeNeoServices) { _%>
import { IdentityModule } from '../Identity/IdentityModule';
import { ReportingModule } from '@singularsystems/neo-reporting';
import { AppReportingModule } from '../Reporting/ReportingModule';
import { NotificationServiceModule } from '@singularsystems/neo-notifications';
<%_ } _%>
<%_ if(hasModules) { _%>
import { AuthorisationAppModule } from '@singularsystems/neo-authorisation';
<%- moduleSetupImports -%>
<%_ } _%>

const appService = AppService as AppServices.AppService;

appService.registerModule(AppServices.NeoModule);
appService.registerModule(NeoReactModule);
<%_ if(hasModules) { _%>
appService.registerModule(AuthorisationAppModule);
<%_ } -%>
<%_ if(includeNeoServices) { _%>
appService.registerModule(IdentityModule)
appService.registerModule(NotificationServiceModule);
appService.registerModule(ReportingModule);
appService.registerModule(AppReportingModule);
<%_ } -%>
appService.registerModule(AppModule);<%_ if(hasModules) { _%>
<%= moduleRegistration -%>
<%_ } -%>