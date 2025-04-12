
![📁Employee_Store](https://github.com/user-attachments/assets/6cc51569-131a-4a0a-be93-623248873ba0)

# Employee Store 📋

A simple web app to upload and manage employee records from a `.csv` file. Built as part of a Junior .NET Developer application to demonstrate data handling, validation, and UI integration in ASP.NET Core MVC.

---

## 🧾 Overview

**Employee Store** allows users to upload external `.csv` files containing employee data. The system validates and stores valid entries into a database and provides feedback on any failed imports. Successfully stored data is displayed in a feature-rich, interactive data grid that supports **searching**, **sorting**, and **editing**.

---

## ⚙️ Tech Stack

- **ASP.NET Core MVC** (.NET 8)
- **Entity Framework Core**
- **SQL Server (LocalDB)**
- **C#**
- **Tabulator.js** (Grid display with search, sort, and edit functionality)

---

## ✨ Features

- Upload and parse `.csv` files with employee data
- Server-side validation with meaningful error messages
- Display imported data in a responsive grid
- Grid supports:
  - 🔍 Searching
  - ↕️ Sorting
  - ✏️ Inline Editing
- Success and error notifications after file upload
- Clean architecture with separation of concerns

---

## 🎥 Video
https://github.com/user-attachments/assets/59bca739-befa-493e-a3e6-15a269066b1b

---

## 🚀 Getting Started
### ✅ Prerequisites:
- .NET 8 SDK
- Visual Studio 2022 or newer
- SQL Server (LocalDB)

### ▶️ Steps to Run:
  1. Clone the repository:
```markdown
https://github.com/PirmatovAlisher/Employee_Store.git
```
  2. Open the solution in Visual Studio.
  3. Run the project (F5 or Ctrl+F5).
  4. Navigate to the file upload page and test with a `.csv`.
---

### 🗂️ Project Structure
```
EmployeeStore.sln
├── src/
│   ├── EmployeeStore.Core/                    # Domain models & interfaces
│   ├── EmployeeStore.Services/                # Employee Service, Parser, CsvMapping 
│   ├── EmployeeStore.Infrastructure.MsSql/    # EF Core DB context, repositories
│   └── EmployeeStore.Web/                     # MVC controllers, views, and UI logic
└── tests/  (xUnit)                            # Dedicated Test Library
    └── EmployeeStore.Tests/                   # Controllers, Services, Repositories
```
- Clean separation using Repository and Service layers
- Dependency Injection used to register services
- UI powered by Tabulator.js for advanced grid interactions


## 🧪 Tests
This project includes unit tests covering controllers, services, and repositories to ensure reliability and maintainability of core logic.

### 🧱 Test Structure
```
EmployeeStore.Tests/
├── Controllers/   # Verifies controller actions return correct views, redirects, and model data.
├── Services/      # Ensures business logic behaves as expected under various scenarios.
└── Repositories/  # Validates data access methods using an in-memory database.
```

### ✅ Technologies Used

- [xUnit](https://xunit.net/) — Test framework
- [Moq](https://github.com/moq/moq4) — For mocking dependencies
- [Microsoft.EntityFrameworkCore.InMemory](https://learn.microsoft.com/en-us/ef/core/testing/in-memory) – for testing EF Core without a real database

### ▶️ Running the Tests:
1. Open the solution in **Visual Studio**
2. Open **Test Explorer**: `Test > Test Explorer`.
3. Click **Run All** to execute all tests.

  Or use the .NET CLI:

  ```bash
  dotnet test
```

## 🧠 Learnings
- Implemented file upload and parsing logic using .NET 8
- Used ModelState and custom validation to handle CSV edge cases
- Integrated third-party JS library (Tabulator) for enhanced UI
- Applied Clean Architecture principles in organizing the project

## 📬 Contact
👤 Alisher
<br>  
📧 pirmatovalisher000@gmail.com 
<br>  
📞 **Phone:** +998 (94) 361-99-25
<br>  
💼 [hh.uz](https://hh.uz/resume/a1a1a635ff0e951e320039ed1f4f6e786e7757) 
