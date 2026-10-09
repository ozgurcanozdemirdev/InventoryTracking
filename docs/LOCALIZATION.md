# Turkish and English interface

The button at the top right shows the language that will be selected: **EN** switches to English and **TR** switches to Turkish. The preference is saved in `%LOCALAPPDATA%\InventoryTracking\language.txt` and restored before the application opens its forms.

Application captions and messages come from `Properties/Resources.resx` (Turkish) and `Properties/Resources.en.resx` (English). The build creates `en/InventoryTracking.resources.dll`; keep this folder beside the executable when distributing the application.

`LanguageManager` selects `CurrentUICulture`. `CurrentCulture` remains unchanged so changing the interface language does not change decimal input or date parsing. Currency remains TRY. Names, descriptions, units and other records entered into the database are preserved. Windows file dialogs, message-box buttons and SQL Server diagnostic messages follow the operating system or server language.

`LocalizedForm` updates captions and grid headings without reconstructing forms. Input fields and record labels are excluded. Dynamic stock and report descriptions are refreshed from already loaded data. Report print descriptions retain the applied filters even if the user edits filters before loading another report. Movement and status values are translated for display and export while their stored Turkish codes remain unchanged.

To add a caption:

1. Add the same descriptive key to both `.resx` files.
2. Use `UiText.Get("Key")` for a caption or `UiText.Format("Key", values)` for a message with placeholders.
3. Keep SQL column names, binding properties and persisted movement codes outside resources.
4. Update the relevant form's `OnLanguageChanged` method when a caption depends on loaded data.
5. Run the checks below. Visual Studio regenerates `Resources.Designer.cs` through **Run Custom Tool** on the neutral resource file.

## Checks

Run from Developer PowerShell for Visual Studio at the repository root:

```powershell
.\tests\RunChecks.ps1
```

This builds the application and checks resource coverage, format placeholders, language round trips, input preservation and preference persistence. It uses a temporary language file rather than the user's saved preference.

For SQL and loaded-form checks, specify a development SQL Server instance accessible with Windows authentication:

```powershell
.\tests\RunChecks.ps1 -SqlServer '.\SQLEXPRESS'
```

The SQL checks create a uniquely named `InventoryTracking_LanguageTest_<guid>` database, fill it with demo records and drop it in `finally`. The login needs permission to create and drop this test database. The tests cover English stock/product queries, report filters, CSV export and print-page drawing. They do not send output to a physical printer.

Optional screenshots can be generated with `-ScreenshotDirectory` using those demo records. The existing `InventoryTracking/Notes/V1IntegrationCheck.ps1` covers additional stock, transfer and schema scenarios in a separate temporary database.

The GitHub Actions workflow runs the build and checks that do not require SQL Server. SQL integration checks are run locally.
