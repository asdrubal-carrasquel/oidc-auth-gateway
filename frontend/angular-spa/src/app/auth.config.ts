import { AuthConfig } from 'angular-oauth2-oidc';

export const authConfig: AuthConfig = {
  issuer: 'http://localhost:8082/realms/auth-gateway-realm',
  redirectUri: window.location.origin,
  clientId: 'angular-spa',
  responseType: 'code',
  scope: 'openid profile email roles',
  requireHttps: false, // Set to true in production
  showDebugInformation: true,
  // PKCE is automatically enabled when using responseType: 'code'
  // The library handles code challenge generation automatically
};
