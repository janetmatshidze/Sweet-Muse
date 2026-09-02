import { IAppMenuItem, IAppRoute } from '../App/Services/RouteService';
import ProductsView from "./Views/ProductsView";
import CreateOrderView from './Views/Orders/CreateOrderView';
import ViewOrdersView from './Views/Orders/ViewOrdersView';
import CategoriesView from './Views/CategoriesView';
import CustomersView from './Views/CustomersView';


export const viewOrdersRoute = { name: "View orders", path: '/viewOrders', component: ViewOrdersView, icon: "receipt_long" };

export const customersRoute = { name: "Customers", path: "/customers", icon: "face", component: CustomersView};

const MenuRoutes: IAppMenuItem[] =
    [
        {
            name: "Domain", children:
                [
                    {
                        name: "Products", path: "/products", icon: "bakery_dining", component: ProductsView
                    },
                    customersRoute,
                    // {
                    //     name: "Categories", path: "/categories", icon: "category", component: CategoriesView
                    // },
                    {
                        name: "Create Order", path: "/order/create", icon: "heart_plus", component: CreateOrderView
                    },
                    viewOrdersRoute,

                ]
        }
    ];

const PureRoutes: IAppRoute[] = [];

export {
    MenuRoutes,
    PureRoutes
}

