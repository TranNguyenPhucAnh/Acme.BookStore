import { RoutesService, eLayoutType } from '@abp/ng.core';
import { eThemeSharedRouteNames } from '@abp/ng.theme.shared';
import { inject, provideAppInitializer } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  provideAppInitializer((routes = inject(RoutesService)) => {
    routes.add([
      {
        path: '/',
        name: '::Menu:Home',
        iconClass: 'fas fa-home',
        order: 1,
        layout: eLayoutType.application,
      },
      {
        path: '/books',
        name: '::Menu:Books',
        iconClass: 'fas fa-book-open',
        layout: eLayoutType.application,
        order: 2,
        requiredPolicy: 'BookStore.Books'
      },
      {
        path: '/authors',
        name: '::Menu:Authors',
        iconClass: 'fas fa-pen',
        layout: eLayoutType.application,
        order: 3,
        requiredPolicy: 'BookStore.Authors',
      },
      {
        path: '/feature-managements',
        name: '::Menu:FeatureManagement',
        iconClass: 'fa fa-wrench',
        parentName: eThemeSharedRouteNames.Administration,
        layout: eLayoutType.application,
        requiredPolicy: 'BookStore.FeatureManagement'
      },
      {
        path: '/schedulers',
        name: 'Schedulers',
        iconClass: 'fa fa-clock',
        parentName: eThemeSharedRouteNames.Administration,
        layout: eLayoutType.application,
        //requiredPolicy: 'BookStore.Schedulers'
      },
    ]);
  }),
];