import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { importProvidersFrom } from '@angular/core';
import { OAuthModule } from 'angular-oauth2-oidc';
import { routes } from './app/app.routes';

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    importProvidersFrom(
      OAuthModule.forRoot({
        resourceServer: {
          allowedUrls: ['http://localhost:5003/api'],
          sendAccessToken: true
        }
      })
    )
  ]
}).catch(err => console.error(err));
