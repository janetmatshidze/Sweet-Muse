import { AppServices } from '@singularsystems/neo-core';
import { <%= moduleName %>Types } from './<%= moduleName %>Types';
import { <%= moduleName %>DataCache } from './Services/<%= moduleName %>DataCache';

export const <%= moduleName %>AppModule = new AppServices.Module("<%= moduleName %>", container => {

    // Api Clients
    // container.bind(<%= moduleName %>Types.ApiClients.ApiClient).to(ApiClient).inSingletonScope();
    
    // Services
    container.bind(<%= moduleName %>Types.Services.DataCache).to(<%= moduleName %>DataCache).inSingletonScope();
});

export const <%= moduleName %>TestModule = new AppServices.Module("<%= moduleName %>", container => {
    // bind test types
});