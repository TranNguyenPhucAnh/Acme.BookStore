import { NgModule } from '@angular/core';

import { NotificationToolbarRoutingModule } from './notification-toolbar-routing.module';
import { NotificationToolbarComponent } from './notification-toolbar.component';
import { SharedModule } from '../shared/shared.module';
import { MatMenuModule } from '@angular/material/menu';
import { InfiniteScrollDirective } from 'ngx-infinite-scroll';
import { MatBadgeModule } from '@angular/material/badge';
import { LocalizePipe } from '../localize.pipe';

@NgModule({
  declarations: [
    NotificationToolbarComponent,
    LocalizePipe
],
  imports: [
    SharedModule,
    NotificationToolbarRoutingModule,
    MatMenuModule,
    InfiniteScrollDirective,
    MatBadgeModule
  ]
})
export class NotificationToolbarModule { }
