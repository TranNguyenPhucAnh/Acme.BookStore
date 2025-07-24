import { Component, OnInit, OnDestroy } from '@angular/core';
import { ExtendedNotificationDto, NotificationDto } from '@proxy/notifications/model';
import { NotificationService } from '@proxy/notifications/notification.service';
import { EnvironmentService, PagedResultDto, PagedResultRequestDto } from '@abp/ng.core';
import { Router } from '@angular/router';
import { SignalRService } from '../signalR.service';
import { BehaviorSubject, catchError, filter, finalize, Observer, of, Subject, switchMap, takeUntil, tap } from 'rxjs';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Constants } from '../shared/constants/constant';

@Component({
  selector: 'app-notification-toolbar',
  templateUrl: './notification-toolbar.component.html',
  styleUrl: './notification-toolbar.component.scss',
  standalone: false
})
export class NotificationToolbarComponent implements OnInit, OnDestroy {
  isLoading = false;
  isEndOfList = false;
  totalUnread = 0;
  hasUnread$ = new BehaviorSubject<boolean>(false);
  readNotificationIds = [];

  data = {
    items: [] as NotificationDto[],
    totalCount: 0
  } as PagedResultDto<NotificationDto>;

  input = {
    skipCount: 0,
    maxResultCount: 5
  } as PagedResultRequestDto;

  envUrl: string;

  private destroy$ = new Subject<void>(); // Để dọn dẹp listener

  constructor(
    private notificationService: NotificationService,
    private router: Router,
    private signalRService: SignalRService,
    private toasterService: ToasterService,
    private confirmationService: ConfirmationService,
    private envService: EnvironmentService
  ) {}

  ngOnInit(): void {
    // Thiết lập listener cho ReloadNotification
    this.envUrl = this.envService.getEnvironment().apis.default.url;
    console.log('notification component:', this.envUrl);
    this.signalRService.addListener(
      this.envUrl.concat(Constants.NotificationHubUrl),
      "NotificationListReload",
      message => {
      console.log('Received SignalR message:', message);
      this.refreshNotifications(); // Làm mới danh sách thông báo
    });

    // Tải dữ liệu ban đầu
    this.loadMore();
  }

  refreshNotifications(): void {
    // Reset skipCount để lấy lại từ đầu
    this.input.skipCount = 0;
    this.data.items = []; // Xóa danh sách hiện tại để làm mới
    this.readNotificationIds = [];
    this.loadMore(); // Gọi lại API GET
  }

  loadMore(): void {
    if (this.isLoading || (this.data.totalCount > 0 && this.input.skipCount >= this.data.totalCount)) {
      this.isEndOfList = true;
      return;
    }

    this.isLoading = true;

    this.notificationService
      .getAll(this.input)
      .pipe(
        catchError((error) =>{
          this.toasterService.error('An error occurred, please retry');
          //console.error(error);
          return of(null);
        }),
        //filter(value => !!value),
        finalize(() => this.isLoading = false),
        takeUntil(this.destroy$),
      ).subscribe({
        next: (val) => {
          this.data.items = [...this.data.items, ...val.result.items];
          this.data.totalCount = val.result.totalCount;
          this.input.skipCount += this.input.maxResultCount;
          this.totalUnread = val.totalUnread;
          this.hasUnread$.next(this.totalUnread > 0);
          //this.isLoading = false; move to finalize()
        },
        error: (err) => console.log(err),
        complete: () => console.log('fetch notifications completed')
      } as Observer<ExtendedNotificationDto>
    );
  }

  handleItemClick(item: NotificationDto, $event: MouseEvent): void {
    $event.stopPropagation();
    $event.preventDefault();
    if (!item.isRead) {
      item.isRead = true;
      this.totalUnread -= 1;
      this.readNotificationIds.push(item.id);
    }
    if (item.redirectUrl) {
      this.router.navigateByUrl(item.redirectUrl);
    }
  }

  markAllAsRead($event: MouseEvent) {
    $event.stopPropagation();
    $event.preventDefault();
    this.confirmationService.warn('Mark As All Read', 'Are You Sure?')
    .pipe(
      filter(status => status == Confirmation.Status.confirm),
      switchMap(()=> {
        this.data.items.forEach(item => (item.isRead = true));
        this.totalUnread = 0;
        return this.notificationService.markAllAsRead()
      }),
      catchError((error) => {
        //console.log(error);
        this.toasterService.error('An error occurred, please retry');
        return of(null);
      }),
      // tap(() => {
      //   this.refreshNotifications();
      //   this.toasterService.success("::MarkAllAsReadSuccessfully");
      // }),
      //filter(value => !!value),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (val) => {
        this.refreshNotifications();
        this.toasterService.success("Mark All As Read Successfully");
      },
      error: (err) => console.log(err),
      complete: () => console.log('mark all as read completed')
    } as Observer<any>);
  }

  onClose() : void {
    if (this.readNotificationIds.length > 0) {
      this.notificationService.markAsRead(this.readNotificationIds)
      .pipe(
        catchError((error) => {
          //console.log(error);
          this.toasterService.error('An error occurred, please retry');
          return of(null);
        }),
        //tap(() => this.refreshNotifications()),
        //filter(value => !!value),
        takeUntil(this.destroy$)
      ).subscribe({
        next: (val) => this.refreshNotifications(),
        error: (err) => console.log(err),
        complete: () => console.log('mark all read completed')
      } as Observer<any>);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}