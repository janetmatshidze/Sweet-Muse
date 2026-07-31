import { IAppMenuItem, IAppRoute } from '../App/Services/RouteService';
import CatalogueView from "./Views/Catalogue/CatalogueView";
import * as CatalogueRoles from "./Models/Security/CatalogueRoles";
import ProductsView from "./Views/ProductsView";

const MenuRoutes: IAppMenuItem[] = 
    [
        { 
            name: "Domain", children: 
            [
                { 
                    name: "Products", path: "/products", icon: "shopping_bag", component: ProductsView
                },
                 { 
                    name: "Orders", path: "/orders", icon: "shopping_bag", component: ProductsView
                },
                { 
                    name: "Catalogue", 
                    path: "/catalogue", 
                    component: CatalogueView,
                    icon: "browse",
                    role: CatalogueRoles.CataloguePage.View,
                    routeChildren: CatalogueView.getRouteChildren()
                }
               
            ]
        }
    ];

const PureRoutes: IAppRoute[] = [];

export { 
    MenuRoutes, 
    PureRoutes 
}