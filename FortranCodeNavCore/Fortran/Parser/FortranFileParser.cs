using System;
using System.Linq;
using FortranCodeNavCore.Fortran.Elements;
using FortranCodeNavCore.SyntaxTrees;
using VSIntegration;
using Type = FortranCodeNavCore.Fortran.Elements.Type;
using System.Threading;
using System.Globalization;

namespace FortranCodeNavCore.Fortran.Parser
{
    public class FortranFileParser
    {
        public bool Debug = true;
        private FortranStreamReader streamReader;

        public SyntaxTree ParseFileContents(string fileContents, FortranStyle style=FortranStyle.Fortran90)
        {
            var oldCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            var sourceAST = new SyntaxTree();

            try
            {   
                streamReader = new FortranStreamReader(fileContents, style);

                IMember parentMember = null;

                while (streamReader.HasMoreToRead())
                {
                    if (streamReader.SkipToNextPotentialCodeElement())
                    {
                        if (TryReadMember<Program>("program", sourceAST, ref parentMember))
                            continue;
                        if (TryReadMember<Module>("module", sourceAST, ref parentMember))
                            continue;
                        if (TryReadMember<Interface>("interface", sourceAST, ref parentMember))
                            continue;
                        if (TryReadMember<Subroutine>("subroutine", sourceAST, ref parentMember))
                            continue;
                        if (TryReadMember<Function>("function", sourceAST, ref parentMember))
                            continue;
                        if (TryReadType("type", sourceAST, ref parentMember))
                            continue;
                        if (TryReadModuleProcedure(ref parentMember))
                            continue;
                    }
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = oldCulture;
            }

            return sourceAST;
        }

        private bool TryReadType(string memberName, SyntaxTree sourceAST, ref IMember parentMember)
        {
            //todo: we're not skipping comments in this code

            if (IsMemberExpected<Type>(parentMember) && TryReadElementString(memberName,false))
            {
                var remainingLine = FortranParseHelper.GetStringUptoEndOfLineOrBeginOfComment(streamReader.Text, streamReader.ReadIndex - 1);
                var skip = false;

                if (remainingLine.IndexOf("(") >= 0)
                {
                    //type use, not a type declaration
                }
                else
                {
                    var indexOfDoubleDots = remainingLine.IndexOf("::");

                    if (indexOfDoubleDots >= 0)
                    {
                        streamReader.ReadIndex--; //read back one
                        streamReader.ReadIndex += indexOfDoubleDots + 2;
                        char c = streamReader.Text[streamReader.ReadIndex];
                        while (FortranParseHelper.IsWhiteSpace(c)) //skip whitespace
                        {
                            c = streamReader.Text[++streamReader.ReadIndex];
                        } 
                    }
                    else
                    {
                        //expect whitespace
                        if (!FortranParseHelper.IsWhiteSpace(streamReader.Text[streamReader.ReadIndex-1]))
                        {
                            skip = true;
                        }
                    }
                    if (!skip)
                    {
                        var name = streamReader.ReadElementName();
                        if (!String.IsNullOrEmpty(name))
                        {
                            OnMemberFound<Type>(ref parentMember, name, sourceAST);
                            return true;
                        }
                    }
                }
            }
            return TryReadEndOfMember<Type>(ref parentMember, memberName);
        }


        private bool TryReadModuleProcedure(ref IMember parentMember)
        {
            if (IsMemberExpected<ProcedureReference>(parentMember))
            {
                if (TryReadElementString("module procedure",true))
                {
                    var name = streamReader.ReadElementName();
                    parentMember.AddScope(new ProcedureReference {Name=name});
                    return true;
                }
            }
            return false;
        }

        private bool TryReadMember<T>(string memberName, SyntaxTree sourceAST, ref IMember parentMember) 
            where T : IMember, new()
        {
            if (IsMemberExpected<T>(parentMember) && TryReadElementString(memberName, true))
            {
                var name = streamReader.ReadElementName();
                OnMemberFound<T>(ref parentMember, name, sourceAST);
                return true;
            }
            return TryReadEndOfMember<T>(ref parentMember, memberName);
        }

        private void OnMemberFound<T>(ref IMember parentMember, string name, SyntaxTree sourceAST) where T : IMember, new()
        {
            var absoluteStartOfElement = (streamReader.ReadIndex - name.Length) + 1;
            var startOfElement = ((absoluteStartOfElement+1) - streamReader.StartOfLineIndex);
            var location = new LocationInFile(streamReader.LineNumber, startOfElement, absoluteStartOfElement);
            AddElementToMember(sourceAST, ref parentMember, new T {Root=sourceAST, Name = name, Location = location });
        }

        private bool TryReadEndOfMember<T>(ref IMember parentMember, string memberName)
        {
            if (!(parentMember is T))
            {
                return false;
            }

            if (TryReadElementString("end " + memberName, true))
            {
                HandleEndOfMember(ref parentMember);
                return true;
            }
            //endsubroutine (for example) without space is also valid f90:
            if (TryReadElementString("end" + memberName, true))
            {
                HandleEndOfMember(ref parentMember);
                return true;
            }
            if (TryReadElementString("end", true))
            {
                var remainingLine = FortranParseHelper.GetStringUptoEndOfLineOrBeginOfComment(streamReader.Text, streamReader.ReadIndex);
                if (String.IsNullOrEmpty(remainingLine))
                {
                    HandleEndOfMember(ref parentMember);
                    return true;
                }
                else                               
                {
                    var trimmedLine = remainingLine.TrimStart();
                    var charsToSkip = remainingLine.Length - trimmedLine.Length;
                    streamReader.ReadIndex += charsToSkip;

                    if (TryReadElementString(memberName, true))
                    {
                        HandleEndOfMember(ref parentMember);
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsMemberExpected<T>(IMember parentMember)
        {
            var type = typeof(T);

            if (type == typeof(Subroutine) || type == typeof(Function))
            {
                return parentMember == null ||
                       parentMember is Module ||
                       parentMember is Interface ||
                       parentMember is Subroutine ||
                       parentMember is Function;
            }
            if (type == typeof(Interface))
            {
                return parentMember == null || parentMember is Module;
            }
            if (type == typeof(Type))
            {
                return parentMember is Module || parentMember is Method;
            }
            if (type == typeof(Program))
            {
                return parentMember == null;
            }
            if (type == typeof(Module))
            {
                return parentMember == null || (parentMember is Program);
            }
            if (type == typeof(ProcedureReference))
            {
                return parentMember is Interface;
            }
            return false;
        }
        
        private void HandleEndOfMember(ref IMember parentMember)
        {
            var name = streamReader.ReadElementName();

            if (!String.IsNullOrEmpty(name) && Debug)
            {
                if (!StringEqual(name, parentMember.Name))
                {
                    var startContext = streamReader.ReadIndex - name.Length;
                    var maxLength = Math.Min(60, streamReader.Text.Length - startContext);
                    var context = streamReader.Text.Substring(startContext, maxLength);
                    throw new InvalidOperationException(
                        String.Format("Unexpected scope change: expected end of '{0}', but was end of '{1}'.\n\nContext:\n{2}",
                            parentMember.Name,
                            name,
                            context));
                }
            }

            FixNamelessInterfaces(parentMember);

            parentMember.EndLocation = new LocationInFile(streamReader.LineNumber, streamReader.ReadIndex-streamReader.StartOfLineIndex, streamReader.ReadIndex);
            parentMember = parentMember.Parent; //up scope
        }

        private void FixNamelessInterfaces(IMember parentMember)
        {
            if (parentMember is Interface && String.IsNullOrEmpty(parentMember.Name))
            {
                //if nameless interface with only one submember, take that name
                var inf = parentMember as Interface;
                var subMembers = inf.References;
                if (subMembers.Count() == 1)
                {
                    parentMember.Name = subMembers.First().Name;
                }
                else
                {
                    parentMember.Name = ""; //true nameless interface?
                }
            }
        }

        private bool StringEqual(string one, string two)
        {
            return one.Equals(two, StringComparison.InvariantCultureIgnoreCase);
        }
        
        private void AddElementToMember(SyntaxTree sourceAST, ref IMember parentMember, IMember newElement)
        {
            if (parentMember == null)
            {
                sourceAST.AddMember(newElement); 
            }
            else
            {
                parentMember.AddScope(newElement);
            }
            parentMember = newElement;
        }

        private bool TryReadElementString(string elementName, bool mustEndInWhitespace)
        {
            var fileContents = streamReader.Text;

            var readTo = streamReader.ReadIndex + elementName.Length;
            var eof = readTo == fileContents.Length;
            if (readTo <= fileContents.Length)
            {
                if (StringEqual(fileContents.Substring(streamReader.ReadIndex, elementName.Length), elementName))
                {
                    if (!mustEndInWhitespace
                        || eof
                        || FortranParseHelper.IsWhiteSpace(fileContents[streamReader.ReadIndex + elementName.Length]))
                    {
                        streamReader.ReadIndex += elementName.Length;
                        if (!eof && fileContents[streamReader.ReadIndex] != '\n')
                            streamReader.ReadIndex += 1; //plus space
                        return true;
                    }
                }
            }            
            return false;
        }
    }
}
