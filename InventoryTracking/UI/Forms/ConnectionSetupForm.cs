using InventoryTracking.UI.Localization;
using InventoryTracking.Connection;
using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryTracking.UI.Forms
{
  public class ConnectionSetupForm : LocalizedForm
  {
    private readonly TextBox txtServer = new TextBox();
    private readonly TextBox txtDatabase = new TextBox();
    private readonly CheckBox chkWindows = new CheckBox();
    private readonly TextBox txtUser = new TextBox();
    private readonly TextBox txtPassword = new TextBox();
    private readonly CheckBox chkTrustCertificate = new CheckBox();

    private readonly string _settingsPath;

    public ConnectionSetupForm() : this(null) { }

    // Testlerde ayrı ayar dosyası kullanılır; normal açılış mevcut kullanıcı ayarlarını kullanır.
    internal ConnectionSetupForm(string settingsPath)
    {
      _settingsPath = settingsPath;
      Text = UiText.Get("DatabaseSettings");
      StartPosition = FormStartPosition.CenterScreen;
      FormBorderStyle = FormBorderStyle.FixedDialog;
      MaximizeBox = false;
      MinimizeBox = false;
      ClientSize = new Size(560, 410);
      BackColor = Color.White;
      Font = new Font("Segoe UI", 10F);

      var header = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(41, 128, 185) };
      header.Controls.Add(new Label
      {
        Text = UiText.Get("DatabaseHeader"), ForeColor = Color.White,
        Font = new Font("Segoe UI", 14F, FontStyle.Bold), Dock = DockStyle.Fill,
        Padding = new Padding(15, 10, 0, 0)
      });
      Controls.Add(header);

      AddRow("SQL Server:", txtServer, 72);
      AddRow(UiText.Get("DatabaseLabel"), txtDatabase, 114);
      AddRow(UiText.Get("UserLabel"), txtUser, 200);
      AddRow(UiText.Get("PasswordLabel"), txtPassword, 242);
      txtPassword.UseSystemPasswordChar = true;
      chkWindows.Text = UiText.Get("WindowsAuth");
      chkWindows.Location = new Point(155, 158);
      chkWindows.Size = new Size(350, 27);
      chkWindows.Checked = true;
      chkWindows.CheckedChanged += (sender, e) => UpdateAuthFields();
      Controls.Add(chkWindows);
      chkTrustCertificate.Text = UiText.Get("TrustCertificate");
      chkTrustCertificate.Location = new Point(155, 284);
      chkTrustCertificate.Size = new Size(390, 27);
      Controls.Add(chkTrustCertificate);
      Controls.Add(new Label
      {
        Text = UiText.Get("SetupHint"),
        Location = new Point(20, 318), Size = new Size(525, 24), ForeColor = Color.FromArgb(52, 73, 94)
      });

      var footer = new Panel { Dock = DockStyle.Bottom, Height = 59, BackColor = Color.FromArgb(236, 240, 241) };
      var btnTest = MakeButton(UiText.Get("TestConnection"), 35, Color.FromArgb(52, 152, 219));
      var btnSave = MakeButton(UiText.Get("SaveAndOpen"), 198, Color.FromArgb(46, 204, 113));
      var btnCancel = MakeButton(UiText.Get("Cancel"), 361, Color.FromArgb(149, 165, 166));
      btnTest.Click += (sender, e) => TestConnection();
      btnSave.Click += (sender, e) => SaveConnection();
      btnCancel.Click += (sender, e) => { DialogResult = DialogResult.Cancel; Close(); };
      footer.Controls.AddRange(new Control[] { btnTest, btnSave, btnCancel });
      Controls.Add(footer);
      AcceptButton = btnSave;
      CancelButton = btnCancel;
      LoadDefaults();
      UpdateAuthFields();
    }

    private void AddRow(string text, Control control, int top)
    {
      Controls.Add(new Label { Text = text, Location = new Point(20, top + 4), Size = new Size(130, 25) });
      control.Location = new Point(155, top);
      control.Size = new Size(380, 29);
      Controls.Add(control);
    }

    private static Button MakeButton(string text, int left, Color color)
    {
      var button = new Button
      {
        Text = text, Location = new Point(left, 10), Size = new Size(150, 39),
        BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
        UseVisualStyleBackColor = false, Font = new Font("Segoe UI", 10F, FontStyle.Bold)
      };
      button.FlatAppearance.BorderSize = 0;
      return button;
    }

    private void LoadDefaults()
    {
      try
      {
        string saved = ConnectionSettingsStore.Load(_settingsPath);
        var builder = new SqlConnectionStringBuilder(string.IsNullOrWhiteSpace(saved)
          ? Properties.Settings.Default.DBConStr : saved);
        txtServer.Text = builder.DataSource;
        txtDatabase.Text = builder.InitialCatalog;
        chkWindows.Checked = builder.IntegratedSecurity;
        txtUser.Text = builder.UserID;
        chkTrustCertificate.Checked = builder.TrustServerCertificate;
      }
      catch (Exception) { txtDatabase.Text = "InventoryTracking"; }
    }

    private void UpdateAuthFields()
    {
      txtUser.Enabled = !chkWindows.Checked;
      txtPassword.Enabled = !chkWindows.Checked;
    }

    private string BuildConnectionString()
    {
      if (string.IsNullOrWhiteSpace(txtServer.Text) || string.IsNullOrWhiteSpace(txtDatabase.Text))
        throw new ArgumentException(UiText.Get("SqlServerRequired"));
      if (!chkWindows.Checked && (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPassword.Text)))
        throw new ArgumentException(UiText.Get("SqlCredentialsRequired"));
      var builder = new SqlConnectionStringBuilder
      {
        DataSource = txtServer.Text.Trim(),
        InitialCatalog = txtDatabase.Text.Trim(),
        IntegratedSecurity = chkWindows.Checked,
        Encrypt = true,
        TrustServerCertificate = chkTrustCertificate.Checked,
        ConnectTimeout = 5,
        PersistSecurityInfo = false
      };
      if (!chkWindows.Checked)
      {
        builder.UserID = txtUser.Text.Trim();
        builder.Password = txtPassword.Text;
      }
      return builder.ConnectionString;
    }

    private void TestConnection()
    {
      try
      {
        string error = ConnectionSettingsStore.Test(BuildConnectionString());
        UiMessage.Show(error ?? UiText.Get("ConnectionVerified"), error == null ? UiText.Get("Success") : UiText.Get("ConnectionError"),
          MessageBoxButtons.OK, error == null ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
      }
      catch (Exception ex) { UiMessage.Show(ex.Message, UiText.Get("ConnectionError"), MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private void SaveConnection()
    {
      try
      {
        string connectionString = BuildConnectionString();
        string error = ConnectionSettingsStore.Test(connectionString);
        if (error != null)
        {
          UiMessage.Show(error, UiText.Get("ConnectionError"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
          return;
        }
        ConnectionSettingsStore.Save(connectionString, _settingsPath);
        SqlDatabaseConnection.ResetConnection();
        DialogResult = DialogResult.OK;
        Close();
      }
      catch (Exception ex) { UiMessage.Show(UiText.Get("ConnectionSaveError") + ex.Message, UiText.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
  }
}
