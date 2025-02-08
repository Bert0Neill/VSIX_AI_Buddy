using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace AI_Buddy.Components
{
    [Guid("A1234567-89AB-4CDE-0123-456789ABCDEF")]
    internal class PromptPanel : ToolWindowPane
    {

         // Generate a unique GUID
            public PromptPanel() : base(null)
            {
                this.Caption = "AI Buddy Prompt"; // Title of the panel
                this.Content = new AIPromptPanelControl(); // Assign the WinForms control

            }
    }
}
