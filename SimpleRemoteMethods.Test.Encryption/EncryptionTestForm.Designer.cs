namespace SimpleRemoteMethods.Test.Encryption
{
    partial class EncryptionTestForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
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
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbSecret = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbSourceData = new System.Windows.Forms.TextBox();
            this.tbEncryptedData = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btEncrypt = new System.Windows.Forms.Button();
            this.btDecrypt = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.tbSalt = new System.Windows.Forms.TextBox();
            this.tbEncryptedBytes = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbSecret
            // 
            this.tbSecret.Location = new System.Drawing.Point(97, 14);
            this.tbSecret.MaxLength = 16;
            this.tbSecret.Name = "tbSecret";
            this.tbSecret.Size = new System.Drawing.Size(756, 22);
            this.tbSecret.TabIndex = 1;
            this.tbSecret.Text = "0123456789123456";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Secret key:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Source data:";
            // 
            // tbSourceData
            // 
            this.tbSourceData.Location = new System.Drawing.Point(12, 100);
            this.tbSourceData.MaxLength = 5000;
            this.tbSourceData.Multiline = true;
            this.tbSourceData.Name = "tbSourceData";
            this.tbSourceData.Size = new System.Drawing.Size(364, 297);
            this.tbSourceData.TabIndex = 4;
            // 
            // tbEncryptedData
            // 
            this.tbEncryptedData.Location = new System.Drawing.Point(489, 100);
            this.tbEncryptedData.MaxLength = 5000;
            this.tbEncryptedData.Multiline = true;
            this.tbEncryptedData.Name = "tbEncryptedData";
            this.tbEncryptedData.Size = new System.Drawing.Size(364, 297);
            this.tbEncryptedData.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(489, 80);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(108, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "Encrypted data:";
            // 
            // btEncrypt
            // 
            this.btEncrypt.Location = new System.Drawing.Point(388, 100);
            this.btEncrypt.Name = "btEncrypt";
            this.btEncrypt.Size = new System.Drawing.Size(89, 35);
            this.btEncrypt.TabIndex = 7;
            this.btEncrypt.Text = "Encrypt >>";
            this.btEncrypt.UseVisualStyleBackColor = true;
            this.btEncrypt.Click += new System.EventHandler(this.btEncrypt_Click);
            // 
            // btDecrypt
            // 
            this.btDecrypt.Location = new System.Drawing.Point(388, 141);
            this.btDecrypt.Name = "btDecrypt";
            this.btDecrypt.Size = new System.Drawing.Size(89, 35);
            this.btDecrypt.TabIndex = 8;
            this.btDecrypt.Text = "<< Decrypt";
            this.btDecrypt.UseVisualStyleBackColor = true;
            this.btDecrypt.Click += new System.EventHandler(this.btDecrypt_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 43);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(36, 17);
            this.label4.TabIndex = 10;
            this.label4.Text = "Salt:";
            // 
            // tbSalt
            // 
            this.tbSalt.Location = new System.Drawing.Point(97, 42);
            this.tbSalt.MaxLength = 16;
            this.tbSalt.Name = "tbSalt";
            this.tbSalt.Size = new System.Drawing.Size(756, 22);
            this.tbSalt.TabIndex = 9;
            // 
            // tbEncryptedBytes
            // 
            this.tbEncryptedBytes.Location = new System.Drawing.Point(12, 437);
            this.tbEncryptedBytes.Multiline = true;
            this.tbEncryptedBytes.Name = "tbEncryptedBytes";
            this.tbEncryptedBytes.ReadOnly = true;
            this.tbEncryptedBytes.Size = new System.Drawing.Size(841, 253);
            this.tbEncryptedBytes.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 417);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(114, 17);
            this.label5.TabIndex = 12;
            this.label5.Text = "Encrypted bytes:";
            // 
            // EncryptionTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(865, 702);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbEncryptedBytes);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbSalt);
            this.Controls.Add(this.btDecrypt);
            this.Controls.Add(this.btEncrypt);
            this.Controls.Add(this.tbEncryptedData);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbSourceData);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbSecret);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "EncryptionTestForm";
            this.Text = "EncryptionTest";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbSecret;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbSourceData;
        private System.Windows.Forms.TextBox tbEncryptedData;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btEncrypt;
        private System.Windows.Forms.Button btDecrypt;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbSalt;
        private System.Windows.Forms.TextBox tbEncryptedBytes;
        private System.Windows.Forms.Label label5;
    }
}

