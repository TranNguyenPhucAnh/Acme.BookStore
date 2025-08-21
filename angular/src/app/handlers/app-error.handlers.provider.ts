import { ErrorHandler } from "@angular/core";
import { AppErrorHandler } from "./app-error.handler";

export const ERROR_HANDLER_PROVIDER = [
    { provide: ErrorHandler, useClass: AppErrorHandler, multi: true }
]