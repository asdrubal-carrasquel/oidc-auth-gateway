import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { authConfig } from './auth.config';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar">
      <div class="nav-container">
        <h1 class="nav-title">🔐 Auth Gateway</h1>
        <div class="nav-links">
          <a routerLink="/" routerLinkActive="active">Home</a>
          <a routerLink="/orders" routerLinkActive="active">Orders</a>
          <a routerLink="/admin" routerLinkActive="active">Admin</a>
          <a routerLink="/reports" routerLinkActive="active">Reports</a>
        </div>
        <div class="nav-auth">
          <span *ngIf="isAuthenticated" class="user-info">
            👤 {{ username }}
          </span>
          <button *ngIf="!isAuthenticated" (click)="login()" class="btn btn-primary">
            Login
          </button>
          <button *ngIf="isAuthenticated" (click)="logout()" class="btn btn-secondary">
            Logout
          </button>
        </div>
      </div>
    </nav>

    <main class="main-content">
      <router-outlet></router-outlet>
    </main>

    <footer class="footer">
      <p>OIDC/OAuth2 + RBAC/ABAC Demo | Built with Angular & .NET 8</p>
    </footer>
  `,
  styles: [`
    .navbar {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      padding: 1rem 2rem;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    }
    .nav-container {
      max-width: 1200px;
      margin: 0 auto;
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
    }
    .nav-title {
      margin: 0;
      font-size: 1.5rem;
    }
    .nav-links {
      display: flex;
      gap: 1.5rem;
    }
    .nav-links a {
      color: white;
      text-decoration: none;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      transition: background 0.3s;
    }
    .nav-links a:hover, .nav-links a.active {
      background: rgba(255,255,255,0.2);
    }
    .nav-auth {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    .user-info {
      font-weight: 500;
    }
    .btn {
      padding: 0.5rem 1.5rem;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 500;
      transition: all 0.3s;
    }
    .btn-primary {
      background: white;
      color: #667eea;
    }
    .btn-primary:hover {
      background: #f0f0f0;
    }
    .btn-secondary {
      background: rgba(255,255,255,0.2);
      color: white;
    }
    .btn-secondary:hover {
      background: rgba(255,255,255,0.3);
    }
    .main-content {
      min-height: calc(100vh - 200px);
      padding: 2rem;
      max-width: 1200px;
      margin: 0 auto;
    }
    .footer {
      background: #f5f5f5;
      text-align: center;
      padding: 2rem;
      margin-top: 2rem;
    }
  `]
})
export class AppComponent implements OnInit {
  isAuthenticated = false;
  username = '';

  constructor(
    private oauthService: OAuthService,
    private http: HttpClient
  ) {
    this.configureOAuth();
  }

  ngOnInit(): void {
    this.oauthService.loadDiscoveryDocumentAndTryLogin().then(() => {
      this.isAuthenticated = this.oauthService.hasValidAccessToken();
      if (this.isAuthenticated) {
        this.loadUserInfo();
      }
    });

    this.oauthService.events.subscribe(event => {
      if (event.type === 'token_received') {
        this.isAuthenticated = true;
        this.loadUserInfo();
      }
    });
  }

  private configureOAuth(): void {
    this.oauthService.configure(authConfig);
    this.oauthService.setupAutomaticSilentRefresh();
  }

  login(): void {
    this.oauthService.initCodeFlow();
  }

  logout(): void {
    this.oauthService.logOut();
    this.isAuthenticated = false;
    this.username = '';
  }

  private loadUserInfo(): void {
    const claims = this.oauthService.getIdentityClaims();
    if (claims) {
      this.username = (claims as any)['preferred_username'] || 'User';
    }
  }
}
