import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { WeeklyTaskReport } from '../models/weekly-task-report.model';

@Injectable({
  providedIn: 'root'
})
export class TaskReportQueryService {
  private readonly apiUrl = 'https://localhost:44322/api/TaskReport/weekly';

  constructor(private http: HttpClient) {}

  getWeeklyReports(startDate: Date, endDate: Date): Observable<WeeklyTaskReport[]> {
    const startDateStr = startDate.toISOString().split('T')[0];
    const endDateStr = endDate.toISOString().split('T')[0];
    const url = `${this.apiUrl}?startDate=${startDateStr}&endDate=${endDateStr}`;
    console.log('API URL:', url);
    return this.http.get<any[]>(url).pipe(
      map(data => {
        console.log('Raw API response:', data);
        return data.map(item => ({
          ...item,
          weekStartDate: new Date(item.weekStartDate),
          weekEndDate: new Date(item.weekEndDate)
        }));
      })
    );
  }
}
