import { ListService, PagedResultDto, PermissionService } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { CreateUpdateSchedulerDto, SchedulerDto, SchedulerService } from '@proxy/schedulers';
import { Constants } from '../shared/constants/constant';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { NgbDateAdapter, NgbDateNativeAdapter, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { SchedulerDetailsComponent } from './scheduler-details/scheduler-details.component';

@Component({
  selector: 'app-scheduler',
  templateUrl: './scheduler.component.html',
  styleUrl: './scheduler.component.scss',
  standalone: false,
  providers: [ListService, { provide: NgbDateAdapter, useClass: NgbDateNativeAdapter }],
})
export class SchedulerComponent implements OnInit {
  schedulers = {
    items: [],
    totalCount: 0
  } as PagedResultDto<SchedulerDto>

  selectedSchedule = {} as SchedulerDto;
  pageSizes = Constants.PageSizeOption;

  constructor(
    private schedulerService: SchedulerService,
    public readonly list: ListService,
    private permissionService: PermissionService,
    private confirmation: ConfirmationService,
    private toasterService: ToasterService,
    private modalService: NgbModal
  ) {}

  ngOnInit(): void {
    this.list.hookToQuery(query => this.schedulerService.getList(query))
    .subscribe(res => this.schedulers = res);
  }

  createScheduler() {
    this.selectedSchedule = {} as SchedulerDto;
    this.openModal(false);
  }

  onPageSizeChange(input: number) {
    this.list.maxResultCount = input;
    this.list.get();
  }

  isEditAndDelete() : boolean {
    return this.permissionService.getGrantedPolicy("BookStore.Schedulers.Edit && BookStore.Schedulers.Delete")
  }

  editScheduler(id: string) {
    this.schedulerService.get(id).subscribe((res) => {
      this.selectedSchedule = res;
      this.openModal(true);
    });
  }

  openModal(isEdit: boolean): void {
    const modalRef = this.modalService.open(SchedulerDetailsComponent);
    modalRef.componentInstance.selectedSchedule = this.selectedSchedule;
    modalRef.result.then(
      (result) => {
        if (isEdit) {
          this.schedulerService.update(
            this.selectedSchedule.id,
            {...result, description: this.selectedSchedule.description } as CreateUpdateSchedulerDto)
            .subscribe(() => this.list.get());
        }
        else {
          this.schedulerService.create(
            {...result, description: '' } as CreateUpdateSchedulerDto)
          .subscribe(() => this.list.get());
        }
      }
    );
  }

  delete(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', 'AbpAccount::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.schedulerService.delete(id).subscribe(() => this.list.get());
        this.toasterService.success("Deleted Successfully")
      }
    });
  }
}