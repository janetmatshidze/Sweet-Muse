import React from "react";
import { Neo, NeoGrid, Views } from "@singularsystems/neo-react";
import ProductsVM from "./ProductsVM";
import { observer } from "mobx-react";
import { Misc } from "@singularsystems/neo-core";
import { getCategoryColorClass } from "../Utils/CategoryColors";

class ProductsParams {
  // TODO: Add parameters here in the form: public paramName = { isQuery?: boolean, required?: boolean };
}

@observer
export default class ProductsView extends Views.ViewBase<
  ProductsVM,
  ProductsParams
> {
  public static params = new ProductsParams();

  constructor(props: unknown) {
    super("Products", ProductsVM, props);
  }

  protected viewParamsUpdated() {}

  public render() {
    return (
      <div>
        <div className="mb-3 text-right mt-3">
          <Neo.Button
            variant="primary"
            icon="plus"
            onClick={() => this.viewModel.addProduct()}
          >
            Add Product
          </Neo.Button>
        </div>

        <div className="products-view mb-5">
          <NeoGrid.Grid items={this.viewModel.products}>
            {(product, productMeta) => (
              <NeoGrid.Row>
                {/* <NeoGrid.Column display={productMeta.productId} /> */}
                <NeoGrid.Column label="Product">
                  <img
                    src={product.imageUrl}
                    alt={product.productName}
                    className="product-image"
                  />
                </NeoGrid.Column>
                      <NeoGrid.Column 
                       className="product-name"
                      display={productMeta.productName} />

                      <NeoGrid.Column 
                       className="product-description"
                      display={productMeta.description} />

                <NeoGrid.Column label="Category" className="category">
                  {(() => {
                    const category = this.viewModel.categories.find(
                      (c) => c.categoryId === product.categoryId,
                    );
                    if (!category) return null;
                    return (
                      <span
                        className={getCategoryColorClass(category.categoryId)}
                      >
                        {category.categoryName}
                      </span>
                    );
                  })()}
                </NeoGrid.Column>

                <NeoGrid.Column
                  className="numbers"
                  display={productMeta.price}
                  numProps={{ format: Misc.NumberFormat.CurrencyDecimals }}
                />

                <NeoGrid.Column 
                className="numbers"
                display={productMeta.stock} />

                <NeoGrid.ButtonColumn>
                  <Neo.Button
                    icon="edit"
                    className="edit-icon"
                    onClick={() => this.viewModel.editProduct(product)}
                  />
                </NeoGrid.ButtonColumn>

                <NeoGrid.ButtonColumn>
                  <Neo.Button
                    icon="delete"
                    className="delete-icon"
                    onClick={() => this.viewModel.deleteProduct(product)}
                  />
                </NeoGrid.ButtonColumn>
              </NeoGrid.Row>
            )}
          </NeoGrid.Grid>
        </div>

        {this.viewModel.editingProduct && (
          <Neo.Modal
            show={!!this.viewModel.editingProduct}
            title={
              this.viewModel.editingProduct.productId
                ? "Edit Product"
                : "Add Product"
            }
            onClose={() => this.viewModel.cancelEdit()}
          >
            <Neo.Form
              model={this.viewModel.editingProduct}
              onSubmit={() => this.viewModel.saveProduct()}
            >
              {(product, productMeta) => (
                <div>
                  <Neo.FormGroupInline bind={productMeta.productName} />
                  <Neo.FormGroupInline bind={productMeta.price} />
                  <Neo.FormGroupInline bind={productMeta.description} />
                  <div className="form-group">
                    <label>Product Image</label>
                    <input
                      type="file"
                      accept="image/*"
                      onChange={(e) => {
                        const file = e.target.files?.[0];
                        if (file) {
                          this.viewModel.uploadProductImage(file);
                        }
                      }}
                    />
                    {product.imageUrl && (
                      <img
                        src={product.imageUrl}
                        alt={product.productName}
                        className="product-image mt-2"
                      />
                    )}
                  </div>

                  <Neo.FormGroupInline bind={productMeta.stock} />
                  <Neo.FormGroupInline
                    bind={productMeta.categoryId}
                    select={{
                      items: this.viewModel.categories,
                      allowNulls: true,
                      nullText: "Select a category",
                    }}
                  />

                  <div className="text-right mt-3">
                    <Neo.Button
                      variant="secondary"
                      onClick={() => this.viewModel.cancelEdit()}
                      className="mr-2"
                    >
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
        )}
      </div>
    );
  }
}
