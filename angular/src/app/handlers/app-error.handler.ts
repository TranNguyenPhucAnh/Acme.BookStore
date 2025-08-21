import { ErrorHandler, Injectable } from '@angular/core';

@Injectable()
export class AppErrorHandler implements ErrorHandler {
  handleError(error: any) {
    console.error('Global error:', error);
    // ở đây bạn có thể gửi log ra server hoặc hiển thị user-friendly message
  }
}
