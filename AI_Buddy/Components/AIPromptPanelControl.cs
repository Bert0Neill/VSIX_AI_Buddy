using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AI_Buddy.Components
{
    //public partial class AIPromptPanelControl : UserControl
    //{
    //    public AIPromptPanelControl()
    //    {
    //        this.Dock = DockStyle.Fill; // Fill the parent window
    //        this.BackColor = System.Drawing.Color.White; // Optional: Set background color

    //        // Example: Add a label to the panel
    //        Label label = new Label
    //        {
    //            Text = "AI Buddy Prompt",
    //            AutoSize = true,
    //            Location = new System.Drawing.Point(10, 10)
    //        };

    //        this.Controls.Add(label);
    //    }
    //}

    public partial class AIPromptPanelControl : UserControl
    {
        private TextBox txtSearch;
        private Button btnSearch;

        public AIPromptPanelControl()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = System.Drawing.Color.White;
            this.Size = new System.Drawing.Size(300, 100); // Set an initial size

            // Label
            Label label = new Label
            {
                Text = "Your Prompt",
                AutoSize = true,
                Location = new System.Drawing.Point(10, 10)
            };

            // TextBox
            txtSearch = new TextBox
            {
                Location = new System.Drawing.Point(10, 40),
                Width = 200
            };

            // Search Button
            btnSearch = new Button
            {
                Text = "Search",
                Location = new System.Drawing.Point(220, 38)
            };
            btnSearch.Click += BtnSearch_Click;

            // Add controls
            this.Controls.Add(label);
            this.Controls.Add(txtSearch);
            this.Controls.Add(btnSearch);

            Console.WriteLine($"Control Count: {this.Controls.Count}");

            this.PerformLayout();
            this.Refresh();

        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();

            if (!string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show($"Searching for: {searchText}", "Search");
            }
            else
            {
                MessageBox.Show("Please enter a search term.", "Warning");
            }
        }
    }
}
