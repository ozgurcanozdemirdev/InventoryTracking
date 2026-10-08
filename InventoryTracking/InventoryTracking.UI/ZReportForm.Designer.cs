namespace InventoryTracking.UI
{
  partial class ZReportForm
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
      this.pnlHeader = new System.Windows.Forms.Panel();
      this.lblTitle = new System.Windows.Forms.Label();
      this.pnlFilter = new System.Windows.Forms.Panel();
      this.rbYillik = new System.Windows.Forms.RadioButton();
      this.rbAylik = new System.Windows.Forms.RadioButton();
      this.rbGunluk = new System.Windows.Forms.RadioButton();
      this.lblBitis = new System.Windows.Forms.Label();
      this.dtpBitis = new System.Windows.Forms.DateTimePicker();
      this.lblBaslangic = new System.Windows.Forms.Label();
      this.dtpBaslangic = new System.Windows.Forms.DateTimePicker();
      this.pnlButtons = new System.Windows.Forms.Panel();
      this.btnKapat = new System.Windows.Forms.Button();
      this.btnPdf = new System.Windows.Forms.Button();
      this.btnExcel = new System.Windows.Forms.Button();
      this.btnGetir = new System.Windows.Forms.Button();
      this.pnlGrid = new System.Windows.Forms.Panel();
      this.dgvRapor = new System.Windows.Forms.DataGridView();
      this.pnlHeader.SuspendLayout();
      this.pnlFilter.SuspendLayout();
      this.pnlButtons.SuspendLayout();
      this.pnlGrid.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgvRapor)).BeginInit();
      this.SuspendLayout();
      // 
      // pnlHeader
      // 
      this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
      this.pnlHeader.Controls.Add(this.lblTitle);
      this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlHeader.Location = new System.Drawing.Point(0, 0);
      this.pnlHeader.Name = "pnlHeader";
      this.pnlHeader.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
      this.pnlHeader.Size = new System.Drawing.Size(884, 50);
      this.pnlHeader.TabIndex = 0;
      // 
      // lblTitle
      // 
      this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
      this.lblTitle.ForeColor = System.Drawing.Color.White;
      this.lblTitle.Location = new System.Drawing.Point(15, 0);
      this.lblTitle.Name = "lblTitle";
      this.lblTitle.Size = new System.Drawing.Size(869, 50);
      this.lblTitle.TabIndex = 0;
      this.lblTitle.Text = "STOK HAREKET RAPORU";
      this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.lblTitle.Click += new System.EventHandler(this.lblTitle_Click);
      // 
      // pnlFilter
      // 
      this.pnlFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
      this.pnlFilter.Controls.Add(this.rbYillik);
      this.pnlFilter.Controls.Add(this.rbAylik);
      this.pnlFilter.Controls.Add(this.rbGunluk);
      this.pnlFilter.Controls.Add(this.lblBitis);
      this.pnlFilter.Controls.Add(this.dtpBitis);
      this.pnlFilter.Controls.Add(this.lblBaslangic);
      this.pnlFilter.Controls.Add(this.dtpBaslangic);
      this.pnlFilter.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlFilter.Location = new System.Drawing.Point(0, 50);
      this.pnlFilter.Name = "pnlFilter";
      this.pnlFilter.Size = new System.Drawing.Size(884, 70);
      this.pnlFilter.TabIndex = 1;
      // 
      // rbYillik
      // 
      this.rbYillik.AutoSize = true;
      this.rbYillik.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.rbYillik.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.rbYillik.Location = new System.Drawing.Point(630, 24);
      this.rbYillik.Name = "rbYillik";
      this.rbYillik.Size = new System.Drawing.Size(60, 23);
      this.rbYillik.TabIndex = 6;
      this.rbYillik.Text = "Yıllık";
      this.rbYillik.UseVisualStyleBackColor = true;
      // 
      // rbAylik
      // 
      this.rbAylik.AutoSize = true;
      this.rbAylik.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.rbAylik.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.rbAylik.Location = new System.Drawing.Point(550, 24);
      this.rbAylik.Name = "rbAylik";
      this.rbAylik.Size = new System.Drawing.Size(61, 23);
      this.rbAylik.TabIndex = 5;
      this.rbAylik.Text = "Aylık";
      this.rbAylik.UseVisualStyleBackColor = true;
      // 
      // rbGunluk
      // 
      this.rbGunluk.AutoSize = true;
      this.rbGunluk.Checked = true;
      this.rbGunluk.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.rbGunluk.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.rbGunluk.Location = new System.Drawing.Point(460, 24);
      this.rbGunluk.Name = "rbGunluk";
      this.rbGunluk.Size = new System.Drawing.Size(73, 23);
      this.rbGunluk.TabIndex = 4;
      this.rbGunluk.TabStop = true;
      this.rbGunluk.Text = "Günlük";
      this.rbGunluk.UseVisualStyleBackColor = true;
      // 
      // lblBitis
      // 
      this.lblBitis.AutoSize = true;
      this.lblBitis.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.lblBitis.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblBitis.Location = new System.Drawing.Point(235, 26);
      this.lblBitis.Name = "lblBitis";
      this.lblBitis.Size = new System.Drawing.Size(82, 19);
      this.lblBitis.TabIndex = 3;
      this.lblBitis.Text = "Bitiş Tarihi:";
      // 
      // dtpBitis
      // 
      this.dtpBitis.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.dtpBitis.Format = System.Windows.Forms.DateTimePickerFormat.Short;
      this.dtpBitis.Location = new System.Drawing.Point(320, 23);
      this.dtpBitis.Name = "dtpBitis";
      this.dtpBitis.Size = new System.Drawing.Size(110, 25);
      this.dtpBitis.TabIndex = 2;
      // 
      // lblBaslangic
      // 
      this.lblBaslangic.AutoSize = true;
      this.lblBaslangic.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.lblBaslangic.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblBaslangic.Location = new System.Drawing.Point(11, 26);
      this.lblBaslangic.Name = "lblBaslangic";
      this.lblBaslangic.Size = new System.Drawing.Size(91, 19);
      this.lblBaslangic.TabIndex = 1;
      this.lblBaslangic.Text = "Başlangıç T.:";
      // 
      // dtpBaslangic
      // 
      this.dtpBaslangic.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.dtpBaslangic.Format = System.Windows.Forms.DateTimePickerFormat.Short;
      this.dtpBaslangic.Location = new System.Drawing.Point(105, 23);
      this.dtpBaslangic.Name = "dtpBaslangic";
      this.dtpBaslangic.Size = new System.Drawing.Size(124, 25);
      this.dtpBaslangic.TabIndex = 0;
      // 
      // pnlButtons
      // 
      this.pnlButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
      this.pnlButtons.Controls.Add(this.btnKapat);
      this.pnlButtons.Controls.Add(this.btnPdf);
      this.pnlButtons.Controls.Add(this.btnExcel);
      this.pnlButtons.Controls.Add(this.btnGetir);
      this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.pnlButtons.Location = new System.Drawing.Point(0, 491);
      this.pnlButtons.Name = "pnlButtons";
      this.pnlButtons.Padding = new System.Windows.Forms.Padding(15);
      this.pnlButtons.Size = new System.Drawing.Size(884, 70);
      this.pnlButtons.TabIndex = 2;
      // 
      // btnKapat
      // 
      this.btnKapat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnKapat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
      this.btnKapat.FlatAppearance.BorderSize = 0;
      this.btnKapat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnKapat.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.btnKapat.ForeColor = System.Drawing.Color.White;
      this.btnKapat.Location = new System.Drawing.Point(749, 15);
      this.btnKapat.Name = "btnKapat";
      this.btnKapat.Size = new System.Drawing.Size(120, 40);
      this.btnKapat.TabIndex = 3;
      this.btnKapat.Text = "✕ Kapat";
      this.btnKapat.UseVisualStyleBackColor = false;
      this.btnKapat.Click += new System.EventHandler(this.btnKapat_Click);
      // 
      // btnPdf
      // 
      this.btnPdf.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(126)))), ((int)(((byte)(34)))));
      this.btnPdf.FlatAppearance.BorderSize = 0;
      this.btnPdf.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnPdf.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.btnPdf.ForeColor = System.Drawing.Color.White;
      this.btnPdf.Location = new System.Drawing.Point(315, 15);
      this.btnPdf.Name = "btnPdf";
      this.btnPdf.Size = new System.Drawing.Size(140, 40);
      this.btnPdf.TabIndex = 2;
      this.btnPdf.Text = "↓ PDF\'e Aktar";
      this.btnPdf.UseVisualStyleBackColor = false;
      this.btnPdf.Click += new System.EventHandler(this.btnPdf_Click);
      // 
      // btnExcel
      // 
      this.btnExcel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
      this.btnExcel.FlatAppearance.BorderSize = 0;
      this.btnExcel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnExcel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.btnExcel.ForeColor = System.Drawing.Color.White;
      this.btnExcel.Location = new System.Drawing.Point(165, 15);
      this.btnExcel.Name = "btnExcel";
      this.btnExcel.Size = new System.Drawing.Size(140, 40);
      this.btnExcel.TabIndex = 1;
      this.btnExcel.Text = "✓ CSV\'ye Aktar";
      this.btnExcel.UseVisualStyleBackColor = false;
      this.btnExcel.Click += new System.EventHandler(this.btnExcel_Click);
      // 
      // btnGetir
      // 
      this.btnGetir.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
      this.btnGetir.FlatAppearance.BorderSize = 0;
      this.btnGetir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnGetir.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.btnGetir.ForeColor = System.Drawing.Color.White;
      this.btnGetir.Location = new System.Drawing.Point(15, 15);
      this.btnGetir.Name = "btnGetir";
      this.btnGetir.Size = new System.Drawing.Size(140, 40);
      this.btnGetir.TabIndex = 0;
      this.btnGetir.Text = "↻ Raporu Getir";
      this.btnGetir.UseVisualStyleBackColor = false;
      this.btnGetir.Click += new System.EventHandler(this.btnGetir_Click);
      // 
      // pnlGrid
      // 
      this.pnlGrid.Controls.Add(this.dgvRapor);
      this.pnlGrid.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlGrid.Location = new System.Drawing.Point(0, 120);
      this.pnlGrid.Name = "pnlGrid";
      this.pnlGrid.Padding = new System.Windows.Forms.Padding(15);
      this.pnlGrid.Size = new System.Drawing.Size(884, 371);
      this.pnlGrid.TabIndex = 3;
      // 
      // dgvRapor
      // 
      this.dgvRapor.AllowUserToAddRows = false;
      this.dgvRapor.AllowUserToDeleteRows = false;
      this.dgvRapor.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
      this.dgvRapor.BackgroundColor = System.Drawing.Color.White;
      this.dgvRapor.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.dgvRapor.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dgvRapor.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dgvRapor.Location = new System.Drawing.Point(15, 15);
      this.dgvRapor.Name = "dgvRapor";
      this.dgvRapor.ReadOnly = true;
      this.dgvRapor.RowHeadersVisible = false;
      this.dgvRapor.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dgvRapor.Size = new System.Drawing.Size(854, 341);
      this.dgvRapor.TabIndex = 0;
      // 
      // ZReportForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(884, 561);
      this.ControlBox = false;
      this.Controls.Add(this.pnlGrid);
      this.Controls.Add(this.pnlButtons);
      this.Controls.Add(this.pnlFilter);
      this.Controls.Add(this.pnlHeader);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "ZReportForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Load += new System.EventHandler(this.ZReportForm_Load);
      this.pnlHeader.ResumeLayout(false);
      this.pnlFilter.ResumeLayout(false);
      this.pnlFilter.PerformLayout();
      this.pnlButtons.ResumeLayout(false);
      this.pnlGrid.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dgvRapor)).EndInit();
      this.ResumeLayout(false);

    }

    private System.Windows.Forms.Panel pnlHeader;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Panel pnlFilter;
    private System.Windows.Forms.Label lblBaslangic;
    private System.Windows.Forms.DateTimePicker dtpBaslangic;
    private System.Windows.Forms.RadioButton rbYillik;
    private System.Windows.Forms.RadioButton rbAylik;
    private System.Windows.Forms.RadioButton rbGunluk;
    private System.Windows.Forms.Label lblBitis;
    private System.Windows.Forms.DateTimePicker dtpBitis;
    private System.Windows.Forms.Panel pnlButtons;
    private System.Windows.Forms.Button btnGetir;
    private System.Windows.Forms.Button btnKapat;
    private System.Windows.Forms.Button btnPdf;
    private System.Windows.Forms.Button btnExcel;
    private System.Windows.Forms.Panel pnlGrid;
    private System.Windows.Forms.DataGridView dgvRapor;
  }
}
