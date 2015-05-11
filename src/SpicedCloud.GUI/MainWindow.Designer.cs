namespace SpicedCloud.GUI
{
    partial class MainWindow
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.diskSpace = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pictureBoxState = new System.Windows.Forms.PictureBox();
            this.button2 = new System.Windows.Forms.Button();
            this.buttonFolder = new System.Windows.Forms.Button();
            this.labelState = new System.Windows.Forms.Label();
            this.notifyIconName = new System.Windows.Forms.NotifyIcon(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.diskSpace)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxState)).BeginInit();
            this.SuspendLayout();
            // 
            // diskSpace
            // 
            this.diskSpace.BackColor = System.Drawing.Color.Transparent;
            this.diskSpace.BackSecondaryColor = System.Drawing.Color.White;
            this.diskSpace.BorderlineColor = System.Drawing.Color.LightGray;
            chartArea2.BackColor = System.Drawing.SystemColors.ButtonFace;
            chartArea2.Name = "ChartArea1";
            this.diskSpace.ChartAreas.Add(chartArea2);
            legend2.BackColor = System.Drawing.SystemColors.ButtonFace;
            legend2.Name = "Legend1";
            this.diskSpace.Legends.Add(legend2);
            this.diskSpace.Location = new System.Drawing.Point(6, 19);
            this.diskSpace.Name = "diskSpace";
            this.diskSpace.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            this.diskSpace.PaletteCustomColors = new System.Drawing.Color[] {
        System.Drawing.Color.SkyBlue,
        System.Drawing.Color.PowderBlue};
            series2.BorderColor = System.Drawing.Color.Transparent;
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series2.Color = System.Drawing.Color.Transparent;
            series2.LabelBackColor = System.Drawing.SystemColors.ButtonHighlight;
            series2.Legend = "Legend1";
            series2.MarkerColor = System.Drawing.Color.White;
            series2.Name = "Series1";
            this.diskSpace.Series.Add(series2);
            this.diskSpace.Size = new System.Drawing.Size(326, 157);
            this.diskSpace.TabIndex = 0;
            this.diskSpace.Text = "chart1";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.diskSpace);
            this.groupBox1.Location = new System.Drawing.Point(176, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(338, 180);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Данные пользователя";
            // 
            // pictureBoxState
            // 
            this.pictureBoxState.Image = global::SpicedCloud.GUI.Properties.Resources.OK;
            this.pictureBoxState.Location = new System.Drawing.Point(12, 162);
            this.pictureBoxState.Name = "pictureBoxState";
            this.pictureBoxState.Size = new System.Drawing.Size(35, 30);
            this.pictureBoxState.TabIndex = 5;
            this.pictureBoxState.TabStop = false;
            // 
            // button2
            // 
            this.button2.FlatAppearance.BorderColor = System.Drawing.Color.Gainsboro;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Image = global::SpicedCloud.GUI.Properties.Resources.User;
            this.button2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button2.Location = new System.Drawing.Point(5, 57);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(165, 39);
            this.button2.TabIndex = 4;
            this.button2.Text = "Другой пользователь";
            this.button2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // buttonFolder
            // 
            this.buttonFolder.FlatAppearance.BorderColor = System.Drawing.Color.LightGray;
            this.buttonFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonFolder.Image = global::SpicedCloud.GUI.Properties.Resources.folder;
            this.buttonFolder.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonFolder.Location = new System.Drawing.Point(5, 12);
            this.buttonFolder.Name = "buttonFolder";
            this.buttonFolder.Size = new System.Drawing.Size(165, 39);
            this.buttonFolder.TabIndex = 1;
            this.buttonFolder.Text = "Выбрать папку";
            this.buttonFolder.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonFolder.UseVisualStyleBackColor = true;
            this.buttonFolder.Click += new System.EventHandler(this.chooseFolder);
            // 
            // labelState
            // 
            this.labelState.AutoSize = true;
            this.labelState.Location = new System.Drawing.Point(53, 175);
            this.labelState.Name = "labelState";
            this.labelState.Size = new System.Drawing.Size(63, 13);
            this.labelState.TabIndex = 6;
            this.labelState.Text = "Обновлено";
            // 
            // notifyIconName
            // 
            this.notifyIconName.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconName.Icon")));
            this.notifyIconName.Text = "SpiceCloud";
            this.notifyIconName.Visible = true;
            this.notifyIconName.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(526, 199);
            this.Controls.Add(this.labelState);
            this.Controls.Add(this.pictureBoxState);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonFolder);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWindow";
            this.Text = "SpiceCloud";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainWindow_FormClosed);
            this.Resize += new System.EventHandler(this.MainWindow_Resize_1);
            ((System.ComponentModel.ISupportInitialize)(this.diskSpace)).EndInit();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxState)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonFolder;
        private System.Windows.Forms.DataVisualization.Charting.Chart diskSpace;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.PictureBox pictureBoxState;
        private System.Windows.Forms.Label labelState;
        private System.Windows.Forms.NotifyIcon notifyIconName;
    }
}

