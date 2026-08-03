import { AppServices } from '@singularsystems/neo-core';
import { DomainTypes } from './DomainTypes';
import { DomainDataCache } from './Services/DomainDataCache';
import { NeoServicesTypes } from "@singularsystems/neo-react-services"
import CatalogueEditService from './Services/CatalogueEditService';
import CatalogueApiClient from "./ApiClients/CatalogueApiClient";

export const DomainAppModule = new AppServices.Module("Domain", container => {

    // Api Clients
    container.bind(DomainTypes.ApiClients.Catalogue).to(CatalogueApiClient).inSingletonScope();
    
    // Services
    container.bind(DomainTypes.Services.DataCache).to(DomainDataCache).inSingletonScope();
    container.bind(NeoServicesTypes.Catalogue.CatalogueEditService).to(CatalogueEditService).inSingletonScope();
});

export const DomainTestModule = new AppServices.Module("Domain", container => {
    // bind test types
});