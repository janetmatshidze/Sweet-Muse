import React from 'react';
import { Neo, Views } from '@singularsystems/neo-react';
import ViewOrdersVM from './ViewOrdersVM';
import { observer } from 'mobx-react';

class ViewOrdersParams {
    // TODO: Add parameters here in the form: public paramName = { isQuery?: boolean, required?: boolean };
}

@observer
export default class ViewOrdersView extends Views.ViewBase<ViewOrdersVM, ViewOrdersParams> {
   public static params = new ViewOrdersParams();

    constructor(props: unknown) {
        super("View Orders", ViewOrdersVM, props);
    }

    protected viewParamsUpdated() {

    }

    public render() {
        return (
            <div>
			    <Neo.Card title="View Orders">
        
                </Neo.Card>
            </div>
        );
    }
}