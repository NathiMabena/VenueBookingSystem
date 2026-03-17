# EventEase: Venue Booking System 🏢

**Course:** Advanced Diploma in Application Development
**Author:** Nkosinathi Mabena

## 📌 Project Overview
EventEase is a full-stack, data-driven MVC web application designed to manage venues, events, and user bookings. Moving beyond basic CRUD functionality, this system incorporates strict business logic, defensive programming, and a premium UI/UX to ensure data integrity and a seamless user experience.

### 🚀 Live Deployment
This application is fully deployed and hosted on Microsoft Azure.
* **Live Demo:** https://eventeasevenuebookingsystem.azurewebsites.net/
* **Database:** Azure SQL Database
* **Hosting:** Azure App Service (Windows)

---

## ⚙️ Core Features & Business Logic

### 1. Data Integrity & Defensive Programming
* **The "Time-Travel" Lock:** The system actively prevents users from creating bookings for events that have already passed. 
* **Backend Route Protection:** If an admin attempts to bypass the UI and force the URL to edit a historical (passed) event, the Controller intercepts the request, blocks the edit, and redirects the user with a customized `TempData` security alert.
* **Orphaned Booking Prevention:** The system blocks the creation of any bookings for events that do not have a registered venue assigned to them yet.
* **Safe Deletion Handling:** Custom `try-catch` logic catches `DbUpdateException` errors, preventing fatal application crashes if an admin attempts to delete a Venue or Event that currently has active foreign-key constraints (bookings) tied to it.

### 2. Automated System Routing
* **Auto-Stamping:** Booking timestamps (`DateTime.Now`) are automatically generated and stamped by the Controller at the exact moment of creation to prevent manipulation.
* **Dynamic Relational Mapping:** When an event is booked, the system automatically queries the database to find the parent Event's Venue ID and locks it into the Booking, removing the burden of manual entry from the user.

### 3. Premium UI/UX Design
* **Dynamic Dashboards:** The Events dashboard dynamically reads dates. If an event has passed, the UI automatically greys out the event card, applies a "Passed" badge, and removes the "Edit" action button.
* **Modern Form Design:** Forms feature floating labels, shadow-lifted cards, and color-coded action psychology (Blue for Create, Yellow for Edit, Red for Delete).
* **Custom Brand Identity:** A cohesive, custom "Deep Pink" (`#FF1493`) styling overrides default Bootstrap themes for a high-end commercial feel.

---

## 💻 Tech Stack
* **Framework:** ASP.NET Core MVC (.NET 8)
* **Language:** C#
* **Database Management:** Entity Framework Core (Code-First Migrations)
* **Cloud Infrastructure:** Microsoft Azure (App Service & Azure SQL)
* **Front-End:** HTML5, CSS3, Razor Syntax, Bootstrap 5

---

## 🛠️ How to Run Locally
If you wish to run this application in a local development environment:
1. Clone this repository to your local machine.
2. Open the solution in **Visual Studio 2022**.
3. Open the `appsettings.json` file and update the `DefaultConnection` string to point to your local SQL Server instance (or leave the Azure connection string if testing cloud connectivity).
4. Open the Package Manager Console and run `Update-Database` to apply migrations.
5. Press `F5` or click **Run** to build and launch the application in your browser.
