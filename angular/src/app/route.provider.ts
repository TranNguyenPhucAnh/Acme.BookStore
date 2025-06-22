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
        path: '/book-store',
        name: '::Menu:BookStore',
        iconClass: 'fas fa-book',
        order: 2,
        layout: eLayoutType.application,
        requiredPolicy: 'BookStore.Books || BookStore.Authors',
      },
      {
        path: '/books',
        name: '::Menu:Books',
        parentName: '::Menu:BookStore',
        layout: eLayoutType.application,
        requiredPolicy: 'BookStore.Books'
      },
      {
        path: '/authors',
        name: '::Menu:Authors',
        parentName: '::Menu:BookStore',
        layout: eLayoutType.application,
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
        name: '::Menu:Schedulers',
        iconClass: 'fa fa-clock',
        parentName: eThemeSharedRouteNames.Administration,
        layout: eLayoutType.application,
        //requiredPolicy: 'BookStore.Schedulers'
      },
    ]);
  }),
];