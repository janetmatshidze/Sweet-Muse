import { Views } from '@singularsystems/neo-react';
import { AppService, Types } from '../DomainTypes';
import { List } from '@singularsystems/neo-core';
import Product from '../Models/Product';
import axios from 'axios';

export default class ProductsVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications)) {

        super(taskRunner);
        this.makeObservable();
    }
    public products = new List(Product);
    
    public async initialise() {
    
    const response = await this.taskRunner.waitFor(axios.get("https://localhost:5001/api/products"));
    this.products.set(response.data);
    }

    
}