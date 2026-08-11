import React from "react";
import { Neo, Views } from "@singularsystems/neo-react";
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
            <div className="sweet-muse-create-order">

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

                                <div className="create-order-layout">

                                    <div className="products-section">

                                        <div className="products-section-header">
                                            <div>
                                                <h2>Products</h2>

                                                <p>
                                                    Select products to add to the order.
                                                </p>
                                            </div>
                                        </div>

                                        <div className="product-cards">

                                            {this.viewModel.products.map(product => (
                                                <div
                                                    className="product-card"
                                                    key={product.productId}
                                                >

                                                    <div className="product-image-wrapper">

                                                        <img
                                                            src={product.imageUrl}
                                                            alt={product.productName}
                                                            className="product-image"
                                                        />

                                                    </div>

                                                    <div className="product-card-content">

                                                        <div className="product-card-info">

                                                            <h3>
                                                                {product.productName}
                                                            </h3>

                                                            <p>
                                                                {product.description}
                                                            </p>

                                                        </div>

                                                        <div className="product-card-footer">

                                                            <span className="product-price">
                                                                R{Number(product.price).toFixed(2)}
                                                            </span>

                                                            <div className="product-card-actions">

                                                                <div className="product-quantity">

                                                                    <Neo.Button
                                                                        type="button"
                                                                        className="quantity-button"
                                                                        onClick={() =>
                                                                            this.viewModel.decreaseProductQuantity(
                                                                                product.productId
                                                                            )
                                                                        }
                                                                    >
                                                                        −
                                                                    </Neo.Button>

                                                                    <span className="quantity-value">
                                                                        {this.viewModel.productQuantities[
                                                                            product.productId
                                                                        ] || 1}
                                                                    </span>

                                                                    <Neo.Button
                                                                        type="button"
                                                                        className="quantity-button"
                                                                        onClick={() =>
                                                                            this.viewModel.increaseProductQuantity(
                                                                                product.productId
                                                                            )
                                                                        }
                                                                    >
                                                                        +
                                                                    </Neo.Button>

                                                                </div>

                                                                <Neo.Button
                                                                    type="button"
                                                                    className="add-to-cart-button"
                                                                    onClick={() =>
                                                                        this.viewModel.addToCart(product)
                                                                    }
                                                                >
                                                                    Add to Cart
                                                                </Neo.Button>

                                                            </div>

                                                        </div>

                                                    </div>

                                                </div>
                                            ))}

                                        </div>

                                    </div>

                                    <div className="shopping-cart">

                                        <div className="shopping-cart-header">

                                            <div>
                                                <h2>Your Order</h2>

                                                <p>
                                                    {this.viewModel.cartItemCount} items
                                                </p>
                                            </div>

                                            {order.orderDetails.length > 0 && (
                                                <span className="cart-item-count">
                                                    {order.orderDetails.length}
                                                </span>
                                            )}

                                        </div>

                                        {order.orderDetails.length === 0 ? (

                                            <div className="empty-cart">

                                                <div className="empty-cart-icon">
                                                    <Neo.Icon name="shopping_cart">

                                                    </Neo.Icon>
                                                </div>

                                                <h3>
                                                    Your cart is empty
                                                </h3>

                                                <p>
                                                    Add products to start creating this order.
                                                </p>

                                            </div>

                                        ) : (

                                            <div className="cart-items">

                                                {order.orderDetails.map(
                                                    (orderDetail, index) => {

                                                        const product =
                                                            this.viewModel.products.find(
                                                                item =>
                                                                    item.productId ===
                                                                    orderDetail.productId
                                                            );

                                                        return (
                                                            <div
                                                                className="cart-item"
                                                                key={
                                                                    orderDetail.productId ||
                                                                    index
                                                                }
                                                            >

                                                                <div className="cart-item-image">

                                                                    {product?.imageUrl && (
                                                                        <img
                                                                            src={product.imageUrl}
                                                                            alt={
                                                                                orderDetail.productName
                                                                            }
                                                                        />
                                                                    )}

                                                                </div>

                                                                <div className="cart-item-details">

                                                                    <div className="cart-item-top">

                                                                        <div>
                                                                            <h3>
                                                                                {
                                                                                    orderDetail.productName
                                                                                }
                                                                            </h3>

                                                                            <span>
                                                                                R
                                                                                {Number(
                                                                                    orderDetail.price
                                                                                ).toFixed(2)}
                                                                            </span>
                                                                        </div>

                                                                        <Neo.Button
                                                                            type="button"
                                                                            className="remove-item-button"
                                                                            onClick={() =>
                                                                                this.viewModel.removeFromCart(
                                                                                    orderDetail
                                                                                )
                                                                            }
                                                                        >
                                                                            <Neo.Icon name="close">

                                                                            </Neo.Icon>
                                                                        </Neo.Button>

                                                                    </div>

                                                                    <div className="cart-item-bottom">

                                                                        <div className="cart-quantity">

                                                                            <Neo.Button
                                                                                type="button"
                                                                                className="cart-quantity-button"
                                                                                onClick={() =>
                                                                                    this.viewModel.decreaseCartQuantity(
                                                                                        orderDetail
                                                                                    )
                                                                                }
                                                                            >
                                                                                −
                                                                            </Neo.Button>

                                                                            <span>
                                                                                {
                                                                                    orderDetail.quantity
                                                                                }
                                                                            </span>

                                                                            <Neo.Button
                                                                                type="button"
                                                                                className="cart-quantity-button"
                                                                                onClick={() =>
                                                                                    this.viewModel.increaseCartQuantity(
                                                                                        orderDetail
                                                                                    )
                                                                                }
                                                                            >
                                                                                +
                                                                            </Neo.Button>

                                                                        </div>

                                                                        <strong>
                                                                            R
                                                                            {Number(
                                                                                orderDetail.value || 0
                                                                            ).toFixed(2)}
                                                                        </strong>

                                                                    </div>

                                                                </div>

                                                            </div>
                                                        );
                                                    }
                                                )}

                                            </div>

                                        )}

                                        <div className="cart-summary">

                                            <div className="summary-row">
                                                <span>
                                                    Subtotal
                                                </span>

                                                <span>
                                                    R
                                                    {this.viewModel.cartTotal.toFixed(2)}
                                                </span>
                                            </div>

                                            <div className="summary-row total-row">

                                                <strong>
                                                    Total
                                                </strong>

                                                <strong>
                                                    R
                                                    {this.viewModel.cartTotal.toFixed(2)}
                                                </strong>

                                            </div>

                                        </div>

                                        <Neo.Button
                                            isSubmit
                                            size="lg"
                                            icon="storefront"
                                            className="place-order-button"
                                            disabled={
                                                order.orderDetails.length === 0
                                            }
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