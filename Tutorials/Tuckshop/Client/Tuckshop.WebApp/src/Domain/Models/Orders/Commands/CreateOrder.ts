import { Attributes, List, ModelBase, Rules, Validation } from '@singularsystems/neo-core';

export class CreateOrder extends ModelBase {
    static typeName = "CreateOrder";

    constructor() {
        super();
        this.makeObservable();
    }

    @Rules.StringLength(100)
    public customerName: string = "";

    public orderDetails = new List(NewOrderDetail);

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<CreateOrder>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        if (this.isNew || !this.customerName) {
            return "New create order";
        } else {
            return this.customerName;
        }
    }
}

export class NewOrderDetail {

    public productId: number = 0;

    @Attributes.Integer()
    public quantity: number = 0;

    // Client only properties / methods
}