import { Data } from '@singularsystems/neo-core';
import { AxiosPromise } from 'axios';
import { injectable } from 'inversify';
import { AppService, Types } from '../DomainTypes';

export interface IImageKitApiClient {

    getAuthParams(): AxiosPromise<any>;

    // Client only properties / methods
}

@injectable()
export default class ImageKitApiClient extends Data.ApiClientBase implements IImageKitApiClient {

    constructor (config = AppService.get(Types.App.Config)) {
        super(`${config.apiPath}/imagekit`);
    }

    public getAuthParams(): AxiosPromise<any> {
        return this.axios.get(`${this.apiPath}`);
    }

    // Client only properties / methods
}