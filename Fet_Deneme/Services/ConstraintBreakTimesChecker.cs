using Fet_Deneme.Models;
using System.Collections.Generic;
using System.Linq;

namespace Fet_Deneme.Services
{
    public class ConstraintBreakTimesChecker
    {
        private readonly List<ConstraintBreakTimes> _constraints;

        public ConstraintBreakTimesChecker(List<ConstraintBreakTimes> constraints)
        {
            _constraints = constraints;
        }

        // Belirli bir gün ve saat break mi?
        public bool IsBreak(string day, string hour)
        {
            foreach (var constraint in _constraints)
            {
                if (constraint.BreakTimes == null) continue;
                foreach (var bt in constraint.BreakTimes)
                {
                    if (bt.Day == day && bt.Hour == hour)
                        return true;
                }
            }
            return false;
        }
    }
}
