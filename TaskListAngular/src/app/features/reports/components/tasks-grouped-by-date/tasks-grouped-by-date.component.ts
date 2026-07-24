import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableDataSource } from '@angular/material/table';
import { provideNativeDateAdapter } from '@angular/material/core';
import { TaskReportQueryService } from '../../../../core/services/task-report-query.service';
import { WeeklyTaskReport } from '../../../../core/models/weekly-task-report.model';

@Component({
  selector: 'app-tasks-grouped-by-date',
  standalone: true,
  providers: [provideNativeDateAdapter()],
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatDatepickerModule,
    MatInputModule,
    MatFormFieldModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="tasks-grouped-by-date">
      <h2>Tasks Grouped by Date</h2>
      
      <div class="date-filters">
        <mat-form-field>
          <mat-label>Start Date</mat-label>
          <input matInput [matDatepicker]="startPicker" [(ngModel)]="startDate">
          <mat-datepicker-toggle matSuffix [for]="startPicker"></mat-datepicker-toggle>
          <mat-datepicker #startPicker></mat-datepicker>
        </mat-form-field>
        
        <mat-form-field>
          <mat-label>End Date</mat-label>
          <input matInput [matDatepicker]="endPicker" [(ngModel)]="endDate">
          <mat-datepicker-toggle matSuffix [for]="endPicker"></mat-datepicker-toggle>
          <mat-datepicker #endPicker></mat-datepicker>
        </mat-form-field>
        
        <button mat-raised-button color="primary" (click)="loadReports()">Load Reports</button>
      </div>

      <div *ngIf="loading" class="loading-container">
        <mat-spinner></mat-spinner>
      </div>

      <div *ngIf="!loading" class="debug-info">
        <p>Debug: Loading = {{ loading }}</p>
        <p>Debug: Data length = {{ dataSource.data.length }}</p>
        <p>Debug: Data = {{ dataSource.data | json }}</p>
      </div>

      <div *ngIf="!loading && dataSource.data.length > 0" class="table-container">
        <table mat-table [dataSource]="dataSource" class="mat-elevation-z8">
          <ng-container matColumnDef="weekNumber">
            <th mat-header-cell *matHeaderCellDef>Week</th>
            <td mat-cell *matCellDef="let element">{{ element.weekNumber }}</td>
          </ng-container>

          <ng-container matColumnDef="year">
            <th mat-header-cell *matHeaderCellDef>Year</th>
            <td mat-cell *matCellDef="let element">{{ element.year }}</td>
          </ng-container>

          <ng-container matColumnDef="weekStartDate">
            <th mat-header-cell *matHeaderCellDef>Start Date</th>
            <td mat-cell *matCellDef="let element">{{ element.weekStartDate | date:'mediumDate' }}</td>
          </ng-container>

          <ng-container matColumnDef="weekEndDate">
            <th mat-header-cell *matHeaderCellDef>End Date</th>
            <td mat-cell *matCellDef="let element">{{ element.weekEndDate | date:'mediumDate' }}</td>
          </ng-container>

          <ng-container matColumnDef="totalTasks">
            <th mat-header-cell *matHeaderCellDef>Total Tasks</th>
            <td mat-cell *matCellDef="let element">{{ element.totalTasks }}</td>
          </ng-container>

          <ng-container matColumnDef="completedTasks">
            <th mat-header-cell *matHeaderCellDef>Completed</th>
            <td mat-cell *matCellDef="let element">{{ element.completedTasks }}</td>
          </ng-container>

          <ng-container matColumnDef="pendingTasks">
            <th mat-header-cell *matHeaderCellDef>Pending</th>
            <td mat-cell *matCellDef="let element">{{ element.pendingTasks }}</td>
          </ng-container>

          <ng-container matColumnDef="completionPercentage">
            <th mat-header-cell *matHeaderCellDef>Completion %</th>
            <td mat-cell *matCellDef="let element">{{ element.completionPercentage }}%</td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
      </div>

      <div *ngIf="!loading && dataSource.data.length === 0" class="no-data">
        <p>No reports found for the selected date range.</p>
      </div>
    </div>
  `,
  styles: `
    .tasks-grouped-by-date {
      padding: 20px;
    }
    .date-filters {
      display: flex;
      gap: 20px;
      align-items: center;
      margin-bottom: 20px;
      flex-wrap: wrap;
    }
    .table-container {
      margin-top: 20px;
      overflow-x: auto;
    }
    .loading-container {
      display: flex;
      justify-content: center;
      padding: 40px;
    }
    .no-data {
      text-align: center;
      padding: 40px;
      color: #666;
    }
  `
})
export class TasksGroupedByDateComponent implements OnInit {
  dataSource = new MatTableDataSource<WeeklyTaskReport>();
  loading = false;
  startDate: Date = new Date(new Date().getFullYear(), 0, 1);
  endDate: Date = new Date(new Date().getFullYear() + 1, 0, 1);
  displayedColumns: string[] = [
    'weekNumber',
    'year',
    'weekStartDate',
    'weekEndDate',
    'totalTasks',
    'completedTasks',
    'pendingTasks',
    'completionPercentage'
  ];

  constructor(private taskReportQueryService: TaskReportQueryService) {}

  ngOnInit(): void {
    this.loadReports();
  }

  loadReports(): void {
    this.loading = true;
    console.log('Loading reports with dates:', this.startDate, this.endDate);
    this.taskReportQueryService.getWeeklyReports(this.startDate, this.endDate).subscribe({
      next: (data) => {
        console.log('Received data:', data);
        console.log('Data length:', data.length);
        this.dataSource.data = data;
        console.log('DataSource data:', this.dataSource.data);
        console.log('DataSource data length:', this.dataSource.data.length);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading reports:', error);
        this.loading = false;
      }
    });
  }
}
