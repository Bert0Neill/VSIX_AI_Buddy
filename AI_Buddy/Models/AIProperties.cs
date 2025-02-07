using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Buddy.Models
{
    internal class AIProperties
    {
        [Category("AI Provider")]
        [DisplayName("Name of AI provider")]
        [Description("For your reference, the name of the prompt provider.")]
        public string AIProvider { get; set; }

        [Category("AI Provider")]
        [DisplayName("Prompt API URL")]
        [Description("The Prompt URL that will get called with your prompt.")]
        
        public string AIPromptURL { get; set; }

        [Category("AI Provider")]
        [DisplayName("API Key")]
        [Description("The key may not be required for internal server prompts.")]
        public string AIPromptKey { get; set; }

        [Category("AI Provider")]
        [DisplayName("LLM Name")]
        [Description("For your reference, what LLM model is your LLM Hoster using.")]
        public string PrompLLMName { get; set; }

        [Category("Coding")]
        [DisplayName("Coding Language that you want AI prompt to respond in.")]
        [Description("Software language, for e.g. .Net9.")]
        public string CodingLanguage { get; set; } = "C#";

        [Category("Coding")]
        [DisplayName("Testing Framework")]
        [Description("What testing framework you want the AI prompt to generate your tests in.")]
        public string TestFramework { get; set; }

        [Category("General Info")]
        [DisplayName("Last Modified")]
        [Description("Whan you last updated the properties.")]
        public DateTime DateLastModified { get; set; } = DateTime.Now;

        [Category("General Info")]
        [DisplayName("Last 10 prompts")]
        [Description("The last 10 prompts you made.")]
        public List<string> LastTenPrompts { get; set; }

        [Category("General Info")]
        [DisplayName("Settings Filename")]
        [Description("The location where these settings are saved to (readonly).")]
        [ReadOnly(true)]
        public string SettingsFilename { get; set; } = "AI_Buddy_Settings.json";

    }
}
