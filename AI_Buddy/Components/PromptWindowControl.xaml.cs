﻿using AI_Buddy.Models;
using AI_Buddy.Services;
using HTMLConverter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Xml;


namespace AI_Buddy.Components
{
    /// <summary>
    /// Interaction logic for ToolWindow1Control.
    /// </summary>
    public partial class PromptWindowControl : System.Windows.Controls.UserControl
    {
        private readonly string _placeholderTextPrompt = "Enter AI prompt...";
        private readonly AIProperties _aiProperties;
        private readonly FileService _fileService;
        private readonly RichTextBoxParagraphGenerator _richTextBoxParagraphGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="PromptWindowControl"/> class.
        /// </summary>
        public PromptWindowControl()
        {
            this.InitializeComponent();
            this.Unloaded += PromptWindowControl_Unloaded;

            _fileService = new FileService();
            _aiProperties = new AIProperties();
            _richTextBoxParagraphGenerator = new RichTextBoxParagraphGenerator();

            // read file settings
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), _aiProperties.SettingsFilename);

            if (File.Exists(filePath))
            {
                _aiProperties = _fileService.LoadFromJson<AIProperties>(filePath);
            }

            rtbResults.Document.Blocks.Clear();
        }

        private void PromptWindowControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        public void AppendResult(string text, bool isError = false)
        {
            rtbResults.Dispatcher.Invoke(() =>
            {
                Paragraph paragraph = null;

                if (!isError && rtbResults.Document.Blocks.LastBlock is Paragraph lastParagraph)
                {
                    paragraph = lastParagraph;
                }
                else
                {
                    paragraph = new Paragraph();
                    rtbResults.Document.Blocks.Add(paragraph);
                }
                paragraph.Foreground = System.Windows.Media.Brushes.Black; // default

                if (isError)
                {
                    // If error, prepend error label in bold (you might want to start a new paragraph for clarity)
                    paragraph.Foreground = System.Windows.Media.Brushes.OrangeRed;
                    paragraph.Inlines.Add(new Bold(new Run("ERROR: ")));
                }

                // Append the new text (you might want to add a space if necessary)
                paragraph.Inlines.Add(new Run(text + " "));

                // Ensure scrolling works correctly
                rtbResults.CaretPosition = rtbResults.Document.ContentEnd;
                rtbResults.ScrollToEnd();
                rtbResults.Focus(); // Optional: Ensures UI refresh
            });
        }

        private async void SubmitClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                var prompt = ExtractRichTextBoxContent();

                if (prompt == _placeholderTextPrompt) return; // nothing to enter

                // Insert the paragraph into the RichTextBox's document
                this.rtbResults.Document.Blocks.Add(_richTextBoxParagraphGenerator.GenerateSubmitPromptParagraph(prompt, _richTextBoxParagraphGenerator.IsRichTextBoxEmpty(this.rtbResults)));

                // Clear the rtbPrompt content
                this.rtbPrompt.Document = new FlowDocument();

                await GetOllamaResponseStreamAsync(prompt, chunk =>
                {
                    AppendResult(chunk); // Display each chunk as it arrives (cater for streaming)
                });
            }
            catch (Exception ex)
            {
                AppendResult(ex.Message, isError: true);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        #region RTB
        private void rtbPrompt_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(System.Windows.Forms.DataFormats.Bitmap))
            {
                e.CancelCommand(); // Prevent the default paste behavior

                // Retrieve the image from the clipboard
                var bitmapSource = System.Windows.Clipboard.GetImage();
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
            using (HttpClient client = new HttpClient())
            {
                var requestBody = new
                { 
                    model = _aiProperties.PromptLLMName,
                    prompt = prompt,
                    stream = _aiProperties.IsPromptResponseStreaming // Enable streaming response
                };

                // append api key to request
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_aiProperties.AIPromptKey}");

                string jsonContent = JsonConvert.SerializeObject(requestBody);
                HttpContent httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                using (var request = new HttpRequestMessage(HttpMethod.Post, _aiProperties.AIPromptURL) { Content = httpContent })
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
                                var jsonResponse = JsonConvert.DeserializeObject<AIHosterResponse>(line);
                                jsonResponse.response = Regex.Replace(jsonResponse.response, @"<\/?think>", "") // remove DeepSeek's <think> elements
                                    .TrimStart('\n', '\r')
                                    .TrimStart();

                                onResponseChunk(jsonResponse.response);
                            }
                        }
                    }
                }
            }
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
                rtbPrompt.Document.Blocks.Add(new Paragraph(new Run(_placeholderTextPrompt)));
            }
        }

        private bool IsPlaceholderActive()
        {
            // Check if the placeholder text is currently active
            return new TextRange(rtbPrompt.Document.ContentStart, rtbPrompt.Document.ContentEnd).Text.Trim() == _placeholderTextPrompt;
        }

        internal void UpdatePromptResponse(string value)
        {
            AppendResult(value);
        }

        private FlowDocument HtmlToFlowDocument(string html)
        {
            // Convert HTML to XAML (you need to include the HtmlToXamlConverter helper)
            string xaml = HtmlToXamlConverter.ConvertHtmlToXaml(html, true);

            // Load FlowDocument from XAML
            using (StringReader stringReader = new StringReader(xaml))
            using (XmlReader xmlReader = XmlReader.Create(stringReader))
            {
                return XamlReader.Load(xmlReader) as FlowDocument;
            }
        }

        internal void UpdateResultPrompt(Paragraph[] value)
        {
            foreach (var item in value)
            {
                //Paragraph paragraph = new Paragraph();
                //paragraph.Inlines.Add(item);
                this.rtbResults.Document.Blocks.Add(item);
            }

            // Ensure scrolling works correctly
            rtbResults.CaretPosition = rtbResults.Document.ContentEnd;
            rtbResults.ScrollToEnd();
            rtbResults.Focus(); // Optional: Ensures UI refresh
        }
    }
}