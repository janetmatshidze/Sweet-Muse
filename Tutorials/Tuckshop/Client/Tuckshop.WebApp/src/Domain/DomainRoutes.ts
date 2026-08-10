import { IAppMenuItem, IAppRoute } from '../App/Services/RouteService';
import CatalogueView from "./Views/Catalogue/CatalogueView";
import * as CatalogueRoles from "./Models/Security/CatalogueRoles";
import ProductsView from "./Views/ProductsView";
import CreateOrderView from './Views/Orders/CreateOrderView';
import ViewOrdersView from './Views/Orders/ViewOrdersView';
import CategoriesView from './Views/CategoriesView';


export const viewOrdersRoute = { name: "View orders", path: '/viewOrders', component:ViewOrdersView, icon:"search"};

const MenuRoutes: IAppMenuItem[] = 
    [
        { 
            name: "Domain", children: 
            [
                { 
                    name: "Products", path: "/products", icon: "storefront", component: ProductsView
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

