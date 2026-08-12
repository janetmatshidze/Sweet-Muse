import { Views } from '@singularsystems/neo-react';
import { AppService, Types } from '../DomainTypes';
import Customer from '../Models/Customer';
import { List } from '@singularsystems/neo-core';

export default class CustomersVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private customersApiClient = AppService.get(Types.Domain.ApiClients.CustomersApiClient),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications)) {

        super(taskRunner);
        this.makeObservable();
    }

    public customers = new List(Customer);

    public editingCustomer: Customer | null = null;

    public searchTerm: string = "";

    public setSearchTerm(value: string) {
        this.searchTerm = value;
        this.currentPage = 1;
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

    public readonly pageSize = 6;

    public currentPage: number = 1;

    public get totalPages() {

        return Math.max(
            1,
            Math.ceil(this.filteredCustomers.length / this.pageSize)
        );
    }

    public get pagedCustomers() {
        const start = (this.currentPage - 1) * this.pageSize;

        return this.filteredCustomers.slice(start, start + this.pageSize);
    }

    public goToPage(page: number) {

        if (page < 1 || page > this.totalPages) {

            return;
        }

        this.currentPage = page;
    }

    public nextPage() {

        this.goToPage(this.currentPage + 1);
    }

    public previousPage() {

        this.goToPage(this.currentPage - 1);
    }

    private clampCurrentPage() {

        if (this.currentPage > this.totalPages) {

            this.currentPage = this.totalPages;
        }
    }

    public async initialise() {
        const response = await this.taskRunner.waitFor(
            this.customersApiClient.get()
        );
        this.customers.set(response.data);
    }

    public addCustomer() {
        this.editingCustomer = new Customer();
    }

    public editCustomer(customer: Customer) {
        const edit = new Customer();

        edit.set(customer.toJSObject());

        this.editingCustomer = edit;
    }

    public deleteCustomer(customer: Customer) {

        this.taskRunner.run(async () => {

            if (customer.customerId) {

                await this.customersApiClient.delete(
                    customer.customerId
                );
            }

            const existing = this.customers.find(
                c => c.customerId == customer.customerId
            );

            if (existing) {
                this.customers.remove(existing);
            }

            this.clampCurrentPage();

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
    }

    public saveCustomer() {
        if (!this.editingCustomer) {
            return;
        }
        this.taskRunner.run(async () => {
            const response =
                await this.customersApiClient.save(
                    this.editingCustomer!.toJSObject()
                );

            const existing = this.customers.find(
                c => c.customerId === response.data.customerId
            );

            if (existing) {
                existing.set(response.data);
            } else {
                const newCustomer = new Customer();

                newCustomer.set(response.data);

                this.customers.push(newCustomer);
            }

            this.notifications.addSuccess(
                "Customer saved",
                `Customer saved successfully`,
                4
            );

            this.editingCustomer = null;

        });
    }


}