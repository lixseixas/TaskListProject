TaskListProjectReact

SubTask 1
Move the content of this Account Movements page to a new page called Home
Create a login page at TaskListProjectReact/src/pages/Login.tsx
Dont create any login logic, just a page with a form to login
After clicking in login button redirect to Home page

Subtask 2
Create a label for balance
Create a service at [text](../TaskListProjectReact/src/services/accountMovementsService.ts)

curl -X 'GET' \
  'https://localhost:44322/api/AccountMovements/balance/785c2e15-8464-4cea-b30e-99faab345eb0' \
  -H 'accept: text/plain'

  {
  "type": "string",
  "title": "string",
  "status": 0,
  "detail": "string",
  "instance": "string",
  "additionalProp1": "string",
  "additionalProp2": "string",
  "additionalProp3": "string"
}