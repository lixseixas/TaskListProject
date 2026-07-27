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
  templateUrl: './tasks-grouped-by-date.component.html',
  styleUrl: './tasks-grouped-by-date.component.css'
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
