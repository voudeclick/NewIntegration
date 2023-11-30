using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Samurai.Integration.APIClient.Millennium;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows.Forms;

namespace Samurai.Integration.MillenniumApiTester
{
    public partial class MillenniumApiTester : Form
    {
        public MillenniumApiTester()
        {
            InitializeComponent();
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            txtProduct.Text = txtOrder.Text = "";
            if (string.IsNullOrWhiteSpace(txtURL.Text))
            {
                MessageBox.Show("URL inválida");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtUser.Text))
            {
                MessageBox.Show("Usuário inválida");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Senha inválida");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtVitrine.Text) || !int.TryParse(txtVitrine.Text, out var vitrine))
            {
                MessageBox.Show("Vitrine inválida");
                return;
            }

            try
            {
                var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
                var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
                var logger = serviceProvider.GetService<ILogger<MillenniumApiClient>>();

                //var client = new MillenniumApiClient(httpClientFactory, null, txtURL.Text, txtUser.Text, txtPassword.Text, null, null);
                //{
                //    string url = "api/millenium_eco/produtos/listavitrine";
                //    var param = new Dictionary<string, string>() { };
                //    param.Add("vitrine", vitrine.ToString());
                //    param.Add("$top", "1");
                //    var response = await client.Get<dynamic>(QueryHelpers.AddQueryString(url, param));
                //    txtProduct.Text = System.Text.Json.JsonSerializer.Serialize(response);
                //}
                //{
                //    string url = "api/millenium_eco/pedido_venda/listapedidos";
                //    var param = new Dictionary<string, string>() { };
                //    param.Add("vitrine", vitrine.ToString());
                //    param.Add("nao_lista_detalhe", "true");
                //    param.Add("$top", "1");
                //    param.Add("trans_id", "0");
                //    var response = await client.Get<dynamic>(QueryHelpers.AddQueryString(url, param));
                //    txtOrder.Text = System.Text.Json.JsonSerializer.Serialize(response);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
    }
}
