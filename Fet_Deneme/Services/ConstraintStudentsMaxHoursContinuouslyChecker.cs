using Fet_Deneme.Models;
using System.Collections.Generic;
using System.Linq;

namespace Fet_Deneme.Services
{
    public class ConstraintStudentsMaxHoursContinuouslyChecker
    {
        private readonly List<ConstraintStudentsMaxHoursContinuously> _constraints;
        private readonly List<Hour> _hours;

        public ConstraintStudentsMaxHoursContinuouslyChecker(List<ConstraintStudentsMaxHoursContinuously> constraints, List<Hour> hours)
        {
            _constraints = constraints;
            _hours = hours;
        }

        // assignments: mevcut yerleştirilmiş aktiviteler
        // activity: yeni eklenecek aktivite
        // day: eklenecek gün
        // hour: eklenecek saat
        // activities: tüm aktiviteler (id -> activity eşlemesi için)
        public bool IsValid(List<GeneratedAssignment> assignments, TimetableActivity activity, string day, string hour, Dictionary<int, TimetableActivity> activities, List<Activity> allActivities)
        {
            foreach (var constraint in _constraints)
            {
                foreach (var student in activity.Students)
                {
                    // O gün, o öğrenciye ait tüm saatleri bul
                    var hoursForStudent = assignments
                        .Where(a => a.Day == day && activities.ContainsKey(a.ActivityId) && activities[a.ActivityId].Students.Contains(student))
                        .Select(a => a.Hour)
                        .ToList();
                    // Yeni eklenecek hour'u da ekle
                    hoursForStudent.Add(hour);
                    // Saatleri sırala (ör: H1, H2, H3...)
                    var sortedHours = hoursForStudent
                        .Select(h => _hours.FindIndex(x => x.Name == h))
                        .Where(idx => idx >= 0)
                        .OrderBy(idx => idx)
                        .ToList();
                    // Maksimum ardışık ders saatini bul
                    int maxStreak = 1, streak = 1;
                    for (int i = 1; i < sortedHours.Count; i++)
                    {
                        if (sortedHours[i] == sortedHours[i - 1] + 1)
                            streak++;
                        else
                            streak = 1;
                        if (streak > maxStreak)
                            maxStreak = streak;
                    }
                    if (maxStreak > constraint.MaximumHoursContinuously)
                        return false;
                }
            }
            return true;
        }
    }
}
