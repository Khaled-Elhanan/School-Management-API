
---

# School Management API — Multi-Tenant SaaS Backend

**School Management API** is a **production-oriented Multi-Tenant SaaS backend** built with
**ASP.NET Core (.NET 8)** using **Clean Architecture**, **CQRS**, and **tenant-aware authentication**.

The project simulates a real-world SaaS platform where a single backend instance securely serves
multiple independent organizations (schools), each operating as a fully isolated tenant.

The system is designed to run a single application instance that serves **multiple independent schools (tenants)** while enforcing:

* Strong **database-level isolation** per tenant
* **Permission-based authorization** instead of hard-coded roles
* Secure and scalable **JWT authentication with refresh tokens**
* A clear, explainable, and testable request lifecycle suitable for real production systems

Each school operates as a fully isolated tenant with its own:

* Database
* Users and roles
* Permissions
* Subscription lifecycle

---

## 🚀 Key Highlights

* **Clean Architecture**

  * Clear separation between **Domain**, **Application**, **Infrastructure**, and **WebApi**
  * Business logic isolated from frameworks and infrastructure concerns

* **Database-Per-Tenant Architecture**

  * Each tenant (school) has its own isolated database
  * No shared business or identity data across tenants

* **Tenant-Aware Authentication**

  * Tenant context is resolved **before authentication**
  * Identity authenticates users against the correct tenant database per request

* **CQRS + MediatR**

  * Commands and Queries are fully separated
  * Thin controllers and focused handlers

* **Selective Validation Pipeline**

  * Centralized validation using FluentValidation via MediatR pipeline behaviors
  * Validation is **explicitly opt-in** using a marker interface (`IValidateMe`)
  * Security-sensitive flows (e.g. login, token refresh) intentionally bypass validation

* **Standardized API Responses**

  * Unified response format using `ResponseWrapper<T>`
  * Consistent success and error handling across the API

* **Permission-Based Authorization**

  * Fine-grained permissions instead of role-based controller checks
  * Dynamic policy generation at startup without manual wiring

* **Subscription Validation**

  * Safe handling of null/default expiration dates
  * Tenant lifecycle enforced during authentication

---

## 🏗 High-Level Architecture & Request Flow

```
Client
↓
ASP.NET Core Middleware Pipeline
↓
Multi-Tenant Middleware (Finbuckle)
↓
Authentication (ASP.NET Identity)
↓
Authorization (Permission Policies)
↓
Controller (Thin)
↓
MediatR
↓
Pipeline Behaviors
  - Validation (IValidateMe only)
↓
Command / Query Handler
↓
Application Layer
↓
Domain Logic
↓
Tenant Database
↓
ResponseWrapper<T>
```

---

## 🔄 Request Flow (End-to-End)

### 1️⃣ Incoming Request

```
GET /api/schools
Headers:
tenant: school1
Authorization: Bearer <JWT>
```

* `tenant` header → identifies the tenant
* `JWT` → identifies the user and granted permissions

---

### 2️⃣ Multi-Tenant Resolution

* Executed **before authentication**
* Resolution order:

  1. HTTP header (`tenant`)
  2. JWT claim (`tenant`)
  3. Shared database (`MultiTenancy.Tenants`)

Tenant metadata:

* Identifier
* ConnectionString
* IsActive
* ValidUpTo

Stored per request in:

```csharp
IMultiTenantContextAccessor<ABCSchoolTenantInfo>
```

---

### 3️⃣ Dynamic Database Selection

```csharp
optionsBuilder.UseSqlServer(TenantInfo.ConnectionString);
```

| Tenant  | Database  |
| ------- | --------- |
| school1 | School1Db |
| school2 | School2Db |
| root    | SharedDb  |

Each request receives its own DbContext instance pointing to the correct database.

---

### Tenant Database Provisioning

When a new tenant is created:

* A **new dedicated SQL Server database** is provisioned automatically
* The tenant database contains:

  * ASP.NET Identity tables
  * Business entities (Schools, etc.)
