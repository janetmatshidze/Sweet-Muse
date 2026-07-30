import { IAppMenuItem, IAppRoute } from '../App/Services/RouteService';

const MenuRoutes: IAppMenuItem[] = 
    [
        { 
            name: "<%= moduleName %>", children: 
            [
                { name: "Screen 1", path: "/Screen1", icon: "question_mark", component: null as any /* TODO: Make this an actual route. */ }
            ]
        }
    ];

const PureRoutes: IAppRoute[] = [];

export { 
    MenuRoutes, 
    PureRoutes 
}