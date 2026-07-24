Create angular v20
Name TaskListAngular

Must contains
Angualar Material
CQRS
Bootstrap
Menu >> Reports >> Tasks grouped by date

This "Tasks grouped by date" calls this service
GET
curl -X 'GET' \
  'https://localhost:44322/api/TaskReport/weekly?startDate=2026-01-01&endDate=2027-01-01' \
  -H 'accept: text/plain'
  
Returned  
  [
  {
    "id": "a7d0bb9a-e684-44f0-80fa-666d22c89e63",
    "weekStartDate": "2026-01-01T00:00:00",
    "weekEndDate": "2026-01-01T00:00:00",
    "weekNumber": 1,
    "year": 2026,
    "totalTasks": 2,
    "completedTasks": 1,
    "pendingTasks": 1,
    "completionPercentage": 50
  },
  {
    "id": "8314f3dd-3117-408f-83c6-6b0e66b6aebf",
    "weekStartDate": "2026-05-21T16:29:44.915",
    "weekEndDate": "2026-05-21T16:29:44.915",
    "weekNumber": 6,
    "year": 2026,
    "totalTasks": 7,
    "completedTasks": 2,
    "pendingTasks": 5,
    "completionPercentage": 20
  },
     
]