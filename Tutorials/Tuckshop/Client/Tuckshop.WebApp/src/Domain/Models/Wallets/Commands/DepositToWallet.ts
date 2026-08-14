import { Attributes, ModelBase, Rules, Validation } from '@singularsystems/neo-core';
export default class DepositToWallet extends ModelBase {
    static typeName = "DepositToWallet";

    constructor() {
        super();
        this.makeObservable();
    }

    @Rules.Required()
    public customerId: number = 0;

    @Attributes.Float()
    public amount: number = 0;

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<DepositToWallet>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        if (this.isNew) {
            return "New deposit to wallet";
        } else {
            return "Deposit To Wallet";
        }
    }
}