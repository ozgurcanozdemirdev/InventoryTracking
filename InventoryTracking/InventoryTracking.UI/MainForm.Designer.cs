namespace InventoryTracking.UI
{
  partial class MainForm
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
      this.pnlTop = new System.Windows.Forms.Panel();
      this.lblTitle = new System.Windows.Forms.Label();
      this.pnlSearch = new System.Windows.Forms.Panel();
      this.lblBarkod = new System.Windows.Forms.Label();
      this.txtBarkod = new System.Windows.Forms.TextBox();
      this.pnlFilters = new System.Windows.Forms.Panel();
      this.lblDepo = new System.Windows.Forms.Label();
      this.cmbDepo = new System.Windows.Forms.ComboBox();
      this.pnlButtons = new System.Windows.Forms.Panel();
      this.btnYeniUrun = new System.Windows.Forms.Button();
      this.btnDuzenle = new System.Windows.Forms.Button();
      this.btnSil = new System.Windows.Forms.Button();
      this.btnYenile = new System.Windows.Forms.Button();
      this.btnKapat = new System.Windows.Forms.Button();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
      this.dgvStoklar = new System.Windows.Forms.DataGridView();
      this.btnZRaporu = new System.Windows.Forms.Button();
      this.pnlTop.SuspendLayout();
      this.pnlSearch.SuspendLayout();
      this.pnlFilters.SuspendLayout();
      this.pnlButtons.SuspendLayout();
      this.statusStrip1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgvStoklar)).BeginInit();
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
      this.pnlTop.Size = new System.Drawing.Size(1200, 60);
      this.pnlTop.TabIndex = 0;
      // 
      // lblTitle
      // 
      this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
      this.lblTitle.ForeColor = System.Drawing.Color.White;
      this.lblTitle.Location = new System.Drawing.Point(15, 15);
      this.lblTitle.Name = "lblTitle";
      this.lblTitle.Size = new System.Drawing.Size(1170, 30);
      this.lblTitle.TabIndex = 0;
      this.lblTitle.Text = "STOK TAKİP SİSTEMİ";
      this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // pnlSearch
      // 
      this.pnlSearch.BackColor = System.Drawing.Color.White;
      this.pnlSearch.Controls.Add(this.lblBarkod);
      this.pnlSearch.Controls.Add(this.txtBarkod);
      this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlSearch.Location = new System.Drawing.Point(0, 60);
      this.pnlSearch.Name = "pnlSearch";
      this.pnlSearch.Padding = new System.Windows.Forms.Padding(15);
      this.pnlSearch.Size = new System.Drawing.Size(1200, 70);
      this.pnlSearch.TabIndex = 1;
      // 
      // lblBarkod
      // 
      this.lblBarkod.AutoSize = true;
      this.lblBarkod.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
      this.lblBarkod.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblBarkod.Location = new System.Drawing.Point(15, 12);
      this.lblBarkod.Name = "lblBarkod";
      this.lblBarkod.Size = new System.Drawing.Size(123, 20);
      this.lblBarkod.TabIndex = 0;
      this.lblBarkod.Text = "Barkod Okutma:";
      // 
      // txtBarkod
      // 
      this.txtBarkod.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.txtBarkod.Font = new System.Drawing.Font("Segoe UI", 14F);
      this.txtBarkod.Location = new System.Drawing.Point(15, 35);
      this.txtBarkod.Name = "txtBarkod";
      this.txtBarkod.Size = new System.Drawing.Size(300, 32);
      this.txtBarkod.TabIndex = 0;
      this.txtBarkod.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtBarkod_KeyPress);
      // 
      // pnlFilters
      // 
      this.pnlFilters.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
      this.pnlFilters.Controls.Add(this.lblDepo);
      this.pnlFilters.Controls.Add(this.cmbDepo);
      this.pnlFilters.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlFilters.Location = new System.Drawing.Point(0, 130);
      this.pnlFilters.Name = "pnlFilters";
      this.pnlFilters.Padding = new System.Windows.Forms.Padding(15);
      this.pnlFilters.Size = new System.Drawing.Size(1200, 70);
      this.pnlFilters.TabIndex = 2;
      // 
      // lblDepo
      // 
      this.lblDepo.AutoSize = true;
      this.lblDepo.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
      this.lblDepo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblDepo.Location = new System.Drawing.Point(15, 12);
      this.lblDepo.Name = "lblDepo";
      this.lblDepo.Size = new System.Drawing.Size(99, 20);
      this.lblDepo.TabIndex = 0;
      this.lblDepo.Text = "Depo Seçimi:";
      // 
      // cmbDepo
      // 
      this.cmbDepo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbDepo.Font = new System.Drawing.Font("Segoe UI", 11F);
      this.cmbDepo.Location = new System.Drawing.Point(15, 35);
      this.cmbDepo.Name = "cmbDepo";
      this.cmbDepo.Size = new System.Drawing.Size(300, 28);
      this.cmbDepo.TabIndex = 1;
      this.cmbDepo.SelectedIndexChanged += new System.EventHandler(this.cmbDepo_SelectedIndexChanged);
      // 
      // pnlButtons
      // 
      this.pnlButtons.BackColor = System.Drawing.Color.White;
      this.pnlButtons.Controls.Add(this.btnZRaporu);
      this.pnlButtons.Controls.Add(this.btnYeniUrun);
      this.pnlButtons.Controls.Add(this.btnDuzenle);
      this.pnlButtons.Controls.Add(this.btnSil);
      this.pnlButtons.Controls.Add(this.btnYenile);
      this.pnlButtons.Controls.Add(this.btnKapat);
      this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlButtons.Location = new System.Drawing.Point(0, 200);
      this.pnlButtons.Name = "pnlButtons";
      this.pnlButtons.Padding = new System.Windows.Forms.Padding(15);
      this.pnlButtons.Size = new System.Drawing.Size(1200, 60);
      this.pnlButtons.TabIndex = 3;
      // 
      // btnYeniUrun
      // 
      this.btnYeniUrun.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
      this.btnYeniUrun.FlatAppearance.BorderSize = 0;
      this.btnYeniUrun.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnYeniUrun.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.btnYeniUrun.ForeColor = System.Drawing.Color.White;
      this.btnYeniUrun.Location = new System.Drawing.Point(15, 10);
      this.btnYeniUrun.Name = "btnYeniUrun";
      this.btnYeniUrun.Size = new System.Drawing.Size(120, 40);
      this.btnYeniUrun.TabIndex = 0;
      this.btnYeniUrun.Text = "✓ Yeni Ürün";
      this.btnYeniUrun.UseVisualStyleBackColor = false;
      this.btnYeniUrun.Click += new System.EventHandler(this.btnYeniUrun_Click);
      // 
      // btnDuzenle
      // 
      this.btnDuzenle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
      this.btnDuzenle.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnDuzenle.FlatAppearance.BorderSize = 0;
      this.btnDuzenle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnDuzenle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.btnDuzenle.ForeColor = System.Drawing.Color.White;
      this.btnDuzenle.Location = new System.Drawing.Point(145, 10);
      this.btnDuzenle.Name = "btnDuzenle";
      this.btnDuzenle.Size = new System.Drawing.Size(120, 40);
      this.btnDuzenle.TabIndex = 1;
      this.btnDuzenle.Text = "✎ Düzenle";
      this.btnDuzenle.UseVisualStyleBackColor = false;
      this.btnDuzenle.Click += new System.EventHandler(this.btnDuzenle_Click);
      // 
      // btnSil
      // 
      this.btnSil.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
      this.btnSil.FlatAppearance.BorderSize = 0;
      this.btnSil.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnSil.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.btnSil.ForeColor = System.Drawing.Color.White;
      this.btnSil.Location = new System.Drawing.Point(275, 10);
      this.btnSil.Name = "btnSil";
      this.btnSil.Size = new System.Drawing.Size(120, 40);
      this.btnSil.TabIndex = 2;
      this.btnSil.Text = "✕ Sil";
      this.btnSil.UseVisualStyleBackColor = false;
      this.btnSil.Click += new System.EventHandler(this.btnSil_Click);
      // 
      // btnYenile
      // 
      this.btnYenile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(89)))), ((int)(((byte)(182)))));
      this.btnYenile.FlatAppearance.BorderSize = 0;
      this.btnYenile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnYenile.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.btnYenile.ForeColor = System.Drawing.Color.White;
      this.btnYenile.Location = new System.Drawing.Point(405, 10);
      this.btnYenile.Name = "btnYenile";
      this.btnYenile.Size = new System.Drawing.Size(120, 40);
      this.btnYenile.TabIndex = 3;
      this.btnYenile.Text = "↻ Yenile";
      this.btnYenile.UseVisualStyleBackColor = false;
      this.btnYenile.Click += new System.EventHandler(this.btnYenile_Click);
      // 
      // btnKapat
      // 
      this.btnKapat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnKapat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
      this.btnKapat.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnKapat.FlatAppearance.BorderSize = 0;
      this.btnKapat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnKapat.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.btnKapat.ForeColor = System.Drawing.Color.White;
      this.btnKapat.Location = new System.Drawing.Point(1065, 10);
      this.btnKapat.Name = "btnKapat";
      this.btnKapat.Size = new System.Drawing.Size(120, 40);
      this.btnKapat.TabIndex = 4;
      this.btnKapat.Text = "✕ Çıkış";
      this.btnKapat.UseVisualStyleBackColor = false;
      this.btnKapat.Click += new System.EventHandler(this.btnKapat_Click);
      // 
      // statusStrip1
      // 
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
      this.statusStrip1.Location = new System.Drawing.Point(0, 660);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new System.Drawing.Size(1200, 22);
      this.statusStrip1.TabIndex = 5;
      // 
      // toolStripStatusLabel
      // 
      this.toolStripStatusLabel.Name = "toolStripStatusLabel";
      this.toolStripStatusLabel.Size = new System.Drawing.Size(34, 17);
      this.toolStripStatusLabel.Text = "Hazır";
      // 
      // dgvStoklar
      // 
      this.dgvStoklar.AllowUserToAddRows = false;
      this.dgvStoklar.AllowUserToDeleteRows = false;
      this.dgvStoklar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dgvStoklar.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dgvStoklar.Location = new System.Drawing.Point(0, 260);
      this.dgvStoklar.Name = "dgvStoklar";
      this.dgvStoklar.ReadOnly = true;
      this.dgvStoklar.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dgvStoklar.Size = new System.Drawing.Size(1200, 400);
      this.dgvStoklar.TabIndex = 6;
      // 
      // btnZRaporu
      // 
      this.btnZRaporu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
      this.btnZRaporu.FlatAppearance.BorderSize = 0;
      this.btnZRaporu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnZRaporu.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.btnZRaporu.ForeColor = System.Drawing.Color.White;
      this.btnZRaporu.Location = new System.Drawing.Point(531, 10);
      this.btnZRaporu.Name = "btnZRaporu";
      this.btnZRaporu.Size = new System.Drawing.Size(120, 40);
      this.btnZRaporu.TabIndex = 5;
      this.btnZRaporu.Text = "📊 Hareketler";
      this.btnZRaporu.UseVisualStyleBackColor = false;
      this.btnZRaporu.Click += new System.EventHandler(this.btnZRaporu_Click);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
      this.ClientSize = new System.Drawing.Size(1200, 682);
      this.ControlBox = false;
      this.Controls.Add(this.dgvStoklar);
      this.Controls.Add(this.pnlButtons);
      this.Controls.Add(this.pnlFilters);
      this.Controls.Add(this.pnlSearch);
      this.Controls.Add(this.pnlTop);
      this.Controls.Add(this.statusStrip1);
      this.Name = "MainForm";
      this.ShowIcon = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
      this.Load += new System.EventHandler(this.MainForm_Load);
      this.pnlTop.ResumeLayout(false);
      this.pnlSearch.ResumeLayout(false);
      this.pnlSearch.PerformLayout();
      this.pnlFilters.ResumeLayout(false);
      this.pnlFilters.PerformLayout();
      this.pnlButtons.ResumeLayout(false);
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgvStoklar)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    private System.Windows.Forms.Panel pnlTop;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Panel pnlSearch;
    private System.Windows.Forms.Label lblBarkod;
    private System.Windows.Forms.TextBox txtBarkod;
    private System.Windows.Forms.Panel pnlFilters;
    private System.Windows.Forms.Label lblDepo;
    private System.Windows.Forms.ComboBox cmbDepo;
    private System.Windows.Forms.Panel pnlButtons;
    private System.Windows.Forms.Button btnYeniUrun;
    private System.Windows.Forms.Button btnDuzenle;
    private System.Windows.Forms.Button btnSil;
    private System.Windows.Forms.Button btnYenile;
    private System.Windows.Forms.Button btnKapat;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    private System.Windows.Forms.DataGridView dgvStoklar;
    private System.Windows.Forms.Button btnZRaporu;
  }
}
