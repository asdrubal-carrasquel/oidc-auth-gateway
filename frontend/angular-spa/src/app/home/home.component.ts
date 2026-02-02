import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OAuthService } from 'angular-oauth2-oidc';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="home-container">
      <div class="hero">
        <h1>🔐 Auth Gateway Demo</h1>
        <p class="subtitle">OIDC/OAuth2 + RBAC/ABAC Implementation</p>
      </div>

      <div class="info-cards">
        <div class="card">
          <h2>📋 Architecture</h2>
          <ul>
            <li>Angular SPA (Frontend)</li>
            <li>Keycloak (OIDC Provider)</li>
            <li>Auth Gateway / BFF (.NET 8)</li>
            <li>Microservices (Orders API, Admin API)</li>
          </ul>
        </div>

        <div class="card">
          <h2>🔑 Authentication</h2>
          <ul>
            <li>OAuth2 Authorization Code Flow</li>
            <li>PKCE (Proof Key for Code Exchange)</li>
            <li>JWT Bearer Tokens</li>
            <li>OpenID Connect</li>
          </ul>
        </div>

        <div class="card">
          <h2>🛡️ Authorization</h2>
          <ul>
            <li><strong>RBAC:</strong> Roles (User, Admin, Support)</li>
            <li><strong>ABAC:</strong> Attributes (country, department, tenant)</li>
            <li>Combined Policies</li>
            <li>Dynamic Rules (working hours)</li>
          </ul>
        </div>
      </div>

      <div class="user-info" *ngIf="isAuthenticated">
        <h2>👤 Your Token Claims</h2>
        <pre>{{ tokenInfo | json }}</pre>
      </div>

      <div class="endpoints" *ngIf="isAuthenticated">
        <h2>🧪 Test Endpoints</h2>
        <div class="endpoint-list">
          <div class="endpoint-item">
            <strong>GET /api/orders</strong>
            <span class="badge badge-info">User + country=CL</span>
          </div>
          <div class="endpoint-item">
            <strong>POST /api/orders</strong>
            <span class="badge badge-warning">Admin</span>
          </div>
          <div class="endpoint-item">
            <strong>GET /api/admin</strong>
            <span class="badge badge-danger">Admin + IT + CL</span>
          </div>
          <div class="endpoint-item">
            <strong>GET /api/admin/reports</strong>
            <span class="badge badge-danger">Admin + IT + Working Hours</span>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .home-container {
      max-width: 1200px;
      margin: 0 auto;
    }
    .hero {
      text-align: center;
      padding: 3rem 0;
    }
    .hero h1 {
      font-size: 3rem;
      margin-bottom: 1rem;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }
    .subtitle {
      font-size: 1.25rem;
      color: #666;
    }
    .info-cards {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 2rem;
      margin: 2rem 0;
    }
    .card {
      background: white;
      padding: 2rem;
      border-radius: 8px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    }
    .card h2 {
      margin-top: 0;
      color: #667eea;
    }
    .card ul {
      list-style: none;
      padding: 0;
    }
    .card li {
      padding: 0.5rem 0;
      border-bottom: 1px solid #f0f0f0;
    }
    .user-info {
      background: white;
      padding: 2rem;
      border-radius: 8px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
      margin: 2rem 0;
    }
    .user-info pre {
      background: #f5f5f5;
      padding: 1rem;
      border-radius: 4px;
      overflow-x: auto;
    }
    .endpoints {
      background: white;
      padding: 2rem;
      border-radius: 8px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
      margin: 2rem 0;
    }
    .endpoint-list {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    .endpoint-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1rem;
      background: #f9f9f9;
      border-radius: 4px;
    }
    .badge {
      padding: 0.25rem 0.75rem;
      border-radius: 12px;
      font-size: 0.875rem;
      font-weight: 500;
    }
    .badge-info {
      background: #e3f2fd;
      color: #1976d2;
    }
    .badge-warning {
      background: #fff3e0;
      color: #f57c00;
    }
    .badge-danger {
      background: #ffebee;
      color: #c62828;
    }
  `]
})
export class HomeComponent {
  isAuthenticated = false;
  tokenInfo: any = {};

  constructor(private oauthService: OAuthService) {
    this.oauthService.events.subscribe(event => {
      if (event.type === 'token_received') {
        this.loadTokenInfo();
      }
    });
    
    if (this.oauthService.hasValidAccessToken()) {
      this.isAuthenticated = true;
      this.loadTokenInfo();
    }
  }

  private loadTokenInfo(): void {
    const claims = this.oauthService.getIdentityClaims();
    const accessToken = this.oauthService.getAccessToken();
    
    if (accessToken) {
      try {
        const payload = JSON.parse(atob(accessToken.split('.')[1]));
        this.tokenInfo = {
          sub: payload.sub,
          preferred_username: payload.preferred_username,
          roles: payload.realm_access?.roles || payload.roles || [],
          country: payload.country,
          department: payload.department,
          tenant: payload.tenant,
          workingHours: payload.workingHours,
          exp: new Date(payload.exp * 1000).toISOString()
        };
        this.isAuthenticated = true;
      } catch (e) {
        console.error('Error parsing token', e);
      }
    }
  }
}
