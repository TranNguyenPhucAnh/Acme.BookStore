import { ListService, PagedResultDto, PermissionService } from '@abp/ng.core';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { CreateUpdateSchedulerDto, SchedulerDto, SchedulerService } from '@proxy/schedulers';
import { Constants } from '../shared/constants/constant';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { NgbDateAdapter, NgbDateNativeAdapter, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { SchedulerDetailsComponent } from './scheduler-details/scheduler-details.component';
import { catchError, filter, Observer, of, Subject, switchMap, takeUntil, tap } from 'rxjs';

@Component({
  selector: 'app-scheduler',
  templateUrl: './scheduler.component.html',
  styleUrl: './scheduler.component.scss',
  standalone: false,
  providers: [ListService, { provide: NgbDateAdapter, useClass: NgbDateNativeAdapter }],
})
export class SchedulerComponent implements OnInit, OnDestroy {
  schedulers = {
    items: [],
    totalCount: 0
  } as PagedResultDto<SchedulerDto>

  selectedSchedule = {} as SchedulerDto;
  pageSizes = Constants.PageSizeOption;
  private destroy$ = new Subject<void>(); // Để dọn dẹp listener

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
    .pipe(
      catchError((error) => {
        //console.log(error);
        this.toasterService.error('An error occurred, please retry');
        return of(null);
      }),
      //filter(value => !!value),
      takeUntil(this.destroy$),
    ).subscribe({
      next: (val) => this.schedulers = val,
      error: (err) => console.log(err),
      complete: () => console.log('fetch schedulers completed')
    } as Observer<PagedResultDto<SchedulerDto>>);
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
    this.schedulerService.get(id)
    .pipe(
      catchError((error) => {
        //console.log(error);
        this.toasterService.error('An error occurred, please retry');
        return of(null);
      }),
      //filter(value => !!value),
      //tap(() => this.openModal(true)),
      takeUntil(this.destroy$),
    ).subscribe({
      next: (val) => {
        this.selectedSchedule = val
        this.openModal(true)
      },
      error: (err) => console.error(err),
      complete: () => console.log('edit scheduler completed')
    } as Observer<SchedulerDto>);
  }

  openModal(isEdit: boolean): void {
    const modalRef = this.modalService.open(SchedulerDetailsComponent);
    modalRef.componentInstance.selectedSchedule = this.selectedSchedule;
    modalRef.result.then(
      (result) => {
        result.timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
        console.log('Modal closed with result:', result);
        const request = isEdit ? this.schedulerService.update(
          this.selectedSchedule.id, result as CreateUpdateSchedulerDto) :
          this.schedulerService.create(result as CreateUpdateSchedulerDto);
        request.pipe(
          catchError((error) => {
            //console.log(error);
            this.toasterService.error('An error occurred, please retry');
            return of(null);
          }),
          //tap(() => this.list.get()),
          //filter(value => !!value),
          takeUntil(this.destroy$),
          ).subscribe({
            next: (val) => this.list.get(),
            error: (err) => console.log(err),
            complete: () => console.log('create/update scheduler completed')
          } as Observer<SchedulerDto>);
      }
    );
  }

delete(id: string): void {
  this.confirmation.warn('::AreYouSureToDelete', 'AbpAccount::AreYouSure').pipe(
    filter(status => status === Confirmation.Status.confirm),
    switchMap(() => this.schedulerService.delete(id)),
    catchError((error) => {
      //console.error('Delete failed', error);
      this.toasterService.error('Delete failed');
      return of(null); // fallback nếu cần
    }),
    // tap(() => {
    //   this.toasterService.success('Deleted Successfully');
    //   this.list.get();
    // }),
    //filter(value => !!value),
    takeUntil(this.destroy$)
  ).subscribe({
    next: (val) => {
      this.toasterService.success('Deleted Successfully');
      this.list.get();
    },
    error: (err) => console.log(err),
    complete: () => console.log('delete scheduler completed')
  } as Observer<any>);
}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}