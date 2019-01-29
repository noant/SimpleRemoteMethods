using System;
using System.Data;
using System.Linq;
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
                var saltBytes = SecureEncoding.CreateSalt();
                tbSalt.Text = Convert.ToBase64String(saltBytes);
                var iv = SecureEncoding.CreateIV(saltBytes, tbSecret.Text);
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
                var saltBytes = Convert.FromBase64String(tbSalt.Text);
                var iv = SecureEncoding.CreateIV(saltBytes, tbSecret.Text);
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
