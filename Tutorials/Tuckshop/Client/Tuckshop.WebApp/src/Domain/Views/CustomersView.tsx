import React from 'react';
import { Neo, Views } from '@singularsystems/neo-react';
import CustomersVM from './CustomersVM';
import { observer } from 'mobx-react';
import { ModalUtils } from '@singularsystems/neo-core';
import Customer from '../Models/Customer';

class CustomersParams {
    // TODO: Add parameters here in the form: public paramName = { isQuery?: boolean, required?: boolean };
}

@observer
export default class CustomersView extends Views.ViewBase<CustomersVM, CustomersParams> {
    public static params = new CustomersParams();

    constructor(props: unknown) {
        super("Customers", CustomersVM, props);
    }

    protected viewParamsUpdated() {

    }

    public render() {

        const pagedCustomers = this.viewModel.pagedCustomers;
        const emptySlots = this.viewModel.pageSize - pagedCustomers.length;
        return (
            <div className="sweet-muse mt-3">
                <div className="page-header">
                    <h1 className="title">
                        Customers
                    </h1>

                    <Neo.Button
                        variant="primary"
                        className="add-btn"
                        onClick={() => this.viewModel.addCustomer()}
                    >
                        <Neo.Icon name="plus" />

                        Add Customer
                    </Neo.Button>
                </div>


                <div className="search-bar mb-3">
                    <Neo.Icon name="search" className="search-icon" />
                    <input
                        type="text"
                        className="search-input"
                        placeholder="Search by first or last name... "
                        value={this.viewModel.searchTerm}
                        onChange={(e) => this.viewModel.setSearchTerm(e.target.value)}
                    />
                </div>


                <div className="table-grid">
                    <table className="table">
                        <thead>
                            <tr>
                                <th>First Name</th>
                                <th>Last Name</th>
                                <th>Email</th>
                                <th>Phone Number</th>
                                <th>Wallet Balance</th>
                                <th></th>
                                <th></th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            {pagedCustomers.map((customer) => {

                                return (
                                    <tr key={customer.customerId}>
                                        <td>
                                            {customer.firstName}
                                        </td>

                                        <td>
                                            {customer.lastName}
                                        </td>

                                        <td>
                                            {customer.email}
                                        </td>

                                        <td>
                                            {customer.phoneNumber}
                                        </td>

                                        <td>
                                            {customer.walletBalance}
                                        </td>

                                        <td>
                                            <Neo.Button
                                                icon="add_card"
                                                className="wallet-icon"
                                                onClick={() =>
                                                    this.viewModel.openWallet(customer)
                                                }
                                            />
                                        </td>

                                        <td>
                                            <Neo.Button
                                                icon="edit"
                                                className="edit-icon"
                                                onClick={() =>
                                                    this.viewModel.editCustomer(customer)
                                                }
                                            />
                                        </td>

                                        <td>
                                            <Neo.Button
                                                icon="delete"
                                                className="delete-icon"
                                                onClick={() =>
                                                    this.deleteCustomer(customer)
                                                }
                                            />
                                        </td>
                                    </tr>
                                );
                            })}

                            {pagedCustomers.length === 0 && (
                                <tr className="customer-customer-filler-row  ">
                                    <td colSpan={8} className="message">
                                        No customers found
                                    </td>
                                </tr>
                            )}

                            {Array.from({
                                length:
                                    pagedCustomers.length === 0
                                        ? emptySlots - 1
                                        : emptySlots,
                            }).map((_, index) => (
                                <tr key={`filler-${index}`} className="customer-filler-row ">
                                    <td colSpan={8}>&nbsp;</td>
                                </tr>
                            ))}

                        </tbody>
                    </table>
                </div>

                <div className="pagination">
                    <Neo.Button
                        className="pagination-btn"
                        icon="keyboard_double_arrow_left"
                        disabled={this.viewModel.currentPage === 1}
                        onClick={() => this.viewModel.previousPage()}
                    >
                    </Neo.Button>

                    <span className="pagination-info">
                        Page {this.viewModel.currentPage} of {this.viewModel.totalPages}
                    </span>

                    <Neo.Button
                        className="pagination-btn"
                        icon="keyboard_double_arrow_right"
                        disabled={
                            this.viewModel.currentPage === this.viewModel.totalPages
                        }
                        onClick={() => this.viewModel.nextPage()}
                    >
                    </Neo.Button>
                </div>

                {this.viewModel.editingCustomer && (
                    <Neo.Modal
                        show={!!this.viewModel.editingCustomer}
                        title={
                            this.viewModel.editingCustomer.customerId
                                ? "Edit Customer"
                                : "Add Customer"
                        }
                       
                        onClose={() => this.viewModel.cancelEdit()
                            
                        }
                    >

                        <Neo.Form
                            model={this.viewModel.editingCustomer}
                            onSubmit={() => this.viewModel.saveCustomer()}
                        >
                            {(customer, customerMeta) => (
                                <div className="form">

                                    <div className="">
                                        <div className="form-field name-field">
                                            <Neo.FormGroupInline
                                                bind={customerMeta.firstName}
                                            />
                                        </div>

                                        <div className="form-field price-field">
                                            <Neo.FormGroupInline
                                                bind={customerMeta.lastName}
                                            />
                                        </div>
                                    </div>

                                    <div className="form-field">
                                        <Neo.FormGroupInline
                                            bind={customerMeta.email}
                                        />
                                    </div>

                                    <div className="form-field">
                                        <Neo.FormGroupInline
                                            bind={customerMeta.phoneNumber}
                                        />
                                    </div>

                                    <Neo.Button
                                        isSubmit
                                        variant="success"
                                        icon="check"
                                        className="save-btn"
                                    >
                                        {this.viewModel.editingCustomer?.customerId
                                            ? "Update Customer"
                                            : "Save Customer"}
                                    </Neo.Button>

                                </div>

                            )}
                        </Neo.Form>
                    </Neo.Modal>
                )}
{this.viewModel.walletCustomer && this.viewModel.newDeposit && this.viewModel.newWithdrawal && (
    <Neo.Modal
        show={!!this.viewModel.walletCustomer}
        title={`Wallet - ${this.viewModel.walletCustomer.firstName} ${this.viewModel.walletCustomer.lastName}`}
        onClose={() => this.viewModel.closeWallet()}
        closeButton={{ className: "wallet-modal-close" }}
    >
        <div className="wallet-modal-body">

            <div className="wallet-balance-display">
                <span>Current balance</span>
                <strong>R{Number(this.viewModel.walletCustomer.walletBalance).toFixed(2)}</strong>
            </div>

            {/* <Neo.Form model={this.viewModel.newDeposit}>
                {(deposit, depositMeta) => (
                    <div className="form wallet-modal-row">
                        <Neo.FormGroupInline
                            bind={depositMeta.amount}
                            label="Deposit amount"
                        />
                    </div>
                )}
            </Neo.Form>

            <Neo.Form model={this.viewModel.newWithdrawal}>
                {(withdrawal, withdrawalMeta) => (
                    <div className="form wallet-modal-row">
                        <Neo.FormGroupInline
                            bind={withdrawalMeta.amount}
                            label="Withdrawal amount"
                        />
                    </div>


                )}
            </Neo.Form> */}

            <Neo.Form model={
                this.viewModel.walletAction === "deposit"
                ? this.viewModel.newDeposit
                : this.viewModel.newWithdrawal
            
            }
        >
                {(model, meta ) => (
                    <div className="form wallet-modal-row">
                         <Neo.FormGroupInline
                            bind={meta.amount}
                            label="Amount"
                        />
                    </div>
                )}
            </Neo.Form>

            <div className="wallet-modal-actions">
                <Neo.Button
                  
                    className="withdraw-btn"
                    onClick={() => {
                        this.viewModel.setWalletAction("deposit");
                        this.viewModel.depositToWallet();
                    }}
                >
                      <Neo.Icon
                    name="plus"/>

                    Deposit
                </Neo.Button>

                <Neo.Button
                    icon="minus"
                    className="withdraw-btn"
                    onClick={() => {
                        this.viewModel.setWalletAction("withdraw");
                        this.viewModel.withdrawFromWallet();
                        
                    }}
                >
                      <Neo.Icon
                    name="minus"/>
                    Withdraw
                </Neo.Button>
            </div>

        </div>
    </Neo.Modal>
)}
            </div>
            
        );
    }

    public deleteCustomer(customer: Customer) {
        ModalUtils.showYesNo(
            "Delete Customer",
            `Are you sure you want to delete ${customer.firstName}?`,
            () => this.viewModel.deleteCustomer(customer)
        );
    }
}