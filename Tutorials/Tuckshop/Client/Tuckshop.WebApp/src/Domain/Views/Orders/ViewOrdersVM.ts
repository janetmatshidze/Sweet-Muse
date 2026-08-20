import { Views } from '@singularsystems/neo-react';
import { AppService, DomainTypes, Types } from '../../DomainTypes';
import OrderLookupCriteria from '../../Models/Orders/Queries/OrderLookupCriteria';
import OrderLookup from '../../Models/Orders/Queries/OrderLookup';
import { List } from '@singularsystems/neo-core';
import Order from '../../Models/Orders/Order';



export default class ViewOrdersVM extends Views.ViewModelBase {

    public criteria = new OrderLookupCriteria();
    public foundOrders = new List(OrderLookup);

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
        private ordersQueryApiClient = AppService.get(DomainTypes.ApiClients.OrdersQueryApiClient),
        private ordersCommandApiClient = AppService.get(DomainTypes.ApiClients.OrdersCommandApiClient)) {

        super(taskRunner);
        this.makeObservable();
    }

    public orders = new List(Order);

    
    public get filteredOrders() {
        let result = [...this.foundOrders];

        return result;
    }

     public readonly pageSize = 6;

    public currentPage: number = 1;

    public get totalPages() {

        return Math.max(
            1,
            Math.ceil(this.filteredOrders.length / this.pageSize)
        );
    }

     public get pagedOrders() {
        const start = (this.currentPage - 1) * this.pageSize;

        return this.filteredOrders.slice(start, start + this.pageSize);
    }

    public goToPage(page: number) {

        if (page < 1 || page > this.totalPages) {

            return;
        }

        this.currentPage = page;
    }

    public nextPage() {

        this.goToPage(this.currentPage + 1);
    }

    public previousPage() {

        this.goToPage(this.currentPage - 1);
    }

    public async initialise() {

    }

   public async findOrders() {
    const response = await this.taskRunner.waitFor(
        this.ordersQueryApiClient.getOrderLookupAsync(
            this.criteria.toQueryObject()
        )
    );

    this.foundOrders.set(response.data);
    this.currentPage = 1;
}

    public completeOrder(order: OrderLookup){
        this.taskRunner.run(async () => {
            await this.ordersCommandApiClient.completeOrder({orderId: order.orderId});
            (order as any).completedOn = new Date();
        })
    }

    public cancelOrder(order: OrderLookup, reason: string){
            this.taskRunner.run(async () => {
            await this.ordersCommandApiClient.cancelOrder({orderId: order.orderId, reason});
            (order as any).cancelledOn = new Date();
            (order as any).cancelledReason = reason;
        })  
    }
}