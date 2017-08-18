using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace FortranCodeNav.CodeSuggestions
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("text")]
    [TagType(typeof(ErrorTag))]
    internal class CodeSuggestionsTaggerProvider : IViewTaggerProvider
    {
        [Import]
        internal ITextSearchService TextSearchService { get; set; }

        [Import]
        internal ITextStructureNavigatorSelectorService TextStructureNavigatorSelector { get; set; }

        [ImportMany(RequiredCreationPolicy = CreationPolicy.Shared)]
        internal IEnumerable<ICodeSuggestionProvider> CodeSuggestionProviders = null;

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            //provide highlighting only on the top buffer 
            if (textView.TextBuffer != buffer)
                return null;

            try
            {
                // filter on fortran files:
                var document = (ITextDocument)buffer.Properties[typeof(ITextDocument)];
                if (document == null)
                    return null;
                if (!FortranCodeNavCore.FortranCodeNavCore.KeepFile(document.FilePath))
                    return null;
                
                return new CodeSuggestionsTagger(CodeSuggestionProviders) as ITagger<T>;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
