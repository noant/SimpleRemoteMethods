﻿using SimpleRemoteMethods.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleRemoteMethods.Test.Encryption
{
    public partial class EncryptionTestForm : Form
    {
        public EncryptionTestForm()
        {
            InitializeComponent();
        }
        
        private void btEncrypt_Click(object sender, EventArgs e)
        {
            try
            {
                tbSalt.Text = SecureEncoding.CreateSalt();
                var iv = SecureEncoding.CreateIV(tbSalt.Text, tbSecret.Text);
                var encrypted = 
                    SecureEncoding
                    .GetSecureEncoding(tbSecret.Text)
                    .Encrypt(tbSourceData.Text, iv);

                tbEncryptedData.Text = encrypted;

                var bytes = Convert.FromBase64String(encrypted);

                var bytesStringView = 
                    bytes
                    .Select(x => x.ToString())
                    .Aggregate((b1, b2) => b1 + " " + b2);

                tbEncryptedBytes.Text = bytesStringView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btDecrypt_Click(object sender, EventArgs e)
        {
            try
            {
                var iv = SecureEncoding.CreateIV(tbSalt.Text, tbSecret.Text);
                var decrypted =
                    SecureEncoding
                    .GetSecureEncoding(tbSecret.Text)
                    .Decrypt(tbEncryptedData.Text, iv);

                tbSourceData.Text = decrypted;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().FullName);
            }
        }
    }
}