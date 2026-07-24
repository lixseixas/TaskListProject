export interface WeeklyTaskReport {
  id: string;
  weekStartDate: Date;
  weekEndDate: Date;
  weekNumber: number;
  year: number;
  totalTasks: number;
  completedTasks: number;
  pendingTasks: number;
  completionPercentage: number;
}
