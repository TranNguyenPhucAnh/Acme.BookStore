import type { AuthorLookupDto, BookDto, BookGetListInput, CreateUpdateBookDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import { HttpResponse } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class BookService {
  apiName = 'Default';
  
  create = (input: CreateUpdateBookDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BookDto>({
      method: 'POST',
      url: '/api/app/book',
      body: input,
    },
    { apiName: this.apiName,...config });
  
  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/book/${id}`
    },
    { apiName: this.apiName,...config });
  
  export = (input?: BookGetListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HttpResponse<Blob>>({
      method: 'POST',
      url: '/api/app/book/export',
      body: input,
      responseType: 'blob'
    },
    { apiName: this.apiName,...config,
      observe: Rest.Observe.Response
    });
    
  import = (input: FormData, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HttpResponse<Blob>>({
      method: 'POST',
      url: '/api/app/book/import',
      body: input,
      responseType: 'blob'
    },
    { apiName: this.apiName,...config,
      observe: Rest.Observe.Response
    });

  upload = (input: FormData, bookId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, boolean>({
      method: 'POST',
      url: `/api/app/book/upload/${bookId}`,
      body: input
    },
    { apiName: this.apiName,...config });

  download = (bookId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, HttpResponse<Blob>>({
      method: 'POST',
      url: `/api/app/book/download/${bookId}`,
      responseType: 'blob'
    },
    { apiName: this.apiName,...config,
      observe: Rest.Observe.Response
    });

  downloadSample = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, HttpResponse<Blob>>({
      method: 'POST',
      url: `/api/app/book/download-sample`,
      responseType: 'blob'
    },
    { apiName: this.apiName,...config,
      observe: Rest.Observe.Response
    });

  preview = (bookId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, string>({
      method: 'POST',
      url: `/api/app/book/preview/${bookId}`,
      responseType: 'text' //httpclient does not parse/deserialize response body
    },
    { apiName: this.apiName,...config });

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BookDto>({
      method: 'GET',
      url: `/api/app/book/${id}`,
    },
    { apiName: this.apiName,...config });
  
  getAuthorLookup = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<AuthorLookupDto>>({
      method: 'GET',
      url: '/api/app/book/author-lookup',
    },
    { apiName: this.apiName,...config });
  
  getList = (input: BookGetListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<BookDto>>({
      method: 'GET',
      url: '/api/app/book',
      params: { filter: input.filter, ["PublishDate.Min"]: input.publishDate.min, ["PublishDate.Max"]: input.publishDate.max, type: input.type, skipCount: input.skipCount, maxResultCount: input.maxResultCount, sorting: input.sorting, combineWith: input.combineWith },
    },
    { apiName: this.apiName,...config });
  
  getMinDateTime = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, string>({
      method: 'GET',
      url: '/api/app/book/min-date-time',
      responseType: 'json' // JSON.parse(response.body)
    },
    { apiName: this.apiName,...config });
  
  update = (id: string, input: CreateUpdateBookDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BookDto>({
      method: 'PUT',
      url: `/api/app/book/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
