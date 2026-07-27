import { IAppMenuItem, IAppRoute } from '../App/Services/RouteService';
import CatalogueView from "./Views/Catalogue/CatalogueView";
import * as CatalogueRoles from "./Models/Security/CatalogueRoles";

const MenuRoutes: IAppMenuItem[] = 
    [
        { 
            name: "Domain", children: 
            [
                { 
                    name: "Screen 1", path: "/Screen1", icon: "question_mark", component: null as any /* TODO: Make this an actual route. */ 
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