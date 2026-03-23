import { AbpWindowService, AuthService, EnvironmentService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { Component, OnInit } from '@angular/core';
import { BookService } from '@proxy/books';
import { DateHelper } from '../shared/helpers/dates.utility';

@Component({
  standalone: false,
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent implements OnInit {
  envUrl: string;

  get hasLoggedIn(): boolean {
    return this.authService.isAuthenticated
  }

  constructor(
    private authService: AuthService,
    private bookService: BookService,
    private toasterService: ToasterService,
    private abpWindowService: AbpWindowService,
    private envService: EnvironmentService
  ) {}

  ngOnInit(): void {
    this.envUrl = this.envService.getEnvironment().apis.default.url;
  }

  download() : void {
    this.bookService.downloadSample().subscribe((val) => {
        this.abpWindowService.downloadBlob(val.body, DateHelper.toFileName(val.headers.get('Content-Disposition')));
        this.toasterService.success('File is ready for download')
    })
  }

  login() {
    this.authService.navigateToLogin();
  }
}
