import { Views } from "@singularsystems/neo-react";
import { AppService, Types } from "../../DomainTypes";
import { CreateOrder } from "../../Models/Orders/Commands/CreateOrder";

export default class CreateOrderVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
        private appDataCache = AppService.get(Types.Domain.Services.DataCache),
        private ordersCommandApiClient = AppService.get(Types.Domain.ApiClients.OrdersCommandApiClient)
    ) {
        super(taskRunner);
        this.makeObservable();
    }

    public newOrder: CreateOrder | null = null;
    public products: any[] = [];
    public productQuantities: { [key: string]: number } = {};

    public async initialise() {
        await this.setupOrder();
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

    public submitOrder() {
        const orderData = this.newOrder!.toJSObject();

        this.taskRunner.run(async () => {
            await this.ordersCommandApiClient.createOrder(orderData);
            this.newOrder = null;
        });
    }
}