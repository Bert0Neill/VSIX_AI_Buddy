using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

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
                paragraph.Foreground = Brushes.OrangeRed;
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
    }
}