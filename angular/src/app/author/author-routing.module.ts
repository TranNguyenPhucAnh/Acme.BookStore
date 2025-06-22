import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthorComponent } from './author.component';
import { authGuard, permissionGuard } from '@abp/ng.core';

const routes: Routes = [
  {
    path: '',
    component: AuthorComponent,
    canMatch: [authGuard, permissionGuard]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthorRoutingModule { }
