import React from 'react';
import { Neo, NeoGrid, Views } from '@singularsystems/neo-react';
import ProductsVM from './ProductsVM';
import { observer } from 'mobx-react';
import { Misc } from '@singularsystems/neo-core';

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
               <div className="mb-3 text-right mt-3">
                <Neo.Button variant="primary" icon="plus" onClick={() => this.viewModel.addProduct()}>
                    Add Product
                </Neo.Button>
                   </div>

                    <NeoGrid.Grid items={this.viewModel.products}>
                        {(product, productMeta) => (
                            <NeoGrid.Row>
                                <NeoGrid.Column display={productMeta.productId} />
                                 <NeoGrid.Column label="Image">
                                     <img
                                     src={product.imageUrl}
                                     alt={product.productName}
                                     className="product-image"
                                 />
                                </NeoGrid.Column>
                                <NeoGrid.Column display={productMeta.productName} />
                                <NeoGrid.Column display={productMeta.price} numProps={{ format: Misc.NumberFormat.CurrencyDecimals }} />
                                <NeoGrid.Column display={productMeta.description}/>
                                {/* <NeoGrid.ButtonColumn showDelete /> */}
                                <NeoGrid.ButtonColumn>
                                    <Neo.Button icon="edit" className="edit-icon" onClick={() => this.viewModel.editProduct(product)}/>
                                </NeoGrid.ButtonColumn> 
                            </NeoGrid.Row>
                        )}
                    </NeoGrid.Grid>

                      {this.viewModel.editingProduct && 
                      <Neo.Modal 
                          show={!!this.viewModel.editingProduct}
                          title={this.viewModel.editingProduct.productId? "Edit Product" : "Add Product"}
                          onClose={() => this.viewModel.cancelEdit()}>

                          <Neo.Form model={this.viewModel.editingProduct} onSubmit={() => this.viewModel.saveProduct()}>
                            {(product, productMeta) => (
                                <div>
                                    <Neo.FormGroupInline bind={productMeta.productName} />
                                    <Neo.FormGroupInline bind={productMeta.price} />
                                    <Neo.FormGroupInline bind={productMeta.description} />
                                    <Neo.FormGroupInline bind={productMeta.imageUrl} />
                                    <Neo.FormGroupInline bind={productMeta.stock} />
                                    {/* CategoryId dropdown - add once category list is wired up */}

                                    <div className="text-right mt-3">
                                        <Neo.Button variant="secondary" onClick={() => this.viewModel.cancelEdit()} className="mr-2">
                                            Cancel
                                        </Neo.Button>
                                        <Neo.Button isSubmit variant="success" icon="check">
                                            Save
                                        </Neo.Button>
                                    </div>
                                </div>
                            )}
                        </Neo.Form>
                    </Neo.Modal>
    }
            </div>
         
        );
        
    }
    
}
