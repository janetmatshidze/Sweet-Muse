import { Data } from '@singularsystems/neo-core';
import { injectable } from 'inversify';
import { AppService, Types } from '../DomainTypes';
import Category from '../Models/Category';

export interface ICategoriesApiClient extends Data.IUpdateableApiClient<Category, number> {

    // Client only properties / methods
}

@injectable()
export default class CategoriesApiClient extends Data.UpdateableApiClient<Category, number> implements ICategoriesApiClient {

    constructor (config = AppService.get(Types.App.Config)) {
        super(`${config.apiPath}/Categories`);
    }

    // Client only properties / methods
}