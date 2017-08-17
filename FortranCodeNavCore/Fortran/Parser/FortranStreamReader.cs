using System;

namespace FortranCodeNavCore.Fortran.Parser
{
    public class FortranStreamReader
    {
        private FortranStyle Style { get; set; }

        public FortranStreamReader(string text, FortranStyle style)
        {
            Style = style;
            Text = text ?? string.Empty;
            LineNumber = 1;
        }

        private int lastIndexVisited = -1; 
        private bool insideCharacterString;
        private bool insideQuoteString;

        public int ReadIndex; //field for performance
        public string Text { get; private set; }
        public int LineNumber { get; private set; }
        public int StartOfLineIndex { get; private set; }
        
        public bool HasMoreToRead()
        {
            return ReadIndex < Text.Length-1;
        }

        public bool SkipToNextPotentialCodeElement()
        {
            if (lastIndexVisited == ReadIndex)
            {
                ReadIndex++; //continue to next character if we already got here
            }

            var start = ReadIndex;
            for (ReadIndex = start; ReadIndex < Text.Length; ReadIndex++)
            {
                var c = Text[ReadIndex];

                if (c == '\n')
                {
                    LineNumber++;
                    StartOfLineIndex = ReadIndex + 1;
                }

                if (FortranParseHelper.IsWhiteSpace(c))
                {
                    continue; //shortcut
                }

                if (!insideCharacterString && c == '"')
                {
                    insideQuoteString = !insideQuoteString; //toggle
                    continue;
                }
                if (!insideQuoteString && c == '\'')
                {
                    insideCharacterString = !insideCharacterString; //toggle
                    continue;
                }

                if (!insideCharacterString && !insideQuoteString)
                {
                    //read comment
                    if (c == '!')
                    {
                        SeekEndOfLine(); //skip comments
                        continue;
                    }

                    if (Style == FortranStyle.Fortran77 && ReadIndex == StartOfLineIndex && (c == 'c' || c == 'C' || c == '*'))
                    {
                        SeekEndOfLine(); //skip comments (f77 style?)
                        continue;
                    }

                    var previousCharWasWhitespace = (ReadIndex > 0) ? FortranParseHelper.IsWhiteSpace(Text[ReadIndex - 1]) : true;

                    if (!previousCharWasWhitespace) //still inside a word
                        continue;

                    //this would improve performance and simply some things, but unfortunately the keywords 
                    //we're looking for are not always the first word on the line. For example the 'function'
                    //keyword can be prefixed by a type.

                    //if (beginOfLineOnly && lastVisitedLine == LineNumber)
                    //    continue;

                    lastIndexVisited = ReadIndex;
                    return true;
                }
            }
            return false; //eof
        }
        
        public string ReadElementName()
        {
            bool nameStarted = false;

            var start = ReadIndex;
            for (ReadIndex = start; ReadIndex < Text.Length; ReadIndex++)
            {
                var c = Text[ReadIndex];
                
                if (!nameStarted && c == ' ') //skip spaces in front of name
                {
                    start++;
                    continue;
                }

                nameStarted = true;

                if (FortranParseHelper.IsWhiteSpace(c)) //quick loop
                {
                    //end of name
                    break;
                }

                if (!FortranParseHelper.IsWordCharacter(c))
                {
                    //non valid name characters: end of name
                    break;
                }
            }
            if (ReadIndex < Text.Length)
            {
                var name = Text.Substring(start, ReadIndex - start);
                if (!String.IsNullOrEmpty(name))
                {
                    ReadIndex--;
                }
                return name;
            }
            return "";
        }

        public string DebugNextFewLines
        {
            get { return Text.Substring(ReadIndex, Math.Min(300, Text.Length - ReadIndex)); }
        }

        private void SeekEndOfLine()
        {
            var start = ReadIndex;
            for (ReadIndex = start; ReadIndex < Text.Length; ReadIndex++)
            {
                if (Text[ReadIndex] == '\n')
                {
                    ReadIndex = ReadIndex - 1;
                    return;
                }
            }
            return;
        }
    }
}