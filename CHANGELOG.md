# Changelog

## Unreleased — 1.3.0

- Organized forms, controls and localization under UI; separated services, repositories and models into individual files.
- Renamed application types and source/script filenames to English and updated namespaces and references.

- Replaced the existing-product barcode question with a quantity-based Sale / Return window.
- Added repeat scanning of the same barcode, transaction-specific confirmations and warehouse history.
- Stored product name and price snapshots with each transaction; stock and movement writes are atomic.
- Added database change logs, application button events, rejected-sale logs and duplicate-request protection.
- Added sale/return regression checks for rescanning, concurrency, rollback and restricted SQL permissions.
- Added daily, monthly and yearly sales/returns summaries with historical price totals and a last-year shortcut.
- Added native Excel export for all report tabs while retaining CSV and PDF export.
- Kept transfers and count adjustments outside financial totals; net sales exclude costs and other expenses.

## 1.2.0 — 2026-10-10

- Added a TR/EN button to the main window and saved language preference per Windows user.
- Localized forms, application messages, stock statuses, report filters, CSV headings and printable report pages.
- Preserved searches, selected records, entered values and SQL movement codes during language changes.
- Included the English resource assembly in customer release packages.
- Added screenshots, localization regression checks and a Windows build workflow.
- Fixed startup component initialization and connection/material button alignment.
- Kept the stock grid on standard DataGridView drawing with a separate frame and defined columns.
- Added repeatable sample stock data for every active warehouse, numeric product codes and Turkish category names.

## 1.1.0

- Updated the main screen with reusable controls and product details.
- Simplified material numbering and refreshed installation notes.

## 1.0.0

- Added warehouse management, stock transfers and connection setup.

## 0.9.0

- Initial product and stock management source snapshot.
