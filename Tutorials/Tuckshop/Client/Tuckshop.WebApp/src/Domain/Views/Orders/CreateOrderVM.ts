import { Views } from "@singularsystems/neo-react";
import { AppService, Types } from "../../DomainTypes";
import { CreateOrder } from "../../Models/Orders/Commands/CreateOrder";
import { List } from "@singularsystems/neo-core";
import Customer from "../../Models/Customers/Customer";
import PaginationHelper from "../../../App/Models/Helpers/PaginationHelper";

export default class CreateOrderVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
        private appDataCache = AppService.get(Types.Domain.Services.DataCache),
        private ordersCommandApiClient = AppService.get(Types.Domain.ApiClients.OrdersCommandApiClient),
        private customersApiClient = AppService.get(Types.Domain.ApiClients.CustomersApiClient)

    ) {
        super(taskRunner);
        this.makeObservable();
    }

    public newOrder: CreateOrder | null = null;
    public products: any[] = [];
    public productQuantities: { [key: string]: number } = {};
    public customers = new List(Customer);
    public pagination = new PaginationHelper(() => this.products, 6);


    public async initialise() {
        await this.setupOrder();

        const customersResponse = await this.taskRunner.waitFor(
            this.customersApiClient.get()
        );

        this.customers.set(customersResponse.data);
    }

    public get selectedCustomer() {
        if(!this.newOrder || this.newOrder.isCashSale || !this.newOrder.customerId) {
            return null;
        }

        return this.customers.find(c => c.customerId === this.newOrder!.customerId) ?? null;
    }

    public get walletShortfall() {
        if(!this.selectedCustomer) {
            return 0;
        }
        
        const shortfall = this.cartTotal - this.selectedCustomer.walletBalance;

        return shortfall > 0 ? shortfall : 0;
    }

    public async setupOrder() {
        const newOrder = new CreateOrder();

        const products = await this.taskRunner.waitFor(
            this.appDataCache.products.getDataAsync()
        );

        this.products = products;
        this.productQuantities = {};

        for (const product of products) {
            this.productQuantities[product.productId] = 1;
        }

        this.newOrder = newOrder;
    }

    //  private getProduct(productId: number) {
    //     return this.products.find(p => p.productId === productId);
    // }

    // private getCartQuantity(productId: number) {
    //     const existing = this.newOrder?.orderDetails.find(
    //         detail => detail.productId === productId
    //     );
        
    //     return existing ? existing.quantity : 0;
    // }

    public increaseProductQuantity(productId: number) {
        const currentQuantity = this.productQuantities[productId] || 1;


        this.productQuantities = {
            ...this.productQuantities,
            [productId]: currentQuantity + 1
        };
    }

    public decreaseProductQuantity(productId: number) {
        const currentQuantity = this.productQuantities[productId] || 1;

        if (currentQuantity > 1) {
            this.productQuantities = {
                ...this.productQuantities,
                [productId]: currentQuantity - 1
            };
        }
    }

    public addToCart(product: any) {
        if (!this.newOrder) {
            return;
        }

        const quantity = this.productQuantities[product.productId] || 1;

        const existingOrderDetail = this.newOrder.orderDetails.find(
            detail => detail.productId === product.productId
        );

        if (existingOrderDetail) {
            existingOrderDetail.quantity += quantity;
        } else {
            const orderDetail = this.newOrder.orderDetails.addNew();

            orderDetail.productId = product.productId;
            orderDetail.productName = product.productName;
            orderDetail.price = product.price;
            orderDetail.quantity = quantity;
        }

        this.productQuantities = {
            ...this.productQuantities,
            [product.productId]: 1
        };
    }


    public increaseCartQuantity(orderDetail: any) {
        orderDetail.quantity += 1;
    }

    public decreaseCartQuantity(orderDetail: any) {
        if (orderDetail.quantity > 1) {
            orderDetail.quantity -= 1;
        } else {
            this.removeFromCart(orderDetail);
        }
    }

    public removeFromCart(orderDetail: any) {
        this.newOrder?.orderDetails.remove(orderDetail);
    }

    public get cartTotal() {
        return this.newOrder?.orderDetails.reduce(
            (total, detail) => total + (detail.value || 0),
            0
        ) || 0;
    }

    public get cartItemCount() {
        return this.newOrder?.orderDetails.reduce(
            (total, detail) => total + (detail.quantity || 0),
            0
        ) || 0;
    }

    public showInsufficientFundsModal: boolean = false;

   public submitOrder() {
    const isWalletOrder = this.newOrder && !this.newOrder.isCashSale;

    if (isWalletOrder && this.walletShortfall > 0) {
        this.showInsufficientFundsModal = true;
        return;
    }

    const orderData = this.newOrder!.toJSObject();

    this.taskRunner.run(async () => {
        await this.ordersCommandApiClient.createOrder(orderData);
    this.notifications.addSuccess("Order created", "The order was placed successfully.", 5);
    this.newOrder = null;
    }).catch(() => {
        // taskRunner already showed its own toast for the error.
    });
}

public closeInsufficientFundsModal() {
    this.showInsufficientFundsModal = false;
}
}