* The shared database remains unchanged

The **shared database** contains **only tenant metadata**:

* `MultiTenancy.Tenants`
* Connection strings
* Subscription and activation status

This guarantees:

* Strong isolation between tenants
* No cross-tenant data leakage
* Independent scaling and deletion per tenant

---

### 4️⃣ Authentication (ASP.NET Identity)

* Runs **after tenant resolution**
* Credentials validated against the tenant database
* User roles loaded
* RoleClaims (permissions) loaded
* Subscription (`ValidUpTo`) validated

If the subscription is expired → authentication is blocked.

---

## 🔐 Permission Model

### Permission Definition

All permissions are **centrally defined in code**:

```csharp
SchoolPermissions
```

Naming convention:

```
Permission.{Feature}.{Action}
```

Example:

```
Permission.Users.Create
```

---

### Role → Permission Assignment

During application startup, permissions are seeded using:

```csharp
ApplicationDbSeeder
```

Role mapping:

* **Admin** → full permissions
* **Basic** → limited permissions
* **Root tenant** → admin + system permissions

Permissions are stored as **RoleClaims**:

```
ClaimType: permissions
ClaimValue: Permission.Users.Create
```

---

## 🔐 JWT Authentication Flow

### Login

1. User submits username & password
2. Credentials validated via ASP.NET Identity
3. User roles loaded
4. Permissions loaded from RoleClaims
5. JWT generated with all claims

### JWT Payload Includes

* UserId
* Email
* Roles
* Permissions
* Tenant

Example:

```json
{
  "role": "Admin",
  "permissions": "Permission.Users.Create",
  "tenant": "school1"
}
```

---

## 🔄 Refresh Token Strategy

* Refresh tokens stored in the database
* Rotated on each refresh
* Short-lived access tokens
* Long-lived refresh tokens

This allows:

* Secure logout
* Token revocation
* Reduced attack surface

---

## 🛡 Authorization System

### Dynamic Policies

* Policies generated automatically using reflection:

```csharp
services.AddAuthorization(...)
```

* Each permission becomes a policy:

```
PolicyName == Permission Name
```

---

### Reflection Usage

Reflection scans:

* `SchoolAction`
* `SchoolFeature`
* `SchoolPermissions`

No manual policy registration required.

---

### PermissionPolicyProvider

* Handles policies not registered at startup
* Acts as a safety net for extensibility

---

### Authorization Handler

```csharp
PermissionAuthorizationHandler
```

* Reads `HttpContext.User.Claims`
* If permission exists → request allowed
* Else → **403 Forbidden**

---

## ❓ Why Permissions Instead of Roles

* Roles are coarse-grained
* Permissions are fine-grained
* Permissions scale better
* No controller-role coupling
* Controllers depend on **capabilities**, not identities

This approach avoids tight coupling between controllers and roles,
and enables permission changes without code redeployment.

---

## 🛠 Run Locally

1. Clone repository
2. Configure connection strings in `appsettings.json`
3. Run EF Core migrations for the shared database
4. Start the API
5. Send requests with header: `tenant: <TenantIdentifier>`

---

## 🔒 Security Considerations

* No debug endpoints in production
* Request-scoped tenant context (no static state)
* Short access token lifetime
* Refresh token rotation
* HTTPS enforced

---

## 📌 Design Decisions & Trade-offs

* Database-per-tenant → strong isolation
* Claims-based authorization → flexibility
* Dynamic policies → scalability
* Centralized permission metadata → consistency
* Middleware-first tenant resolution → correctness

---

## 👤 Author

**Khaled Abd Elhanan** — Backend Software Engineer  
📧 khaled.elhanan@gmail.com  
GitHub: https://github.com/Khaled-Elhanan  
LinkedIn: https://www.linkedin.com/in/khaled-abd-elhanan-253328217/

---

## 📝 License

MIT License

---
