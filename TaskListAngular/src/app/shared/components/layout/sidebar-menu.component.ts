import { Component } from '@angular/core';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-sidebar-menu',
  standalone: true,
  imports: [MatSidenavModule, MatListModule, MatIconModule, RouterModule],
  template: `
    <mat-sidenav-container class="sidebar-container">
      <mat-sidenav mode="side" opened class="sidebar">
        <mat-nav-list>
          <a mat-list-item routerLink="/reports">
            <mat-icon mat-list-icon>assessment</mat-icon>
            <span mat-line>Reports</span>
          </a>
          <a mat-list-item routerLink="/reports/tasks-grouped-by-date">
            <mat-icon mat-list-icon>calendar_view_week</mat-icon>
            <span mat-line>Tasks Grouped by Date</span>
          </a>
        </mat-nav-list>
      </mat-sidenav>
      <mat-sidenav-content class="content">
        <ng-content></ng-content>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: `
    .sidebar-container {
      height: 100vh;
    }
    .sidebar {
      width: 250px;
      background: #f5f5f5;
    }
    .content {
      padding: 20px;
      height: 100%;
    }
  `
})
export class SidebarMenuComponent {}
