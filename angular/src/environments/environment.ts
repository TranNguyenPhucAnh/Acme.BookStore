 import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'http://localhost:44374/',
  redirectUri: baseUrl,
  clientId: 'BookStore_App',
  responseType: 'code',
  scope: 'offline_access BookStore',
  requireHttps: false,
};

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: "BookStore",
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'http://localhost:44374',
      rootNamespace: 'Acme.BookStore',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
} as Environment;
