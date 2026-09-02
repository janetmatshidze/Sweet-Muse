import React from 'react';
import { Neo, NeoGrid, Views } from '@singularsystems/neo-react';
import ViewOrdersVM from './ViewOrdersVM';
import { observer } from 'mobx-react';
import { Data, Misc, ModalUtils } from '@singularsystems/neo-core';
import { OrderStatus } from '../../Models/Orders/Enums/OrderStatus';
import OrderLookup from '../../Models/Orders/Queries/OrderLookup';
import CancelOrder from '../../Models/Orders/Commands/CancelOrder';
import Pagination from '../../../App/Components/Pagination';

class ViewOrdersParams {
}

@observer
export default class ViewOrdersView extends Views.ViewBase<
    ViewOrdersVM,
    ViewOrdersParams
> {
    public static params = new ViewOrdersParams();

    constructor(props: unknown) {
        super("View Orders", ViewOrdersVM, props);
    }

    protected viewParamsUpdated() {
    }

    public render() {

        const pagedOrders = this.viewModel.pagination.pagedItems;
        const emptySlots = this.viewModel.pagination.pageSize - pagedOrders.length;

        return (
            <div className="sweet-muse-orders">

                <Neo.Card className="orders-search-card">
                    <div className="orders-section-heading">
                        <div className="orders-heading-icon">
                            <span className="orders-heading-symbol">
                                <Neo.Icon name="search"/>
                            </span>
                        </div>

                        <div>
                            <h2>Search Orders</h2>
                            <p>Filter orders using the criteria below.</p>
                        </div>
                    </div>

                    <Neo.Form
                        model={this.viewModel.criteria}
                        onSubmit={() => this.viewModel.findOrders()}
                    >
                        {(criteria, criteriaMeta) => (
                            <div className="orders-search-grid">

                                <Neo.FormGroup
                                    bind={criteriaMeta.orderStatus}
                                    select={{
                                        itemSource:
                                            Data.StaticDataSource.fromEnum(
                                                OrderStatus
                                            )
                                    }}
                                />

                                <Neo.FormGroup
                                    bind={criteriaMeta.startDate}
                                />

                                <Neo.FormGroup
                                    bind={criteriaMeta.endDate}
                                />

                                <Neo.Button
                                    variant="primary"
                                    className=" orders-search-btn"
                                    isSubmit
                                >
                                    <Neo.Icon
                                    name="search"
                                    />
                                    Search
                                </Neo.Button>

                            </div>
                        )}
                    </Neo.Form>
                </Neo.Card>


                    <div className="table-grid">

                        <NeoGrid.Grid
                            items={pagedOrders}
                        >
                            {(order, orderMeta) => (
                                <NeoGrid.RowGroup
                                    expandProperty={orderMeta.isExpanded}
                                >

                                    <NeoGrid.Row>

                                        <NeoGrid.Column
                                            className="order-customer-column"
                                            display={orderMeta.customerName}
                                        />

                                        <NeoGrid.Column
                                            className="order-date-column"
                                            display={orderMeta.orderedOn}
                                            dateProps={{
                                                formatString: "dd MMM yyyy - HH:mm"
                                            }}
                                        />
                                        
                                         {this.viewModel.criteria.orderStatus !== OrderStatus.Cancelled && (
                                         <NeoGrid.Column
                                            headers="Completed On"
                                            className="order-date-column"
                                            display={orderMeta.completedOn}
                                            dateProps={{
                                                formatString: "dd MMM yyyy - HH:mm"}}
                                        />
                                         )}
                                                

                                         {this.viewModel.criteria.orderStatus !== OrderStatus.Completed && (
                                         <NeoGrid.Column
                                            headers="Cancelled On"
                                            className="order-date-column"
                                            display={orderMeta.cancelledOn}
                                            dateProps={{
                                                formatString: "dd MMM yyyy - HH:mm"}}
                                        />
                                         )}

                                        <NeoGrid.Column
                                            className="order-total-column"
                                            display={orderMeta.orderTotal}
                                            numProps={{
                                                format:
                                                    Misc.NumberFormat
                                                        .CurrencyDecimals
                                            }}
                                        />

                                        <NeoGrid.ButtonColumn
                                            className="order-actions-column"
                                        >
                                            {order.canAction && (
                                                <div className="order-actions">

                                                    <Neo.Button
                                                        variant="danger"
                                                        icon="times"
                                                        className="cancel-order-btn"
                                                        onClick={() =>
                                                            this.cancelOrder(order)
                                                        }
                                                    >
                                                        Cancel
                                                    </Neo.Button>

                                                    <Neo.Button
                                                        variant="success"
                                                        icon="check"
                                                        className="complete-order-btn"
                                                        onClick={() =>
                                                            this.completeOrder(order)
                                                        }
                                                    >
                                                        Complete
                                                    </Neo.Button>

                                                </div>
                                            )}
                                        </NeoGrid.ButtonColumn>

                                    </NeoGrid.Row>

                                    <NeoGrid.ChildRow>


                                            <NeoGrid.Grid
                                                items={order.items}
                                            >
                                                {(orderDetail, orderDetailMeta) => (
                                                    <NeoGrid.Row>

                                                        <NeoGrid.Column
                                                            className="order-product-column"
                                                            display={
                                                                orderDetailMeta.product
                                                            }
                                                        />

                                                        <NeoGrid.Column
                                                            className="order-vat-column"
                                                            display={
                                                                orderDetailMeta.vat
                                                            }
                                                        />

                                                        <NeoGrid.Column
                                                            className="order-value-column"
                                                            display={
                                                                orderDetailMeta.value
                                                            }
                                                        />

                                                    </NeoGrid.Row>
                                                )}
                                            </NeoGrid.Grid>


                                    </NeoGrid.ChildRow>

                                </NeoGrid.RowGroup>
                            )}
                        </NeoGrid.Grid>

                         {pagedOrders.length === 0 && (
                                <tr className="filler-row">
                                    <td colSpan={6} className="message">
                                        No Orders found
                                    </td>
                                </tr>
                            )}

                        {emptySlots > 0 && (
                            <div className="orders-empty-slots">
                                {Array.from({ length: emptySlots }).map((_, i) => (
                                    <div key={i} className="order-row-placeholder" />
                                ))}
                            </div>
                        )}

                    </div>

                   


               <Pagination
                    currentPage={this.viewModel.pagination.currentPage}
                    totalPages={this.viewModel.pagination.totalPages}
                    onNext={() => this.viewModel.pagination.nextPage()}
                    onPrevious={() => this.viewModel.pagination.previousPage()}
                    onPageSelect={(page) => this.viewModel.pagination.currentPage = page}

                />
            </div>
        );
    }

    public completeOrder(order: OrderLookup) {
        ModalUtils.showYesNo(
            "Complete order",
            "Are you sure you want to complete this order?",
            () => this.viewModel.completeOrder(order)
        );
    }

    public async cancelOrder(order: OrderLookup) {
        const cancelInfo = new CancelOrder();

        if (
            (await ModalUtils.showOkCancel(
                "Cancel order",
                <Neo.FormGroup
                    bind={cancelInfo.meta.reason}
                    label="Please enter a reason:"
                />,
                cancelInfo
            )) === Misc.ModalResult.Yes
        ) {
            this.viewModel.cancelOrder(
                order,
                cancelInfo.reason
            );
        }
    }
}