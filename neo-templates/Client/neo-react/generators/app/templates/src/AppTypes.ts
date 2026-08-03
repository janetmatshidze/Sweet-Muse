import { AppServices } from '@singularsystems/neo-core';
import { NeoReactTypes } from '@singularsystems/neo-react';
import { AppConfig } from './Models/AppConfig';
<%_ if(!hasModules) { _%>
import { AppDataCache } from './Services/AppDataCache';
<%_ } _%>
import { RouteService } from './Services/RouteService';
import { IAppLayout } from './Services/AppLayout';
import { AuthenticationService } from './Services/AuthenticationService';
<%_ if(includeNeoServices) { _%>
import { NotificationServiceTypes } from '@singularsystems/neo-notifications';
import { ReportingTypes } from '@singularsystems/neo-reporting';
<%_ } _%>
<%_ if(hasModules) { _%>
<%- moduleTypeImports -%>
<%_ } _%>

const Types = {
<%_ if(!hasModules) { _%>
    Services: {
        AuthenticationService: AppServices.NeoTypes.Security.AuthenticationService.asType<AuthenticationService>(),
        DataCache: new AppServices.ServiceIdentifier<AppDataCache>("Services.DataCache"),
        AppLayout: new AppServices.ServiceIdentifier<IAppLayout>("Services.AppLayout"),
        RouteService: new AppServices.ServiceIdentifier<RouteService>("Services.RouteService"),
    },
    Config: AppServices.NeoTypes.Config.ConfigModel.asType<AppConfig>(),
<%_ } _%>
<%_ if(hasModules) { _%>
    App: {
        Services: {
            AuthenticationService: AppServices.NeoTypes.Security.AuthenticationService.asType<AuthenticationService>(),
            AppLayout: new AppServices.ServiceIdentifier<IAppLayout>("Services.AppLayout"),
            RouteService: new AppServices.ServiceIdentifier<RouteService>("Services.RouteService"),
        },
        Config: AppServices.NeoTypes.Config.ConfigModel.asType<AppConfig>(),
    },
<%_ } _%>
    Neo: NeoReactTypes,
<%_ if(includeNeoServices) { _%>
    Notifications: NotificationServiceTypes,
    Reporting: ReportingTypes,
<%_ } _%>
<%_ if(hasModules) { _%>
<%= moduleTypeExports %>
<%_ } _%>
};

export default Types;