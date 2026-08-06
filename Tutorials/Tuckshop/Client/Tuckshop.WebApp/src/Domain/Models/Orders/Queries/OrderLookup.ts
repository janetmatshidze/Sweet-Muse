import { Attributes, List, LookupBase } from '@singularsystems/neo-core';
import OrderDetailLookup from './OrderDetailLookup';
// import OrderDetailLookup from './OrderDetailLookup';

export default class OrderLookup extends LookupBase {

    constructor() {
        super();
        this.makeBindable();
    }

    public readonly orderId: number = 0;

    public readonly customerName: string = "";

    @Attributes.Date()
    public readonly orderedOn: Date = new Date();
    
    @Attributes.Observable()
    @Attributes.Date()
    public readonly completedOn: Date | null = null;

    @Attributes.Observable()
    @Attributes.Date()
    public readonly cancelledOn: Date | null = null;

    public readonly cancelledReason: string = "";

    public readonly completedBy: string = "";

    public readonly cancelledBy: string = "";

    @Attributes.Float()
    public readonly orderTotalExcl: number = 0;

    @Attributes.Float()
    public readonly orderTotal: number = 0;

    public readonly items = new List(OrderDetailLookup);

    public readonly completedByFirstName: string | null = null;

    public readonly completedByLastName: string | null = null;

    public readonly cancelledByFirstName: string | null = null;

    public readonly cancelledByLastName: string | null = null;

    public get canAction(){
        return !this.completedOn && !this.cancelledOn;
    }
    // Client only properties / methods

    @Attributes.Observable()
    public isExpanded = false; // This property is used to control the expansion of the order details in the UI. It is not persisted to the server and is only relevant for the client-side view.
}