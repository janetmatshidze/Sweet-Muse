import { AppServices } from '@singularsystems/neo-core';
import { AppService, Types as AppTypes } from '../App/Services/AppService';
import { <%= moduleName %>ExportedTypes } from './<%= moduleName %>ExportedTypes';
import { <%= moduleName %>DataCache } from './Services/<%= moduleName %>DataCache';

// Symbols specific to this module.
const <%= moduleName %>Types = {
    ApiClients: {

    },
    Services: {
        ...<%= moduleName %>ExportedTypes.Services,
        DataCache: new AppServices.ServiceIdentifier<<%= moduleName %>DataCache>("<%= moduleName %>.Services.DataCache"),
    }
}

// Merged symbols from app for convenience.
const Types = {
    ...AppTypes,
    <%= moduleName %>: <%= moduleName %>Types
}

export { AppService, Types, <%= moduleName %>Types }