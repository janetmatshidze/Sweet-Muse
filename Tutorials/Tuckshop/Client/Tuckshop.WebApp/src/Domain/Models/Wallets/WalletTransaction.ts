import { Attributes, ModelBase, Rules, Validation } from '@singularsystems/neo-core';
import { WalletTransactionType } from './Enums/WalletTransactionType';

export default class WalletTransaction extends ModelBase {
    static typeName = "WalletTransaction";

    constructor() {
        super();
        this.makeObservable();
    }

    public walletTransactionId: number = 0;

    public customerId: number = 0;

    @Attributes.Float()
    public amount: number = 0;

    public type: WalletTransactionType | null = null;

    @Rules.Required()
    @Attributes.Date()
    public occurredOn: Date | null = null;

    public processedByUserId: number = 0;

    @Attributes.Nullable()
    public orderId: number | null = null;

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<WalletTransaction>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        if (this.isNew) {
            return "New wallet transaction";
        } else {
            return "Wallet Transaction";
        }
    }
}