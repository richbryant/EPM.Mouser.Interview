using EPM.Mouser.Interview.Models;
using System.Runtime.CompilerServices;

namespace EPM.Mouser.Interview.Web
{
    public static class Utility
    {
        public static Product UniquifyName(this Product self, List<string> comparators, int iterator = 1)
        {
            if (string.IsNullOrEmpty(self.Name))
            {
                throw new Exception("Invalid value");
            }

            if (!comparators.Contains(self.Name))
            {
                return self;
            }

            self.Name += iterator;
            return self.UniquifyName(comparators, iterator++);
        }
    }
}
