import { ModelBase, Validation } from '@singularsystems/neo-core';
export default class DeleteCustomer extends ModelBase {
    static typeName = "DeleteCustomer";

    constructor() {
        super();
        this.makeObservable();
    }

    public customerId: number = 0;

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<DeleteCustomer>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        if (this.isNew) {
            return "New delete customer";
        } else {
            return "Delete Customer";
        }
    }
}