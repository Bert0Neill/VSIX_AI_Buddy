using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AI_Buddy.Components
{
    /// <summary>
    /// Interaction logic for ToolWindow1Control.
    /// </summary>
    public partial class PromptWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PromptWindowControl"/> class.
        /// </summary>
        public PromptWindowControl()
        {
            this.InitializeComponent();

            // load property settings from file
            // save (last) prompt
        }

        // Clear results
        public void ClearResults()
        {
            rtbResults.Document = new FlowDocument();
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

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            ClearResults();
            AppendResult("Processing your request...");

            // call AI API
            // save prompt to file
            // append result to RTB (keep appending if streaming)

            try
            {
                // Your AI processing logic
                AppendResult("Successfully processed!");
            }
            catch (Exception ex)
            {
                AppendResult(ex.Message, isError: true);
            }
            finally
            {
                this.txtPrompt.Text = string.Empty; // clear for next prompt
            }
        }

        #region RTB
        private void RichTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                HandlePaste();
                e.Handled = true;
            }
        }

        private void HandlePaste()
        {
            if (Clipboard.ContainsImage())
            {
                InsertImageFromClipboard();
            }
            else
            {
                rtbResults.Paste(); // Default text paste
            }
        }

        private void InsertImageFromClipboard()
        {
            var bitmapSource = Clipboard.GetImage();
            if (bitmapSource == null) return;

            // Convert to BitmapImage
            var bitmapImage = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(stream);
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            // Create image container
            //var image = new System.Windows.Controls.Image { Source = bitmapImage};

            var image = new System.Windows.Controls.Image
            {
                Source = bitmapImage,
                Width = 100, // Thumbnail width
                Height = 100, // Thumbnail height
                Stretch = Stretch.Uniform
            };


            var container = new InlineUIContainer(image);

            // Get current paragraph or create a new one
            TextPointer caretPos = rtbResults.CaretPosition;
            Paragraph currentParagraph = caretPos.Paragraph ?? new Paragraph();

            // Add the image to the paragraph's inlines
            currentParagraph.Inlines.Add(container);

            // If new paragraph, add to document
            if (caretPos.Paragraph == null)
            {
                rtbResults.Document.Blocks.Add(currentParagraph);
            }

            // Update caret position
            rtbResults.CaretPosition = currentParagraph.ContentEnd;
        }

        private void rtbResults_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg"))
                    {
                        InsertThumbnailImage(file);
                    }
                }
            }
        }


        private void InsertThumbnailImage(string imagePath)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage(new Uri(imagePath));
                var image = new System.Windows.Controls.Image
                {
                    Source = bitmap,
                    Width = 100, // Thumbnail width
                    Height = 100, // Thumbnail height
                    Stretch = Stretch.Uniform
                };

                InlineUIContainer container = new InlineUIContainer(image);

                // Get current paragraph or create a new one
                TextPointer caretPos = rtbResults.CaretPosition;
                Paragraph currentParagraph = caretPos.Paragraph ?? new Paragraph();

                // Add the image to the paragraph's inlines
                currentParagraph.Inlines.Add(container);

                // If new paragraph, add to document
                if (caretPos.Paragraph == null)
                {
                    rtbResults.Document.Blocks.Add(currentParagraph);
                }

                // Update caret position
                rtbResults.CaretPosition = currentParagraph.ContentEnd;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting image: " + ex.Message);
            }
        }
        #endregion
    }
}