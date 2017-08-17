using System.Collections.Generic;
using VSIntegration;

namespace FortranCodeNavCore.Fortran.Elements
{
    public class SyntaxTree
    {
        public CodeFile CodeFile { get; set; }
        public string FileName { get; set; }
        private IList<IMember> members = new List<IMember>();
        public IEnumerable<IMember> Members { get { return members; } }
        public void AddMember(IMember member)
        {
            members.Add(member);
        }
                
        public IEnumerable<IMember> GetAllMembers()
        {
            Queue<IMember> queue = new Queue<IMember>();
            
            foreach (var member in members)
            {
                queue.Enqueue(member);
            }

            while (queue.Count > 0)
            {
                var member = queue.Dequeue();
                foreach (var item in member.SubItems)
                {
                    if (item is IMember)
                    {
                        queue.Enqueue(item as IMember);
                    }
                }
                yield return member;
            }
        }
    }
}