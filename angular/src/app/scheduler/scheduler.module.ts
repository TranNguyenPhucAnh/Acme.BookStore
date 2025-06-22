import { NgModule } from '@angular/core';

import { SchedulerRoutingModule } from './scheduler-routing.module';
import { SchedulerComponent } from './scheduler.component';
import { SharedModule } from '../shared/shared.module';
import { CronJobsModule } from 'ngx-cron-jobs';
import { SchedulerDetailsComponent } from './scheduler-details/scheduler-details.component';


@NgModule({
  declarations: [
    SchedulerComponent,
    SchedulerDetailsComponent
  ],
  imports: [
    SharedModule,
    SchedulerRoutingModule,
    CronJobsModule
  ]
})
export class SchedulerModule { }
