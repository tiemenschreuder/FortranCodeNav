using System.Drawing;

namespace VSIntegration.CodeComplete
{
    public class CompletionItem
    {
        public CompletionItem(string name, Bitmap bitmap)
        {
            Name = name;
            Image = bitmap;
        }

        public string Name { get; set; }
        public Bitmap Image { get; set; }
        public string ToolTip { get; set; }       
        
        public override string ToString()
        {
            return Name;
        }
    }
}
