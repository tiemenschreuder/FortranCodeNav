using System;
using System.IO;

namespace VSIntegration
{
    public class CodeFile
    {
        public string ProjectName { get; set; }
        
        public string FileName { get; private set; }
        
        public string Contents { get; set; }

        private string filePath;
        public string FilePath 
        { 
            get{return filePath;}
            set
            {
                filePath = value;
                if (!String.IsNullOrEmpty(filePath))
                {
                    FileName = Path.GetFileName(filePath);
                }
            }
        }
    }
}
