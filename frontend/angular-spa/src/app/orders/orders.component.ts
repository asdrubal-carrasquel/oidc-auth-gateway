import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { OAuthService } from 'angular-oauth2-oidc';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './orders.component.html',
  styleUrls: ['./orders.component.css']
})
export class OrdersComponent implements OnInit {
  orders: any[] = [];
  metadata: any = null;
  loading = false;
  error: string | null = null;

  constructor(
    private http: HttpClient,
    private oauthService: OAuthService
  ) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    if (!this.oauthService.hasValidAccessToken()) {
      this.error = 'Not authenticated. Please login first.';
      return;
    }

    this.loading = true;
    this.error = null;

    // Get access token and add to headers
    const token = this.oauthService.getAccessToken();
    
    this.http.get<any>('http://localhost:5003/api/orders', {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    })
      .subscribe({
        next: (response) => {
          this.orders = response.orders || [];
          this.metadata = response.metadata;
          this.loading = false;
        },
        error: (err) => {
          console.error('Error loading orders:', err);
          this.error = err.error?.message || err.message || 'Failed to load orders';
          if (err.status === 0) {
            this.error = 'CORS error or service unavailable. Check console for details.';
          }
          this.loading = false;
        }
      });
  }
}
