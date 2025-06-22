import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home.component';
import { authGuard, permissionGuard } from '@abp/ng.core';

const routes: Routes = [
  {
    path: '',
    component: HomeComponent,
    canMatch: [authGuard, permissionGuard]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class HomeRoutingModule {}
