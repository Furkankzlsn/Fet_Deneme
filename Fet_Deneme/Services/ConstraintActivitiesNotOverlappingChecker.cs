using Fet_Deneme.Models;
using System.Collections.Generic;
using System.Linq;

namespace Fet_Deneme.Services
{
    public class ConstraintActivitiesNotOverlappingChecker
    {
        private readonly List<ConstraintActivitiesNotOverlapping> _constraints;

        public ConstraintActivitiesNotOverlappingChecker(List<ConstraintActivitiesNotOverlapping> constraints)
        {
            _constraints = constraints;
        }

        // activity: yeni eklenecek aktivite
        // day, hour: eklenecek slot
        // assignments: mevcut atamalar
        // allActivities: id -> TimetableActivity
        public bool IsValid(TimetableActivity activity, string day, string hour, List<GeneratedAssignment> assignments, Dictionary<int, TimetableActivity> allActivities)
        {
            foreach (var constraint in _constraints)
            {
                if (!constraint.Active || constraint.ActivityIds == null || constraint.ActivityIds.Count < 2)
                    continue;
                // Bu kısıt, bu aktiviteyi kapsıyor mu?
                if (!constraint.ActivityIds.Contains(activity.Id))
                    continue;
                // Aynı kısıttaki diğer aktivitelerden, bu slotta atanmış olan var mı?
                foreach (var assign in assignments)
                {
                    if (assign.Day == day && assign.Hour == hour && constraint.ActivityIds.Contains(assign.ActivityId) && assign.ActivityId != activity.Id)
                    {
                        return false; // Çakışma var
                    }
                }
            }
            return true;
        }
    }
}
