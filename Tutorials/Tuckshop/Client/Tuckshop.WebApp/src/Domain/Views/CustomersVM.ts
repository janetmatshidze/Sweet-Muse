import { Views } from '@singularsystems/neo-react';
import { AppService, Types } from '../DomainTypes';
import Customer from '../Models/Customers/Customer';
import { List } from '@singularsystems/neo-core';
import DepositToWallet from '../Models/Wallets/Commands/DepositToWallet';
import WithdrawFromWallet from '../Models/Wallets/Commands/WithdrawFromWallet';
import UpdateCustomerDetails from '../Models/Customers/Commands/UpdateCustomerDetails';
import WalletAmountInput from '../Models/WalletAmountInput';
import DeleteCustomer from '../Models/Customers/Commands/DeleteCustomer';
import CreateCustomer from '../Models/Customers/Commands/CreateCustomer';
import PaginationHelper from '../../App/Models/Helpers/PaginationHelper';

export default class CustomersVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private customersApiClient = AppService.get(Types.Domain.ApiClients.CustomersApiClient),
        private customersCommandApiClient = AppService.get(Types.Domain.ApiClients.CustomersCommandApiClient),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications)) {

        super(taskRunner);
        this.makeObservable();
    }

    public walletCustomer: Customer | null = null;

    // Plain UI input — "what the user typed". Not a domain command itself.
    // Both deposit and withdraw build their own command object fresh at
    // submit time, reading .amount from here.

    public walletInput = new WalletAmountInput();

    public newDeposit: DepositToWallet | null = null;

    public newWithdrawal: WithdrawFromWallet | null = null;

    public customers = new List(Customer);

    public editingCustomer: UpdateCustomerDetails | null = null;

    public newCustomer: CreateCustomer | null = null;

    public pagination = new PaginationHelper(() => this.filteredCustomers, 6);

    public searchTerm: string = "";

    public setSearchTerm(value: string) {
        this.searchTerm = value;
        this.pagination.reset();
    }

    public openWallet(customer: Customer) {
        this.walletCustomer = customer;
        this.walletInput = new WalletAmountInput();

    }

    public closeWallet() {
        this.walletCustomer = null;
    }

    public depositToWallet() {
        if (!this.walletCustomer) {
            return;
        }

        const deposit = new DepositToWallet();
        deposit.customerId = this.walletCustomer.customerId;
        deposit.amount = this.walletInput.amount;

        this.taskRunner.run(async () => {
            const response = await this.customersCommandApiClient.deposit(
                deposit.toJSObject()
            );

            this.walletCustomer!.walletBalance = response.data.walletBalance;

            this.notifications.addSuccess(
                "Deposit successful",
                "Wallet topped up successfully",
                4
            );

            this.closeWallet();
        }).catch(() => {

        });
    }

    public withdrawFromWallet() {
        if (!this.walletCustomer) {
            return;
        }

        const withdrawal = new WithdrawFromWallet();
        withdrawal.customerId = this.walletCustomer.customerId;
        withdrawal.amount = this.walletInput.amount;

        this.taskRunner.run(async () => {
            const response = await this.customersCommandApiClient.withdraw(
                withdrawal.toJSObject()
            );

            this.walletCustomer!.walletBalance = response.data.walletBalance;

            this.notifications.addSuccess(
                "Withdrawal successful",
                "Withdrawal processed successfully",
                4
            );

            this.closeWallet();

        }).catch(() => {

        });
    }

    public get filteredCustomers() {
        let result = [...this.customers];

        const term = this.searchTerm.trim().toLowerCase();

        if (term) {
            result = result.filter(c => `${c.firstName ?? ""} ${c.lastName ?? ""}`.toLowerCase().includes(term)
            );
        }

        return result;
    }



    public async initialise() {
        const response = await this.taskRunner.waitFor(
            this.customersApiClient.get()
        );
        this.customers.set(response.data);
    }

    public addCustomer() {
        this.newCustomer = new CreateCustomer();
    }

    public editCustomer(customer: Customer) {
        const edit = new UpdateCustomerDetails();

        edit.set(customer.toJSObject());

        this.editingCustomer = edit;
    }

    public deleteCustomer(customer: Customer) {

        const command = new DeleteCustomer();
        command.customerId = customer.customerId;

        this.taskRunner.run(async () => {

            if (customer.customerId) {

                await this.customersCommandApiClient.delete(
                    command.toJSObject()
                );
            }

            const existing = this.customers.find(
                c => c.customerId == customer.customerId
            );

            if (existing) {
                this.customers.remove(existing);
            }

            this.pagination.clamp();

            this.notifications.addSuccess(
                "Customer deleted",
                `Customer with ${customer} deleted successfully`,
                4
            );

            this.editingCustomer = null;

        }).catch(() => {
        });
    }

    public cancelEdit() {
        this.editingCustomer = null;
        this.newCustomer = null;
    }

    public saveCustomer() {
        if (this.newCustomer) {
            this.saveNewCustomer();
        } else if (this.editingCustomer) {
            this.saveEditedCustomer();
        }
    }

    private saveNewCustomer() {
        if (!this.newCustomer) {
            return;
        }

        this.taskRunner.run(async () => {
            const response = await this.customersCommandApiClient.create(
                this.newCustomer!.toJSObject()
            );

            const newCustomer = new Customer();

            newCustomer.set(response.data);

            this.customers.push(newCustomer);

            this.notifications.addSuccess(
                "Customer created",
                "Customer created successfully",
                4
            );

            this.newCustomer = null;

        }).catch((err) => {
            console.error("Create customer failed:", err);
            this.notifications.addDanger("Create failed", err?.message ?? "Unknown error", 6);
        });
    }

    private saveEditedCustomer() {
        if (!this.editingCustomer) {
            return;
        }

        this.taskRunner.run(async () => {
            const response =
                await this.customersCommandApiClient.updateDetails(
                    this.editingCustomer!.toJSObject()
                );

            const existing = this.customers.find(
                c => c.customerId === response.data.customerId
            );

            if (existing) {
                existing.set(response.data);
            }

            this.notifications.addSuccess(
                "Customer saved",
                `Customer saved successfully`,
                4
            );

            this.editingCustomer = null;

        }).catch((err) => {
            console.error("Save customer failed:", err);
            this.notifications.addDanger("Save failed", err?.message ?? "Unknown error", 6);

        });
    }


}