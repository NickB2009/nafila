
---

# 🧠 Project Overview

This is a SaaS platform with a multi-tenant architecture built around **Organizations** and **Locations**. The system uses **role-based access control (RBAC)** with **tenant-aware security** and scoped permissions.

---

# 👤 Roles & Permission Levels

## 🏁 Platform-level Operations (**PlatformAdmin**)

The system-level admin (you). Full, unrestricted control over the entire platform.

* Full access to all Organizations and Locations
* Can manage system-wide settings and global templates
* Can assign templates to Organizations or individual Locations
* Can view and manage cross-Organization analytics
* Can apply system updates and manage subscription plans
* Can deactivate or suspend Organizations
* Controls redirect rules, system theming, and core logic

---

## 🏢 Organization-level Operations (**Admin**)

Admins "own" their Organization and manage its inner workings. They do not control templates or global settings.

* Full control over their Organization
* Can manage multiple Locations within that Organization
* Can invite/manage Barbers and other staff
* Can handle branding, scheduling, business hours, and local analytics
* Can update Organization info and control data sharing preferences
* Can view metrics scoped to their Organization

---

## 🏪 Location-level Operations (**Barber**)

Barbers operate within Locations assigned under an Organization.

* Assigned to specific Locations
* Can manage their own schedule and availability
* Can view and handle their client appointments
* Can start/end breaks, call next clients, save haircut records
* Operate within their Location context only

---

## 🎟️ Client-level Operations (**Client**)

Clients interact only with the front-facing experience.

* Can view services and availability
* Can book and cancel appointments
* Can join queues and check wait times
* Can access public interfaces like kiosks and queue displays

---

## ⚙️ Service Operations (**ServiceAccount**)

Handles backend automation and system-level tasks.

* Background processes
* System maintenance
* Automated tasks (e.g., reset averages, update capacities, clear inactive users)
* Not tied to any Organization or Location
* Operates silently and efficiently in the background

---

# 🗺️ Organization & Location Hierarchy

* Each **Organization** can have **multiple Locations**.
* Every Organization is **"owned" by an Admin**, but **created and ultimately controlled by the PlatformAdmin**.
* **Barbers** are assigned to specific **Locations** within an Organization and may be moved between them.
* **Admins** manage everything within their Organization, including its Locations.
* **PlatformAdmin** has top-down authority to manage, modify, override, or inspect all Organizations and Locations.

---

# 🎨 Templates & Theming

* Templates are created and managed exclusively by **PlatformAdmin**.
* PlatformAdmin assigns templates globally or per Organization/Location.
* Admins can apply branding (logos, business names, colors) to their Organization.
* Public interfaces (e.g., kiosks, queue displays) adapt based on template + location config.
* Clients only see what’s exposed through these public interfaces.

---
