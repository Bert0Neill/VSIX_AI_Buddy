using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TestOllama
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            string prompt = "Explain quantum mechanics in simple terms.";
            string response = await CallOllamaDeepSeek(prompt);
            Console.WriteLine("Ollama Response: \n" + response);


            //using var ollama = new OllamaApiClient();

            //var models = await ollama.Models.ListModelsAsync();

            //// Pulling a model and reporting progress
            //await foreach (var response in ollama.PullModelAsync("all-minilm", stream: true))
            //{
            //    Console.WriteLine($"{response.Status}. Progress: {response.Completed}/{response.Total}");
            //}
            //// or just pull the model and wait for it to finish
            //await ollama.Models.PullModelAsync("all-minilm").EnsureSuccessAsync();

            //// Generating an embedding
            //var embedding = await ollama.Embeddings.GenerateEmbeddingAsync(
            //    model: "all-minilm",
            //    prompt: "hello");

            //// Streaming a completion directly into the console
            //// keep reusing the context to keep the chat topic going
            //IList<long>? context = null;
            //var enumerable = ollama.Completions.GenerateCompletionAsync("llama3.2", "answer 5 random words");
            //await foreach (var response in enumerable)
            //{
            //    Console.WriteLine($"> {response.Response}");

            //    context = response.Context;
            //}

            //var lastResponse = await ollama.Completions.GenerateCompletionAsync("llama3.2", "answer 123", stream: false, context: context).WaitAsync();
            //Console.WriteLine(lastResponse.Response);

            //var chat = ollama.Chat("mistral");
            //while (true)
            //{
            //    var message = await chat.SendAsync("answer 123");

            //    Console.WriteLine(message.Content);

            //    var newMessage = Console.ReadLine();
            //    await chat.Send(newMessage);
            //}
        }

        static async Task<string> CallOllamaDeepSeek(string prompt)
        {
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
    }

    class OllamaResponse
    {
        public string response { get; set; }
    }
}

