import { Attributes, List, ModelBase, Rules, Validation } from '@singularsystems/neo-core';
import WalletTransaction from './Wallets/WalletTransaction';

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
    @Rules.StringLength(20)
    public phoneNumber: string = "";

    @Attributes.Float()
    public walletBalance: number = 0;

    public walletTransactions = new List(WalletTransaction);

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<Customer>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        if (this.isNew || !this.firstName) {
            return "New customer";
        } else {
            return this.firstName;
        }
    }
}