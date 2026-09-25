# Gift of the Givers Foundation — Web Application

[![Build Status](PASTE-YOUR-BADGE-IMAGE-URL-HERE)]()

A web application built for the **Gift of the Givers Foundation**, developed to centralise
disaster relief coordination — connecting donors, volunteers, and foundation employees
through a single platform.

## Project Overview

This solution supports two core user roles:

- **Donor** — can register/log in, or donate anonymously as a guest, with support for
  one-time or recurring donations in ZAR, USD, or EUR, and receives a tax certificate
  generated via an Azure Function.
- **Employee** — can log in, post updates on ongoing relief projects (logged to Azure
  Storage via an Azure Function), and view volunteer sign-ups.

The public **Volunteer** registration form lets anyone register their name, skills, and
availability to be contacted for relief missions.

## Solution Structure

This repository contains a single Visual Studio solution with three projects:

| Project | Purpose |
|---|---|
| `GiftOfTheGivers2` | The main ASP.NET Core MVC web application — Account, Donation, Employee, Volunteer, and Home controllers |
| `GiftOfTheGivers2.function` | Azure Functions (isolated worker model) — `GenerateTaxCertificate` and `LogEmployeeUpdate`, called by the web app |
| `GiftOfTheGivers.Helpers` | A shared Class Library, packaged as a NuGet package and published to our Azure Artifacts feed, providing reusable utility methods (e.g. tax certificate number formatting) |

## Tech Stack

- **ASP.NET Core MVC** (.NET) — main web application
- **Bootstrap 5.3** — responsive UI styling
- **Azure Functions (Isolated Worker)** — serverless processing for tax certificates and employee update logging
- **Azure Blob Storage** — durable storage for employee update logs
- **Azure SQL Database** — planned data persistence layer (schema designed in Part 1)
- **Azure DevOps** — Boards (agile planning), Repos (Git version control), Pipelines (CI), Artifacts (NuGet package hosting)

## Team

| Name | Role |
|---|---|
| Luvuyo | Project Lead |
| Nyiko | Database Designer |
| Presh | Quality Tester (QA) |
| Naledi | Developer |

## CI/CD

Every commit to `main` automatically triggers an Azure Pipeline that:
1. Authenticates with our private Azure Artifacts feed
2. Restores all NuGet dependencies (including our custom `GiftOfTheGivers.Helpers` package)
3. Builds the full solution
4. Runs the unit test suite

The badge at the top of this file reflects the current build status.

## Project Planning

Full project planning — Epics, User Stories, sprint schedule, and dashboards — is tracked
in [Azure Boards](https://dev.azure.com/AWSAssignment/Gift%20of%20the%20Givers/_boards).

## Running Locally

1. Open `GiftOfTheGivers2.sln` in Visual Studio.
2. Set both `GiftOfTheGivers2` and `GiftOfTheGivers2.function` as startup projects
   (right-click Solution → **Set Startup Projects** → **Multiple startup projects**).
3. Ensure [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite)
   is running for local Azure Storage emulation.
4. Press **F5** to run both projects together.