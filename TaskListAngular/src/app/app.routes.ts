import { Routes } from '@angular/router';
import { TasksGroupedByDateComponent } from './features/reports/components/tasks-grouped-by-date/tasks-grouped-by-date.component';
import { LoginComponent } from './features/login/login.component';
import { authGuard } from './core/services/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'reports/tasks-grouped-by-date',
    component: TasksGroupedByDateComponent,
    canActivate: [authGuard]
  },
  {
    path: 'reports',
    redirectTo: 'reports/tasks-grouped-by-date',
    pathMatch: 'full'
  },
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  }
];
