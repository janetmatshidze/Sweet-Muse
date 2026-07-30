import { Data } from '@singularsystems/neo-core';
import { injectable } from 'inversify';
// import { AppService, Types } from '../DomainTypes';

export enum LifeTime {
    Short = 30,
    Long = 240
}

@injectable()
export class DomainDataCache extends Data.CachedDataService {

    // Register cached data here:
    // public examples = this.registerList(Example, this.apiClient.examples.get, LifeTime.Short);
}