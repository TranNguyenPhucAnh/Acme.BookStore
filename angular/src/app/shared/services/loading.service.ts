import { Injectable } from "@angular/core";
import { BehaviorSubject, debounceTime, distinctUntilChanged } from "rxjs";

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private loadingSubject = new BehaviorSubject<boolean>(false); //luồng dữ liệu động multicast & giữ lại current value 
  loading$ = this.loadingSubject.asObservable()
  .pipe(
    debounceTime(250), // delay để tránh nháy spinner
    distinctUntilChanged()
  );;

  private requestCount = 0;

  show() {
    this.requestCount++; // tăng khi có request
    // nếu là request đầu tiên, thì bật emit true để hiện spinner
    if (this.requestCount === 1) { 
      this.loadingSubject.next(true);
    }
  }

  hide() {
    //khi request kết thúc thì giảm requestCount, e.g. nếu forkJoin/mergeMap 2 API, 1 request kết thúc vẫn hiện spinner
    this.requestCount = Math.max(0, this.requestCount - 1);
    //Nếu không còn request nào đang chạy, thì emit false để ẩn spinner
    if (this.requestCount === 0) {
      this.loadingSubject.next(false);
    }
  }
}