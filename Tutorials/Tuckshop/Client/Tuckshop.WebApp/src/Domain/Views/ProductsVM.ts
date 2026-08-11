import { Views } from '@singularsystems/neo-react';
import { AppService, Types } from '../DomainTypes';
import { List } from '@singularsystems/neo-core';
import Product from '../Models/Product';
import Category from '../Models/Category';
import { upload } from '@imagekit/react';

export default class ProductsVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
        private productsApiClient = AppService.get(Types.Domain.ApiClients.ProductsApiClient),
        private categoriesApiClient = AppService.get(Types.Domain.ApiClients.CategoriesApiClient),
        private imageKitApiClient = AppService.get(Types.Domain.ApiClients.ImageKitApiClient),
        private dataCache = AppService.get(Types.Domain.Services.DataCache)
    ) {

        super(taskRunner);
        this.makeObservable();
    }

    public products = new List(Product);

    public categories = new List(Category);

    public editingProduct: Product | null = null;


    public async initialise() {

        // Load products from the API, used WaitFor to show
        // progress bar while records are being loaded.
        const response = await this.taskRunner.waitFor(
            this.productsApiClient.get()
        );

        this.products.set(response.data);


        const categoriesResponse = await this.taskRunner.waitFor(
            this.categoriesApiClient.get()
        );

        this.categories.set(categoriesResponse.data);
    }


    public addProduct() {

        this.editingProduct = new Product();
    }


    public editProduct(product: Product) {

        const edit = new Product();

        edit.set(product.toJSObject());

        this.editingProduct = edit;
    }


    public deleteProduct(product: Product) {

        this.taskRunner.run(async () => {

            if (product.productId) {

                await this.productsApiClient.delete(
                    product.productId
                );

                this.dataCache.products.expire();
            }


            const existing = this.products.find(
                p => p.productId === product.productId
            );


            if (existing) {

                this.products.remove(existing);
            }


            this.notifications.addSuccess(
                "Product deleted",
                " Product deleted successfully",
                4
            );

            this.editingProduct = null;

        }).catch(() => {

            // taskRunner already showed its own toast for the error.
            // Catching here just prevents React's dev overlay
            // from also firing.

        });
    }


    public cancelEdit() {

        this.editingProduct = null;
    }


    public saveProduct() {

        if (!this.editingProduct) {

            return;
        }


        this.taskRunner.run(async () => {

            const response =
                await this.productsApiClient.save(
                    this.editingProduct!.toJSObject()
                );


            const existing = this.products.find(
                p =>
                    p.productId ===
                    response.data.productId
            );


            if (existing) {

                existing.set(response.data);

            } else {

                const newProduct = new Product();

                newProduct.set(response.data);

                this.products.push(newProduct);
            }


            this.notifications.addSuccess(
                "Products saved",
                "Products saved successfully",
                4
            );


            this.dataCache.products.expire();

            this.editingProduct = null;
        });
    }


    public async uploadProductImage(file: File) {

        if (!this.editProduct) {

            return;
        }


        const allowedTypes = [
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif"
        ];


        if (!allowedTypes.includes(file.type)) {

            this.notifications.addDanger(
                "Invalid file",
                "Please upload a JPG, PNG, WebP, or GIF image.",
                5
            );

            return;
        }


        await this.taskRunner.run(async () => {

            const authResponse =
                await this.imageKitApiClient.getAuthParams();


            const result = await upload({

                file,

                fileName: file.name,

                folder: "/Sweet-Muse",

                publicKey:
                    authResponse.data.publicKey,

                signature:
                    authResponse.data.signature,

                expire:
                    authResponse.data.expire,

                token:
                    authResponse.data.token
            });


            if (result.url) {

                this.editingProduct!.imageUrl =
                    result.url;

            } else {

                this.notifications.addDanger(
                    "Upload failed",
                    "The image was uploaded but no URL was returned.",
                    4
                );
            }
        });
    }

}