using System.Collections.Generic;
using FortranCodeNavCore.Fortran.Elements;
using VSIntegration;

namespace FortranCodeNavCore.SyntaxTrees
{
    public class SyntaxTree
    {
        public CodeFile CodeFile { get; set; }
        public string FileName { get; set; }
        private readonly IList<IMember> members = new List<IMember>();
        public IEnumerable<IMember> Members { get { return members; } }
        public void AddMember(IMember member)
        {
            members.Add(member);
        }
                
        public IEnumerable<IMember> GetAllMembers()
        {
            var queue = new Queue<IMember>();
            
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