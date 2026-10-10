namespace InventoryTracking.UI.Forms
{
  partial class ProductDetailForm
  {

    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
        components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.pnlStatus = new System.Windows.Forms.Panel();
      this.lblTitle = new System.Windows.Forms.Label();
      this.pnlDetails = new System.Windows.Forms.Panel();
      this.lblBarkod = new System.Windows.Forms.Label();
      this.lblBarkodValue = new System.Windows.Forms.Label();
      this.lblUrunAdi = new System.Windows.Forms.Label();
      this.lblUrunAdiValue = new System.Windows.Forms.Label();
      this.lblKategori = new System.Windows.Forms.Label();
      this.lblKategoriValue = new System.Windows.Forms.Label();
      this.lblBirim = new System.Windows.Forms.Label();
      this.lblBirimValue = new System.Windows.Forms.Label();
      this.lblFiyat = new System.Windows.Forms.Label();
      this.lblFiyatValue = new System.Windows.Forms.Label();
      this.lblDepo = new System.Windows.Forms.Label();
      this.lblDepoValue = new System.Windows.Forms.Label();
      this.pnlStockInfo = new System.Windows.Forms.Panel();
      this.lblMiktar = new System.Windows.Forms.Label();
      this.lblMiktarValue = new System.Windows.Forms.Label();
      this.lblMinStok = new System.Windows.Forms.Label();
      this.lblMinStokValue = new System.Windows.Forms.Label();
      this.lblMaksStok = new System.Windows.Forms.Label();
      this.lblMaksStokValue = new System.Windows.Forms.Label();
      this.lblDurum = new System.Windows.Forms.Label();
      this.lblDurumValue = new System.Windows.Forms.Label();
      this.pnlOperation = new System.Windows.Forms.Panel();
      this.lblOperationTitle = new System.Windows.Forms.Label();
      this.lblMiktarInput = new System.Windows.Forms.Label();
      this.numMiktar = new System.Windows.Forms.NumericUpDown();
      this.lblAciklama = new System.Windows.Forms.Label();
      this.txtAciklama = new System.Windows.Forms.TextBox();
      this.pnlButtons = new System.Windows.Forms.Panel();
      this.btnGiris = new System.Windows.Forms.Button();
      this.btnCikis = new System.Windows.Forms.Button();
      this.btnKapat = new System.Windows.Forms.Button();
      this.pnlStatus.SuspendLayout();
      this.pnlDetails.SuspendLayout();
      this.pnlStockInfo.SuspendLayout();
      this.pnlOperation.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numMiktar)).BeginInit();
      this.pnlButtons.SuspendLayout();
      this.SuspendLayout();
      // 
      // pnlStatus
      // 
      this.pnlStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(255)))), ((int)(((byte)(200)))));
      this.pnlStatus.Controls.Add(this.lblTitle);
      this.pnlStatus.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlStatus.Location = new System.Drawing.Point(0, 0);
      this.pnlStatus.Name = "pnlStatus";
      this.pnlStatus.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
      this.pnlStatus.Size = new System.Drawing.Size(459, 50);
      this.pnlStatus.TabIndex = 4;
      // 
      // lblTitle
      // 
      this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
      this.lblTitle.ForeColor = System.Drawing.Color.DarkGreen;
      this.lblTitle.Location = new System.Drawing.Point(15, 0);
      this.lblTitle.Name = "lblTitle";
      this.lblTitle.Size = new System.Drawing.Size(444, 50);
      this.lblTitle.TabIndex = 0;
      this.lblTitle.Text = "ÜRÜN DETAYI";
      this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // pnlDetails
      // 
      this.pnlDetails.BackColor = System.Drawing.Color.White;
      this.pnlDetails.Controls.Add(this.lblBarkod);
      this.pnlDetails.Controls.Add(this.lblBarkodValue);
      this.pnlDetails.Controls.Add(this.lblUrunAdi);
      this.pnlDetails.Controls.Add(this.lblUrunAdiValue);
      this.pnlDetails.Controls.Add(this.lblKategori);
      this.pnlDetails.Controls.Add(this.lblKategoriValue);
      this.pnlDetails.Controls.Add(this.lblBirim);
      this.pnlDetails.Controls.Add(this.lblBirimValue);
      this.pnlDetails.Controls.Add(this.lblFiyat);
      this.pnlDetails.Controls.Add(this.lblFiyatValue);
      this.pnlDetails.Controls.Add(this.lblDepo);
      this.pnlDetails.Controls.Add(this.lblDepoValue);
      this.pnlDetails.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlDetails.Location = new System.Drawing.Point(0, 50);
      this.pnlDetails.Name = "pnlDetails";
      this.pnlDetails.Size = new System.Drawing.Size(459, 185);
      this.pnlDetails.TabIndex = 3;
      // 
      // lblBarkod
      // 
      this.lblBarkod.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.lblBarkod.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblBarkod.Location = new System.Drawing.Point(20, 15);
      this.lblBarkod.Name = "lblBarkod";
      this.lblBarkod.Size = new System.Drawing.Size(170, 23);
      this.lblBarkod.TabIndex = 0;
      this.lblBarkod.Text = "Barkod:";
      this.lblBarkod.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblBarkodValue
      // 
      this.lblBarkodValue.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.lblBarkodValue.ForeColor = System.Drawing.Color.Black;
      this.lblBarkodValue.Location = new System.Drawing.Point(200, 15);
      this.lblBarkodValue.Name = "lblBarkodValue";
      this.lblBarkodValue.Size = new System.Drawing.Size(280, 23);
      this.lblBarkodValue.TabIndex = 1;
      this.lblBarkodValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblUrunAdi
      // 
      this.lblUrunAdi.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.lblUrunAdi.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblUrunAdi.Location = new System.Drawing.Point(20, 43);
      this.lblUrunAdi.Name = "lblUrunAdi";
      this.lblUrunAdi.Size = new System.Drawing.Size(170, 23);
      this.lblUrunAdi.TabIndex = 2;
      this.lblUrunAdi.Text = "Ürün Adı:";
      this.lblUrunAdi.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblUrunAdiValue
      // 
      this.lblUrunAdiValue.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.lblUrunAdiValue.ForeColor = System.Drawing.Color.Black;
      this.lblUrunAdiValue.Location = new System.Drawing.Point(200, 43);
      this.lblUrunAdiValue.Name = "lblUrunAdiValue";
      this.lblUrunAdiValue.Size = new System.Drawing.Size(280, 23);
      this.lblUrunAdiValue.TabIndex = 3;
      this.lblUrunAdiValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblKategori
      // 
      this.lblKategori.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.lblKategori.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblKategori.Location = new System.Drawing.Point(20, 71);
      this.lblKategori.Name = "lblKategori";
      this.lblKategori.Size = new System.Drawing.Size(170, 23);
      this.lblKategori.TabIndex = 4;
      this.lblKategori.Text = "Kategori:";
      this.lblKategori.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblKategoriValue
      // 
      this.lblKategoriValue.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.lblKategoriValue.ForeColor = System.Drawing.Color.Black;
      this.lblKategoriValue.Location = new System.Drawing.Point(200, 71);
      this.lblKategoriValue.Name = "lblKategoriValue";
      this.lblKategoriValue.Size = new System.Drawing.Size(280, 23);
      this.lblKategoriValue.TabIndex = 5;
      this.lblKategoriValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblBirim
      // 
      this.lblBirim.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.lblBirim.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblBirim.Location = new System.Drawing.Point(20, 99);
      this.lblBirim.Name = "lblBirim";
      this.lblBirim.Size = new System.Drawing.Size(170, 23);
      this.lblBirim.TabIndex = 6;
      this.lblBirim.Text = "Birim:";
      this.lblBirim.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblBirimValue
      // 
      this.lblBirimValue.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.lblBirimValue.ForeColor = System.Drawing.Color.Black;
      this.lblBirimValue.Location = new System.Drawing.Point(200, 99);
      this.lblBirimValue.Name = "lblBirimValue";
      this.lblBirimValue.Size = new System.Drawing.Size(280, 23);
      this.lblBirimValue.TabIndex = 7;
      this.lblBirimValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblFiyat
      // 
      this.lblFiyat.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.lblFiyat.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblFiyat.Location = new System.Drawing.Point(20, 127);
      this.lblFiyat.Name = "lblFiyat";
      this.lblFiyat.Size = new System.Drawing.Size(170, 23);
      this.lblFiyat.TabIndex = 8;
      this.lblFiyat.Text = "Fiyat:";
      this.lblFiyat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblFiyatValue
      // 
      this.lblFiyatValue.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.lblFiyatValue.ForeColor = System.Drawing.Color.Black;
      this.lblFiyatValue.Location = new System.Drawing.Point(200, 127);
      this.lblFiyatValue.Name = "lblFiyatValue";
      this.lblFiyatValue.Size = new System.Drawing.Size(280, 23);
      this.lblFiyatValue.TabIndex = 9;
      this.lblFiyatValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblDepo
      // 
      this.lblDepo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.lblDepo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblDepo.Location = new System.Drawing.Point(20, 155);
      this.lblDepo.Name = "lblDepo";
      this.lblDepo.Size = new System.Drawing.Size(170, 23);
      this.lblDepo.TabIndex = 10;
      this.lblDepo.Text = "Depo:";
      this.lblDepo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblDepoValue
      // 
      this.lblDepoValue.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.lblDepoValue.ForeColor = System.Drawing.Color.Black;
      this.lblDepoValue.Location = new System.Drawing.Point(200, 155);
      this.lblDepoValue.Name = "lblDepoValue";
      this.lblDepoValue.Size = new System.Drawing.Size(280, 23);
      this.lblDepoValue.TabIndex = 11;
      this.lblDepoValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // pnlStockInfo
      // 
      this.pnlStockInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
      this.pnlStockInfo.Controls.Add(this.lblMiktar);
      this.pnlStockInfo.Controls.Add(this.lblMiktarValue);
      this.pnlStockInfo.Controls.Add(this.lblMinStok);
      this.pnlStockInfo.Controls.Add(this.lblMinStokValue);
      this.pnlStockInfo.Controls.Add(this.lblMaksStok);
      this.pnlStockInfo.Controls.Add(this.lblMaksStokValue);
      this.pnlStockInfo.Controls.Add(this.lblDurum);
      this.pnlStockInfo.Controls.Add(this.lblDurumValue);
      this.pnlStockInfo.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlStockInfo.Location = new System.Drawing.Point(0, 235);
      this.pnlStockInfo.Name = "pnlStockInfo";
      this.pnlStockInfo.Size = new System.Drawing.Size(459, 125);
      this.pnlStockInfo.TabIndex = 2;
      // 
      // lblMiktar
      // 
      this.lblMiktar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.lblMiktar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblMiktar.Location = new System.Drawing.Point(20, 12);
      this.lblMiktar.Name = "lblMiktar";
      this.lblMiktar.Size = new System.Drawing.Size(170, 23);
      this.lblMiktar.TabIndex = 0;
      this.lblMiktar.Text = "Mevcut Stok:";
      this.lblMiktar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblMiktarValue
      // 
      this.lblMiktarValue.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
      this.lblMiktarValue.ForeColor = System.Drawing.Color.Black;
      this.lblMiktarValue.Location = new System.Drawing.Point(200, 12);
      this.lblMiktarValue.Name = "lblMiktarValue";
      this.lblMiktarValue.Size = new System.Drawing.Size(280, 23);
      this.lblMiktarValue.TabIndex = 1;
      this.lblMiktarValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblMinStok
      // 
      this.lblMinStok.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.lblMinStok.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblMinStok.Location = new System.Drawing.Point(20, 40);
      this.lblMinStok.Name = "lblMinStok";
      this.lblMinStok.Size = new System.Drawing.Size(170, 23);
      this.lblMinStok.TabIndex = 2;
      this.lblMinStok.Text = "Min. Stok:";
      this.lblMinStok.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblMinStokValue
      // 
      this.lblMinStokValue.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.lblMinStokValue.ForeColor = System.Drawing.Color.Black;
      this.lblMinStokValue.Location = new System.Drawing.Point(200, 40);
      this.lblMinStokValue.Name = "lblMinStokValue";
      this.lblMinStokValue.Size = new System.Drawing.Size(280, 23);
      this.lblMinStokValue.TabIndex = 3;
      this.lblMinStokValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblMaksStok
      // 
      this.lblMaksStok.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.lblMaksStok.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblMaksStok.Location = new System.Drawing.Point(20, 68);
      this.lblMaksStok.Name = "lblMaksStok";
      this.lblMaksStok.Size = new System.Drawing.Size(170, 23);
      this.lblMaksStok.TabIndex = 4;
      this.lblMaksStok.Text = "Maks. Stok:";
      this.lblMaksStok.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblMaksStokValue
      // 
      this.lblMaksStokValue.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.lblMaksStokValue.ForeColor = System.Drawing.Color.Black;
      this.lblMaksStokValue.Location = new System.Drawing.Point(200, 68);
      this.lblMaksStokValue.Name = "lblMaksStokValue";
      this.lblMaksStokValue.Size = new System.Drawing.Size(280, 23);
      this.lblMaksStokValue.TabIndex = 5;
      this.lblMaksStokValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblDurum
      // 
      this.lblDurum.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
      this.lblDurum.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblDurum.Location = new System.Drawing.Point(20, 96);
      this.lblDurum.Name = "lblDurum";
      this.lblDurum.Size = new System.Drawing.Size(170, 23);
      this.lblDurum.TabIndex = 6;
      this.lblDurum.Text = "Stok Durumu:";
      this.lblDurum.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblDurumValue
      // 
      this.lblDurumValue.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
      this.lblDurumValue.ForeColor = System.Drawing.Color.DarkGreen;
      this.lblDurumValue.Location = new System.Drawing.Point(200, 96);
      this.lblDurumValue.Name = "lblDurumValue";
      this.lblDurumValue.Size = new System.Drawing.Size(280, 23);
      this.lblDurumValue.TabIndex = 7;
      this.lblDurumValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // pnlOperation
      // 
      this.pnlOperation.BackColor = System.Drawing.Color.White;
      this.pnlOperation.Controls.Add(this.lblOperationTitle);
      this.pnlOperation.Controls.Add(this.lblMiktarInput);
      this.pnlOperation.Controls.Add(this.numMiktar);
      this.pnlOperation.Controls.Add(this.lblAciklama);
      this.pnlOperation.Controls.Add(this.txtAciklama);
      this.pnlOperation.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlOperation.Location = new System.Drawing.Point(0, 360);
      this.pnlOperation.Name = "pnlOperation";
      this.pnlOperation.Size = new System.Drawing.Size(459, 145);
      this.pnlOperation.TabIndex = 1;
      // 
      // lblOperationTitle
      // 
      this.lblOperationTitle.AutoSize = true;
      this.lblOperationTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
      this.lblOperationTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblOperationTitle.Location = new System.Drawing.Point(20, 10);
      this.lblOperationTitle.Name = "lblOperationTitle";
      this.lblOperationTitle.Size = new System.Drawing.Size(90, 20);
      this.lblOperationTitle.TabIndex = 0;
      this.lblOperationTitle.Text = "Stok İşlemi:";
      // 
      // lblMiktarInput
      // 
      this.lblMiktarInput.AutoSize = true;
      this.lblMiktarInput.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.lblMiktarInput.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblMiktarInput.Location = new System.Drawing.Point(20, 40);
      this.lblMiktarInput.Name = "lblMiktarInput";
      this.lblMiktarInput.Size = new System.Drawing.Size(52, 19);
      this.lblMiktarInput.TabIndex = 1;
      this.lblMiktarInput.Text = "Miktar:";
      // 
      // numMiktar
      // 
      this.numMiktar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.numMiktar.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.numMiktar.Location = new System.Drawing.Point(110, 38);
      this.numMiktar.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
      this.numMiktar.Name = "numMiktar";
      this.numMiktar.Size = new System.Drawing.Size(150, 25);
      this.numMiktar.TabIndex = 0;
      // 
      // lblAciklama
      // 
      this.lblAciklama.AutoSize = true;
      this.lblAciklama.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.lblAciklama.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
      this.lblAciklama.Location = new System.Drawing.Point(20, 72);
      this.lblAciklama.Name = "lblAciklama";
      this.lblAciklama.Size = new System.Drawing.Size(66, 19);
      this.lblAciklama.TabIndex = 2;
      this.lblAciklama.Text = "Açıklama:";
      // 
      // txtAciklama
      // 
      this.txtAciklama.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.txtAciklama.Font = new System.Drawing.Font("Segoe UI", 10F);
      this.txtAciklama.Location = new System.Drawing.Point(110, 70);
      this.txtAciklama.Multiline = true;
      this.txtAciklama.Name = "txtAciklama";
      this.txtAciklama.Size = new System.Drawing.Size(338, 65);
      this.txtAciklama.TabIndex = 1;
      // 
      // pnlButtons
      // 
      this.pnlButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
      this.pnlButtons.Controls.Add(this.btnGiris);
      this.pnlButtons.Controls.Add(this.btnCikis);
      this.pnlButtons.Controls.Add(this.btnKapat);
      this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlButtons.Location = new System.Drawing.Point(0, 505);
      this.pnlButtons.Name = "pnlButtons";
      this.pnlButtons.Padding = new System.Windows.Forms.Padding(15);
      this.pnlButtons.Size = new System.Drawing.Size(459, 61);
      this.pnlButtons.TabIndex = 0;
      // 
      // btnGiris
      // 
      this.btnGiris.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.btnGiris.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
      this.btnGiris.FlatAppearance.BorderSize = 0;
      this.btnGiris.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnGiris.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
      this.btnGiris.ForeColor = System.Drawing.Color.White;
      this.btnGiris.Location = new System.Drawing.Point(12, 14);
      this.btnGiris.Name = "btnGiris";
      this.btnGiris.Size = new System.Drawing.Size(140, 40);
      this.btnGiris.TabIndex = 0;
      this.btnGiris.Text = "↑ Giriş Yap";
      this.btnGiris.UseVisualStyleBackColor = false;
      this.btnGiris.Click += new System.EventHandler(this.btnGiris_Click);
      // 
      // btnCikis
      // 
      this.btnCikis.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
      this.btnCikis.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
      this.btnCikis.FlatAppearance.BorderSize = 0;
      this.btnCikis.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnCikis.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
      this.btnCikis.ForeColor = System.Drawing.Color.White;
      this.btnCikis.Location = new System.Drawing.Point(160, 14);
      this.btnCikis.Name = "btnCikis";
      this.btnCikis.Size = new System.Drawing.Size(140, 40);
      this.btnCikis.TabIndex = 1;
      this.btnCikis.Text = "↓ Çıkış Yap";
      this.btnCikis.UseVisualStyleBackColor = false;
      this.btnCikis.Click += new System.EventHandler(this.btnCikis_Click);
      // 
      // btnKapat
      // 
      this.btnKapat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnKapat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(149)))), ((int)(((byte)(165)))), ((int)(((byte)(166)))));
      this.btnKapat.FlatAppearance.BorderSize = 0;
      this.btnKapat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnKapat.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
      this.btnKapat.ForeColor = System.Drawing.Color.White;
      this.btnKapat.Location = new System.Drawing.Point(308, 14);
      this.btnKapat.Name = "btnKapat";
      this.btnKapat.Size = new System.Drawing.Size(140, 40);
      this.btnKapat.TabIndex = 2;
      this.btnKapat.Text = "✕ Kapat";
      this.btnKapat.UseVisualStyleBackColor = false;
      this.btnKapat.Click += new System.EventHandler(this.btnKapat_Click);
      // 
      // ProductDetailForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(459, 566);
      this.ControlBox = false;
      this.Controls.Add(this.pnlButtons);
      this.Controls.Add(this.pnlOperation);
      this.Controls.Add(this.pnlStockInfo);
      this.Controls.Add(this.pnlDetails);
      this.Controls.Add(this.pnlStatus);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ProductDetailForm";
      this.ShowIcon = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      //this.Text = "Ürün Detayı";
      this.Load += new System.EventHandler(this.ProductDetailForm_Load);
      this.pnlStatus.ResumeLayout(false);
      this.pnlDetails.ResumeLayout(false);
      this.pnlStockInfo.ResumeLayout(false);
      this.pnlOperation.ResumeLayout(false);
      this.pnlOperation.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numMiktar)).EndInit();
      this.pnlButtons.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    private System.Windows.Forms.Panel pnlStatus;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Panel pnlDetails;
    private System.Windows.Forms.Label lblBarkod;
    private System.Windows.Forms.Label lblBarkodValue;
    private System.Windows.Forms.Label lblUrunAdi;
    private System.Windows.Forms.Label lblUrunAdiValue;
    private System.Windows.Forms.Label lblKategori;
    private System.Windows.Forms.Label lblKategoriValue;
    private System.Windows.Forms.Label lblBirim;
    private System.Windows.Forms.Label lblBirimValue;
    private System.Windows.Forms.Label lblFiyat;
    private System.Windows.Forms.Label lblFiyatValue;
    private System.Windows.Forms.Label lblDepo;
    private System.Windows.Forms.Label lblDepoValue;
    private System.Windows.Forms.Panel pnlStockInfo;
    private System.Windows.Forms.Label lblMiktar;
    private System.Windows.Forms.Label lblMiktarValue;
    private System.Windows.Forms.Label lblMinStok;
    private System.Windows.Forms.Label lblMinStokValue;
    private System.Windows.Forms.Label lblMaksStok;
    private System.Windows.Forms.Label lblMaksStokValue;
    private System.Windows.Forms.Label lblDurum;
    private System.Windows.Forms.Label lblDurumValue;
    private System.Windows.Forms.Panel pnlOperation;
    private System.Windows.Forms.Label lblOperationTitle;
    private System.Windows.Forms.Label lblMiktarInput;
    private System.Windows.Forms.NumericUpDown numMiktar;
    private System.Windows.Forms.Label lblAciklama;
    private System.Windows.Forms.TextBox txtAciklama;
    private System.Windows.Forms.Panel pnlButtons;
    private System.Windows.Forms.Button btnGiris;
    private System.Windows.Forms.Button btnCikis;
    private System.Windows.Forms.Button btnKapat;
  }
}