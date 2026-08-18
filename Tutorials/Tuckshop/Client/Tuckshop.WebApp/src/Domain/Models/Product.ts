import { Attributes, ModelBase, Rules, Validation } from '@singularsystems/neo-core';

export default class Product extends ModelBase {
    static typeName = "Product";

    constructor() {
        super();
        this.makeObservable();
    }

    public productId: number = 0;

    @Rules.Required()
    @Rules.StringLength(100)
    public productName: string = "";

    @Rules.Required()
    @Rules.StringLength(250)
    public description: string = "";

    @Attributes.Integer()
    public stock: number = 0;

    public categoryId: number = 0;

    @Rules.Required()
    @Rules.StringLength(500)
    public imageUrl: string = "";

    @Attributes.Float()
    public price: number = 0;

    // Client only properties / methods

    addBusinessRules(rules: Validation.Rules<this>) {
        super.addBusinessRules(rules);

        rules.failWhen(p => p.price <= 0 , "Price must be above zero.");

    

    }

    public toString(): string {
        if (this.isNew || !this.productName) {
            return "New product";
        } else {
            return this.productName;
        }
    }
}