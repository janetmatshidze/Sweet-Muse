import { Data, Model } from '@singularsystems/neo-core';
import { AxiosPromise } from 'axios';
import { injectable } from 'inversify';
import { AppService, Types } from '../DomainTypes';
import { CreateOrder } from '../Models/Orders/Commands/CreateOrder';
import Order from '../Models/Orders/Order';
import CompleteOrder from '../Models/Orders/Commands/CompleteOrder';
import CancelOrder from '../Models/Orders/Commands/CancelOrder';

export interface IOrdersCommandApiClient {

    /** 
     * Creates a new order.
     * @param command The create order command.
     * @returns The created order.
     */
    createOrder(command: Model.PartialPlainObject<CreateOrder>): AxiosPromise<Model.PlainTrackedObject<Order>>;

    /** 
     * Completes an existing order.
     * @param command The complete order command.
     * @returns A task representing the asynchronous operation.
     */
    completeOrder(command: Model.PartialPlainObject<CompleteOrder>): AxiosPromise;

    /** 
     * Cancels an exisiting order.
     * @param command The cancel order command.
     * @returns A task representing the asynchronous operation.
     */
    cancelOrder(command: Model.PartialPlainObject<CancelOrder>): AxiosPromise;

    // Client only properties / methods
}

@injectable()
export default class OrdersCommandApiClient extends Data.ApiClientBase implements IOrdersCommandApiClient {

    constructor (config = AppService.get(Types.App.Config)) {
        super(`${config.apiPath}/orders/commands`);
    }

    public createOrder(command: Model.PartialPlainObject<CreateOrder>): AxiosPromise<Model.PlainTrackedObject<Order>> {
        return this.axios.post(`${this.apiPath}/create`, command);
    }

    public completeOrder(command: Model.PartialPlainObject<CompleteOrder>): AxiosPromise {
        return this.axios.put(`${this.apiPath}/complete`, command);
    }

    public cancelOrder(command: Model.PartialPlainObject<CancelOrder>): AxiosPromise {
        return this.axios.put(`${this.apiPath}/cancel`, command);
    }

    // Client only properties / methods
}