import { Data, Model } from '@singularsystems/neo-core';
import { AxiosPromise } from 'axios';
import { injectable } from 'inversify';
import { AppService, Types } from '../DomainTypes';
import DepositToWallet from '../Models/Wallets/Commands/DepositToWallet';
import Customer from '../Models/Customer';
import WithdrawFromWallet from '../Models/Wallets/Commands/WithdrawFromWallet';
import UpdateCustomerDetails from '../Models/Customers/Commands/UpdateCustomerDetails';
import DeleteCustomer from '../Models/Customers/Commands/DeleteCustomer';
import CreateCustomer from '../Models/Customers/Commands/CreateCustomer';

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

    /** 
     * Creates a new customer.
     * @param command The create customer command.
     * @returns The created customer.
     */
    create(command: Model.PartialPlainObject<CreateCustomer>): AxiosPromise<Model.PlainTrackedObject<Customer>>;

    /**
     * Updates a customer's profile details (name, email, phone number).
     * Does not affect wallet balance.
     * @param command The update details command.
     * @returns The updated customer.
     */
    updateDetails(command: Model.PartialPlainObject<UpdateCustomerDetails>): AxiosPromise<Model.PlainTrackedObject<Customer>>;

    /**
     * Deletes a customer however if customers has existing orders it refuses.
     * @param command Delete command
     * @returns The deleted customer.
     */
    delete(command: Model.PartialPlainObject<DeleteCustomer>): AxiosPromise;

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

    public create(command: Model.PartialPlainObject<CreateCustomer>): AxiosPromise<Model.PlainTrackedObject<Customer>> {
        return this.axios.post(`${this.apiPath}/create`, command);
    }

    public updateDetails(command: Model.PartialPlainObject<UpdateCustomerDetails>): AxiosPromise<Model.PlainTrackedObject<Customer>> {
        return this.axios.put(`${this.apiPath}/update-details`, command);
    }

    public delete(command: Model.PartialPlainObject<DeleteCustomer>): AxiosPromise {
        return this.axios.post(`${this.apiPath}/delete`, command);
    }

    // Client only properties / methods
}