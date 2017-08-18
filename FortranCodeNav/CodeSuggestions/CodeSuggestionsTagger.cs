using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;

namespace FortranCodeNav.CodeSuggestions
{
    internal class CodeSuggestionsTagger : ITagger<ErrorTag>
    {
        private IEnumerable<ICodeSuggestionProvider> codeSuggestionProviders { get; set; }
        private object updateLock = new object();        
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public CodeSuggestionsTagger(IEnumerable<ICodeSuggestionProvider> codeSuggestionProviders)
        {
            this.codeSuggestionProviders = codeSuggestionProviders;
        }
        
        public IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (var line in GetIntersectingLines(spans))
            {
                foreach (var codeSuggestionProvider in codeSuggestionProviders)
                {
                    foreach (var suggestion in codeSuggestionProvider.GetSuggestions(RemoveComments(line.GetText())))
                    {
                        var lineSpan = new SnapshotSpan(line.Start + suggestion.StartIndexInLine, line.Start + suggestion.EndIndexInLine);
                        yield return new TagSpan<ErrorTag>(lineSpan, new ErrorTag(PredefinedErrorTypeNames.Warning, suggestion.SuggestionTooltip));
                    }
                }
            }            
        }

        private string RemoveComments(string line)
        {
            var indexOfComment = line.IndexOf("!");
            if (indexOfComment >= 0)
            {
                line = line.Substring(0, indexOfComment);
            }
            return line;
        }

        private IEnumerable<ITextSnapshotLine> GetIntersectingLines(NormalizedSnapshotSpanCollection spans)
        {
            // borrowed from: https://github.com/Microsoft/VSSDK-Extensibility-Samples/blob/master/Intra-text_Adornment/C%23/Support/RegexTagger.cs

            if (spans.Count == 0)
                yield break;

            int lastVisitedLineNumber = -1;
            ITextSnapshot snapshot = spans[0].Snapshot;
            foreach (var span in spans)
            {
                int firstLine = snapshot.GetLineNumberFromPosition(span.Start);
                int lastLine = snapshot.GetLineNumberFromPosition(span.End);

                for (int i = Math.Max(lastVisitedLineNumber, firstLine); i <= lastLine; i++)
                {
                    yield return snapshot.GetLineFromLineNumber(i);
                }

                lastVisitedLineNumber = lastLine;
            }
        }
    }
}
