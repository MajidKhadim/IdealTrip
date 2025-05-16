# 🌍 IdealTrip – Travel Booking Backend API

Welcome to the backend of **IdealTrip**, a comprehensive travel booking platform built with **ASP.NET Core**. This backend powers all core functionalities of the IdealTrip system – from user registration to real-time booking confirmation and payments.

---

## 🧳 Features

The backend is designed as a modular and scalable RESTful API that supports:

- 🏨 **Hotels**: Add, update, and book hotel rooms with real-time availability
- 🏡 **Local Homes**: Book unique local stays hosted by home owners
- 🚐 **Transport**: 
  - Book bus seats (with automatic seat deduction)
- 🎒 **Tour Guides**: Browse, hire, and provide feedback for guides
- 🔐 **User Management**:
  - Role-based registration (`Tourist`, `HotelOwner`, `Transporter`, `TourGuide`, `LocalHomeOwner`, `Admin`)
  - Email verification and approval workflow
  - JWT-based authentication and authorization

---

## ⚙️ Tech Stack

- **Backend Framework**: ASP.NET Core
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens), ASP.NET Identity
- **Documentation**: Swagger
- **Real-time Communication**: SignalR
- **Payments**: Stripe Integration
- **Email Services**: Custom templates + bounce handling
- **Others**: Clean architecture, RESTful API design

---

## 📁 Folder Structure

```plaintext
/Controllers         → API endpoints
/Models              → Data models and enums
/Repositories        → Data access logic (interfaces + SQL repositories)
/Services            → Business logic and email/SMS/payment helpers
/Middleware          → Custom error handling and auth middleware
/DTOs                → Request and response data transfer objects
