using System;

namespace VSIntegration
{
    //Copyright: Adopted from HyperAddin (https://hyperaddin.svn.codeplex.com)

    /// <summary>
    /// CommandInfo represents all the information the command needs to give to specify its user interface.  
    /// </summary>
    public class VSCommand
    {
        public VSCommand(string fullName, Action action, bool addToToolsMenu = false, bool clearOtherKeyUsages = false, string buttonText = "", int imageId = 1)
        {
            this.Name = fullName;
            this.ClearOtherKeyUsages = clearOtherKeyUsages;
            this.ButtonText = buttonText;
            this.Category = "";
            int idx = fullName.LastIndexOf('.');
            if (idx >= 0)
            {
                this.Name = fullName.Substring(idx + 1);
                this.Category = fullName.Substring(0, idx);
            }
            this.OnToolBar = addToToolsMenu;
            this.ToolTip = this.Name;
            this.Action = action;
            this.ImageId = imageId;
        }

        /// <summary>
        /// The name that will be used from the VS Command window (Ctrl-W A) to invoke the command.
        /// This is the last component of the 'fullName' that was passed to the constructor. 
        /// </summary>
        public string Name;
        /// <summary>
        /// Commands are grouped into categories (which can be multi-leveled separated by '.').
        /// </summary>
        public string Category;
        /// <summary>
        /// The name of the button and the floating tooltip used for the command. Defaults to 'Name'
        /// </summary>
        public string ToolTip;
        /// <summary>
        /// A keyboard shortcut for the command.  For example 'Global::Shift+Alt+C,Alt+I' defines a two
        /// keystroke shortcut.   
        /// </summary>
        public string KeyboardBinding;
        /// <summary>
        /// Action is a deletate that gets called when the User invoked the command.  
        /// </summary>
        public Action Action;
        /// <summary>
        /// By default commands show up on the 'Addins' Menu dropdown.  Setting this will also place
        /// it on the Addin Toolbar.  
        /// </summary>
        public bool OnToolBar;
        /// <summary>
        /// By default commands show up on the 'Addins' Menu dropdown.  Setting this will also place
        /// it on the right click context menu of the text window.   
        /// </summary>
        public bool OnContextMenu;
        /// <summary>
        /// If nonNull we call this predicate to determine if the command is enabled. 
        /// </summary>
        public Predicate Enabled;
        public delegate bool Predicate();

        public int ImageId { get; set; }

        public bool ClearOtherKeyUsages { get; set; }
        public string ButtonText { get; set; }
    };
}
