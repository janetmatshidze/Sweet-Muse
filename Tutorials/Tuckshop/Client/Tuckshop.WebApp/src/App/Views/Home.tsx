import * as React from 'react';
import { Neo, Views } from '@singularsystems/neo-react';
import { observer } from 'mobx-react';
import HighchartsReact from 'highcharts-react-official';
import Highcharts from 'highcharts';
import DashboardVM from '../../Domain/Views/DashboardVM';

class HomeParams { }

@observer
export default class Home extends Views.ViewBase<DashboardVM, HomeParams> {
    public static params = new HomeParams();

    constructor(props: unknown) {
        super("Dashboard", DashboardVM, props);
    }

    protected viewParamsUpdated() { }

    public render() {

        const overview = this.viewModel.salesOverview;

        const overviewOptions: Highcharts.Options = {
            chart: { type: 'areaspline', height: 300, backgroundColor: 'transparent' },
            title: { text: undefined },
            xAxis: {
                categories: overview.categories,
                lineColor: '#eee',
                tickLength: 0
            },
            yAxis: {
                title: { text: undefined },
                gridLineColor: '#f0ece4',
                labels: {
                    formatter: function () {
                        return 'R' + (Number(this.value) / 1000) + 'K';
                    }
                }
            },
            tooltip: { shared: true, valuePrefix: 'R' },
            credits: { enabled: false },
            legend: { enabled: false },
            plotOptions: {
                areaspline: { marker: { enabled: false } }
            },
            series: [
                {
                    type: 'areaspline',
                    name: overview.previousLabel,
                    data: overview.previous,
                    color: '#c9c2b6',
                    dashStyle: 'Dot',
                    fillOpacity: 0,
                    lineWidth: 1.5
                },
                {
                    type: 'areaspline',
                    name: overview.currentLabel,
                    data: overview.current,
                    color: '#c0564c',
                    fillColor: {
                        linearGradient: { x1: 0, y1: 0, x2: 0, y2: 1 },
                        stops: [
                            [0, 'rgba(192,86,76,0.25)'],
                            [1, 'rgba(192,86,76,0)']
                        ]
                    },
                    lineWidth: 2.5
                }
            ]
        };

        const paymentSplitOptions: Highcharts.Options = {
            chart: { type: 'pie', height: 220, backgroundColor: 'transparent' },
            title: { text: undefined },
            tooltip: {
                pointFormat: 'R{point.y:.2f} ({point.percentage:.1f}%)'
            },
            credits: { enabled: false },
            legend: { enabled: false },
            plotOptions: {
                pie: {
                    innerSize: '65%',
                    dataLabels: { enabled: false },
                    borderWidth: 0
                }
            },
            series: [{
                type: 'pie',
                name: 'Sales',
                data: [
                    { name: 'Cash', y: this.viewModel.paymentMethodSplit[0].y, color: '#c0564c' },
                    { name: 'Wallet', y: this.viewModel.paymentMethodSplit[1].y, color: '#e0b8a8' }
                ]
            }]
        };

        const topCustomers = this.viewModel.topCustomers;

        const topCustomersOptions: Highcharts.Options = {
            chart: { type: 'bar', height: Math.max(220, topCustomers.length * 38), backgroundColor: 'transparent' },
            title: { text: undefined },
            xAxis: {
                categories: topCustomers.map(c => c.name),
                lineColor: '#eee',
                labels: { style: { fontSize: '0.8rem' } }
            },
            yAxis: {
                title: { text: undefined },
                gridLineColor: '#f0ece4',
                labels: {
                    formatter: function () {
                        return 'R' + (Number(this.value) / 1000) + 'K';
                    }
                }
            },
            tooltip: { valuePrefix: 'R' },
            credits: { enabled: false },
            legend: { enabled: false },
            plotOptions: {
                bar: {
                    color: '#c0564c',
                    borderRadius: 4,
                    borderWidth: 0
                }
            },
            series: [{
                type: 'bar',
                name: 'Sales',
                data: topCustomers.map(c => c.total)
            }]
        };

        return (
            <div className="sweet-muse mt-3">
                <h1 className="title mb-3">Dashboard</h1>

                <Neo.GridLayout lg={3} className="mb-3">
                    <Neo.Card title="Sales today">
                        <div className="dashboard-stat">
                            R{this.viewModel.totalSalesToday.toFixed(2)}
                        </div>
                        <div className="dashboard-stat-sub">
                            {this.viewModel.orderCountToday} orders
                        </div>
                    </Neo.Card>

                    <Neo.Card title="Sales this month">
                        <div className="dashboard-stat">
                            R{this.viewModel.totalSalesMonth.toFixed(2)}
                        </div>
                        <div className="dashboard-stat-sub">
                            {this.viewModel.orderCountMonth} orders
                        </div>
                    </Neo.Card>

                    <Neo.Card title="Average order value">
                        <div className="dashboard-stat">
                            R{this.viewModel.averageOrderValue.toFixed(2)}
                        </div>
                        <div className="dashboard-stat-sub">
                            {this.viewModel.totalOrdersAllTime} orders all-time
                        </div>
                    </Neo.Card>
                </Neo.GridLayout>

                <div className="dashboard-row mb-3">

                    <Neo.Card className="sales-overview-card">

                        <div className="sales-overview-header">
                            <h2>Sales Overview</h2>

                            <div className="period-dropdown-wrapper">
                                <button
                                    type="button"
                                    className="period-dropdown-btn"
                                    onClick={() => this.viewModel.togglePeriodDropdown()}
                                >
                                    {this.viewModel.periodLabel(this.viewModel.selectedPeriod)}
                                    <Neo.Icon name="expand_more" />
                                </button>

                                {this.viewModel.showPeriodDropdown && (
                                    <div className="period-dropdown-menu">
                                        <button type="button" onClick={() => this.viewModel.setPeriod('day')}>
                                            Today
                                        </button>
                                        <button type="button" onClick={() => this.viewModel.setPeriod('week')}>
                                            This Week
                                        </button>
                                        <button type="button" onClick={() => this.viewModel.setPeriod('month')}>
                                            This Month
                                        </button>
                                    </div>
                                )}
                            </div>

                        </div>

                        <div className="sales-overview-chart-wrapper">
                            <HighchartsReact highcharts={Highcharts} options={overviewOptions} />
                        </div>

                        <div className="sales-overview-legend">
                            <span className="legend-item">
                                <span className="legend-swatch solid" />
                                {overview.currentLabel}
                            </span>

                            <span className="legend-item">
                                <span className="legend-swatch dotted" />
                                {overview.previousLabel}
                            </span>
                        </div>

                    </Neo.Card>

                    <Neo.Card title="Cash vs Wallet" className="payment-split-card">
                        <div className="payment-split-chart-wrapper">
                            <HighchartsReact highcharts={Highcharts} options={paymentSplitOptions} />

                            <div className="payment-split-center">
                                <span className="payment-split-total">
                                    R{this.viewModel.paymentMethodTotal.toFixed(0)}
                                </span>
                                <span className="payment-split-label">Total</span>
                            </div>
                        </div>

                        <div className="payment-split-legend">
                            <span className="legend-item">
                                <span className="legend-swatch cash" />
                                Cash
                            </span>

                            <span className="legend-item">
                                <span className="legend-swatch wallet" />
                                Wallet
                            </span>
                        </div>
                    </Neo.Card>

                </div>

                <Neo.Card title="Top 10 Customers" className="top-customers-card">
                    {topCustomers.length === 0 ? (
                        <div className="dashboard-empty-state">
                            No customer sales yet.
                        </div>
                    ) : (
                        <HighchartsReact highcharts={Highcharts} options={topCustomersOptions} />
                    )}
                </Neo.Card>

            </div>
        );
    }
}