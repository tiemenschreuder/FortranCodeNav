using System;
using System.Collections.Generic;
using System.Linq;
using FortranCodeNavCore.Commands.Matches;
using FortranCodeNavCore.Fortran.Elements;
using FortranCodeNavCore.Fortran.Parser;
using FortranCodeNavCore.SyntaxTrees;
using Type = FortranCodeNavCore.Fortran.Elements.Type;

namespace FortranCodeNavCore.Fortran
{
    public static class FortranSyntaxTreeModel
    {
        public static IEnumerable<SearchResult> FindInterfaceReferences(Interface @interface, SyntaxTree syntaxTree)
        {
            var parentMember = @interface.Parent;
            foreach(var reference in @interface.References)
            {
                var matchingMember = parentMember.SubItems.OfType<IMember>().
                    FirstOrDefault(m => String.Equals(m.Name, reference.Name, StringComparison.InvariantCultureIgnoreCase));
                if (matchingMember != null)
                {
                    yield return new SearchResult {Member = matchingMember, SyntaxTree = syntaxTree};
                }
            }
        }

        public static IEnumerable<INameable> GetElementsAvailableInScope(ICollection<SyntaxTree> syntaxTrees, IMember member)
        {
            var allMembersAvailableFromScope =
                GetElementsAvailableInScopeCore(syntaxTrees, member, new List<Module>()).ToList();

            AddGlobalElements(syntaxTrees, allMembersAvailableFromScope);

            return allMembersAvailableFromScope.Distinct().ToList();
        }

        private static IEnumerable<Module> GetAllModules(IEnumerable<SyntaxTree> syntaxTrees)
        {
            return syntaxTrees.SelectMany(a => a.Members.OfType<Module>()).ToList(); //assume no nested modules
        }

        private static IEnumerable<INameable> GetElementsAvailableInScopeCore(ICollection<SyntaxTree> syntaxTrees, IMember member, IList<Module> visitedModules)
        {
            var items = new List<INameable>();

            if (member is IMethod)
            {
                var method = member as IMethod;
                new FortranDeclarationParser().ParseDeclarationsAndUses(method);
                items.AddRange(method.LocalVariables.Cast<INameable>());
                items.AddRange(GetElementsAvailableInScopeCore(syntaxTrees, method.Parent, visitedModules));
                items.AddRange(GetMembersFromUses(method, syntaxTrees, visitedModules));
                items.AddRange(method.SubItems.OfType<Type>().Cast<INameable>()); //subroutines can have types in them
            }
            else if (member is Module)
            {
                var module = member as Module;

                if (visitedModules.Contains(module))
                    return items; // prevent infinite recursion

                visitedModules.Add(module);

                new FortranDeclarationParser().ParseDeclarationsAndUses(module);
                items.AddRange(module.LocalVariables.Cast<INameable>());
                items.AddRange(GetMembersFromUses(module, syntaxTrees, visitedModules));
                items.AddRange(GetMembersFromModule(module));
            }

            return items;
        }

        private static IEnumerable<INameable> GetMembersFromUses(IMember member, ICollection<SyntaxTree> syntaxTrees, IList<Module> visitedModules)
        {
            var items = new List<INameable>();
            var allModules = GetAllModules(syntaxTrees);

            foreach (var use in member.Uses)
            {
                var module = allModules.FirstOrDefault(m => String.Equals(m.Name, use.Module, StringComparison.InvariantCultureIgnoreCase));
                if (module != null)
                {
                    items.AddRange(GetElementsAvailableInScopeCore(syntaxTrees, module, visitedModules));
                }
                else
                {
                    Log.Write(String.Format("Cannot find module {0} mentioned in use statement in member {1}", use.Module, use.Parent));
                }
            }

            return items;
        }
        
        private static IEnumerable<INameable> GetMembersFromModule(Module module)
        {
            return
                module.LocalVariables.Cast<INameable>().Concat(
                    module.SubItems.OfType<IMethod>().Cast<INameable>()).Concat(
                        module.SubItems.OfType<Type>().Cast<INameable>());
        }

        private static void AddGlobalElements(ICollection<SyntaxTree> syntaxTrees, List<INameable> allMembersAvailableFromScope)
        {
            // add methods & types in programs
            var programs = syntaxTrees.SelectMany(a => a.Members.OfType<Program>()).ToList();
            foreach (var program in programs)
            {
                new FortranDeclarationParser().ParseDeclarationsAndUses(program);
            }
            allMembersAvailableFromScope.AddRange(programs.SelectMany(GetMembersFromModule).OfType<INameable>());

            // add methods outside any scope
            allMembersAvailableFromScope.AddRange(
                syntaxTrees.SelectMany(a => a.Members.Where(m => m is Type || m is IMethod).OfType<INameable>()));
        }
    }
}