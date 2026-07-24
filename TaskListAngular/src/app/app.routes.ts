import { Routes } from '@angular/router';
import { TasksGroupedByDateComponent } from './features/reports/components/tasks-grouped-by-date/tasks-grouped-by-date.component';

export const routes: Routes = [
  {
    path: 'reports/tasks-grouped-by-date',
    component: TasksGroupedByDateComponent
  },
  {
    path: 'reports',
    redirectTo: 'reports/tasks-grouped-by-date',
    pathMatch: 'full'
  },
  {
    path: '',
    redirectTo: 'reports/tasks-grouped-by-date',
    pathMatch: 'full'
  }
];
