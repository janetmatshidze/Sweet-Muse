import { Attributes, ModelBase, Rules, Validation } from '@singularsystems/neo-core';
export default class WithdrawFromWallet extends ModelBase {
    static typeName = "WithdrawFromWallet";

    constructor() {
        super();
        this.makeObservable();
    }

    @Rules.Required()
    public customerId: number = 0;

    @Attributes.Float()
    public amount: number = 0;

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<WithdrawFromWallet>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        if (this.isNew) {
            return "New withdraw from wallet";
        } else {
            return "Withdraw From Wallet";
        }
    }
}