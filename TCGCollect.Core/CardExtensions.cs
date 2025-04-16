using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCGCollect.Core
{
    public static class CardExtensions
    {
        public static string GetDisplayName(this Card card)
        {
            var parts = card.Faces?.FirstOrDefault()?.Parts;
            if (parts == null || parts.Length == 0)
            {
                return card.Id;
            }
            else if (parts.Length == 1)
            {
                return parts[0].Name;
            }
            else if (parts.Length == 2)
            {
                return String.Concat(parts[0].Name, " // ", parts[1].Name);
            }
            else
            {
                throw new AbandonedMutexException("Card has more than 2 parts");
            }
        }
    }
}
