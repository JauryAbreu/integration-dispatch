namespace integration_dispatch
{
    partial class DataTransferApp
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnRun = new Button();
            lblStatus = new Label();
            dtpStartDate = new DateTimePicker();
            lblcounter = new Label();
            SuspendLayout();
            // 
            // btnRun
            // 
            btnRun.BackColor = Color.MediumSeaGreen;
            btnRun.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            btnRun.ForeColor = Color.White;
            btnRun.Location = new Point(224, 12);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(171, 46);
            btnRun.TabIndex = 0;
            btnRun.Text = "Enviar";
            btnRun.UseVisualStyleBackColor = false;
            btnRun.Click += btnRun_Click;
            // 
            // lblStatus
            // 
            lblStatus.BackColor = SystemColors.ButtonHighlight;
            lblStatus.Font = new Font("Segoe UI Semibold", 15.75F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            lblStatus.Location = new Point(12, 116);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(383, 106);
            lblStatus.TabIndex = 1;
            lblStatus.Text = "Pendiente de Envio.";
            // 
            // dtpStartDate
            // 
            dtpStartDate.CustomFormat = "dd/MM/yyyy";
            dtpStartDate.Font = new Font("Segoe UI", 21.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dtpStartDate.Format = DateTimePickerFormat.Custom;
            dtpStartDate.Location = new Point(12, 12);
            dtpStartDate.MaxDate = new DateTime(2200, 12, 31, 0, 0, 0, 0);
            dtpStartDate.MinDate = new DateTime(2020, 1, 1, 0, 0, 0, 0);
            dtpStartDate.Name = "dtpStartDate";
            dtpStartDate.Size = new Size(206, 46);
            dtpStartDate.TabIndex = 2;
            dtpStartDate.Value = new DateTime(2025, 5, 6, 0, 0, 0, 0);
            // 
            // lblcounter
            // 
            lblcounter.BackColor = SystemColors.ButtonHighlight;
            lblcounter.Font = new Font("Segoe UI Semibold", 15.75F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            lblcounter.Location = new Point(12, 70);
            lblcounter.Name = "lblcounter";
            lblcounter.Size = new Size(383, 37);
            lblcounter.TabIndex = 3;
            lblcounter.Text = "0/0";
            lblcounter.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // DataTransferApp
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(402, 236);
            Controls.Add(lblcounter);
            Controls.Add(btnRun);
            Controls.Add(lblStatus);
            Controls.Add(dtpStartDate);
            Name = "DataTransferApp";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Data Transfer App";
            ResumeLayout(false);
        }

        #endregion

        private Label lblcounter;
    }
}
