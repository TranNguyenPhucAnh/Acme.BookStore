import { Injectable, OnDestroy } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { Constants } from './shared/constants/constant';

@Injectable({
  providedIn: 'root',
})
export class SignalRService implements OnDestroy {
  private hubConnections: Map<string, HubConnection> = new Map();
  private connectionStates: Map<string, BehaviorSubject<boolean>> = new Map();
  private destroy$ = new Subject<void>();
  private readonly hubUrls = [
    Constants.EnvironmentUrl + Constants.NotificationHubUrl,
    Constants.EnvironmentUrl + Constants.EntityHubUrl, 
    //add more hub urls here
  ];

  constructor() {
    // Có thể khởi tạo ở đây hoặc trong AppComponent
  }

  // Khởi tạo kết nối cho tất cả hub
  initializeConnection(): void {
    this.hubUrls.forEach((hubUrl) => {
      if (this.hubConnections.has(hubUrl)) {
        return;
      }

      const hubConnection = new HubConnectionBuilder()
        .withUrl(hubUrl)
        .configureLogging(LogLevel.Information)
        .withAutomaticReconnect()
        .build();

      this.hubConnections.set(hubUrl, hubConnection);
      this.connectionStates.set(hubUrl, new BehaviorSubject<boolean>(false));

      hubConnection
        .start()
        .then(() => {
          this.connectionStates.get(hubUrl)!.next(true);
          console.log(`SignalR connected to ${hubUrl}`);
        })
        .catch((err) => console.error(`SignalR connection error for ${hubUrl}:`, err));

      hubConnection.onclose(() => {
        this.connectionStates.get(hubUrl)!.next(false);
        console.log(`SignalR disconnected from ${hubUrl}`);
      });
    });
  }

  // Kiểm tra trạng thái kết nối
  isConnected(hubUrl: string): boolean {
    return this.connectionStates.get(hubUrl)?.value || false;
  }

  // Lắng nghe sự kiện generic
  addListener<T>(hubUrl: string, eventName: string, callback: (data: T) => void): void {
    const hubConnection = this.hubConnections.get(hubUrl);
    if (!hubConnection) {
      console.error(`SignalR connection not initialized for ${hubUrl}`);
      return;
    }
    hubConnection.on(eventName, callback);
    this.destroy$.subscribe(() => hubConnection.off(eventName, callback));
  }

  // Gửi thông điệp generic
  send<T>(hubUrl: string, methodName: string, data: T): Promise<void> {
    const hubConnection = this.hubConnections.get(hubUrl);
    if (!this.isConnected(hubUrl) || !hubConnection) {
      return Promise.reject(`SignalR connection is not established for ${hubUrl}`);
    }
    return hubConnection
      .invoke(methodName, data)
      .catch((err) => {
        console.error(`Error invoking ${methodName} on ${hubUrl}:`, err);
        throw err;
      });
  }

  // Hủy kết nối và listener
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.hubConnections.forEach((connection, hubUrl) => {
      connection.stop().then(() => console.log(`SignalR connection stopped for ${hubUrl}`));
    });
    this.hubConnections.clear();
    this.connectionStates.clear();
  }

  // Observable để theo dõi trạng thái kết nối
  getConnectionState(hubUrl: string): Observable<boolean> {
    return this.connectionStates.get(hubUrl)?.asObservable() || new BehaviorSubject<boolean>(false).asObservable();
  }
}