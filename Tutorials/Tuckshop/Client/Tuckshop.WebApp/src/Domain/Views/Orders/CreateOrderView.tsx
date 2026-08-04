import React from 'react';
import { Neo, NeoGrid, Views } from '@singularsystems/neo-react';
import CreateOrderVM from './CreateOrderVM';
import { observer } from 'mobx-react';
import Link from '@singularsystems/neo-react/dist/ReactComponents/Link';

class CreateOrderParams {
    // TODO: Add parameters here in the form: public paramName = { isQuery?: boolean, required?: boolean };
}

@observer
export default class CreateOrderView extends Views.ViewBase<CreateOrderVM, CreateOrderParams> {
    public static params = new CreateOrderParams();

    constructor(props: unknown) {
        super("Create Order", CreateOrderVM, props);
    }

    protected viewParamsUpdated() {

    }
    // conditional rendering of the form based on whether newOrder is null or not.
    public render() {
        return (
            <div>
                {this.viewModel.newOrder &&
                    <Neo.Form model={this.viewModel.newOrder} showSummaryModal onSubmit={() => this.viewModel.submitOrder()}>
                        {(order, orderMeta) => (
                            <div>
                                <Neo.FormGroupInline bind={orderMeta.customerName} />
                                <NeoGrid.Grid items={order.orderDetails}>
                                    {(orderDetail, orderDetailMeta) => (
                                        <NeoGrid.Row>
                                            {/* <NeoGrid.Column display={orderDetailMeta.productId} /> */}
                                            <NeoGrid.Column display={orderDetailMeta.productName} />
                                            <NeoGrid.Column display={orderDetailMeta.price} />
                                            <NeoGrid.Column display={orderDetailMeta.value} sum />
                                            <NeoGrid.Column bind={orderDetailMeta.quantity} />
                                        </NeoGrid.Row>
                                    )}
                                </NeoGrid.Grid>
                                <div className="text-right mt-3">
                                    <Neo.Button isSubmit size="lg" icon="storefront">Place Order</Neo.Button>
                                </div>
                            </div>
                        )}
                    </Neo.Form>

                }

                {!this.viewModel.newOrder &&
                    <Neo.Alert variant="success" heading="Order submitted" animateInitial className="mt-4">
                        Your order has been submitted, <Link to="/">view your orders here</Link>,
                        or <Neo.Button variant="link" className="btn-link-inline" onClick={() => this.viewModel.setupOrder()}>create another order</Neo.Button>.

                    </Neo.Alert>}

            </div>


        );
    }
}

