# InventoryTracking

A Windows Forms inventory management application built with C# and SQL Server, with a switchable Turkish and English interface.

This personal project follows the development of an inventory application through several versions: product and stock management, warehouse transfers, movement reports, reusable interface controls and localization.

## Features

- Material catalog with material numbers, barcodes, descriptions and units.
- Product management with categories, prices and stock limits.
- Stock tracking by warehouse, including entries, withdrawals and count adjustments.
- Transfers between warehouses with movement history.
- Movement reports filtered by date, warehouse, product/barcode, movement type and direction.
- Sales/returns reports with daily, monthly or yearly quantities, amounts and net sales, plus a one-click last-year range.
- Native `.xlsx` and CSV export for the selected report tab, with PDF reports using Windows **Microsoft Print to PDF**.
- Database connection setup from the application.
- One-click **TR/EN** switching with a saved language preference.
- Barcode sale/return window: one scan starts at one item; rescanning the same barcode increases the quantity.
- Sale/return history with timestamp, product name, quantity, price snapshot and Windows user.
- Database change log for products, materials, categories, warehouses, stock and transactions.

## Screenshots

Screenshots use an isolated demo database and a demo user label.

### English main window

![InventoryTracking main window in English](docs/screenshots/main-screen.png)

### Turkish main window

![InventoryTracking main window in Turkish](docs/screenshots/main-screen-tr.png)

### Stock movement report

![Stock movement report summary in English](docs/screenshots/report-summary-en.png)

![Stock movement report details in English](docs/screenshots/report-details-en.png)

<details>
<summary>Product and warehouse transfer screens</summary>

![Edit product in English](docs/screenshots/product-en.png)

![Warehouse transfer in English](docs/screenshots/transfer-en.png)

</details>

## Technologies

- C# and .NET Framework 4.8.1
- Windows Forms
- Microsoft SQL Server
- ADO.NET / System.Data.SqlClient
- `.resx` resources and ResourceManager

The application and localization checks use framework libraries and require no NuGet packages.

## Requirements

- Windows and .NET Framework 4.8.1.
- Visual Studio with the .NET desktop development workload and .NET Framework 4.8.1 targeting support.
- An accessible SQL Server 2016 or newer instance.
- SQL Server Management Studio for database setup.
- Microsoft Print to PDF enabled for PDF export.

## Getting started

1. Clone or download the repository.
2. Copy `InventoryTracking/App.config.example` to `InventoryTracking/App.config`.
3. For a fresh demo installation, open `InventoryTracking/Notes/DatabaseSetup.sql` in SQL Server Management Studio and execute it against your development SQL Server instance. It creates the `InventoryTracking` database, its schema and sample records.
4. Open `InventoryTracking.slnx` in Visual Studio. Alternatively, open `InventoryTracking/InventoryTracking.csproj` directly.
5. Build and run the application.
6. Configure the SQL Server connection for the `InventoryTracking` database. If the initial connection fails, the connection setup window opens. The main window also has a **Connection Settings** button.
7. Click **EN** or **TR** at the top right to change language.

**Database setup:** The demo installation script drops existing application tables in the `InventoryTracking` database. Use it only for a fresh installation with no existing data. Instructions for updating an existing installation are provided in the installation notes.

For additional sample data, select the application database in SSMS and run `InventoryTracking/Notes/DemoData.sql`. It adds 20 sample products with numeric barcodes and material numbers and a stock entry for each product in every active warehouse, including matching stock movements. Existing records are preserved; running it again does not duplicate the samples. If you previously installed the samples with DEMO-prefixed codes, run `InventoryTracking/Notes/2026-10-10_ProductCodesAndCategoryNames.sql` first to update their codes, names and Turkish category spelling while retaining stocks and history.

Local `App.config` settings are excluded from Git. Connection settings saved through the application are encrypted for the current Windows user. The optional `INVENTORYTRACKING_CONNECTION_STRING` environment variable takes precedence over saved settings and the application configuration.

## Sales, returns and history

For an existing database, back it up and apply `InventoryTracking/Notes/2026-10-10_SalesReturnsAndAuditLog.sql` in that database. Fresh installations already include this schema. The migration preserves existing products, stock and movements.

Focus the barcode search box and scan a registered active product with an Enter suffix. The sale/return window starts at **1** item. Scan the same barcode again while that window is open to increase its quantity, or enter the quantity manually. Select **Sale** to reduce stock or **Return** to increase it. Other barcodes do not change the current product; close the window to scan another product. An unregistered barcode opens the product creation window directly, with the barcode filled in. If it is also missing from the material catalog, use the **Material** button in that window before saving the product.

The stock change, stock movement and sale/return record are committed together. Insufficient stock leaves all three unchanged. A request identifier prevents the same request from being saved twice after a retry. Transaction records retain the product name and unit price used at the time, so later product edits do not rewrite history.

