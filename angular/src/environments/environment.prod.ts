import { Environment } from '@abp/ng.core';

const baseUrl = 'https://bookstore-demo.online';

const oAuthConfig = {
  issuer: 'https://bookstore-demo.online/',
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
      url: 'https://bookstore-demo.online',
      rootNamespace: 'Acme.BookStore',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'BookStore',
    },
  },
  remoteEnv: {
    url: '/getEnvConfig',
    mergeStrategy: 'deepmerge'
  }
} as Environment;