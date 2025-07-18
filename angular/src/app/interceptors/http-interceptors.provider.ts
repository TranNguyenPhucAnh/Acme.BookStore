import { HTTP_INTERCEPTORS } from "@angular/common/http";
import { SpinnerInterceptor } from "./spinner.interceptors";

export const HTTP_INTERCEPTOR_PROVIDER = [
    { provide: HTTP_INTERCEPTORS, useClass: SpinnerInterceptor, multi: true }
]