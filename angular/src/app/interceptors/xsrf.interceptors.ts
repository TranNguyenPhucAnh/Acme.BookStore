import { Injectable } from '@angular/core';
import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest
} from '@angular/common/http';
import { Observable } from 'rxjs';

function getCookie(name: string): string | null {
  const m = document.cookie.match(
    new RegExp('(?:^|; )' + name.replace(/([$?*|{}()[\]\\/+^])/g, '\\$1') + '=([^;]*)')
  );
  return m ? decodeURIComponent(m[1]) : null;
}

function isMutating(method: string): boolean {
  return method === 'POST' || method === 'PUT' || method === 'PATCH' || method === 'DELETE';
}

function isSameOrigin(url: string): boolean {
  // Hỗ trợ cả đường dẫn tương đối và tuyệt đối
  const u = new URL(url, window.location.origin);
  return u.origin === window.location.origin;
}

@Injectable()
export class XsrfInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    let cloned = req;

    if (isMutating(req.method) && isSameOrigin(req.url)) {
      const token = getCookie('XSRF-TOKEN'); // tên cookie bạn đang có trong log
      if (token && !req.headers.has('RequestVerificationToken')) {
        cloned = req.clone({
          withCredentials: true, // đảm bảo cookie đi kèm
          setHeaders: { RequestVerificationToken: token }
        });
      } else if (req.withCredentials !== true) {
        // vẫn bật withCredentials để cookie được gửi kèm
        cloned = req.clone({ withCredentials: true });
      }
    } else if (req.withCredentials !== true) {
      // tuỳ bạn: nếu muốn tất cả request đều gửi cookie
      cloned = req.clone({ withCredentials: true });
    }

    return next.handle(cloned);
  }
}
