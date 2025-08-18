import { Environment } from '@abp/ng.core';

const baseUrl = 'https://mynginx.store';

const oAuthConfig = {
  issuer: 'https://mynginx.store/',
  redirectUri: baseUrl,
  clientId: 'BookStore_App',
  responseType: 'code',
  scope: 'offline_access BookStore',
  requireHttps: true,
};

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'BookStore',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://mynginx.store',
      rootNamespace: 'Acme.BookStore',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'BookStore',
    },
  }
} as Environment;