The **Sale / Return History** tab shows transactions for the selected warehouse and refreshes after each completed transaction. The application currently identifies the acting user by the Windows user name; it does not yet have application login accounts.

`IslemGunlugu` records database inserts, updates and deletes with before/after JSON, timestamp, Windows user and SQL login. Button actions are also recorded while the audit database is accessible; failed sale attempts are logged separately. Passwords and connection strings are not logged. Existing stock movements are retained; earlier product edits cannot be reconstructed retrospectively.

The report window has **Stock Summary**, **Movement Details** and **Sales / Returns Summary** tabs. Common date, warehouse and product/barcode filters apply to all tabs. Stock movement type and direction affect only the stock tabs; transfers and count adjustments never enter financial totals. Select a daily, monthly or yearly period and click **Load Report**, or click **Last Year** to load one calendar year through the end of today. The date range includes both displayed boundary dates; the first date is calculated from tomorrow minus one year to avoid adding an extra day.

The sales/returns summary shows sales quantity, returns quantity, net quantity, sales amount, returns amount, net sales and transaction count. It uses each transaction's stored price. Net sales means sales amount minus returns amount; it is not profit and does not include purchase costs or other business expenses. Returns record the product price at the time of the return, without linking to an original sale receipt.

Select a report tab before using **Export Excel**, **Export CSV** or **Export PDF**. Excel export creates a real `.xlsx` workbook with numeric amounts, TRY formats, frozen header rows and column filters, without requiring Excel or NuGet packages. The workbook includes the applied report filters and totals. Changed filters take effect when the report is loaded again. PDF export requires Windows Microsoft Print to PDF.

To test transactions, rescans, rollback, concurrent sales and logging in a separate temporary database:

```powershell
.\tests\RunSalesReturnChecks.ps1 -SqlServer '.\SQLEXPRESS' -Configuration Release
```

To verify financial calculations, date boundaries, filters, localization, native Excel files and PDF pagination in a separate temporary database:

```powershell
.\tests\RunFinancialReportChecks.ps1 -SqlServer '.\SQLEXPRESS' -Configuration Release
```

## Language behavior

Changing language preserves searches, selected records, entered values and loaded reports. The preference is stored per Windows user. Product names, warehouse names, units and descriptions entered into the database are preserved. Currency remains TRY; decimal input and date parsing use the existing Windows culture. Native Windows dialogs and SQL Server diagnostic messages follow the system or server language.

See [localization and regression checks](docs/LOCALIZATION.md) for resource keys, report behavior and testing instructions.

## Build and checks

From Developer PowerShell for Visual Studio at the repository root:

```powershell
.\tests\RunChecks.ps1 -Configuration Release
```

The script builds the application and checks resource coverage, placeholder compatibility, language switching, saved preferences and input preservation. GitHub Actions runs these checks on Windows.

To include loaded-form and SQL checks using Windows authentication:

```powershell
.\tests\RunChecks.ps1 -SqlServer '.\SQLEXPRESS'
```

These checks create and remove a separate test database. They verify stock operations, English queries, report filters, CSV contents and print-page drawing. Physical printing and the Windows PDF driver are separate runtime requirements.

## Release packaging

After building **Release**, run:

```powershell
.\InventoryTracking\Notes\PrepareCustomerRelease.ps1 -SkipBuild
```

The package is created under `dist/` with the executable, a configuration example without SQL credentials, the `en/` resource assembly, installation notes and SQL scripts. Keep the `en/` folder next to the executable. Build outputs and release archives are excluded from Git.

## Project organization

The application uses folders within one WinForms project:

| Folder | Purpose |
| --- | --- |
| UI/Forms | Forms and interface behavior |
| UI/Controls | Reusable interface controls and drawing helpers |
| Business | Application operations and business rules |
| DataAccess | Database access |
| Connection | Connection configuration and management |
| Models | Data models |
| UI/Localization | Language preference, captions and display translation |
| Properties | Application settings and Turkish/English resource files |
| Reporting | Framework-based report file exporters |
| Notes | SQL scripts, installation notes and maintenance scripts |
| tests | Localization, transaction and financial report regression checks |
| docs | Screenshots and localization documentation |

See [coding conventions](docs/CODING_STANDARDS.md) for folder responsibilities, namespaces and naming rules.

## Version history

| Version | Main changes |
| --- | --- |
| v0.9.0 | Initial inventory management source snapshot |
| v1.0.0 | Warehouse management, stock transfers and connection setup |
| v1.1.0 | Main window updates, reusable interface controls and material numbering changes |
| v1.2.0 | Turkish/English interface, localized exports, stock grid fixes and sample data scripts |
| v1.3.0 (unreleased) | Barcode sales/returns, transaction history, audit logs and financial reports with native Excel export |

Tags preserve source snapshots to show the project's development. Earlier versions may contain issues addressed in later versions. See [CHANGELOG.md](CHANGELOG.md) for release notes.
