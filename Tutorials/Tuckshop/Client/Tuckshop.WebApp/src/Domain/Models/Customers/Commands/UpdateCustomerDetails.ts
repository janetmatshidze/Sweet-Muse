import { ModelBase, Rules, Validation } from '@singularsystems/neo-core';
export default class UpdateCustomerDetails extends ModelBase {
    static typeName = "UpdateCustomerDetails";

    constructor() {
        super();
        this.makeObservable();
    }

    @Rules.Required()
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

    @Rules.StringLength(100)
    public phoneNumber: string = "";

    // Client only properties / methods

   addBusinessRules(rules: Validation.Rules<UpdateCustomerDetails>) {
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
            return "New update customer details";
        } else {
            return this.firstName;
        }
    }
}