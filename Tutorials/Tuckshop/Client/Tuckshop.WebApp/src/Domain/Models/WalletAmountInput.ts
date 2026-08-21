import { Attributes, ModelBase } from '@singularsystems/neo-core';

// Purely a UI input holder — represents "what the user typed", not
// a domain action. DepositToWallet and WithdrawFromWallet each represent
// a specific intent and are built fresh at submit time from this value,
// so this class deliberately does NOT double as either of them.
export default class WalletAmountInput extends ModelBase {
    static typeName = "WalletAmountInput";

    constructor() {
        super();
        this.makeObservable();
    }
    
    @Attributes.Float(2)
    public amount: number = 0;

    public toString(): string {
        return "Wallet amount";
    }
}