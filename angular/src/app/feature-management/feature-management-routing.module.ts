import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { FeatureManagementComponent } from './feature-management.component';
import { authGuard, permissionGuard } from '@abp/ng.core';

const routes: Routes = [
  {
    path: '',
    component: FeatureManagementComponent,
    canMatch: [authGuard, permissionGuard]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class FeatureManagementRoutingModule { }
