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
            this.txtPrompt.Text = string.Empty;
        }

        //private void btnSubmit_Click(object sender, RoutedEventArgs e)
        //{
        //    //UpdateResults(string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()));

        //    //MessageBox.Show(
        //    //  string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
        //    //  "ToolWindow1");
        //}

        private void btnClearText_Click(object sender, RoutedEventArgs e)
        {

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

        // Example usage
        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            ClearResults();
            AppendResult("Processing your request...");

            try
            {
                // Your AI processing logic
                AppendResult("Successfully processed!");
            }
            catch (Exception ex)
            {
                AppendResult(ex.Message, isError: true);
            }
        }

        //public void UpdateResults(string content)
        //{
        //    txtResults.Text = content;
        //    // Optional: Auto-scroll to bottom
        //    txtResults.ScrollToEnd();
        //}
    }
}