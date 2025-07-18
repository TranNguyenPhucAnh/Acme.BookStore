import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { identityEntityPropContributors } from './entity/entity-prop-contributors';
import { identityCreateFormPropContributors, identityEditFormPropContributors } from './entity/form-prop-contributors';

const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadChildren: () => import('./home/home.module').then(m => m.HomeModule),
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(m => m.AccountModule.forLazy()), 
  },
  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(m => m.IdentityModule.forLazy({
      entityPropContributors: identityEntityPropContributors,
      createFormPropContributors: identityCreateFormPropContributors,
      editFormPropContributors: identityEditFormPropContributors,
    })),
  },
  {
    path: 'tenant-management',
    loadChildren: () =>
      import('@abp/ng.tenant-management').then(m => m.TenantManagementModule.forLazy()),
  },
  {
    path: 'setting-management',
    loadChildren: () =>
      import('@abp/ng.setting-management').then(m => m.SettingManagementModule.forLazy()),
  },
  { path: 'books',
    loadChildren: () =>
      import('./book/book.module').then(m => m.BookModule)
  },
  { path: 'authors',
    loadChildren: () =>
      import('./author/author.module').then(m => m.AuthorModule)
  },
  {
    path: 'feature-managements',
    loadChildren: () =>
      import('./feature-management/feature-management.module').then(m => m.FeatureManagementModule)
  },
  { path: 'schedulers',
    loadChildren: () =>
      import('./scheduler/scheduler.module').then(m => m.SchedulerModule)
  },
  { path: 'notifications-toolbar',
    loadChildren: () =>
      import('./notification-toolbar/notification-toolbar.module').then(m => m.NotificationToolbarModule) }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {})],
  exports: [RouterModule],
})
export class AppRoutingModule {}
