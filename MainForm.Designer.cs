namespace kingstar2femasfee
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btn_femas_dir = new System.Windows.Forms.Button();
            this.textBox_femas = new System.Windows.Forms.TextBox();
            this.btn_kingstar_dir = new System.Windows.Forms.Button();
            this.textBox_kingstar = new System.Windows.Forms.TextBox();
            this.button_calc = new System.Windows.Forms.Button();
            this.textBox_log = new System.Windows.Forms.RichTextBox();
            this.tabControl1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(4, 49);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1316, 450);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 28);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage1.Size = new System.Drawing.Size(1308, 418);
            this.tabPage1.TabIndex = 1;
            this.tabPage1.Text = "金士达客户手续费率";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 28);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage2.Size = new System.Drawing.Size(1308, 418);
            this.tabPage2.TabIndex = 0;
            this.tabPage2.Text = "飞马交易所手续费率";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 28);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1308, 418);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "飞马客户手续费率";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Location = new System.Drawing.Point(4, 28);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(1308, 418);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "导出飞马客户浮动费率";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 1324F));
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBox_log, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1324, 663);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 6;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel2.Controls.Add(this.btn_femas_dir, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.textBox_femas, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.btn_kingstar_dir, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.textBox_kingstar, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.button_calc, 4, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1318, 39);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // btn_femas_dir
            // 
            this.btn_femas_dir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_femas_dir.Location = new System.Drawing.Point(772, 4);
            this.btn_femas_dir.Margin = new System.Windows.Forms.Padding(4);
            this.btn_femas_dir.Name = "btn_femas_dir";
            this.btn_femas_dir.Size = new System.Drawing.Size(192, 31);
            this.btn_femas_dir.TabIndex = 4;
            this.btn_femas_dir.Text = "选择飞马费率目录";
            this.btn_femas_dir.UseVisualStyleBackColor = true;
            this.btn_femas_dir.Click += new System.EventHandler(this.Btn_femas_dir_Click);
            // 
            // textBox_femas
            // 
            this.textBox_femas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_femas.Location = new System.Drawing.Point(488, 4);
            this.textBox_femas.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_femas.Name = "textBox_femas";
            this.textBox_femas.Size = new System.Drawing.Size(276, 28);
            this.textBox_femas.TabIndex = 2;
            // 
            // btn_kingstar_dir
            // 
            this.btn_kingstar_dir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_kingstar_dir.Location = new System.Drawing.Point(288, 4);
            this.btn_kingstar_dir.Margin = new System.Windows.Forms.Padding(4);
            this.btn_kingstar_dir.Name = "btn_kingstar_dir";
            this.btn_kingstar_dir.Size = new System.Drawing.Size(192, 31);
            this.btn_kingstar_dir.TabIndex = 1;
            this.btn_kingstar_dir.Text = "选择金士达费率目录";
            this.btn_kingstar_dir.UseVisualStyleBackColor = true;
            this.btn_kingstar_dir.Click += new System.EventHandler(this.Btn_kingstar_dir_Click);
            // 
            // textBox_kingstar
            // 
            this.textBox_kingstar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_kingstar.Location = new System.Drawing.Point(4, 4);
            this.textBox_kingstar.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_kingstar.Name = "textBox_kingstar";
            this.textBox_kingstar.Size = new System.Drawing.Size(276, 28);
            this.textBox_kingstar.TabIndex = 0;
            // 
            // button_calc
            // 
            this.button_calc.Dock = System.Windows.Forms.DockStyle.Right;
            this.button_calc.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_calc.Location = new System.Drawing.Point(1105, 3);
            this.button_calc.Name = "button_calc";
            this.button_calc.Size = new System.Drawing.Size(160, 33);
            this.button_calc.TabIndex = 5;
            this.button_calc.Text = "一键生成";
            this.button_calc.UseVisualStyleBackColor = true;
            this.button_calc.Click += new System.EventHandler(this.Button_calc_Click);
            // 
            // textBox_log
            // 
            this.textBox_log.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_log.Location = new System.Drawing.Point(3, 506);
            this.textBox_log.Name = "textBox_log";
            this.textBox_log.ReadOnly = true;
            this.textBox_log.Size = new System.Drawing.Size(1318, 154);
            this.textBox_log.TabIndex = 2;
            this.textBox_log.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1324, 663);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "金士达转飞马浮动费率工具";
            this.tabControl1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox textBox_kingstar;
        private System.Windows.Forms.Button btn_kingstar_dir;
        private System.Windows.Forms.TextBox textBox_femas;
        private System.Windows.Forms.Button btn_femas_dir;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button button_calc;
        private System.Windows.Forms.RichTextBox textBox_log;
    }
}

