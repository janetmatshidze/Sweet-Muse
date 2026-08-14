import { Data, Model } from '@singularsystems/neo-core';
import { AxiosPromise } from 'axios';
import { injectable } from 'inversify';
import { AppService, Types } from '../DomainTypes';
import DepositToWallet from '../Models/Wallets/Commands/DepositToWallet';
import Customer from '../Models/Customer';
import WithdrawFromWallet from '../Models/Wallets/Commands/WithdrawFromWallet';

export interface ICustomersCommandApiClient {

    /** 
     * Deposits an amount into a customer's wallet.
     * @param command The deposit command.
     * @returns The updated customer.
     */
    deposit(command: Model.PartialPlainObject<DepositToWallet>): AxiosPromise<Model.PlainTrackedObject<Customer>>;

    /** 
     * Withdraws an amount from a customer's wallet.
     * @param command The withdraw command.
     * @returns The updated customer.
     */
    withdraw(command: Model.PartialPlainObject<WithdrawFromWallet>): AxiosPromise<Model.PlainTrackedObject<Customer>>;

    // Client only properties / methods
}

@injectable()
export default class CustomersCommandApiClient extends Data.ApiClientBase implements ICustomersCommandApiClient {

    constructor (config = AppService.get(Types.App.Config)) {
        super(`${config.apiPath}/customers/commands`);
    }

    public deposit(command: Model.PartialPlainObject<DepositToWallet>): AxiosPromise<Model.PlainTrackedObject<Customer>> {
        return this.axios.post(`${this.apiPath}/deposit`, command);
    }

    public withdraw(command: Model.PartialPlainObject<WithdrawFromWallet>): AxiosPromise<Model.PlainTrackedObject<Customer>> {
        return this.axios.post(`${this.apiPath}/withdraw`, command);
    }

    // Client only properties / methods
}

