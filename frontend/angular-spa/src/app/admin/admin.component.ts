import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { OAuthService } from 'angular-oauth2-oidc';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="admin-container">
      <h1>⚙️ Admin Panel</h1>
      <p class="description">Endpoint: GET /api/admin (Requires: Admin role + IT department + CL country)</p>

      <div *ngIf="loading" class="loading">Loading admin data...</div>
      <div *ngIf="error" class="error">{{ error }}</div>

      <div *ngIf="adminData" class="admin-content">
        <div class="info-card">
          <h2>System Information</h2>
          <pre>{{ adminData.systemInfo | json }}</pre>
        </div>

        <div class="info-card">
          <h2>User Information</h2>
          <pre>{{ adminData.user | json }}</pre>
        </div>

        <div class="info-card" *ngIf="users">
          <h2>Users</h2>
          <div class="users-list">
            <div class="user-item" *ngFor="let user of users">
              <strong>{{ user.username }}</strong> - {{ user.email }} ({{ user.role }})
            </div>
          </div>
        </div>

        <div class="info-card" *ngIf="settings">
          <h2>Settings</h2>
          <pre>{{ settings | json }}</pre>
        </div>
      </div>

      <button (click)="loadAdminData()" class="btn btn-primary" [disabled]="loading">
        Refresh Data
      </button>
    </div>
  `,
  styles: [`
    .admin-container {
      max-width: 1000px;
      margin: 0 auto;
    }
    .description {
      color: #666;
      margin-bottom: 2rem;
    }
    .loading, .error {
      padding: 2rem;
      text-align: center;
      background: #f5f5f5;
      border-radius: 8px;
      margin: 2rem 0;
    }
    .error {
      background: #ffebee;
      color: #c62828;
    }
    .admin-content {
      display: grid;
      gap: 1.5rem;
      margin: 2rem 0;
    }
    .info-card {
      background: white;
      padding: 1.5rem;
      border-radius: 8px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    }
    .info-card h2 {
      margin-top: 0;
      color: #667eea;
    }
    .info-card pre {
      background: #f5f5f5;
      padding: 1rem;
      border-radius: 4px;
      overflow-x: auto;
    }
    .users-list {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
    .user-item {
      padding: 0.75rem;
      background: #f9f9f9;
      border-radius: 4px;
    }
    .btn {
      padding: 0.75rem 1.5rem;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 500;
      transition: all 0.3s;
      background: #667eea;
      color: white;
    }
    .btn:hover:not(:disabled) {
      background: #5568d3;
    }
    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
  `]
})
export class AdminComponent implements OnInit {
  adminData: any = null;
  users: any[] = [];
  settings: any = null;
  loading = false;
  error: string | null = null;

  constructor(
    private http: HttpClient,
    private oauthService: OAuthService
  ) {}

  ngOnInit(): void {
    this.loadAdminData();
  }

  loadAdminData(): void {
    if (!this.oauthService.hasValidAccessToken()) {
      this.error = 'Not authenticated. Please login first.';
      return;
    }

    this.loading = true;
    this.error = null;

    const token = this.oauthService.getAccessToken();
    const headers = { 'Authorization': `Bearer ${token}` };

    this.http.get<any>('http://localhost:5003/api/admin', { headers })
      .subscribe({
        next: (response) => {
          this.adminData = response;
          this.loading = false;
        },
        error: (err) => {
          console.error('Error loading admin data:', err);
          this.error = err.error?.message || err.message || 'Failed to load admin data. Check your permissions.';
          if (err.status === 0) {
            this.error = 'CORS error or service unavailable. Check console for details.';
          }
          this.loading = false;
        }
      });

    this.http.get<any[]>('http://localhost:5003/api/admin/users', { headers })
      .subscribe({
        next: (response) => {
          this.users = response;
        },
        error: () => {
          // Silently fail for users endpoint
        }
      });

    this.http.get<any>('http://localhost:5003/api/admin/settings', { headers })
      .subscribe({
        next: (response) => {
          this.settings = response;
        },
        error: () => {
          // Silently fail for settings endpoint
        }
      });
  }
}
