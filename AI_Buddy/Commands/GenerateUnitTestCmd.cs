﻿using AI_Buddy.Components;
using AI_Buddy.Models;
using AI_Buddy.Resources;
using AI_Buddy.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Windows.Documents;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using Task = System.Threading.Tasks.Task;

namespace AI_Buddy.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GenerateUnitTestCmd
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0102;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("f3ae7c69-4f62-438d-a386-a11549044a8c");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Microsoft.VisualStudio.Shell.Package package;
        private readonly AsyncPackage _package;
        private readonly EditorService _editorService;
        private readonly AIService _aiService;
        private readonly AIProperties _aiProperties;
        private readonly FileService _fileService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateUnitTestCmd"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private GenerateUnitTestCmd(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);

            _editorService = new EditorService();
            _aiService = new AIService();
            _aiProperties = new AIProperties();
            _fileService = new FileService();

            // read file settings
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), _aiProperties.SettingsFilename);

            if (File.Exists(filePath))
            {
                _aiProperties = _fileService.LoadFromJson<AIProperties>(filePath);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static GenerateUnitTestCmd Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this._package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                new GenerateUnitTestCmd(package, commandService);
            }
        }

        private async void Execute(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            string text = await _editorService.GetSelectedTextAsync(this._package);

            if (!string.IsNullOrEmpty(text))
            {
                // generate unit test for prompt
                string prompt = String.Format(PromptStrings.UnitTestPrompt, _aiProperties.CodingLanguage, _aiProperties.TestFramework, Environment.NewLine + text);

                // Initialize and show the window once before streaming
                var promptWindow = this._package.FindToolWindow(typeof(PromptWindow), 0, true) as PromptWindow;
                if (promptWindow?.Frame == null)
                {
                    throw new NotSupportedException("Cannot create Prompt Window.");
                }

                Paragraph paragraph = new Paragraph();
                var promptDetails = new Paragraph[2];
                Run promptLabel = new Run($"Prompt: ")
                {
                    Foreground = System.Windows.Media.Brushes.Black,                    
                };                

                Run promptDescription = new Run($"Generating a Unit Test ({_aiProperties.TestFramework}) in {_aiProperties.CodingLanguage} for your code: {Environment.NewLine}")
                {
                    Foreground = System.Windows.Media.Brushes.Blue                    
                };
                paragraph.Inlines.Add(promptLabel);
                paragraph.Inlines.Add(promptDescription);

                promptDetails[0] = paragraph;

                // create a Run for the code (blue & italic)
                paragraph = new Paragraph();
                Run promptCode = new Run($"{text} {Environment.NewLine + Environment.NewLine}") 
                {
                    Foreground = System.Windows.Media.Brushes.Black,
                    FontStyle = System.Windows.FontStyles.Italic
                };
                paragraph.Inlines.Add(promptCode);

                promptDetails[1] = paragraph;
                promptWindow.FormattedPrompt = promptDetails; // update window panel control

                var windowFrame = (IVsWindowFrame)promptWindow.Frame;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());

                // Start streaming and update the window's control for each chunk
                await _aiService.GetOllamaResponseStreamAsync(prompt, chunk =>
                {                    
                    promptWindow.PromptResponse = chunk; // append the response
                });
            }
        }
    }
}
