import React from 'react';
import { Neo, Views } from '@singularsystems/neo-react';
import ProductsVM from './ProductsVM';
import { observer } from 'mobx-react';

class ProductsParams {
    // TODO: Add parameters here in the form: public paramName = { isQuery?: boolean, required?: boolean };
}

@observer
export default class ProductsView extends Views.ViewBase<ProductsVM, ProductsParams> {
   public static params = new ProductsParams();

    constructor(props: unknown) {
        super("Products", ProductsVM, props);
    }

    protected viewParamsUpdated() {

    }

    public render() {
        return (
            <div>
			    <Neo.Card title="Products">
                </Neo.Card>
            </div>
        );
    }
}