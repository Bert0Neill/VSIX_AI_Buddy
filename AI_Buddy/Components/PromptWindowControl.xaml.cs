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
                rtbPrompt.Document = new FlowDocument();
                // clear for next prompt
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
    }
}