import { Views } from '@singularsystems/neo-react';
import { AppService, Types } from '../DomainTypes';
import { List } from '@singularsystems/neo-core';
import Order from '../Models/Orders/Order';

export default class DashboardVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private ordersApiClient = AppService.get(Types.Domain.ApiClients.OrdersApiClient)
    ) {
        super(taskRunner);
        this.makeObservable();
    }

    public orders = new List(Order);

    public async initialise() {
        const response = await this.taskRunner.waitFor(
            this.ordersApiClient.get()
        );

        this.orders.set(response.data);
    }

    // Helpers 

    private isSameDay(date: Date, other: Date) {
        return date.getFullYear() === other.getFullYear() &&
            date.getMonth() === other.getMonth() &&
            date.getDate() === other.getDate();
    }

    private isSameMonth(date: Date, other: Date) {
        return date.getFullYear() === other.getFullYear() &&
            date.getMonth() === other.getMonth();
    }

    private startOfDay(date: Date) {
        const d = new Date(date);
        d.setHours(0, 0, 0, 0);
        return d;
    }

    private startOfWeek(date: Date) {
        // Monday as the start of the week
        const d = this.startOfDay(date);
        const day = d.getDay(); // 0 = Sunday
        const diff = (day === 0 ? -6 : 1) - day;
        d.setDate(d.getDate() + diff);
        return d;
    }

    private addDays(date: Date, days: number) {
        const d = new Date(date);
        d.setDate(d.getDate() + days);
        return d;
    }

    private isCancelled(order: Order) {
        return !!order.cancelled?.on;
    }

    // OrderDetail.value is already the line total (qty x unit price at time of sale).
    private orderTotal(order: Order) {
        return order.orderDetails.reduce(
            (sum, detail) => sum + (detail.value || 0),
            0
        );
    }

    private get activeOrders() {
        return this.orders.filter(o => !this.isCancelled(o) && o.orderedOn);
    }

    // Total sales today 

    public get totalSalesToday() {
        const today = new Date();

        return this.activeOrders
            .filter(o => this.isSameDay(new Date(o.orderedOn!), today))
            .reduce((sum, o) => sum + this.orderTotal(o), 0);
    }

    public get orderCountToday() {
        const today = new Date();

        return this.activeOrders
            .filter(o => this.isSameDay(new Date(o.orderedOn!), today))
            .length;
    }

    // Sales overview (day / week / month toggle) 

    public selectedPeriod: 'day' | 'week' | 'month' = 'week';

    public showPeriodDropdown: boolean = false;

    public togglePeriodDropdown() {
        this.showPeriodDropdown = !this.showPeriodDropdown;
    }

    public setPeriod(period: 'day' | 'week' | 'month') {
        this.selectedPeriod = period;
        this.showPeriodDropdown = false;
    }

    public periodLabel(period: 'day' | 'week' | 'month') {
        switch (period) {
            case 'day': return 'Today';
            case 'month': return 'This Month';
            case 'week':
            default: return 'This Week';
        }
    }

    // Hourly, today vs yesterday.
    private get dailyOverview() {
        const today = this.startOfDay(new Date());
        const yesterday = this.addDays(today, -1);

        const current = new Array(24).fill(0);
        const previous = new Array(24).fill(0);

        this.activeOrders.forEach(o => {
            const orderedOn = new Date(o.orderedOn!);
            const hour = orderedOn.getHours();

            if (this.isSameDay(orderedOn, today)) {
                current[hour] += this.orderTotal(o);
            } else if (this.isSameDay(orderedOn, yesterday)) {
                previous[hour] += this.orderTotal(o);
            }
        });

        return {
            categories: Array.from({ length: 24 }, (_, h) => `${h}:00`),
            current,
            previous,
            currentLabel: "Today",
            previousLabel: "Yesterday"
        };
    }

    // Mon-Sun, this week vs last week. 
    private get weeklyOverview() {
        const thisWeekStart = this.startOfWeek(new Date());
        const lastWeekStart = this.addDays(thisWeekStart, -7);

        const current = new Array(7).fill(0);
        const previous = new Array(7).fill(0);

        this.activeOrders.forEach(o => {
            const orderedOn = this.startOfDay(new Date(o.orderedOn!));

            const daysFromThisStart = Math.round(
                (orderedOn.getTime() - thisWeekStart.getTime()) / 86400000
            );
            const daysFromLastStart = Math.round(
                (orderedOn.getTime() - lastWeekStart.getTime()) / 86400000
            );

            if (daysFromThisStart >= 0 && daysFromThisStart < 7) {
                current[daysFromThisStart] += this.orderTotal(o);
            } else if (daysFromLastStart >= 0 && daysFromLastStart < 7) {
                previous[daysFromLastStart] += this.orderTotal(o);
            }
        });

        return {
            categories: ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
            current,
            previous,
            currentLabel: "This week",
            previousLabel: "Last week"
        };
    }

    // Day-of-month, this month vs last month. This is "total sales by day for the month".
    private get monthlyOverview() {
        const today = new Date();
        const thisMonthStart = new Date(today.getFullYear(), today.getMonth(), 1);
        const lastMonthStart = new Date(today.getFullYear(), today.getMonth() - 1, 1);

        const daysInThisMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0).getDate();
        const daysInLastMonth = new Date(today.getFullYear(), today.getMonth(), 0).getDate();

        const maxDays = Math.max(daysInThisMonth, daysInLastMonth);

        const current: (number | null)[] = new Array(maxDays).fill(null);
        const previous: (number | null)[] = new Array(maxDays).fill(null);

        for (let i = 0; i < daysInThisMonth; i++) current[i] = 0;
        for (let i = 0; i < daysInLastMonth; i++) previous[i] = 0;

        this.activeOrders.forEach(o => {
            const orderedOn = new Date(o.orderedOn!);

            if (this.isSameMonth(orderedOn, thisMonthStart)) {
                const day = orderedOn.getDate() - 1;
                current[day] = (current[day] || 0) + this.orderTotal(o);
            } else if (this.isSameMonth(orderedOn, lastMonthStart)) {
                const day = orderedOn.getDate() - 1;
                previous[day] = (previous[day] || 0) + this.orderTotal(o);
            }
        });

        return {
            categories: Array.from({ length: maxDays }, (_, i) => `${i + 1}`),
            current,
            previous,
            currentLabel: "This month",
            previousLabel: "Last month"
        };
    }

    public get salesOverview() {
        switch (this.selectedPeriod) {
            case 'day': return this.dailyOverview;
            case 'month': return this.monthlyOverview;
            case 'week':
            default: return this.weeklyOverview;
        }
    }

    public get paymentMethodSplit() {

        let cash = 0;
        let wallet = 0;

        this.activeOrders.forEach(o => {
            if (o.isCashSale) {
                cash += this.orderTotal(o);
            } else {
                wallet += this.orderTotal(o);
            }
        });

        return [
            { name: "Cash", y: cash },
            { name: "Wallet", y: wallet }
        ];
    }

    public get paymentMethodTotal() {
        return this.paymentMethodSplit.reduce((sum, s) => sum + s.y, 0);
    }

    public get topCustomers() {

        const totalsByCustomer = new Map<string, number>();

        this.activeOrders
        .filter(o => !o.isCashSale) // cash sales arent linked to a real customer.
        .forEach(o => {
            const name = o.customerName || "Unknown";

            const existing = totalsByCustomer.get(name) ?? 0;

            totalsByCustomer.set(name, existing + this.orderTotal(o));
        });

        return Array.from(totalsByCustomer.entries())
            .map(([name, total]) => ({ name, total }))
            .sort((a, b) => b.total - a.total)
            .slice(0, 10);
    }

    // ---------- Sales this month ----------

    public get totalSalesMonth() {
        const today = new Date();

        return this.activeOrders
            .filter(o => this.isSameMonth(new Date(o.orderedOn!), today))
            .reduce((sum, o) => sum + this.orderTotal(o), 0);
    }

    public get orderCountMonth() {
        const today = new Date();

        return this.activeOrders
            .filter(o => this.isSameMonth(new Date(o.orderedOn!), today))
            .length;
    }

    // ---------- Total orders (all time) ----------

    public get totalOrdersAllTime() {
        return this.activeOrders.length;
    }

    public get averageOrderValue() {
        if (this.activeOrders.length === 0) {
            return 0;
        }

        const total = this.activeOrders.reduce((sum, o) => sum + this.orderTotal(o), 0);

        return total / this.activeOrders.length;
    }
}