namespace InventoryTracking.UI.Forms
{
  partial class AddEditProductForm
  {
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddEditProductForm));
      this.pnlTop = new System.Windows.Forms.Panel();
      this.lblTitle = new System.Windows.Forms.Label();
      this.pnlForm = new System.Windows.Forms.Panel();
      this.lblBarkod = new System.Windows.Forms.Label();
      this.txtBarkod = new System.Windows.Forms.TextBox();
      this.lblUrunAdi = new System.Windows.Forms.Label();
      this.txtUrunAdi = new System.Windows.Forms.TextBox();
      this.lblKategori = new System.Windows.Forms.Label();
      this.cmbKategori = new System.Windows.Forms.ComboBox();
      this.lblBirim = new System.Windows.Forms.Label();
      this.cmbBirim = new System.Windows.Forms.ComboBox();
      this.lblFiyat = new System.Windows.Forms.Label();
      this.numFiyat = new System.Windows.Forms.NumericUpDown();
      this.lblMinStok = new System.Windows.Forms.Label();
      this.numMinStok = new System.Windows.Forms.NumericUpDown();
      this.lblMaksStok = new System.Windows.Forms.Label();
      this.numMaksStok = new System.Windows.Forms.NumericUpDown();
      this.lblMiktar = new System.Windows.Forms.Label();
      this.numMiktar = new System.Windows.Forms.NumericUpDown();
      this.pnlButtons = new System.Windows.Forms.Panel();
      this.btnKaydet = new System.Windows.Forms.Button();
      this.btnIptal = new System.Windows.Forms.Button();
      this.pnlTop.SuspendLayout();
      this.pnlForm.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numFiyat)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numMinStok)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numMaksStok)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numMiktar)).BeginInit();
      this.pnlButtons.SuspendLayout();
      this.SuspendLayout();
      // 
      // pnlTop
      // 
      this.pnlTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
      this.pnlTop.Controls.Add(this.lblTitle);
      this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlTop.Location = new System.Drawing.Point(0, 0);
      this.pnlTop.Name = "pnlTop";
      this.pnlTop.Padding = new System.Windows.Forms.Padding(15);
      this.pnlTop.Size = new System.Drawing.Size(418, 50);
      this.pnlTop.TabIndex = 0;
      // 
      // lblTitle
      // 
      this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
      this.lblTitle.ForeColor = System.Drawing.Color.White;
      this.lblTitle.Location = new System.Drawing.Point(15, 15);
      this.lblTitle.Name = "lblTitle";
      this.lblTitle.Size = new System.Drawing.Size(388, 20);
      this.lblTitle.TabIndex = 0;
      this.lblTitle.Text = "ÜRÜN BİLGİLERİ";
      this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // pnlForm
      // 
      this.pnlForm.BackColor = System.Drawing.Color.White;
      this.pnlForm.Controls.Add(this.lblBarkod);
      this.pnlForm.Controls.Add(this.txtBarkod);
      this.pnlForm.Controls.Add(this.lblUrunAdi);
      this.pnlForm.Controls.Add(this.txtUrunAdi);
      this.pnlForm.Controls.Add(this.lblKategori);
      this.pnlForm.Controls.Add(this.cmbKategori);
      this.pnlForm.Controls.Add(this.lblBirim);
      this.pnlForm.Controls.Add(this.cmbBirim);
      this.pnlForm.Controls.Add(this.lblFiyat);
      this.pnlForm.Controls.Add(this.numFiyat);
      this.pnlForm.Controls.Add(this.lblMinStok);
      this.pnlForm.Controls.Add(this.numMinStok);
      this.pnlForm.Controls.Add(this.lblMaksStok);
      this.pnlForm.Controls.Add(this.numMaksStok);
      this.pnlForm.Controls.Add(this.lblMiktar);
      this.pnlForm.Controls.Add(this.numMiktar);
      this.pnlForm.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlForm.Location = new System.Drawing.Point(0, 50);
      this.pnlForm.Name = "pnlForm";
      this.pnlForm.Padding = new System.Windows.Forms.Padding(20);
      this.pnlForm.Size = new System.Drawing.Size(418, 332);
      this.pnlForm.TabIndex = 1;
      // 
      // lblBarkod
      // 
      this.lblBarkod.AutoSize = true;
      this.lblBarkod.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
      this.lblBarkod.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblBarkod.Location = new System.Drawing.Point(20, 15);
      this.lblBarkod.Name = "lblBarkod";
      this.lblBarkod.Size = new System.Drawing.Size(63, 20);
      this.lblBarkod.TabIndex = 0;
      this.lblBarkod.Text = "Barkod:";
      // 
      // txtBarkod
      // 
      this.txtBarkod.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.txtBarkod.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.txtBarkod.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.txtBarkod.Location = new System.Drawing.Point(150, 12);
      this.txtBarkod.Name = "txtBarkod";
      this.txtBarkod.Size = new System.Drawing.Size(250, 25);
      this.txtBarkod.TabIndex = 0;
      // 
      // lblUrunAdi
      // 
      this.lblUrunAdi.AutoSize = true;
      this.lblUrunAdi.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
      this.lblUrunAdi.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblUrunAdi.Location = new System.Drawing.Point(20, 55);
      this.lblUrunAdi.Name = "lblUrunAdi";
      this.lblUrunAdi.Size = new System.Drawing.Size(76, 20);
      this.lblUrunAdi.TabIndex = 1;
      this.lblUrunAdi.Text = "Ürün Adı:";
      // 
      // txtUrunAdi
      // 
      this.txtUrunAdi.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.txtUrunAdi.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.txtUrunAdi.Location = new System.Drawing.Point(150, 52);
      this.txtUrunAdi.Name = "txtUrunAdi";
      this.txtUrunAdi.Size = new System.Drawing.Size(250, 25);
      this.txtUrunAdi.TabIndex = 1;
      // 
      // lblKategori
      // 
      this.lblKategori.AutoSize = true;
      this.lblKategori.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
      this.lblKategori.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblKategori.Location = new System.Drawing.Point(20, 95);
      this.lblKategori.Name = "lblKategori";
      this.lblKategori.Size = new System.Drawing.Size(73, 20);
      this.lblKategori.TabIndex = 2;
      this.lblKategori.Text = "Kategori:";
      // 
      // cmbKategori
      // 
      this.cmbKategori.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbKategori.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.cmbKategori.Location = new System.Drawing.Point(150, 92);
      this.cmbKategori.Name = "cmbKategori";
      this.cmbKategori.Size = new System.Drawing.Size(250, 25);
      this.cmbKategori.TabIndex = 2;
      // 
      // lblBirim
      // 
      this.lblBirim.AutoSize = true;
      this.lblBirim.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
      this.lblBirim.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblBirim.Location = new System.Drawing.Point(20, 135);
      this.lblBirim.Name = "lblBirim";
      this.lblBirim.Size = new System.Drawing.Size(51, 20);
      this.lblBirim.TabIndex = 3;
      this.lblBirim.Text = "Birim:";
      // 
      // cmbBirim
      // 
      this.cmbBirim.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbBirim.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.cmbBirim.Location = new System.Drawing.Point(150, 132);
      this.cmbBirim.Name = "cmbBirim";
      this.cmbBirim.Size = new System.Drawing.Size(250, 25);
      this.cmbBirim.TabIndex = 3;
      // 
      // lblFiyat
      // 
      this.lblFiyat.AutoSize = true;
      this.lblFiyat.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
      this.lblFiyat.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblFiyat.Location = new System.Drawing.Point(20, 175);
      this.lblFiyat.Name = "lblFiyat";
      this.lblFiyat.Size = new System.Drawing.Size(47, 20);
      this.lblFiyat.TabIndex = 4;
      this.lblFiyat.Text = "Fiyat:";
      // 
      // numFiyat
      // 
      this.numFiyat.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.numFiyat.DecimalPlaces = 2;
      this.numFiyat.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.numFiyat.Location = new System.Drawing.Point(150, 172);
      this.numFiyat.Name = "numFiyat";
      this.numFiyat.Size = new System.Drawing.Size(250, 25);
      this.numFiyat.TabIndex = 4;
      // 
      // lblMinStok
      // 
      this.lblMinStok.AutoSize = true;
      this.lblMinStok.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
      this.lblMinStok.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblMinStok.Location = new System.Drawing.Point(20, 215);
      this.lblMinStok.Name = "lblMinStok";
      this.lblMinStok.Size = new System.Drawing.Size(80, 20);
      this.lblMinStok.TabIndex = 5;
      this.lblMinStok.Text = "Min. Stok:";
      // 
      // numMinStok
      // 
      this.numMinStok.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.numMinStok.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.numMinStok.Location = new System.Drawing.Point(150, 212);
      this.numMinStok.Name = "numMinStok";
      this.numMinStok.Size = new System.Drawing.Size(250, 25);
      this.numMinStok.TabIndex = 5;
      // 
      // lblMaksStok
      // 
      this.lblMaksStok.AutoSize = true;
      this.lblMaksStok.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
      this.lblMaksStok.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblMaksStok.Location = new System.Drawing.Point(20, 255);
      this.lblMaksStok.Name = "lblMaksStok";
      this.lblMaksStok.Size = new System.Drawing.Size(90, 20);
      this.lblMaksStok.TabIndex = 6;
      this.lblMaksStok.Text = "Maks. Stok:";
      // 
      // numMaksStok
      // 
      this.numMaksStok.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.numMaksStok.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.numMaksStok.Location = new System.Drawing.Point(150, 252);
      this.numMaksStok.Name = "numMaksStok";
      this.numMaksStok.Size = new System.Drawing.Size(250, 25);
      this.numMaksStok.TabIndex = 6;
      // 
      // lblMiktar
      // 
      this.lblMiktar.AutoSize = true;
      this.lblMiktar.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
      this.lblMiktar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblMiktar.Location = new System.Drawing.Point(20, 295);
      this.lblMiktar.Name = "lblMiktar";
      this.lblMiktar.Size = new System.Drawing.Size(60, 20);
      this.lblMiktar.TabIndex = 7;
      this.lblMiktar.Text = "Miktar:";
      // 
      // numMiktar
      // 
      this.numMiktar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.numMiktar.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.numMiktar.Location = new System.Drawing.Point(150, 292);
      this.numMiktar.Name = "numMiktar";
      this.numMiktar.Size = new System.Drawing.Size(250, 25);
      this.numMiktar.TabIndex = 7;
      // 
      // pnlButtons
      // 
      this.pnlButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
      this.pnlButtons.Controls.Add(this.btnKaydet);
      this.pnlButtons.Controls.Add(this.btnIptal);
      this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.pnlButtons.Location = new System.Drawing.Point(0, 382);
      this.pnlButtons.Name = "pnlButtons";
      this.pnlButtons.Padding = new System.Windows.Forms.Padding(20);
      this.pnlButtons.Size = new System.Drawing.Size(418, 60);
      this.pnlButtons.TabIndex = 2;
      // 
      // btnKaydet
      // 
      this.btnKaydet.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.btnKaydet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
      this.btnKaydet.FlatAppearance.BorderSize = 0;
      this.btnKaydet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnKaydet.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
      this.btnKaydet.ForeColor = System.Drawing.Color.White;
      this.btnKaydet.Location = new System.Drawing.Point(12, 10);
      this.btnKaydet.Name = "btnKaydet";
      this.btnKaydet.Size = new System.Drawing.Size(120, 40);
      this.btnKaydet.TabIndex = 0;
      this.btnKaydet.Text = "✓ Kaydet";
      this.btnKaydet.UseVisualStyleBackColor = false;
      // 
      // btnIptal
      // 
      this.btnIptal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnIptal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(165)))), ((int)(((byte)(166)))));
      this.btnIptal.FlatAppearance.BorderSize = 0;
      this.btnIptal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnIptal.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
      this.btnIptal.ForeColor = System.Drawing.Color.White;
      this.btnIptal.Location = new System.Drawing.Point(280, 10);
      this.btnIptal.Name = "btnIptal";
      this.btnIptal.Size = new System.Drawing.Size(120, 40);
      this.btnIptal.TabIndex = 1;
      this.btnIptal.Text = "✕ İptal";
      this.btnIptal.UseVisualStyleBackColor = false;
      // 
      // AddEditProductForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(418, 442);
      this.ControlBox = false;
      this.Controls.Add(this.pnlForm);
      this.Controls.Add(this.pnlButtons);
      this.Controls.Add(this.pnlTop);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "AddEditProductForm";
      this.ShowIcon = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Load += new System.EventHandler(this.AddEditProductForm_Load);
      this.pnlTop.ResumeLayout(false);
      this.pnlForm.ResumeLayout(false);
      this.pnlForm.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numFiyat)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numMinStok)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numMaksStok)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numMiktar)).EndInit();
      this.pnlButtons.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    private System.Windows.Forms.Panel pnlTop;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Panel pnlForm;
    private System.Windows.Forms.Label lblBarkod;
    private System.Windows.Forms.TextBox txtBarkod;
    private System.Windows.Forms.Label lblUrunAdi;
    private System.Windows.Forms.TextBox txtUrunAdi;
    private System.Windows.Forms.Label lblKategori;
    private System.Windows.Forms.ComboBox cmbKategori;
    private System.Windows.Forms.Label lblBirim;
    private System.Windows.Forms.ComboBox cmbBirim;
    private System.Windows.Forms.Label lblFiyat;
    private System.Windows.Forms.NumericUpDown numFiyat;
    private System.Windows.Forms.Label lblMinStok;
    private System.Windows.Forms.NumericUpDown numMinStok;
    private System.Windows.Forms.Label lblMaksStok;
    private System.Windows.Forms.NumericUpDown numMaksStok;
    private System.Windows.Forms.Label lblMiktar;
    private System.Windows.Forms.NumericUpDown numMiktar;
    private System.Windows.Forms.Panel pnlButtons;
    private System.Windows.Forms.Button btnKaydet;
    private System.Windows.Forms.Button btnIptal;
  }
}