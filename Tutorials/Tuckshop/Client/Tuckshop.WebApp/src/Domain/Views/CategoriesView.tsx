import React from 'react';
import { Neo, Views } from '@singularsystems/neo-react';
import CategoriesVM from './CategoriesVM';
import { observer } from 'mobx-react';

class CategoriesParams {
    // TODO: Add parameters here in the form: public paramName = { isQuery?: boolean, required?: boolean };
}

@observer
export default class CategoriesView extends Views.ViewBase<CategoriesVM, CategoriesParams> {
   public static params = new CategoriesParams();

    constructor(props: unknown) {
        super("Categories", CategoriesVM, props);
    }

    protected viewParamsUpdated() {

    }

    public render() {
        return (
            <div>
			    <Neo.Card title="Categories">
        
                </Neo.Card>
            </div>
        );
    }
}