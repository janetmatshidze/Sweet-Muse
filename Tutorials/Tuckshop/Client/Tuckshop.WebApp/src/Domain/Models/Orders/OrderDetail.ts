import { Attributes, Misc, ModelBase, Validation } from '@singularsystems/neo-core';
import Product from '../Product';

export default class OrderDetail extends ModelBase {
    static typeName = "OrderDetail";

    constructor() {
        super();
        this.makeObservable();
    }

    public orderDetailId: number = 0;

    @Attributes.Serialisation(Misc.SerialiseType.NotNull)
    public productId: number | null = null;

    @Attributes.ChildObject(Product)
    public product: Product | null = null;

    @Attributes.Integer()
    public quantity: number = 0;

    @Attributes.Float()
    public value: number = 0;

    @Attributes.Float()
    public vat: number = 0;

    // Client only properties / methods

    addBusinessRules(rules: Validation.Rules<this>) {
        super.addBusinessRules(rules);

        rules.failWhen(
          od => od.quantity > 0 && od.product != null && od.quantity > od.product.stock,
          od => `Insufficient stock for ${od.product?.productName}. Available stock: ${od.product?.stock}.`
        );
    }

    public toString(): string {
        if (this.isNew) {
            return "New order detail";
        } else {
            return "Order Detail";
        }
    }
}