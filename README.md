# InventoryTracking

A Windows Forms inventory management application built with C# and SQL Server.

This personal project follows the development of an inventory application through several versions, from basic product and stock management to warehouse transfers, movement reports, and reusable interface controls. The application interface is in Turkish.

## Features

- Material catalog with material numbers, barcodes, descriptions, and units.
- Product management with categories, prices, and stock limits.
- Stock tracking by warehouse.
- Stock entries, withdrawals, and adjustments with movement history.
- Transfers between warehouses.
- Movement reports with date, warehouse, product, and movement filters.
- CSV export and PDF export through Microsoft Print to PDF.
- Database connection setup from the application.

## Technologies

- C#
- .NET Framework 4.8.1
- Windows Forms
- Microsoft SQL Server
- ADO.NET / System.Data.SqlClient

## Requirements

- Windows.
- Visual Studio with the .NET desktop development workload and .NET Framework 4.8.1 targeting support.
- An accessible SQL Server instance.
- SQL Server Management Studio for database setup.
- Microsoft Print to PDF enabled for PDF export.

## Getting Started

1. Clone or download the repository.
2. Copy `InventoryTracking/App.config.example` to `InventoryTracking/App.config`.
3. For a fresh demo installation, open `InventoryTracking/Notes/SqlDataBaseScriptV1.txt` in SQL Server Management Studio and execute it against your development SQL Server instance. It creates the `InventoryTracking` database, its schema, and sample records.
4. Open `InventoryTracking.slnx` in Visual Studio.
5. Build and run the application.
6. Configure the SQL Server connection for the `InventoryTracking` database. If the initial connection fails, the connection setup window opens. Connection settings can also be changed from the main window.

**Database setup:** The demo installation script drops existing application tables in the `InventoryTracking` database. Use it only for a fresh installation with no existing data. Instructions for updating an existing installation are provided in the installation notes.

Local `App.config` settings are excluded from Git. Connection settings saved through the application are encrypted for the current Windows user.

The optional `INVENTORYTRACKING_CONNECTION_STRING` environment variable takes precedence over saved connection settings and the application configuration.

## Project Organization

The application is organized into folders within a single WinForms project:

| Folder | Purpose |
| --- | --- |
| InventoryTracking.UI | Forms and interface behavior |
| InventoryTracking.Controls | Reusable interface controls and drawing helpers |
| InventoryTracking.Business | Application operations and business rules |
| InventoryTracking.DataAccess | Database access |
| InventoryTracking.Connection | Connection configuration and management |
| InventoryTracking.Models | Data models |
| Notes | SQL scripts, installation notes, and maintenance scripts |

## Version History

| Tag | Main changes |
| --- | --- |
| v0.9.0 | Initial inventory management source snapshot |
| v1.0.0 | Expanded warehouse management, stock transfers, and connection setup |
| v1.1.0 | Main window updates, reusable interface controls, and material numbering changes |

The tags preserve source snapshots to show the project's development. Earlier versions may contain issues addressed in later versions. The `main` branch contains the latest source and repository documentation.