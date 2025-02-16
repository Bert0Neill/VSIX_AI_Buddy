using AI_Buddy.Components;
using AI_Buddy.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace AI_Buddy.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class HighlightedTextCmd
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

      

        /// <summary>
        /// Initializes a new instance of the <see cref="HighlightedTextCmd"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private HighlightedTextCmd(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);

            _editorService = new EditorService();
            _aiService = new AIService();
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static HighlightedTextCmd Instance
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
                new HighlightedTextCmd(package, commandService);
            }
        }



        private async void Execute(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            string text = await _editorService.GetSelectedTextAsync(this._package);

            if (!string.IsNullOrEmpty(text))
            {
                // generate unit test for prompt
                string prompt = "Write a unit test for this code: " + text;

                await _aiService.GetOllamaResponseStreamAsync(prompt, chunk =>
                {
                    var promnptWindow = this._package.FindToolWindow(typeof(PromptWindow), 0, true) as PromptWindow;

                    if (promnptWindow?.Frame == null)
                    {
                        throw new NotSupportedException("Cannot create Prompt Window.");
                    }

                    promnptWindow.PromptResponse = chunk; // update window panel control

                    var windowFrame = (IVsWindowFrame)promnptWindow.Frame;
                    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
                });
            }
        }
    }
}
