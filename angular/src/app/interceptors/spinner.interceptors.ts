import { Injectable } from "@angular/core";
import { LoadingService } from "../shared/services/loading.service";
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { finalize, Observable } from "rxjs";

@Injectable()
export class SpinnerInterceptor implements HttpInterceptor {
  constructor(private loadingService: LoadingService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const bypassUrls = [
      'https://mynginx.store/connect/authorize',
      'https://mynginx.store/Account/Login',
      'https://mynginx.store/.well-known/openid-configuration',
      'https://mynginx.store/.well-known/jwks',
      'https://mynginx.store/api/abp/application-configuration',
      'https://mynginx.store/api/abp/application-localization',
    ];

    const shouldBypass = bypassUrls.some(url => req.url.startsWith(url));

    //phù hợp long-latency request như export, download, submit form,...hoặc gọi API parallel 
    if (!shouldBypass) {
      //Nếu gọi tiếp 1 request trước khi request trước đó kết thúc
      //từng request sẽ lần lượt gọi show() dồn dập, không đến lượt hide()
      //phải đến khi angular thực thi xong các dòng code chạy lệnh gọi, mới đến lượt hide() được interceptor gọi
      //Vì vậy spinner không bị tắt rồi bật lại, mà sáng liên tục → tạo cảm giác liền mạch, không có khoảng dừng (to be test)
      this.loadingService.show();
    }

    return next.handle(req) //chuyển request xuống backend như bình thường
    .pipe(
      finalize(() => { //luôn được gọi cuối cùng bất kể request `thành công` hay `thất bại`
        if (!shouldBypass) {
          this.loadingService.hide();
        }
      })
    );
  }
}
