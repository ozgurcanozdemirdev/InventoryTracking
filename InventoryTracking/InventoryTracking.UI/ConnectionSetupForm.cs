using InventoryTracking.Connection;
using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryTracking.UI
{
  public class ConnectionSetupForm : Form
  {
    private readonly TextBox txtServer = new TextBox();
    private readonly TextBox txtDatabase = new TextBox();
    private readonly CheckBox chkWindows = new CheckBox();
    private readonly TextBox txtUser = new TextBox();
    private readonly TextBox txtPassword = new TextBox();
    private readonly CheckBox chkTrustCertificate = new CheckBox();

    public ConnectionSetupForm()
    {
      Text = "Veritabanı Bağlantı Ayarları";
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
        Text = "VERİTABANI BAĞLANTISI", ForeColor = Color.White,
        Font = new Font("Segoe UI", 14F, FontStyle.Bold), Dock = DockStyle.Fill,
        Padding = new Padding(15, 10, 0, 0)
      });
      Controls.Add(header);

      AddRow("SQL Server:", txtServer, 72);
      AddRow("Veritabanı:", txtDatabase, 114);
      AddRow("Kullanıcı:", txtUser, 200);
      AddRow("Parola:", txtPassword, 242);
      txtPassword.UseSystemPasswordChar = true;
      chkWindows.Text = "Windows kimliğiyle bağlan";
      chkWindows.Location = new Point(155, 158);
      chkWindows.Size = new Size(350, 27);
      chkWindows.Checked = true;
      chkWindows.CheckedChanged += (sender, e) => UpdateAuthFields();
      Controls.Add(chkWindows);
      chkTrustCertificate.Text = "Sunucu sertifikasına güven (yalnızca güvenilir yerel sunucu)";
      chkTrustCertificate.Location = new Point(155, 284);
      chkTrustCertificate.Size = new Size(390, 27);
      Controls.Add(chkTrustCertificate);
      Controls.Add(new Label
      {
        Text = "Önce SQL Server'da V1 kurulum betiğini veya mevcut veritabanı geçişlerini çalıştırın.",
        Location = new Point(20, 318), Size = new Size(525, 24), ForeColor = Color.FromArgb(52, 73, 94)
      });

      var footer = new Panel { Dock = DockStyle.Bottom, Height = 59, BackColor = Color.FromArgb(236, 240, 241) };
      var btnTest = MakeButton("Bağlantıyı Sına", 35, Color.FromArgb(52, 152, 219));
      var btnSave = MakeButton("Kaydet ve Aç", 198, Color.FromArgb(46, 204, 113));
      var btnCancel = MakeButton("İptal", 361, Color.FromArgb(149, 165, 166));
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
        string saved = ConnectionSettingsStore.Load();
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
        throw new ArgumentException("SQL Server ve veritabanı adı zorunludur.");
      if (!chkWindows.Checked && (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPassword.Text)))
        throw new ArgumentException("SQL kullanıcı adı ve parola zorunludur.");
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
        MessageBox.Show(error ?? "Bağlantı ve gerekli tablolar doğrulandı.", error == null ? "Başarılı" : "Bağlantı Hatası",
          MessageBoxButtons.OK, error == null ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
      }
      catch (Exception ex) { MessageBox.Show(ex.Message, "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private void SaveConnection()
    {
      try
      {
        string connectionString = BuildConnectionString();
        string error = ConnectionSettingsStore.Test(connectionString);
        if (error != null)
        {
          MessageBox.Show(error, "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
          return;
        }
        ConnectionSettingsStore.Save(connectionString);
        SqlDBConnection.ResetConnection();
        DialogResult = DialogResult.OK;
        Close();
      }
      catch (Exception ex) { MessageBox.Show("Bağlantı kaydedilemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
  }
}
