using SimpleRemoteMethods.ClientSide;
using SimpleRemoteMethods.Test.Bases;
using System;
using Xamarin.Forms;

namespace SimpleRemoteMethods.CrossTest
{
    public partial class MainPage : ContentPage
    {
        private ClientTest _client = CreateClient();
        private int _counter = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private static ClientTest CreateClient(string pass = "123123", string secretCode = "0123456789123457", bool ssl = false)
        {
            var client = new Client("192.168.1.200", ssl ? (ushort)4041 : (ushort)8082, ssl, secretCode, "usr", pass);
            var testClient = new ClientTest() { Client = client };
            return testClient;
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            try
            {
                _counter += await _client.TestMethod5(1);
                testLabel.Text = _counter.ToString();
            }
            catch (Exception ex)
            {
                testLabel.Text = ex.Message;
            }
        }
    }
}