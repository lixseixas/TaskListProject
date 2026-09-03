TaskListProjectReact
Using react

Create a service to insert values

curl -X 'POST' \
  'https://localhost:44322/api/AccountMovements' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "amount": 0,
  "type": "string",
  "date": "2026-09-03T03:16:38.490Z",
  "description": "string"
}'


At Home.tsx
Add a field to insert the amount
Add a button to insert values
refresh the table after insert