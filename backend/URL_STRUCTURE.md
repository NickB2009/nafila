# URL Structure Guidelines

## Overview

This document outlines the URL structure for the EuToNaFila platform across different domains (barbershops, health clinics, etc.) and frontend types (web, mobile, kiosk).

## Core URL Philosophy

EuToNaFila uses a location-centric URL structure that prioritizes:
- Brevity and simplicity
- Global uniqueness
- Human readability
- QR code friendliness

## URL Pattern

```
https://www.eutonafila.com.br/{location-slug}
```

### Location Slug Rules

| Rule                            | Details                                                                                                                                            |
| ------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| **location-slug**               | A single, lower-case, ASCII token that uniquely identifies that physical site on the entire platform                                               |
| **Types / prefixes** (optional) | You can keep the descriptive prefix ("barb", "posto", "clin", etc.) if it helps human readability, but it's not required for technical uniqueness |
| **Branch numbering**            | If two slugs would collide, append "-2", "-3"…                                                                                                     |

### Examples

| Location name (user-friendly)            | Suggested slug          | Final URL                                                |
| ---------------------------------------- | ----------------------- | -------------------------------------------------------- |
| Barbearia Mineiro (Morro Grande)         | `mineiro-morrogrande`   | https://www.eutonafila.com.br/mineiro-morrogrande       |
| Barbearia Mineiro (Morro da Fumaça)      | `mineiro-morrodafumaca` | https://www.eutonafila.com.br/mineiro-morrodafumaca     |
| Posto de Saúde Morro Grande (1ª unidade) | `posto-morrogrande-1`   | https://www.eutonafila.com.br/posto-morrogrande-1       |
| Posto de Saúde Morro Grande (2ª unidade) | `posto-morrogrande-2`   | https://www.eutonafila.com.br/posto-morrogrande-2       |

## Extended URL Patterns

### Client Queue View
```
https://www.eutonafila.com.br/{location-slug}/queue
```

### Admin Views
```
https://www.eutonafila.com.br/{location-slug}/admin
```

### Kiosk Mode
```
https://kiosk.eutonafila.com.br/{location-slug}
```

### Mobile Deep Links
```
eutonafila://{location-slug}
eutonafila://{location-slug}/join
```

## Backend Implementation

The location slug approach simplifies the BFF architecture by:
1. Making all URLs consistent regardless of business domain
2. Allowing business domain identification to happen at the data level
3. Enabling simpler routing rules across all frontend types

### Data Model

```
Location {
  LocationId: GUID,
  Slug: "posto-morrogrande-1",
  FriendlyName: "Posto de Saúde Morro Grande – Unidade 1",
  OrganizationId: GUID (FK),
  LocationTypeId: GUID (FK),  // Barbershop, Health Clinic, etc.
  ...
}

Organization {
  OrganizationId: GUID,
  Name: "Prefeitura Municipal do Sangão",
  ...
}
```

## Workflow for New Locations

1. Admin types a friendly name for the location
2. System auto-generates a candidate slug:
   * Normalize → ASCII, lower-case
   * Convert spaces and accents → "-"
   * Remove special characters
3. Check uniqueness; if taken, append "-2", "-3"…
4. Save. The URL is now instantly routable.

## Technical Implementation

### BFF Handling

All BFF services (web, mobile, kiosk) should use the same core slug-based routing mechanism:

1. Extract location slug from URL
2. Query backend API to get location details and type
3. Route to appropriate domain-specific handling based on location type
4. Apply frontend-specific optimizations

### Routing Configuration

Azure App Service or API Management should be configured to route by:
1. Frontend type subdomain (www, kiosk, etc.)
2. Location slug
3. Action path (queue, admin, etc.)
