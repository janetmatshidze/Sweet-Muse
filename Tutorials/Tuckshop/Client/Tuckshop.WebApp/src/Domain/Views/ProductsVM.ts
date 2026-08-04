import { Views } from '@singularsystems/neo-react';
import { AppService, Types } from '../DomainTypes';
import { List } from '@singularsystems/neo-core';
import Product from '../Models/Product';

export default class ProductsVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
        private productsApiClient = AppService.get(Types.Domain.ApiClients.ProductsApiClient),
        private dataCache = AppService.get(Types.Domain.Services.DataCache)
    ) {

        super(taskRunner);
        this.makeObservable();
    }
    public products = new List(Product);

    public async initialise() {

    // Load products from the API , used WaitFor to show progress bar while records are being loaded.
    const response = await this.taskRunner.waitFor(this.productsApiClient.get());
    this.products.set(response.data);
    }
    
    // task Runner used to run async code in a safe way . Progress bar will be shown whie records are being saved.
        saveProducts() {
        this.taskRunner.run(async () => { 
        const response = await this.productsApiClient.saveList(this.products.toJSArray());
        this.products.set(response.data);
        this.notifications.addSuccess("Products saved", "Products saved successfully", 4);
        this.dataCache.products.expire(); // Expire the cache so that next time products are loaded from the API.

    });
    
    
    }
}