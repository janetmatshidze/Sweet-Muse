import React from "react";
import { Neo, NeoGrid, Views } from "@singularsystems/neo-react";
import CreateOrderVM from "./CreateOrderVM";
import { observer } from "mobx-react";
import Link from "@singularsystems/neo-react/dist/ReactComponents/Link";
import { viewOrdersRoute } from "../../DomainRoutes";

class CreateOrderParams {
}

@observer
export default class CreateOrderView extends Views.ViewBase<
    CreateOrderVM,
    CreateOrderParams
> {
    public static params = new CreateOrderParams();

    constructor(props: unknown) {
        super("Create Order", CreateOrderVM, props);
    }

    protected viewParamsUpdated() {
    }

    public render() {
        return (
            <div className="sweet-muse-create-order mt-3">

                {this.viewModel.newOrder && (
                    <Neo.Form
                        model={this.viewModel.newOrder}
                        showSummaryModal
                        onSubmit={() => this.viewModel.submitOrder()}
                    >
                        {(order, orderMeta) => (
                            <div>

                                <div className="create-order-header">
                                    <div>
                                        <h1 className="create-order-title">
                                            Create Order
                                        </h1>

                                        <p className="create-order-subtitle">
                                            Create a new order for a customer.
                                        </p>
                                    </div>
                                </div>

                                <div className="customer-card">

                                    <div className="customer-card-heading">
                                        Customer Information
                                    </div>

                                    <Neo.FormGroup
                                        bind={orderMeta.customerName}
                                    />

                                    <p className="customer-helper">
                                        Enter the customer's name to create a new order.
                                    </p>

                                </div>

                                <div className="order-products-card">

                                    <div className="order-products-header">
                                        <div>
                                            <h2>Products</h2>
                                            <p>
                                                Select the quantity of each product for this order.
                                            </p>
                                        </div>
                                    </div>

                                    <div className="order-products-grid">

                                        <NeoGrid.Grid items={order.orderDetails}>

                                            {(orderDetail, orderDetailMeta) => (
                                                <NeoGrid.Row>

                                                    <NeoGrid.Column label="Product">
                                                        <div className="order-product">

                                                            <div className="order-product-name">
                                                                {orderDetail.productName}
                                                            </div>

                                                        </div>
                                                    </NeoGrid.Column>

                                                    <NeoGrid.Column
                                                        label="Price"
                                                        display={orderDetailMeta.price}
                                                    />

                                                    <NeoGrid.Column label="Quantity">
                                                        <div className="quantity-control">

                                                            <Neo.Button
                                                                type="button"
                                                                className="quantity-button"
                                                                onClick={() => {
                                                                    if (orderDetail.quantity > 0) {
                                                                        orderDetail.quantity =
                                                                            orderDetail.quantity - 1;
                                                                    }
                                                                }}
                                                            >
                                                                −
                                                            </Neo.Button>

                                                            <span className="quantity-value">
                                                                {orderDetail.quantity}
                                                            </span>

                                                            <Neo.Button
                                                                type="button"
                                                                className="quantity-button"
                                                                onClick={() => {
                                                                    orderDetail.quantity =
                                                                        orderDetail.quantity + 1;
                                                                }}
                                                            >
                                                                +
                                                            </Neo.Button>

                                                        </div>
                                                    </NeoGrid.Column>

                                                    <NeoGrid.Column
                                                        label="Subtotal"
                                                        display={orderDetailMeta.value}
                                                    />

                                                </NeoGrid.Row>
                                            )}

                                        </NeoGrid.Grid>

                                    </div>

                                    <div className="order-total">

                                        <span className="order-total-label">
                                            Total
                                        </span>

                                        <span className="order-total-value">
                                            {order.orderDetails
                                                .reduce(
                                                    (total, detail) =>
                                                        total + (detail.value || 0),
                                                    0
                                                )
                                                .toFixed(2)}
                                        </span>

                                    </div>

                                    <div className="place-order-section">

                                        <Neo.Button
                                            isSubmit
                                            size="lg"
                                            icon="storefront"
                                            className="place-order-button"
                                        >
                                            Place Order
                                        </Neo.Button>

                                    </div>

                                </div>

                            </div>
                        )}

                    </Neo.Form>
                )}

                {!this.viewModel.newOrder && (
                    <Neo.Alert
                        variant="success"
                        heading="Order submitted"
                        animateInitial
                        className="mt-4"
                    >
                        Your order has been submitted,{" "}
                        <Link to={viewOrdersRoute.path}>
                            view your orders here
                        </Link>
                        , or{" "}
                        <Neo.Button
                            variant="link"
                            className="btn-link-inline"
                            onClick={() =>
                                this.viewModel.setupOrder()
                            }
                        >
                            create another order
                        </Neo.Button>
                        .
                    </Neo.Alert>
                )}

            </div>
        );
    }
}