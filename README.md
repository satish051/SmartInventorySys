# 🚀 Smart Inventory & Sales Management System (ERP)

![.NET 8](https://img.shields.io/badge/.NET%20Core-8.0-purple) ![Status](https://img.shields.io/badge/Status-Completed-success) ![License](https://img.shields.io/badge/License-MIT-blue)

A commercial-grade **Enterprise Resource Planning (ERP)** system built with **ASP.NET Core 8 MVC**. Designed for retail businesses like hardware stores, pharmacies, and supermarkets to manage inventory, sales, suppliers, and billing efficiently.

## 🌟 Key Features

### 🔐 1. Security & Roles
- **Role-Based Access Control (RBAC):** distinct permissions for **Admins** (full control) and **Staff** (sales only).
- **Secure Authentication:** Built on ASP.NET Identity with password recovery and secure login layouts.

### 📦 2. Smart Inventory
- **Complete CRUD:** Add, Edit, Delete Products, Categories, and Sub-Categories.
- **Automated Tracking:** Stock automatically decreases on sale and increases on purchase.
- **Low Stock Alerts:** Visual indicators on the dashboard when items need restocking.

### 🛒 3. Dynamic Point of Sale (POS)
- **Native Search:** Instant product lookup without page reloads (HTML5 Datalist).
- **Cart System:** Add multiple items, adjust quantities, and calculate totals dynamically.
- **Financials:** Real-time calculation of **Sub-Total**, **Discount (%)**, and **VAT (13%)**.
- **Digital Payments:** Integrated **Khalti** and **eSewa** payment gateways.

### 🚚 4. Supply Chain
- **Supplier Management:** Track vendor details and contact info.
- **Purchase Orders (Stock In):** Record new stock arrivals and update inventory automatically.

### 📊 5. Advanced Reporting
- **Admin Dashboard:** Interactive charts (Chart.js) showing revenue trends.
- **Sales Reports:** Filter by Date Range and Invoice Number.
- **My Sales:** Staff members can view and edit notes for their own sales history.
- **PDF Invoices:** Generate and download professional PDF bills instantly.
- **Audit Trail:** Every transaction records the user who performed it.

---

## 🛠 Tech Stack & Architecture

This project follows a clean **Enterprise Architecture** to ensure scalability and maintainability.

- **Framework:** ASP.NET Core MVC 8 (.NET 8.0)
- **Database:** SQL Server (Entity Framework Core Code-First)
- **Design Pattern:** Repository Pattern & Unit of Work (Dependency Injection).
- **Frontend:** Bootstrap 5, JavaScript (Vanilla + jQuery), Chart.js.
- **Tools:** HTML2PDF (Invoices), Visual Studio 2022.

---

## 🚀 Getting Started

Follow these steps to run the project locally.

### Prerequisites
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (with ASP.NET and Web Development workload).
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download).
- SQL Server (LocalDB or Express).

### Installation

1.  **Clone the Repository**
    ```bash
    git clone [https://github.com/yourusername/SmartInventorySys.git](https://github.com/yourusername/SmartInventorySys.git)
    ```
2.  **Open the Project**
    Open `SmartInventorySys.sln` in Visual Studio.

3.  **Configure Database**
    Update the connection string in `appsettings.json` if necessary (defaults to LocalDB).
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartInventoryDB;Trusted_Connection=True;MultipleActiveResultSets=true"
    }
    ```

4.  **Run Migrations**
    Open the **Package Manager Console** (Tools > NuGet Package Manager) and run:
    ```powershell
    Update-Database
    ```

5.  **Build & Run**
    Press `F5` or click the Green Play button.

---

## 🔑 Default Login Credentials

When the application starts, the database seeder will create these default roles and a user:

* **User:** `admin@inventory.com`
* **Password:** `Admin@123`

*(You can create Staff users via the "Register" link on the login page and assign roles via the Admin Panel).*

---

## 📂 Folder Structure
