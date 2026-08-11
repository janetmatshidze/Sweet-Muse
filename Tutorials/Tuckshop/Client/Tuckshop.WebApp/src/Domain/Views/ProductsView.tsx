import React from "react";
import { Neo, Views } from "@singularsystems/neo-react";
import ProductsVM from "./ProductsVM";
import { observer } from "mobx-react";
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

        const pagedProducts = this.viewModel.pagedProducts;
        const emptySlots = this.viewModel.pageSize - pagedProducts.length;

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

                 <div className="products-category-wrapper">

                <div className="products-search-bar mb-2">
                    <Neo.Icon name="search" className="products-search-icon" />
                    <input
                        type="text"
                        className="products-search-input"
                        placeholder="Search by product name..."
                        value={this.viewModel.searchTerm}
                        onChange={(e) => this.viewModel.setSearchTerm(e.target.value)}
                    />
                </div>

                <div className="products-filter-wrapper">
    <button
        type="button"
        className="products-filter-btn"
        onClick={() => this.viewModel.toggleCategoryFilter()}
    >
        <Neo.Icon name="filter_list" />
        <span>Filter</span>
        {this.viewModel.selectedCategoryId !== null && (
            <span className="filter-active-dot" />
        )}
    </button>

    {this.viewModel.showCategoryFilter && (
        <div className="products-filter-dropdown">
            <button
                type="button"
                className={
                    "filter-option" +
                    (this.viewModel.selectedCategoryId === null ? " active" : "")
                }
                onClick={() => this.viewModel.setSelectedCategory(null)}
            >
                All Categories
            </button>

            {this.viewModel.categories.map((category) => (
                <button
                    key={category.categoryId}
                    type="button"
                    className={
                        "filter-option" +
                        (this.viewModel.selectedCategoryId === category.categoryId
                            ? " active"
                            : "")
                    }
                    onClick={() =>
                        this.viewModel.setSelectedCategory(category.categoryId)
                    }
                >
                    {category.categoryName}
                </button>
            ))}
        </div>
        
    )}
</div>
</div>
                <div className="products-grid">
                    <table className="table">
                        <thead>
                            <tr>
                                <th>Product Image</th>
                                <th>Product Name</th>
                                <th>Description</th>
                                <th>Category</th>
                                <th>Stock</th>
                                <th>Price</th>
                                <th></th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            {pagedProducts.map((product) => {
                                const category = this.viewModel.categories.find(
                                    (c) => c.categoryId === product.categoryId
                                );

                                return (
                                    <tr key={product.productId}>
                                        <td>
                                            <div className="product-image-container">
                                                <img
                                                    src={product.imageUrl}
                                                    alt={product.productName}
                                                    className="product-list-image"
                                                />
                                            </div>
                                        </td>

                                        <td className="product-name">
                                            {product.productName}
                                        </td>

                                        <td className="product-description">
                                            {product.description}
                                        </td>

                                        <td className="category">
                                            {category && (
                                                <span
                                                    className={`category-pill ${getCategoryColorClass(
                                                        category.categoryId
                                                    )}`}
                                                >
                                                    {category.categoryName}
                                                </span>
                                            )}
                                        </td>

                                        <td className="numbers product-stock">
                                            {product.stock}
                                        </td>

                                        <td className="numbers product-price">
                                            R{product.price.toFixed(2)}
                                        </td>

                                        <td>
                                            <Neo.Button
                                                icon="edit"
                                                className="edit-icon"
                                                onClick={() =>
                                                    this.viewModel.editProduct(product)
                                                }
                                            />
                                        </td>

                                        <td>
                                            <Neo.Button
                                                icon="delete"
                                                className="delete-icon"
                                                onClick={() =>
                                                    this.viewModel.deleteProduct(product)
                                                }
                                            />
                                        </td>
                                    </tr>
                                );
                            })}

                            {pagedProducts.length === 0 && (
                                <tr className="filler-row">
                                    <td colSpan={8} className="no-products-message">
                                        No products found
                                    </td>
                                </tr>
                            )}

                            {Array.from({
                                length:
                                    pagedProducts.length === 0
                                        ? emptySlots - 1
                                        : emptySlots,
                            }).map((_, index) => (
                                <tr key={`filler-${index}`} className="filler-row">
                                    <td colSpan={8}>&nbsp;</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>

                <div className="products-pagination">
                    <Neo.Button
                        className="pagination-btn"
                        icon="keyboard_double_arrow_left"
                        disabled={this.viewModel.currentPage === 1}
                        onClick={() => this.viewModel.previousPage()}
                    >
                    </Neo.Button>

                    <span className="pagination-info">
                        Page {this.viewModel.currentPage} of {this.viewModel.totalPages}
                    </span>

                    <Neo.Button
                        className="pagination-btn"
                        icon="keyboard_double_arrow_right"
                        disabled={
                            this.viewModel.currentPage === this.viewModel.totalPages
                        }
                        onClick={() => this.viewModel.nextPage()}
                    >
                    </Neo.Button>
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

                                    <div className="">
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
                                                    <Neo.Icon
                                                        name="laps"
                                                    />

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
                                                    <Neo.Icon name="cloud_upload" />
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
                                   

                                    <div className="product-form-actions">
                                     
                                     
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