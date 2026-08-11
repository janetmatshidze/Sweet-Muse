import React from "react";
import { Neo, NeoGrid, Views } from "@singularsystems/neo-react";
import ProductsVM from "./ProductsVM";
import { observer } from "mobx-react";
import { Misc } from "@singularsystems/neo-core";
import { getCategoryColorClass } from "../Utils/CategoryColors";

class ProductsParams { }

@observer
export default class ProductsView extends Views.ViewBase<
    ProductsVM,
    ProductsParams
> {
    public static params = new ProductsParams();

    constructor(props: unknown) {
        super("Products", ProductsVM, props);
    }

    protected viewParamsUpdated() { }

    public render() {
        return (
            <div className="sweet-muse-products mt-3">

                <div className="products-page-header">
                    <div>
                        <h1 className="products-title">
                            Products
                        </h1>
                    </div>

                    <Neo.Button
                        variant="primary"
                        icon="plus"
                        className="add-product-btn"
                        onClick={() => this.viewModel.addProduct()}
                    >
                        Add Product
                    </Neo.Button>
                </div>

                <div className="products-grid">
                    <NeoGrid.Grid items={this.viewModel.products}>
                        {(product, productMeta) => (
                            <NeoGrid.Row>

                                <NeoGrid.Column label="Product">
                                    <div className="product-image-container">
                                        <img
                                            src={product.imageUrl}
                                            alt={product.productName}
                                            className="product-list-image"
                                        />
                                    </div>
                                </NeoGrid.Column>

                                <NeoGrid.Column
                                    className="product-name"
                                    display={productMeta.productName}
                                />

                                <NeoGrid.Column
                                    className="product-description"
                                    display={productMeta.description}
                                />

                                <NeoGrid.Column
                                    label="Category"
                                    className="category"
                                >
                                    {(() => {
                                        const category =
                                            this.viewModel.categories.find(
                                                (c) =>
                                                    c.categoryId === product.categoryId
                                            );

                                        if (!category) {
                                            return null;
                                        }

                                        return (
                                            <span
                                                className={`category-pill ${getCategoryColorClass(
                                                    category.categoryId
                                                )}`}
                                            >
                                                {category.categoryName}
                                            </span>
                                        );
                                    })()}
                                </NeoGrid.Column>

                                <NeoGrid.Column
                                    className="numbers product-price"
                                    display={productMeta.price}
                                    numProps={{
                                        format:
                                            Misc.NumberFormat.CurrencyDecimals,
                                    }}
                                />

                                <NeoGrid.Column
                                    className="numbers product-stock"
                                    display={productMeta.stock}
                                />

                                <NeoGrid.ButtonColumn>
                                    <Neo.Button
                                        icon="edit"
                                        className="edit-icon"
                                        onClick={() =>
                                            this.viewModel.editProduct(product)
                                        }
                                    />
                                </NeoGrid.ButtonColumn>

                                <NeoGrid.ButtonColumn>
                                    <Neo.Button
                                        icon="delete"
                                        className="delete-icon"
                                        onClick={() =>
                                            this.viewModel.deleteProduct(product)
                                        }
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
                                <div className="product-form">

                                    <div className="product-form-row">
                                        <div className="product-form-field product-name-field">
                                            <Neo.FormGroupInline
                                                bind={productMeta.productName}
                                            />
                                        </div>

                                        <div className="product-form-field product-price-field">
                                            <Neo.FormGroupInline
                                                bind={productMeta.price}
                                            />
                                        </div>
                                    </div>

                                    <div className="product-form-field">
                                        <Neo.FormGroupInline
                                            bind={productMeta.description}
                                        />
                                    </div>

                                    <div className="product-image-field">
                                        <label className="product-form-label">
                                            Product Image
                                        </label>

                                        {product.imageUrl ? (
                                            <div className="product-image-edit">

                                                <div className="modal-image-preview">
                                                    <img
                                                        src={product.imageUrl}
                                                        alt={product.productName}
                                                    />
                                                </div>

                                                <label className="replace-image-box">
                                                    <span className="replace-image-icon">
                                                        ↻
                                                    </span>

                                                    <span className="replace-image-title">
                                                        Replace image
                                                    </span>

                                                    <span className="replace-image-text">
                                                        PNG, JPG up to 5MB
                                                    </span>

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
                                                </label>

                                            </div>
                                        ) : (
                                            <label className="image-upload-box">

                                                <span className="image-upload-icon">
                                                    ↑
                                                </span>

                                                <span className="image-upload-title">
                                                    Click to upload or drag and drop
                                                </span>

                                                <span className="image-upload-text">
                                                    PNG, JPG up to 5MB
                                                </span>

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

                                            </label>
                                        )}
                                    </div>

                                    <div className="product-form-row">
                                        <div className="product-form-field">
                                            <Neo.FormGroupInline
                                                bind={productMeta.stock}
                                            />
                                        </div>

                                        <div className="product-form-field">
                                            <Neo.FormGroupInline
                                                bind={productMeta.categoryId}
                                                select={{
                                                    items: this.viewModel.categories,
                                                    allowNulls: true,
                                                    nullText: "Select a category",
                                                }}
                                            />
                                        </div>
                                    </div>

                                    <div className="product-form-actions">

                                        <Neo.Button
                                            className="modal-close-btn"
                                            onClick={() => this.viewModel.cancelEdit()}
                                        >
                                            Close
                                        </Neo.Button>

                                        <Neo.Button
                                            isSubmit
                                            variant="success"
                                            icon="check"
                                            className="save-btn"
                                        >
                                            {this.viewModel.editingProduct?.productId
                                                ? "Update Product"
                                                : "Save Product"}
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