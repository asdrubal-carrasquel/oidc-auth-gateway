import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { OAuthService } from 'angular-oauth2-oidc';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="reports-container">
      <h1>📊 Reports</h1>
      <p class="description">Endpoint: GET /api/admin/reports (Requires: Admin + IT + Working Hours 12:00-22:00 UTC)</p>
      <p class="time-info">Current UTC Time: {{ currentTime }}</p>

      <div *ngIf="loading" class="loading">Loading reports...</div>
      <div *ngIf="error" class="error">{{ error }}</div>

      <div *ngIf="reports" class="reports-list">
        <div class="report-card" *ngFor="let report of reports">
          <div class="report-header">
            <h3>{{ report.name }}</h3>
            <span class="report-type">{{ report.type }}</span>
          </div>
          <div class="report-details">
            <p><strong>Generated:</strong> {{ report.generatedAt | date:'medium' }}</p>
          </div>
        </div>
      </div>

      <div *ngIf="metadata" class="metadata">
        <h3>Request Metadata</h3>
        <pre>{{ metadata | json }}</pre>
      </div>

      <button (click)="loadReports()" class="btn btn-primary" [disabled]="loading">
        Refresh Reports
      </button>
    </div>
  `,
  styles: [`
    .reports-container {
      max-width: 1000px;
      margin: 0 auto;
    }
    .description {
      color: #666;
      margin-bottom: 0.5rem;
    }
    .time-info {
      color: #999;
      font-size: 0.875rem;
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
    .reports-list {
      display: grid;
      gap: 1.5rem;
      margin: 2rem 0;
    }
    .report-card {
      background: white;
      padding: 1.5rem;
      border-radius: 8px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    }
    .report-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
    }
    .report-type {
      padding: 0.25rem 0.75rem;
      background: #e3f2fd;
      color: #1976d2;
      border-radius: 12px;
      font-size: 0.875rem;
      font-weight: 500;
    }
    .metadata {
      background: #f5f5f5;
      padding: 1.5rem;
      border-radius: 8px;
      margin: 2rem 0;
    }
    .metadata pre {
      background: white;
      padding: 1rem;
      border-radius: 4px;
      overflow-x: auto;
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
export class ReportsComponent implements OnInit {
  reports: any[] = [];
  metadata: any = null;
  loading = false;
  error: string | null = null;
  currentTime = '';

  constructor(
    private http: HttpClient,
    private oauthService: OAuthService
  ) {
    this.updateTime();
    setInterval(() => this.updateTime(), 1000);
  }

  ngOnInit(): void {
    this.loadReports();
  }

  private updateTime(): void {
    this.currentTime = new Date().toUTCString();
  }

  loadReports(): void {
    if (!this.oauthService.hasValidAccessToken()) {
      this.error = 'Not authenticated. Please login first.';
      return;
    }

    this.loading = true;
    this.error = null;

    const token = this.oauthService.getAccessToken();

    this.http.get<any>('http://localhost:5003/api/admin/reports', {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    })
      .subscribe({
        next: (response) => {
          this.reports = response.reports || [];
          this.metadata = response.metadata;
          this.loading = false;
        },
        error: (err) => {
          console.error('Error loading reports:', err);
          this.error = err.error?.message || err.message || 'Failed to load reports. Check your permissions and working hours (12:00-22:00 UTC).';
          if (err.status === 0) {
            this.error = 'CORS error or service unavailable. Check console for details.';
          }
          this.loading = false;
        }
      });
  }
}
