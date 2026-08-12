import { IAppMenuItem, IAppRoute } from '../App/Services/RouteService';
import ProductsView from "./Views/ProductsView";
import CreateOrderView from './Views/Orders/CreateOrderView';
import ViewOrdersView from './Views/Orders/ViewOrdersView';
import CategoriesView from './Views/CategoriesView';
import CustomersView from './Views/CustomersView';


export const viewOrdersRoute = { name: "View orders", path: '/viewOrders', component:ViewOrdersView, icon:"search"};

const MenuRoutes: IAppMenuItem[] = 
    [
        { 
            name: "Domain", children: 
            [
                { 
                    name: "Products", path: "/products", icon: "bakery_dining", component: ProductsView
                },
                { 
                    name: "Customers", path: "/customers", icon: "face", component: CustomersView
                },
                 { 
                    name: "Categories", path: "/categories", icon: "category", component: CategoriesView
                },
                 { 
                    name: "Create Order", path: "/order/create", icon: "receipt-long", component: CreateOrderView
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

