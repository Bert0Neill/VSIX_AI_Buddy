using AI_Buddy.Models;
using AI_Buddy.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;


namespace AI_Buddy.Components
{
    /// <summary>
    /// Interaction logic for ToolWindow1Control.
    /// </summary>
    public partial class PromptWindowControl : UserControl
    {
        private readonly string _placeholderText = "Enter AI prompt...";
        AIProperties _aiProperties;
        FileService _fileService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PromptWindowControl"/> class.
        /// </summary>
        public PromptWindowControl()
        {
            this.InitializeComponent();

            _fileService = new FileService();
            _aiProperties = new AIProperties();

            // read file settings
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), _aiProperties.SettingsFilename);

            if (File.Exists(filePath))
            {
                _aiProperties = _fileService.LoadFromJson<AIProperties>(filePath);
            }
        }

        // Append formatted text
        public void AppendResult(string text, bool isError = false)
        {
            var paragraph = new Paragraph
            {
                Margin = new Thickness(0, 0, 0, 5)
            };

            if (isError)
            {
                paragraph.Foreground = System.Windows.Media.Brushes.OrangeRed;
                paragraph.Inlines.Add(new Bold(new Run("ERROR: ")));
            }

            paragraph.Inlines.Add(new Run(text));

            rtbResults.Document.Blocks.Add(paragraph);
            rtbResults.ScrollToEnd();
        }

        private async void SubmitClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                var prompt = ExtractRichTextBoxContent();

                if (prompt == _placeholderText) return; // nothing to enter

                // Create a Run element with the text and set its foreground color to blue
                Run run = new Run("Prompt: " + prompt)
                {
                    Foreground = System.Windows.Media.Brushes.Yellow
                };

                // Create a new Paragraph and add the Run element
                Paragraph paragraph = new Paragraph(run);

                // Insert the paragraph into the RichTextBox's document
                this.rtbResults.Document.Blocks.Add(paragraph);

                this.rtbPrompt.Document = new FlowDocument(); // clear prompt rtb

                await GetOllamaResponseStreamAsync(prompt, chunk =>
                {
                    AppendResult(chunk); // Display each chunk as it arrives
                });
            }
            catch (Exception ex)
            {
                AppendResult(ex.Message, isError: true);
            }
            finally
            {
            }
        }

        #region RTB
        private void rtbPrompt_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Bitmap))
            {
                e.CancelCommand(); // Prevent the default paste behavior

                // Retrieve the image from the clipboard
                var bitmapSource = Clipboard.GetImage();
                if (bitmapSource != null)
                {
                    // Create an Image element
                    System.Windows.Controls.Image image = new System.Windows.Controls.Image
                    {
                        Source = bitmapSource,
                        Width = 100, // Set width or adjust dynamically
                        Height = 100, // Set height or adjust dynamically
                        Stretch = System.Windows.Media.Stretch.Uniform
                    };

                    var container = new InlineUIContainer(image);

                    // Get current paragraph or create a new one
                    TextPointer caretPos = rtbPrompt.CaretPosition;
                    Paragraph currentParagraph = caretPos.Paragraph ?? new Paragraph();

                    // Add the image to the paragraph's inlines
                    currentParagraph.Inlines.Add(container);

                    // If new paragraph, add to document
                    if (caretPos.Paragraph == null)
                    {
                        rtbPrompt.Document.Blocks.Add(currentParagraph);
                    }

                    // Insert a new empty paragraph after the image
                    Paragraph newParagraph = new Paragraph();
                    rtbPrompt.Document.Blocks.Add(newParagraph);

                    // Move the caret to the new paragraph
                    rtbPrompt.CaretPosition = newParagraph.ContentStart;
                }
            }
        }
        #endregion

        private async Task GetOllamaResponseStreamAsync(string prompt, Action<string> onResponseChunk)
        {
            string ollamaUrl = "http://localhost:11434/api/generate";
            string model = "deepseek-r1:1.5b"; // Ensure the model is running locally

            using (HttpClient client = new HttpClient())
            {
                var requestBody = new
                {
                    model = model,
                    prompt = prompt,
                    stream = false // Enable streaming response
                };

                string jsonContent = JsonConvert.SerializeObject(requestBody);
                HttpContent httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                using (var request = new HttpRequestMessage(HttpMethod.Post, ollamaUrl) { Content = httpContent })
                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        onResponseChunk($"Error: {response.StatusCode}");
                        return;
                    }

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = await reader.ReadLineAsync();
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                var jsonResponse = JsonConvert.DeserializeObject<OllamaResponse>(line);
                                jsonResponse.response = Regex.Replace(jsonResponse.response, @"<\/?think>", "")
                                    .TrimStart('\n', '\r')
                                    .TrimStart();

                                onResponseChunk(jsonResponse.response);
                            }
                        }
                    }
                }
            }
        }

        public class OllamaResponse
        {
            public string response { get; set; }
        }

        private string ExtractRichTextBoxContent()
        {
            StringBuilder extractedText = new StringBuilder();
            List<BitmapSource> images = new List<BitmapSource>();

            foreach (Block block in rtbPrompt.Document.Blocks)
            {
                if (block is Paragraph paragraph)
                {
                    foreach (Inline inline in paragraph.Inlines)
                    {
                        if (inline is Run run)
                        {
                            // Extract text
                            extractedText.Append(run.Text);
                        }
                        else if (inline is InlineUIContainer container && container.Child is System.Windows.Controls.Image image)
                        {
                            // Extract image
                            BitmapSource bitmap = image.Source as BitmapSource;
                            if (bitmap != null)
                            {
                                images.Add(bitmap);
                                extractedText.Append("[Image]"); // Placeholder for image in text
                            }
                        }
                    }
                }
            }

            // Display extracted text
            string plainText = extractedText.ToString();

            // Process extracted images
            int count = 1;
            foreach (var img in images)
            {
                SaveImage(img, $"ExtractedImage_{count}.png");
                count++;
            }

            return plainText;
        }

        private void SaveImage(BitmapSource image, string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(stream);
            }
        }

        private void rtbPrompt_Loaded(object sender, RoutedEventArgs e)
        {
            SetPlaceholderIfNeeded();
        }

        private void rtbPrompt_GotFocus(object sender, RoutedEventArgs e)
        {
            // If the placeholder is visible, clear it when user starts typing
            if (IsPlaceholderActive())
            {
                rtbPrompt.Document.Blocks.Clear();
                rtbPrompt.Foreground = System.Windows.Media.Brushes.Black; // Change text color to black
            }
        }

        private void rtbPrompt_LostFocus(object sender, RoutedEventArgs e)
        {
            // Restore the placeholder if the content is empty
            //SetPlaceholderIfNeeded();
        }

        private void SetPlaceholderIfNeeded()
        {
            // Check if the document is empty or contains only the placeholder text
            if (rtbPrompt.Document.Blocks.Count == 0 || string.IsNullOrWhiteSpace(new TextRange(rtbPrompt.Document.ContentStart, rtbPrompt.Document.ContentEnd).Text.Trim()))
            {
                rtbPrompt.Foreground = System.Windows.Media.Brushes.Gray; // Set text color to gray for placeholder
                rtbPrompt.Document.Blocks.Clear();
                rtbPrompt.Document.Blocks.Add(new Paragraph(new Run(_placeholderText)));
            }
        }

        private bool IsPlaceholderActive()
        {
            // Check if the placeholder text is currently active
            return new TextRange(rtbPrompt.Document.ContentStart, rtbPrompt.Document.ContentEnd).Text.Trim() == _placeholderText;
        }
    }
}