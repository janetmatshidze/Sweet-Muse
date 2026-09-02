import { Views } from '@singularsystems/neo-react';
import { AppService, DomainTypes, Types } from '../../DomainTypes';
import OrderLookupCriteria from '../../Models/Orders/Queries/OrderLookupCriteria';
import OrderLookup from '../../Models/Orders/Queries/OrderLookup';
import { List } from '@singularsystems/neo-core';
import Order from '../../Models/Orders/Order';
import PaginationHelper from '../../../App/Models/Helpers/PaginationHelper';



export default class ViewOrdersVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
        private ordersQueryApiClient = AppService.get(DomainTypes.ApiClients.OrdersQueryApiClient),
        private ordersCommandApiClient = AppService.get(DomainTypes.ApiClients.OrdersCommandApiClient)) {

        super(taskRunner);
        this.makeObservable();
    }

    public criteria = new OrderLookupCriteria();

    public foundOrders = new List(OrderLookup);

    public orders = new List(Order);

    public pagination = new PaginationHelper(() => this.filteredOrders, 6);


    public get filteredOrders() {
        let result = [...this.foundOrders];

        return result;
    }



    public async initialise() {
      await this.findOrders();
    }

    public async findOrders() {
        const response = await this.taskRunner.waitFor(
            this.ordersQueryApiClient.getOrderLookupAsync(
                this.criteria.toQueryObject()
            )
        );

        this.foundOrders.set(response.data);
        this.pagination.currentPage = 1;
    }

    public completeOrder(order: OrderLookup) {
        this.taskRunner.run(async () => {
            await this.ordersCommandApiClient.completeOrder({ orderId: order.orderId });
            (order as any).completedOn = new Date();
        })
    }

    public cancelOrder(order: OrderLookup, reason: string) {
        this.taskRunner.run(async () => {
            await this.ordersCommandApiClient.cancelOrder({ orderId: order.orderId, reason });
            (order as any).cancelledOn = new Date();
            (order as any).cancelledReason = reason;
        })
    }
}