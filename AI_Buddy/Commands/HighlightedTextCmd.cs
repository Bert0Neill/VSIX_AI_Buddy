using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO.Packaging;
using System.Threading;
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
      

        /// <summary>
        /// Initializes a new instance of the <see cref="HighlightedTextCmd"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private HighlightedTextCmd(AsyncPackage package, OleMenuCommandService commandService)
        {
            //this.package = package ?? throw new ArgumentNullException(nameof(package));
            //commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            //var menuCommandID = new CommandID(CommandSet, CommandId);
            //var menuItem = new MenuCommand(this.Execute, menuCommandID);
            //commandService.AddCommand(menuItem);

            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
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
            //// Switch to the main thread - the call to AddCommand in HighlightedTextCommand's constructor requires
            //// the UI thread.
            //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            //OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            //Instance = new HighlightedTextCommand(package, commandService);

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

            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "HighlightedTextCommand";
            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                    this._package,
                    message,
                    title,
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

            string text = await GetSelectedTextAsync();
            if (!string.IsNullOrEmpty(text))
            {
                System.Windows.Forms.MessageBox.Show($"Selected Text: {text}", "VSIX Command");
            }
        }


        //private string GetSelectedText()
        //{
        //    ThreadHelper.ThrowIfNotOnUIThread();
        //    var textManager = _package.GetService(typeof(SVsTextManager)) as IVsTextManager;
        //    if (textManager == null) return string.Empty;

        //    textManager.GetActiveView(1, null, out IVsTextView textView);
        //    if (textView == null) return string.Empty;

        //    textView.GetSelectedText(out string selectedText);
        //    return selectedText;
        //}

        private async Task<string> GetSelectedTextAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var textManager = await _package.GetServiceAsync(typeof(SVsTextManager)) as IVsTextManager;
            if (textManager == null) return string.Empty;

            textManager.GetActiveView(1, null, out IVsTextView textView);
            if (textView == null) return string.Empty;

            textView.GetSelectedText(out string selectedText);
            return selectedText;
        }


    }
}
