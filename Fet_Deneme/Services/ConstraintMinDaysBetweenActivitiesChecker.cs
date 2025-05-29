using Fet_Deneme.Models;
using System.Collections.Generic;
using System.Linq;

namespace Fet_Deneme.Services
{
    public class ConstraintMinDaysBetweenActivitiesChecker
    {
        private readonly List<ConstraintMinDaysBetweenActivities> _constraints;
        private readonly List<Day> _days;

        public ConstraintMinDaysBetweenActivitiesChecker(List<ConstraintMinDaysBetweenActivities> constraints, List<Day> days)
        {
            _constraints = constraints;
            _days = days;
        }

        // assignments: mevcut atamalar
        // activity: yeni eklenecek aktivite
        // day: eklenecek gün
        // allAssignments: tüm atamalar
        // activities: id -> TimetableActivity
        public bool IsValid(List<GeneratedAssignment> assignments, TimetableActivity activity, string day, Dictionary<int, TimetableActivity> activities, List<Activity> allActivities)
        {
            foreach (var constraint in _constraints)
            {
                if (!constraint.Active || constraint.ActivityIds == null || constraint.ActivityIds.Count < 2)
                    continue;
                if (!constraint.ActivityIds.Contains(activity.Id))
                    continue;
                int minDays = constraint.MinDays;
                // Bu constraintteki diğer aktivitelerden, bu gün atanmış olanları bul
                foreach (var assign in assignments)
                {
                    if (assign.ActivityId == activity.Id)
                        continue;
                    if (!constraint.ActivityIds.Contains(assign.ActivityId))
                        continue;
                    // Günler arası farkı bul
                    int idx1 = _days.FindIndex(d => d.Name == day);
                    int idx2 = _days.FindIndex(d => d.Name == assign.Day);
                    if (idx1 == -1 || idx2 == -1)
                        continue;
                    int diff = System.Math.Abs(idx1 - idx2);
                    if (diff < minDays)
                        return false; // Aradaki gün farkı yetersiz
                }
            }
            return true;
        }
    }
}
