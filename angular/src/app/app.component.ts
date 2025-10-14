import { Component, OnInit } from '@angular/core';
import { NotificationToolbarComponent } from './notification-toolbar/notification-toolbar.component';
import { NavItemsService } from '@abp/ng.theme.shared';
import { SignalRService } from './signalR.service';
import { LoadingService } from './shared/services/loading.service';
import { AuthService } from '@abp/ng.core';
import { environment as env } from '../environments/environment.prod';

@Component({
  standalone: false,
  selector: 'app-root',
  template: `<abp-loader-bar></abp-loader-bar>
      <div *ngIf="loadingService.loading$ | async" class="global-spinner-overlay">
        <mat-spinner></mat-spinner>
      </div>
    <abp-dynamic-layout></abp-dynamic-layout>`
})

export class AppComponent implements OnInit {
  constructor(
    private navItems: NavItemsService,
    private signalR: SignalRService,
    public loadingService: LoadingService,
    private oAuthService: AuthService,
  ) {
    navItems.addItems([
      {
        id: 'Notification',
        order: 1,
        component: NotificationToolbarComponent,
      },
      {
        id: 'SignOutIcon',
        html: '<i class="fas fa-sign-out-alt fa-lg m-2 sign-out-icon"></i>',
        action: () => this.oAuthService.logout().subscribe(),
        order: 101, // puts as last element
      },
    ]);
  }

  ngOnInit(): void {
    this.signalR.initializeConnection();
    const authUrl = `${env.oAuthConfig.issuer}connect/authorize?` +
    `client_id=${env.oAuthConfig.clientId}&` +
    `redirect_uri=${env.oAuthConfig.redirectUri}&` +
    `response_type=${env.oAuthConfig.responseType}&` +
    `scope=${encodeURIComponent(env.oAuthConfig.scope)}`;
    console.log('Auth redirect URL:', authUrl);
    window.location.href = authUrl;

    const logData = [
      `OAuth config: ${JSON.stringify(env.oAuthConfig)}`,
      `Auth redirect URL: ${authUrl}`
    ].join('\n');

    const blob = new Blob([logData], { type: 'text/plain' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'auth-log.txt';
      document.body.appendChild(a);
      a.click();
      setTimeout(() => {
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        // Đặt redirect sau khi xong download, hẹn chút timeout cho chắc
        window.location.href = authUrl;
      }, 250);
    //change document title in environment.ts & index.html
    //here using Title service setTitle() with optionally ngAfterViewInit
  }
}
