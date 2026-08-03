import { Attributes, LookupBase } from '@singularsystems/neo-core';

export default class OrderLookup extends LookupBase {

    constructor() {
        super();
        this.makeBindable();
    }

    public readonly orderId: number = 0;

    public readonly customerName: string = "";

    @Attributes.Date()
    public readonly orderedOn: Date = new Date();

    @Attributes.Date()
    public readonly completedOn: Date | null = null;

    @Attributes.Date()
    public readonly cancelledOn: Date | null = null;

    public readonly cancelledReason: string = "";

    public readonly completedBy: string = "";

    public readonly cancelledBy: string = "";

    @Attributes.Float()
    public readonly orderTotalExcl: number = 0;

    @Attributes.Float()
    public readonly orderTotal: number = 0;

    public readonly items: object | null = null;

    // Client only properties / methods
}