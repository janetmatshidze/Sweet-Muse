import { AppServices } from '@singularsystems/neo-core';
import { AppService, Types as AppTypes } from '../App/Services/AppService';
import { DomainExportedTypes } from './DomainExportedTypes';
import { DomainDataCache } from './Services/DomainDataCache';
import { ICatalogueApiClient } from "./ApiClients/CatalogueApiClient";
import { IProductsApiClient } from './ApiClients/ProductsApiClient';
import { IOrdersCommandApiClient } from './ApiClients/OrdersCommandApiClient';
import { IOrdersQueryApiClient } from './ApiClients/OrdersQueryApiClient';
import { ICategoriesApiClient } from './ApiClients/CategoriesApiClient';
import ImageKitApiClient from './ApiClients/ImageKitApiClient';
import CustomersApiClient from './ApiClients/CustomersApiClient';

// Symbols specific to this module.
const DomainTypes = {
    ApiClients: {
        Catalogue: new AppServices.ServiceIdentifier<ICatalogueApiClient>("Domain.ApiClients.Catalogue"),

        OrdersCommandApiClient : new AppServices.ServiceIdentifier<IOrdersCommandApiClient>("Domain.ApiClients.OrdersCommandApiClient"),

        ProductsApiClient : new AppServices.ServiceIdentifier<IProductsApiClient>("Domain.ApiClients.ProductsApiClient"),

        OrdersQueryApiClient : new AppServices.ServiceIdentifier<IOrdersQueryApiClient>("Domain.ApiClients.OrdersQueryApiClient"),

        CategoriesApiClient : new AppServices.ServiceIdentifier<ICategoriesApiClient>("Domain.ApiClients.CategoriesApiClient"),

        ImageKitApiClient : new AppServices.ServiceIdentifier<ImageKitApiClient>("Domain.ApiClients.ImageKitApiClient"),

        CustomersApiClient: new AppServices.ServiceIdentifier<CustomersApiClient>("Domain.ApiClients.CustomersApiClient")
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