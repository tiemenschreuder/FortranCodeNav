using System.Collections.Generic;
using FortranCodeNavCore.SyntaxTrees;
using VSIntegration;

namespace FortranCodeNavCore.Fortran.Elements
{
    public class Member : IMember
    {
        protected readonly IList<IScope> subItems = new List<IScope>();

        public bool ImplicitNone { get; set; }

        private readonly IList<Use> uses = new List<Use>();
        public IEnumerable<Use> Uses
        {
            get { return uses; }
        }

        public void AddUse(Use use)
        {
            uses.Add(use);
        }

        public void ClearUses()
        {
            uses.Clear();
        }

        private readonly IList<Variable> localVariables = new List<Variable>();
        public IEnumerable<Variable> LocalVariables
        {
            get { return localVariables; }
        }

        public void AddLocalVariable(Variable variable)
        {
            localVariables.Add(variable);
        }

        public void ClearLocalVariables()
        {
            localVariables.Clear();
        }

        public SyntaxTree Root { get; set; }
        public LocationInFile Location { get; set; }
        public string Name { get; set; }
        public LocationInFile EndLocation { get; set; }

        public virtual IEnumerable<IScope> SubItems { get { return subItems; } }
        
        public void AddScope(IScope subItem)
        {
            subItem.Parent = this;
            subItems.Add(subItem);
        }

        public IMember Parent { get; set; }
        
        public override string ToString()
        {
            return Name + " (" + GetType().Name +")";
        }

        public IEnumerable<IScope> GetSubItemsRecursive()
        {
            foreach (var subItem in subItems)
            {
                yield return subItem;
                var subItemAsMember = subItem as IMember;
                foreach (var subSubItem in subItemAsMember.GetSubItemsRecursive())
                {
                    yield return subSubItem;
                }
            }
        }
    }
}