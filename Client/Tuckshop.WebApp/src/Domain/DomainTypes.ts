import { AppServices } from '@singularsystems/neo-core';
import { AppService, Types as AppTypes } from '../App/Services/AppService';
import { DomainExportedTypes } from './DomainExportedTypes';
import { DomainDataCache } from './Services/DomainDataCache';
import { ICatalogueApiClient } from "./ApiClients/CatalogueApiClient";

// Symbols specific to this module.
const DomainTypes = {
    ApiClients: {
        Catalogue: new AppServices.ServiceIdentifier<ICatalogueApiClient>("Domain.ApiClients.Catalogue"),
    },
    Services: {
        ...DomainExportedTypes.Services,
        DataCache: new AppServices.ServiceIdentifier<DomainDataCache>("Domain.Services.DataCache"),
    }
}

// Merged symbols from app for convenience.
const Types = {
    ...AppTypes,
    Domain: DomainTypes
}

export { AppService, Types, DomainTypes }