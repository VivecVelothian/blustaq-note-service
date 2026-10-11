# BluestaqNoteService Backend

## Technology Stack

* **Language:** C# 12 / .NET 9 (LTS)
* **Framework:** ASP.NET Core Web API (Controllers)
* **Database Access:** ADO.NET
* **Database:** SQLite
* **Documentation:** Swagger / OpenAPI

## Features

* **Team-Scoped Collaboration:** Notes and users belong to specific teams. Any team member can read or update notes within their team context.
* **Zero Configuration Startup:** The SQLite database file and tables are automatically created and seeded when the application starts.
* **RESTful Design:** Standard HTTP methods and status codes are used across all endpoints.

## Database Schema

The database consists of three tables

### 1. Teams
* `Id` (Integer, Primary Key)
* `Name` (String)
* `CreatedAt` (DateTime)

### 2. Users
* `Id` (Integer, Primary Key)
* `TeamId` (Integer, Foreign Key referencing Teams)
* `Username` (String)

### 3. Notes
* `Id` (Integer, Primary Key)
* `TeamId` (Integer, Foreign Key referencing Teams)
* `CreatedById` (Integer, Foreign Key referencing Users)
* `Title` (String)
* `Content` (String)
* `CreatedAt` (DateTime)
* `UpdatedAt` (DateTime)

## API Endpoints

### Teams

* **POST /api/teams**
  * Description: Create a new team.
  * Request Body: `{"name": "Engineering Team"}`
  * Response: 201 Created with the created team object.

* **POST /api/teams/{teamId}/users**
  * Description: Add a new user to a specific team.
  * Request Body: `{"username": "alice"}`
  * Response: 201 Created with the created user object.

* **GET /api/teams/{teamId}/notes**
  * Description: Retrieve all notes associated with a specific team.
  * Response: 200 OK with a list of notes including creator details.

* **POST /api/teams/{teamId}/notes**
  * Description: Create a new note within a team.
  * Request Body: `{"createdById": 1, "title": "Sprint Planning", "content": "Discuss backend tasks."}`
  * Response: 201 Created with the created note object.

### Notes

* **GET /api/notes/{id}**
  * Description: Retrieve a single note by its unique identifier.
  * Response: 200 OK or 404 Not Found.

* **PUT /api/notes/{id}**
  * Description: Update an existing note title and content.
  * Request Body: `{"title": "Updated Title", "content": "Updated content here."}`
  * Response: 204 No Content.

* **DELETE /api/notes/{id}**
  * Description: Delete a note by its unique identifier.
  * Response: 204 No Content.

## Getting Started

1. Clone the repository to your local machine.
2. Ensure you have the .NET 9 SDK installed.
3. Run the following command in the project root directory:
   ```bash
   dotnet run
   ```
4. Navigate to `https://localhost:5000/swagger` in your browser to view and test the API documentation.