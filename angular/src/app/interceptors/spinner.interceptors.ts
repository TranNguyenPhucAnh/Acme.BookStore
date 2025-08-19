import { Injectable } from "@angular/core";
import { LoadingService } from "../shared/services/loading.service";
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { finalize, Observable } from "rxjs";

@Injectable()
export class SpinnerInterceptor implements HttpInterceptor {
  constructor(private loadingService: LoadingService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    console.log('Outgoing request, url and headers:', req.url, req.headers);

    const bypassUrls = [
      '/connect/authorize',
      '/Account/Login'
    ];

    const shouldBypass = bypassUrls.some(url => req.url.includes(url));

    console.log('Should bypass:', shouldBypass);

    const modifiedReq = shouldBypass
      ? req.clone({ setHeaders: { 'X-Silent-Request': 'true' } })
      : req;

    //Request có header không ảnh hưởng giao diện như background sync, api tracking/log, refresh token, preload,...
    const isSilent = req.headers.get('X-Silent-Request') === 'true';
    
    //phù hợp long-latency request như export, download, submit form,...hoặc gọi API parallel 
    if (!isSilent) {
      //Nếu gọi tiếp 1 request trước khi request trước đó kết thúc
      //từng request sẽ lần lượt gọi show() dồn dập, không đến lượt hide()
      //phải đến khi angular thực thi xong các dòng code chạy lệnh gọi, mới đến lượt hide() được interceptor gọi
      //Vì vậy spinner không bị tắt rồi bật lại, mà sáng liên tục → tạo cảm giác liền mạch, không có khoảng dừng (to be test)
      this.loadingService.show();
    }

    return next.handle(modifiedReq) //chuyển request xuống backend như bình thường
    .pipe(
      finalize(() => { //luôn được gọi cuối cùng bất kể request `thành công` hay `thất bại`
        if (!isSilent) {
          this.loadingService.hide();
        }
      })
    );
  }
}
