import { Attributes, List, ModelBase, Rules, Validation } from '@singularsystems/neo-core';
import WalletTransaction from '../Wallets/WalletTransaction';

export default class Customer extends ModelBase {
    static typeName = "Customer";

    constructor() {
        super();
        this.makeObservable();
    }

    public customerId: number = 0;

    @Rules.Required()
    @Rules.StringLength(100)
    public firstName: string = "";

    @Rules.Required()
    @Rules.StringLength(100)
    public lastName: string = "";

    @Rules.Required()
    @Rules.StringLength(100)
    public email: string = "";

    @Rules.Required()
    @Rules.StringLength(10)
    public phoneNumber: string = "";

    @Attributes.Float()
    public walletBalance: number = 0;

    public get fullName() {
        return `${this.firstName} ${this.lastName}`
    }

    public walletTransactions = new List(WalletTransaction);

    // Client only properties / methods

    addBusinessRules(rules: Validation.Rules<Customer>) {
        super.addBusinessRules(rules);

        rules.failWhen(c => !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(c.email),
        "Please enter a valid email address."
    );

     rules.failWhen(
        c => !/^\d{10}$/.test(c.phoneNumber),
        "Phone number must be exactly 10 digits."
    );

    rules.failWhen(
    c => /^\d{10}$/.test(c.phoneNumber) && /^(\d)\1{9}$/.test(c.phoneNumber),
    "Please enter a valid phone number."
);
    }

    public toString(): string {
        if (this.isNew || !this.firstName) {
            return "New customer";
        } else {
            return this.firstName;
        }
    }
}