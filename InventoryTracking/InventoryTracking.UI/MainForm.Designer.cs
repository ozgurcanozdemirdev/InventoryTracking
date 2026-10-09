namespace InventoryTracking.UI
{
  partial class MainForm
  {
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        DisposeAppearanceResources();
        if (components != null) components.Dispose();
      }
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.pnlLayout = new System.Windows.Forms.TableLayoutPanel();
      this.pnlTop = new System.Windows.Forms.TableLayoutPanel();
      this.picLogo = new System.Windows.Forms.PictureBox();
      this.lblTitle = new System.Windows.Forms.Label();
      this.pnlHeaderInfo = new System.Windows.Forms.FlowLayoutPanel();
      this.pnlSearch = new System.Windows.Forms.TableLayoutPanel();
      this.pnlBarcode = new System.Windows.Forms.Panel();
      this.pnlBarkodKutusu = new System.Windows.Forms.Panel();
      this.txtBarkod = new InventoryTracking.Controls.InventorySearchTextBox();
      this.picBarkod = new System.Windows.Forms.PictureBox();
      this.lblBarkod = new System.Windows.Forms.Label();
      this.pnlFilters = new System.Windows.Forms.Panel();
      this.pnlDepoKutusu = new System.Windows.Forms.Panel();
      this.cmbDepo = new System.Windows.Forms.ComboBox();
      this.picDepo = new System.Windows.Forms.PictureBox();
      this.lblDepo = new System.Windows.Forms.Label();
      this.pnlQuantity = new System.Windows.Forms.Panel();
      this.pnlQuantityValue = new System.Windows.Forms.FlowLayoutPanel();
      this.picQuantity = new System.Windows.Forms.PictureBox();
      this.lblStockAmount = new System.Windows.Forms.Label();
      this.lblStockUnit = new System.Windows.Forms.Label();
      this.lblQuantityTitle = new System.Windows.Forms.Label();
      this.pnlButtons = new System.Windows.Forms.TableLayoutPanel();
      this.btnYeniUrun = new System.Windows.Forms.Button();
      this.btnDuzenle = new System.Windows.Forms.Button();
      this.btnSil = new System.Windows.Forms.Button();
      this.btnYenile = new System.Windows.Forms.Button();
      this.btnZRaporu = new System.Windows.Forms.Button();
      this.btnKapat = new System.Windows.Forms.Button();
      this.pnlWorkspace = new System.Windows.Forms.TableLayoutPanel();
      this.dgvStoklar = new System.Windows.Forms.DataGridView();
      this.pnlProductDetail = new System.Windows.Forms.Panel();
      this.pnlHint = new System.Windows.Forms.TableLayoutPanel();
      this.lblHint = new System.Windows.Forms.Label();
      this.lblRecordCount = new System.Windows.Forms.Label();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStripConnection = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStripDepot = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStripVersion = new System.Windows.Forms.ToolStripStatusLabel();
      this.pnlLayout.SuspendLayout();
      this.pnlTop.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
      this.pnlSearch.SuspendLayout();
      this.pnlBarcode.SuspendLayout();
      this.pnlBarkodKutusu.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.picBarkod)).BeginInit();
      this.pnlFilters.SuspendLayout();
      this.pnlDepoKutusu.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.picDepo)).BeginInit();
      this.pnlQuantity.SuspendLayout();
      this.pnlQuantityValue.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.picQuantity)).BeginInit();
      this.pnlButtons.SuspendLayout();
      this.pnlWorkspace.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgvStoklar)).BeginInit();
      this.pnlHint.SuspendLayout();
      this.statusStrip1.SuspendLayout();
      this.SuspendLayout();
      //
      // pnlLayout
      //
      this.pnlLayout.ColumnCount = 1;
      this.pnlLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.pnlLayout.Controls.Add(this.pnlTop, 0, 0);
      this.pnlLayout.Controls.Add(this.pnlSearch, 0, 1);
      this.pnlLayout.Controls.Add(this.pnlButtons, 0, 2);
      this.pnlLayout.Controls.Add(this.pnlWorkspace, 0, 3);
      this.pnlLayout.Controls.Add(this.pnlHint, 0, 4);
      this.pnlLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlLayout.Location = new System.Drawing.Point(0, 0);
      this.pnlLayout.Name = "pnlLayout";
      this.pnlLayout.RowCount = 5;
      this.pnlLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
      this.pnlLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
      this.pnlLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
      this.pnlLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.pnlLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
      this.pnlLayout.Size = new System.Drawing.Size(1440, 808);
      this.pnlLayout.TabIndex = 0;
      //
      // pnlTop
      //
      this.pnlTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(117)))), ((int)(((byte)(201)))));
      this.pnlTop.ColumnCount = 3;
      this.pnlTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 48F));
      this.pnlTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.pnlTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.pnlTop.Controls.Add(this.picLogo, 0, 0);
      this.pnlTop.Controls.Add(this.lblTitle, 1, 0);
      this.pnlTop.Controls.Add(this.pnlHeaderInfo, 2, 0);
      this.pnlTop.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlTop.Location = new System.Drawing.Point(0, 0);
      this.pnlTop.Margin = new System.Windows.Forms.Padding(0);
      this.pnlTop.Name = "pnlTop";
      this.pnlTop.Padding = new System.Windows.Forms.Padding(16, 8, 16, 8);
      this.pnlTop.RowCount = 1;
      this.pnlTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.pnlTop.Size = new System.Drawing.Size(1440, 60);
      this.pnlTop.TabIndex = 0;
      //
      // picLogo
      //
      this.picLogo.Dock = System.Windows.Forms.DockStyle.Fill;
      this.picLogo.Location = new System.Drawing.Point(19, 11);
      this.picLogo.Name = "picLogo";
      this.picLogo.Size = new System.Drawing.Size(42, 38);
      this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      this.picLogo.TabIndex = 0;
      this.picLogo.TabStop = false;
      //
      // lblTitle
      //
      this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
      this.lblTitle.ForeColor = System.Drawing.Color.White;
      this.lblTitle.Location = new System.Drawing.Point(67, 8);
      this.lblTitle.Name = "lblTitle";
      this.lblTitle.Size = new System.Drawing.Size(1348, 44);
      this.lblTitle.TabIndex = 1;
      this.lblTitle.Text = "STOK TAKİP SİSTEMİ";
      this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      //
      // pnlHeaderInfo
      //
      this.pnlHeaderInfo.AutoSize = true;
      this.pnlHeaderInfo.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlHeaderInfo.Location = new System.Drawing.Point(1421, 11);
      this.pnlHeaderInfo.Name = "pnlHeaderInfo";
      this.pnlHeaderInfo.Size = new System.Drawing.Size(1, 38);
      this.pnlHeaderInfo.TabIndex = 2;
      this.pnlHeaderInfo.WrapContents = false;
      //
      // pnlSearch
      //
      this.pnlSearch.ColumnCount = 3;
      this.pnlSearch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 48F));
      this.pnlSearch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27F));
      this.pnlSearch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
      this.pnlSearch.Controls.Add(this.pnlBarcode, 0, 0);
      this.pnlSearch.Controls.Add(this.pnlFilters, 1, 0);
      this.pnlSearch.Controls.Add(this.pnlQuantity, 2, 0);
      this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlSearch.Location = new System.Drawing.Point(16, 72);
      this.pnlSearch.Margin = new System.Windows.Forms.Padding(16, 12, 16, 10);
      this.pnlSearch.Name = "pnlSearch";
      this.pnlSearch.RowCount = 1;
      this.pnlSearch.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.pnlSearch.Size = new System.Drawing.Size(1408, 78);
      this.pnlSearch.TabIndex = 1;
      //
      // pnlBarcode
      //
      this.pnlBarcode.Controls.Add(this.pnlBarkodKutusu);
      this.pnlBarcode.Controls.Add(this.lblBarkod);
      this.pnlBarcode.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlBarcode.Location = new System.Drawing.Point(0, 0);
      this.pnlBarcode.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
      this.pnlBarcode.Name = "pnlBarcode";
      this.pnlBarcode.Size = new System.Drawing.Size(659, 78);
      this.pnlBarcode.TabIndex = 0;
      //
      // pnlBarkodKutusu
      //
      this.pnlBarkodKutusu.BackColor = System.Drawing.Color.White;
      this.pnlBarkodKutusu.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pnlBarkodKutusu.Controls.Add(this.txtBarkod);
      this.pnlBarkodKutusu.Controls.Add(this.picBarkod);
      this.pnlBarkodKutusu.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.pnlBarkodKutusu.Location = new System.Drawing.Point(0, 34);
      this.pnlBarkodKutusu.Name = "pnlBarkodKutusu";
      this.pnlBarkodKutusu.Padding = new System.Windows.Forms.Padding(8, 10, 8, 7);
      this.pnlBarkodKutusu.Size = new System.Drawing.Size(659, 44);
      this.pnlBarkodKutusu.TabIndex = 0;
      //
      // txtBarkod
      //
      this.txtBarkod.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.txtBarkod.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtBarkod.Font = new System.Drawing.Font("Segoe UI", 11F);
      this.txtBarkod.Location = new System.Drawing.Point(42, 10);
      this.txtBarkod.Name = "txtBarkod";
      this.txtBarkod.PlaceholderText = "Barkod okutun veya ürün adı yazın...";
      this.txtBarkod.Size = new System.Drawing.Size(607, 20);
      this.txtBarkod.TabIndex = 0;
      this.txtBarkod.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtBarkod_KeyPress);
      //
      // picBarkod
      //
      this.picBarkod.Dock = System.Windows.Forms.DockStyle.Left;
      this.picBarkod.Location = new System.Drawing.Point(8, 10);
      this.picBarkod.Name = "picBarkod";
      this.picBarkod.Size = new System.Drawing.Size(34, 25);
      this.picBarkod.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      this.picBarkod.TabIndex = 1;
      this.picBarkod.TabStop = false;
      //
      // lblBarkod
      //
      this.lblBarkod.Dock = System.Windows.Forms.DockStyle.Top;
      this.lblBarkod.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.lblBarkod.Location = new System.Drawing.Point(0, 0);
      this.lblBarkod.Name = "lblBarkod";
      this.lblBarkod.Size = new System.Drawing.Size(659, 26);
      this.lblBarkod.TabIndex = 1;
      this.lblBarkod.Text = "Barkod / Ürün Ara";
      //
      // pnlFilters
      //
      this.pnlFilters.Controls.Add(this.pnlDepoKutusu);
      this.pnlFilters.Controls.Add(this.lblDepo);
      this.pnlFilters.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlFilters.Location = new System.Drawing.Point(675, 0);
      this.pnlFilters.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
      this.pnlFilters.Name = "pnlFilters";
      this.pnlFilters.Size = new System.Drawing.Size(364, 78);
      this.pnlFilters.TabIndex = 1;
      //
      // pnlDepoKutusu
      //
      this.pnlDepoKutusu.BackColor = System.Drawing.Color.White;
      this.pnlDepoKutusu.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pnlDepoKutusu.Controls.Add(this.cmbDepo);
      this.pnlDepoKutusu.Controls.Add(this.picDepo);
      this.pnlDepoKutusu.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.pnlDepoKutusu.Location = new System.Drawing.Point(0, 34);
      this.pnlDepoKutusu.Name = "pnlDepoKutusu";
      this.pnlDepoKutusu.Padding = new System.Windows.Forms.Padding(8, 6, 6, 4);
      this.pnlDepoKutusu.Size = new System.Drawing.Size(364, 44);
      this.pnlDepoKutusu.TabIndex = 0;
      //
      // cmbDepo
      //
      this.cmbDepo.Dock = System.Windows.Forms.DockStyle.Fill;
      this.cmbDepo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbDepo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.cmbDepo.Font = new System.Drawing.Font("Segoe UI", 11F);
      this.cmbDepo.Location = new System.Drawing.Point(42, 6);
      this.cmbDepo.Name = "cmbDepo";
      this.cmbDepo.Size = new System.Drawing.Size(314, 28);
      this.cmbDepo.TabIndex = 1;
      this.cmbDepo.SelectedIndexChanged += new System.EventHandler(this.cmbDepo_SelectedIndexChanged);
      //
      // picDepo
      //
      this.picDepo.Dock = System.Windows.Forms.DockStyle.Left;
      this.picDepo.Location = new System.Drawing.Point(8, 6);
      this.picDepo.Name = "picDepo";
      this.picDepo.Size = new System.Drawing.Size(34, 32);
      this.picDepo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      this.picDepo.TabIndex = 2;
      this.picDepo.TabStop = false;
      //
      // lblDepo
      //
      this.lblDepo.Dock = System.Windows.Forms.DockStyle.Top;
      this.lblDepo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.lblDepo.Location = new System.Drawing.Point(0, 0);
      this.lblDepo.Name = "lblDepo";
      this.lblDepo.Size = new System.Drawing.Size(364, 26);
      this.lblDepo.TabIndex = 1;
      this.lblDepo.Text = "Depo Seçimi";
      //
      // pnlQuantity
      //
      this.pnlQuantity.Controls.Add(this.pnlQuantityValue);
      this.pnlQuantity.Controls.Add(this.lblQuantityTitle);
      this.pnlQuantity.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlQuantity.Location = new System.Drawing.Point(1055, 0);
      this.pnlQuantity.Margin = new System.Windows.Forms.Padding(0);
      this.pnlQuantity.Name = "pnlQuantity";
      this.pnlQuantity.Size = new System.Drawing.Size(353, 78);
      this.pnlQuantity.TabIndex = 2;
      //
      // pnlQuantityValue
      //
      this.pnlQuantityValue.Controls.Add(this.picQuantity);
      this.pnlQuantityValue.Controls.Add(this.lblStockAmount);
      this.pnlQuantityValue.Controls.Add(this.lblStockUnit);
      this.pnlQuantityValue.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.pnlQuantityValue.Location = new System.Drawing.Point(0, 34);
      this.pnlQuantityValue.Name = "pnlQuantityValue";
      this.pnlQuantityValue.Size = new System.Drawing.Size(353, 44);
      this.pnlQuantityValue.TabIndex = 0;
      this.pnlQuantityValue.WrapContents = false;
      //
      // picQuantity
      //
      this.picQuantity.Location = new System.Drawing.Point(3, 3);
      this.picQuantity.Name = "picQuantity";
      this.picQuantity.Size = new System.Drawing.Size(36, 38);
      this.picQuantity.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      this.picQuantity.TabIndex = 0;
      this.picQuantity.TabStop = false;
      //
      // lblStockAmount
      //
      this.lblStockAmount.AutoSize = true;
      this.lblStockAmount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(239)))), ((int)(((byte)(248)))));
      this.lblStockAmount.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
      this.lblStockAmount.Location = new System.Drawing.Point(45, 0);
      this.lblStockAmount.MinimumSize = new System.Drawing.Size(92, 40);
      this.lblStockAmount.Name = "lblStockAmount";
      this.lblStockAmount.Size = new System.Drawing.Size(92, 40);
      this.lblStockAmount.TabIndex = 1;
      this.lblStockAmount.Text = "—";
      this.lblStockAmount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      //
      // lblStockUnit
      //
      this.lblStockUnit.AutoSize = true;
      this.lblStockUnit.Location = new System.Drawing.Point(143, 0);
      this.lblStockUnit.MinimumSize = new System.Drawing.Size(0, 40);
      this.lblStockUnit.Name = "lblStockUnit";
      this.lblStockUnit.Size = new System.Drawing.Size(0, 40);
      this.lblStockUnit.TabIndex = 2;
      this.lblStockUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      //
      // lblQuantityTitle
      //
      this.lblQuantityTitle.Dock = System.Windows.Forms.DockStyle.Top;
      this.lblQuantityTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.lblQuantityTitle.Location = new System.Drawing.Point(0, 0);
      this.lblQuantityTitle.Name = "lblQuantityTitle";
      this.lblQuantityTitle.Size = new System.Drawing.Size(353, 26);
      this.lblQuantityTitle.TabIndex = 1;
      this.lblQuantityTitle.Text = "Stok Miktarı";
      //
      // pnlButtons
      //
      this.pnlButtons.ColumnCount = 9;
      this.pnlButtons.Controls.Add(this.btnYeniUrun, 0, 0);
      this.pnlButtons.Controls.Add(this.btnDuzenle, 1, 0);
      this.pnlButtons.Controls.Add(this.btnSil, 2, 0);
      this.pnlButtons.Controls.Add(this.btnYenile, 3, 0);
      this.pnlButtons.Controls.Add(this.btnZRaporu, 4, 0);
      this.pnlButtons.Controls.Add(this.btnKapat, 8, 0);
      this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlButtons.Location = new System.Drawing.Point(16, 160);
      this.pnlButtons.Margin = new System.Windows.Forms.Padding(16, 0, 16, 14);
      this.pnlButtons.Name = "pnlButtons";
      this.pnlButtons.RowCount = 1;
      this.pnlButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.pnlButtons.Size = new System.Drawing.Size(1408, 58);
      this.pnlButtons.TabIndex = 2;
      //
      // btnYeniUrun
      //
      this.btnYeniUrun.Location = new System.Drawing.Point(3, 3);
      this.btnYeniUrun.Name = "btnYeniUrun";
      this.btnYeniUrun.Size = new System.Drawing.Size(75, 23);
      this.btnYeniUrun.TabIndex = 0;
      this.btnYeniUrun.Text = "Yeni Ürün";
      this.btnYeniUrun.Click += new System.EventHandler(this.btnYeniUrun_Click);
      //
      // btnDuzenle
      //
      this.btnDuzenle.Location = new System.Drawing.Point(84, 3);
      this.btnDuzenle.Name = "btnDuzenle";
      this.btnDuzenle.Size = new System.Drawing.Size(75, 23);
      this.btnDuzenle.TabIndex = 1;
      this.btnDuzenle.Text = "Düzenle";
      this.btnDuzenle.Click += new System.EventHandler(this.btnDuzenle_Click);
      //
      // btnSil
      //
      this.btnSil.Location = new System.Drawing.Point(165, 3);
      this.btnSil.Name = "btnSil";
      this.btnSil.Size = new System.Drawing.Size(75, 23);
      this.btnSil.TabIndex = 2;
      this.btnSil.Text = "Sil";
      this.btnSil.Click += new System.EventHandler(this.btnSil_Click);
      //
      // btnYenile
      //
      this.btnYenile.Location = new System.Drawing.Point(246, 3);
      this.btnYenile.Name = "btnYenile";
      this.btnYenile.Size = new System.Drawing.Size(75, 23);
      this.btnYenile.TabIndex = 3;
      this.btnYenile.Text = "Yenile";
      this.btnYenile.Click += new System.EventHandler(this.btnYenile_Click);
      //
      // btnZRaporu
      //
      this.btnZRaporu.Location = new System.Drawing.Point(327, 3);
      this.btnZRaporu.Name = "btnZRaporu";
      this.btnZRaporu.Size = new System.Drawing.Size(75, 23);
      this.btnZRaporu.TabIndex = 4;
      this.btnZRaporu.Text = "Hareketler";
      this.btnZRaporu.Click += new System.EventHandler(this.btnZRaporu_Click);
      //
      // btnKapat
      //
      this.btnKapat.Location = new System.Drawing.Point(408, 3);
      this.btnKapat.Name = "btnKapat";
      this.btnKapat.Size = new System.Drawing.Size(75, 23);
      this.btnKapat.TabIndex = 5;
      this.btnKapat.Text = "Çıkış";
      this.btnKapat.Click += new System.EventHandler(this.btnKapat_Click);
      //
      // pnlWorkspace
      //
      this.pnlWorkspace.ColumnCount = 2;
      this.pnlWorkspace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.pnlWorkspace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 320F));
      this.pnlWorkspace.Controls.Add(this.dgvStoklar, 0, 0);
      this.pnlWorkspace.Controls.Add(this.pnlProductDetail, 1, 0);
      this.pnlWorkspace.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlWorkspace.Location = new System.Drawing.Point(16, 232);
      this.pnlWorkspace.Margin = new System.Windows.Forms.Padding(16, 0, 16, 12);
      this.pnlWorkspace.Name = "pnlWorkspace";
      this.pnlWorkspace.RowCount = 1;
      this.pnlWorkspace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.pnlWorkspace.Size = new System.Drawing.Size(1408, 518);
      this.pnlWorkspace.TabIndex = 3;
      //
      // dgvStoklar
      //
      this.dgvStoklar.AllowUserToAddRows = false;
      this.dgvStoklar.AllowUserToDeleteRows = false;
      this.dgvStoklar.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dgvStoklar.Location = new System.Drawing.Point(0, 0);
      this.dgvStoklar.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
      this.dgvStoklar.MultiSelect = false;
      this.dgvStoklar.Name = "dgvStoklar";
      this.dgvStoklar.ReadOnly = true;
      this.dgvStoklar.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dgvStoklar.Size = new System.Drawing.Size(1076, 518);
      this.dgvStoklar.TabIndex = 0;
      //
      // pnlProductDetail
      //
      this.pnlProductDetail.AutoScroll = true;
      this.pnlProductDetail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pnlProductDetail.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlProductDetail.Location = new System.Drawing.Point(1088, 0);
      this.pnlProductDetail.Margin = new System.Windows.Forms.Padding(0);
      this.pnlProductDetail.Name = "pnlProductDetail";
      this.pnlProductDetail.Size = new System.Drawing.Size(320, 518);
      this.pnlProductDetail.TabIndex = 1;
      //
      // pnlHint
      //
      this.pnlHint.ColumnCount = 2;
      this.pnlHint.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.pnlHint.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.pnlHint.Controls.Add(this.lblHint, 0, 0);
      this.pnlHint.Controls.Add(this.lblRecordCount, 1, 0);
      this.pnlHint.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlHint.Location = new System.Drawing.Point(16, 762);
      this.pnlHint.Margin = new System.Windows.Forms.Padding(16, 0, 16, 10);
      this.pnlHint.Name = "pnlHint";
      this.pnlHint.RowCount = 1;
      this.pnlHint.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.pnlHint.Size = new System.Drawing.Size(1408, 36);
      this.pnlHint.TabIndex = 4;
      //
      // lblHint
      //
      this.lblHint.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lblHint.Location = new System.Drawing.Point(3, 0);
      this.lblHint.Name = "lblHint";
      this.lblHint.Size = new System.Drawing.Size(1396, 36);
      this.lblHint.TabIndex = 0;
      this.lblHint.Text = "Seçili ürün için stok işlemini açın veya ürün satırına çift tıklayın.";
      this.lblHint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      //
      // lblRecordCount
      //
      this.lblRecordCount.AutoSize = true;
      this.lblRecordCount.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lblRecordCount.Location = new System.Drawing.Point(1405, 0);
      this.lblRecordCount.Name = "lblRecordCount";
      this.lblRecordCount.Size = new System.Drawing.Size(1, 36);
      this.lblRecordCount.TabIndex = 1;
      this.lblRecordCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      //
      // statusStrip1
      //
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.toolStripConnection,
            this.toolStripDepot,
            this.toolStripVersion});
      this.statusStrip1.Location = new System.Drawing.Point(0, 808);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new System.Drawing.Size(1440, 22);
      this.statusStrip1.TabIndex = 1;
      //
      // toolStripStatusLabel
      //
      this.toolStripStatusLabel.Name = "toolStripStatusLabel";
      this.toolStripStatusLabel.Size = new System.Drawing.Size(1229, 17);
      this.toolStripStatusLabel.Spring = true;
      this.toolStripStatusLabel.Text = "Hazır";
      this.toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      //
      // toolStripConnection
      //
      this.toolStripConnection.Name = "toolStripConnection";
      this.toolStripConnection.Size = new System.Drawing.Size(108, 17);
      this.toolStripConnection.Text = "Bağlantı bekleniyor";
      //
      // toolStripDepot
      //
      this.toolStripDepot.Name = "toolStripDepot";
      this.toolStripDepot.Size = new System.Drawing.Size(88, 17);
      this.toolStripDepot.Text = "Depo seçilmedi";
      //
      // toolStripVersion
      //
      this.toolStripVersion.Name = "toolStripVersion";
      this.toolStripVersion.Size = new System.Drawing.Size(0, 17);
      //
      // MainForm
      //
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(252)))));
      this.ClientSize = new System.Drawing.Size(1440, 830);
      this.Controls.Add(this.pnlLayout);
      this.Controls.Add(this.statusStrip1);
      this.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(43)))), ((int)(((byte)(61)))));
      this.MinimumSize = new System.Drawing.Size(1180, 740);
      this.Name = "MainForm";
      this.ShowIcon = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Stok Takip Sistemi";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
      this.Load += new System.EventHandler(this.MainForm_Load);
      this.pnlLayout.ResumeLayout(false);
      this.pnlTop.ResumeLayout(false);
      this.pnlTop.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
      this.pnlSearch.ResumeLayout(false);
      this.pnlBarcode.ResumeLayout(false);
      this.pnlBarkodKutusu.ResumeLayout(false);
      this.pnlBarkodKutusu.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.picBarkod)).EndInit();
      this.pnlFilters.ResumeLayout(false);
      this.pnlDepoKutusu.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.picDepo)).EndInit();
      this.pnlQuantity.ResumeLayout(false);
      this.pnlQuantityValue.ResumeLayout(false);
      this.pnlQuantityValue.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.picQuantity)).EndInit();
      this.pnlButtons.ResumeLayout(false);
      this.pnlWorkspace.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dgvStoklar)).EndInit();
      this.pnlHint.ResumeLayout(false);
      this.pnlHint.PerformLayout();
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    private System.Windows.Forms.TableLayoutPanel pnlLayout, pnlTop, pnlSearch, pnlButtons, pnlWorkspace, pnlHint;
    private System.Windows.Forms.Panel pnlBarcode, pnlBarkodKutusu, pnlFilters, pnlDepoKutusu, pnlQuantity, pnlProductDetail;
    private System.Windows.Forms.FlowLayoutPanel pnlHeaderInfo, pnlQuantityValue;
    private System.Windows.Forms.PictureBox picLogo, picBarkod, picDepo, picQuantity;
    private System.Windows.Forms.Label lblTitle, lblBarkod, lblDepo, lblQuantityTitle, lblStockAmount, lblStockUnit, lblHint, lblRecordCount;
    private InventoryTracking.Controls.InventorySearchTextBox txtBarkod;
    private System.Windows.Forms.ComboBox cmbDepo;
    private System.Windows.Forms.Button btnYeniUrun, btnDuzenle, btnSil, btnYenile, btnZRaporu, btnKapat;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel, toolStripConnection, toolStripDepot, toolStripVersion;
    private System.Windows.Forms.DataGridView dgvStoklar;
  }
}
