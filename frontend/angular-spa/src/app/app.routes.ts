import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { OrdersComponent } from './orders/orders.component';
import { AdminComponent } from './admin/admin.component';
import { ReportsComponent } from './reports/reports.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'orders', component: OrdersComponent },
  { path: 'admin', component: AdminComponent },
  { path: 'reports', component: ReportsComponent }
];
