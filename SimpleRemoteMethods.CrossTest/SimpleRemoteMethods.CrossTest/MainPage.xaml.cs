using SimpleRemoteMethods.ClientSide;
using SimpleRemoteMethods.Test.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SimpleRemoteMethods.CrossTest
{
    public partial class MainPage : ContentPage
    {
        private ClientTest _client = CreateClientSsl();

        public MainPage()
        {
            InitializeComponent();
        }

        private static ClientTest CreateClientSsl(string pass = "123123", string secretCode = "1234123412341234")
        {
            var client = new Client("192.168.1.200", 4041, true, secretCode, "usr", pass);
            var testClient = new ClientTest() { Client = client };
            return testClient;
        }

        private static ClientTest CreateClient(string pass = "123123", string secretCode = "1234123412341234")
        {
            var client = new Client("192.168.1.200", 8082, false, secretCode, "usr", pass);
            var testClient = new ClientTest() { Client = client };
            return testClient;
        }

        private int i = 0;
        private async void Button_Clicked(object sender, EventArgs e)
        {
            try
            {
                i += await _client.TestMethod5(1);
                testLabel.Text = i.ToString();
            }
            catch (Exception ex)
            {
                testLabel.Text = ex.Message;
            }
        }
    }
}
