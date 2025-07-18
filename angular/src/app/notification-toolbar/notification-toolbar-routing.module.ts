import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { NotificationToolbarComponent } from './notification-toolbar.component';

const routes: Routes = [{ path: '', component: NotificationToolbarComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class NotificationToolbarRoutingModule { }
