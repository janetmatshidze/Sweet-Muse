import React from "react";
import { Neo, Views } from "@singularsystems/neo-react";
import ProductsVM from "./ProductsVM";
import { observer } from "mobx-react";
import { getCategoryColorClass } from "../Utils/CategoryColors";
import Product from "../Models/Product";
import { ModalUtils, Validation } from "@singularsystems/neo-core";
import Pagination from "../../App/Components/Pagination";
import { isLowStock } from "../../App/Constants/StockThresholds";
import FilterOptionGroup from "../../App/Components/FilterOptionGroup";

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

        const pagedProducts = this.viewModel.pagination.pagedItems;
        const emptySlots = this.viewModel.pagination.pageSize - pagedProducts.length;

        return (
            <div className="sweet-muse mt-3">

                <div className="page-header">
                    <div>
                        <h1 className="title">
                            Products
                        </h1>
                    </div>

                    <Neo.Button
                        variant="primary"
                        className="add-btn"
                        onClick={() => this.viewModel.addProduct()}
                    >
                        <Neo.Icon name="plus" />

                        Add Product
                    </Neo.Button>
                </div>

                <div className="products-category-wrapper">

                    <div className="search-bar mb-3">
                        <Neo.Icon name="search" className="search-icon" />
                        <input
                            type="text"
                            className="search-input"
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
                                <FilterOptionGroup
                                    options={[
                                        { label: "All Categories", value: null },
                                        ...this.viewModel.categories.map(c => ({ label: c.categoryName, value: c.categoryId })),
                                    ]}
                                    selectedValue={this.viewModel.selectedCategoryId}
                                    onSelect={(value) => this.viewModel.setSelectedCategory(value)}
                                />

                                <div className="filter-divider" />

                                <FilterOptionGroup
                                    options={[
                                        { label: "All Stock Levels", value: "all" },
                                        { label: "Low Stock", value: "low" },
                                        { label: "In Stock", value: "high" },
                                    ]}
                                    selectedValue={this.viewModel.stockFilter}
                                    onSelect={(value) => this.viewModel.setStockFilter(value)}
                                />
                            </div>
                        )}
                    </div>
                </div>
                <div className="table-grid">
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

                                        <td className={`numbers product-stock ${isLowStock(product.stock) ? 'stock-low' : 'stock-high'}`}>
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
                                                    this.deleteProduct(product)
                                                }

                                            />

                                        </td>
                                    </tr>
                                );
                            })}

                            {pagedProducts.length === 0 && (
                                <tr className="filler-row">
                                    <td colSpan={8} className="message">
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

                <Pagination
                    currentPage={this.viewModel.pagination.currentPage}
                    totalPages={this.viewModel.pagination.totalPages}
                    onNext={() => this.viewModel.pagination.nextPage()}
                    onPrevious={() => this.viewModel.pagination.previousPage()}
                />

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
                                <div className="form">

                                    <div className="">
                                        <div className="form-field product-name-field">
                                            <Neo.FormGroupInline
                                                bind={productMeta.productName}
                                            />
                                        </div>

                                        <div className="form-field price-field">
                                            <Neo.FormGroupInline
                                                bind={productMeta.price}
                                            />
                                        </div>
                                    </div>

                                    <div className="form-field">
                                        <Neo.FormGroupInline
                                            bind={productMeta.description}
                                        />
                                    </div>

                                    <div className="product-image-field">
                                        <label className="form-label">
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
                                                        className="replace-image-icon"
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

                                        {!productMeta.imageUrl.validator.isValid && (
                                            <div className="field-error">
                                                {productMeta.imageUrl.validator
                                                    .getDisplayState(Validation.DisplayMode.AfterSubmit)
                                                    .displayText}
                                            </div>
                                        )}
                                    </div>

                                    <div className="form-field">
                                        <Neo.FormGroupInline
                                            bind={productMeta.stock}
                                        />
                                    </div>

                                    <div className="form-field">
                                        <Neo.FormGroupInline
                                            bind={productMeta.categoryId}
                                            select={{
                                                items: this.viewModel.categories,
                                                allowNulls: true,
                                                nullText: "Select a category",
                                            }}
                                        />
                                    </div>

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

                            )}
                        </Neo.Form>
                    </Neo.Modal>
                )}
            </div>
        );
    }
    public deleteProduct(product: Product) {
        ModalUtils.showYesNo(
            "Delete Product",
            `Are you sure you want to delete ${product.productName}?`,
            () => this.viewModel.deleteProduct(product)
        );
    }

}