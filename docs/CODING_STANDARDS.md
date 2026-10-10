# Coding conventions

## Structure

- Forms, their Designer/resource files and their partial files belong in `UI/Forms`.
- Reusable visual controls belong in `UI/Controls`; language and caption behavior belongs in `UI/Localization`.
- Application services belong in `Business`, database access in `DataAccess`, connection management in `Connection`, and data classes in `Models`.
- Exporters that accept ordinary data rather than WinForms controls belong in `Reporting`.
- Keep one top-level application type in each file. A form can use several partial files, such as `MainForm.Language.cs`; keep these beside the main form.
- Match namespaces to the project and folder: `UI/Forms` uses `InventoryTracking.UI.Forms`.

## Names

- Use English class and source-file names. Match the file to its main type: `SalesReturnRepository.cs` declares `SalesReturnRepository`.
- Use PascalCase for types, methods, properties and constants; camelCase for parameters and local variables.
- Use `_camelCase` for handwritten private instance fields. Keep Designer-generated code under the Designer workflow.
- Name forms with `Form`, services with `Service`, and database repositories with `Repository`.
- Use descriptive WinForms control names with the existing prefixes, such as `btnSave`, `txtBarcode` and `dgvProducts`.
- Turkish comments should explain behavior and reasons. User-facing captions belong in the TR/EN resource system.
- Existing database tables, columns, parameters and movement codes retain their SQL names. Model property and binding names that form part of that contract remain compatible with it; translating that contract is a separate migration.

## Changes

- Update project file paths, all partial namespaces and references together when moving or renaming a type.
- Keep a form's `.Designer.cs` and `.resx` beside its `.cs` file; do not move or edit generated files independently.
- Run the relevant existing checks after a structural change. SQL checks use separate temporary databases.
