using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AI_Buddy.Forms
{
    public partial class VisualPrompt : Form
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public VisualPrompt()
        {
            InitializeComponent();
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            string userPrompt = txtPrompt.Text;
            if (string.IsNullOrWhiteSpace(userPrompt))
            {
                MessageBox.Show("Please enter a prompt!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string responseText = await GetOllamaResponseAsync(userPrompt);
            txtResponse.Text = responseText;
        }

        private async Task<string> GetOllamaResponseAsync(string prompt)
        {
            //string apiUrl = "http://localhost:11434/api/generate"; // Ollama local server

            //var requestBody = new
            //{
            //    model = "deepseek-r1:1.5b", // Ensure correct model name
            //    prompt = prompt,
            //    stream = false // Disable streaming for simplicity
            //};

            //try
            //{
            //    var response = await _httpClient.PostAsync(apiUrl,
            //        new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

            //    if (response.IsSuccessStatusCode)
            //    {
            //        var jsonResponse = await response.Content.ReadAsStringAsync();
            //        var result = JsonSerializer.Deserialize<OllamaResponse>(jsonResponse);

            //        return result?.response ?? "No response from API.";
            //    }
            //    else
            //    {
            //        return $"Error: {response.StatusCode}";
            //    }
            //}
            //catch (Exception ex)
            //{
            //    return $"Exception: {ex.Message}";
            //}
            string ollamaUrl = "http://localhost:11434/api/generate";
            string model = "deepseek-r1:1.5b"; // Ensure the model is running locally

            using (HttpClient client = new HttpClient())
            {
                var requestBody = new
                {
                    model = model,
                    prompt = prompt,
                    stream = false // Set to `true` if you want streamed responses
                };

                string jsonContent = JsonConvert.SerializeObject(requestBody);
                HttpContent httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(ollamaUrl, httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    return $"Error: {response.StatusCode}";
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonConvert.DeserializeObject<OllamaResponse>(responseContent);
                return jsonResponse.response;
            }
        }





        public class OllamaResponse
        {
            public string response { get; set; }
        }
    }
}
