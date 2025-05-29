using Fet_Deneme.Models;
using System.Collections.Generic;
using System.Linq;

namespace Fet_Deneme.Services
{
    public class ConstraintStudentsMaxHoursDailyChecker
    {
        private readonly List<ConstraintStudentsMaxHoursDaily> _constraints;
        private readonly List<Day> _days;

        public ConstraintStudentsMaxHoursDailyChecker(List<ConstraintStudentsMaxHoursDaily> constraints, List<Day> days)
        {
            _constraints = constraints;
            _days = days;
        }

        // assignments: mevcut yerleştirilmiş aktiviteler
        // activity: yeni eklenecek aktivite
        // day: eklenecek gün
        // hour: eklenecek saat
        // activities: tüm aktiviteler (id -> activity eşlemesi için)
        public bool IsValid(List<GeneratedAssignment> assignments, TimetableActivity activity, string day, Dictionary<int, TimetableActivity> activities, List<Activity> allActivities)
        {
            foreach (var constraint in _constraints)
            {
                foreach (var student in activity.Students)
                {
                    // O gün, o öğrenciye ait toplam ders süresi (Duration)
                    int currentDuration = 0;
                    foreach (var assign in assignments.Where(a => a.Day == day))
                    {
                        if (activities.ContainsKey(assign.ActivityId) && activities[assign.ActivityId].Students.Contains(student))
                        {
                            var act = allActivities.FirstOrDefault(x => x.Id == assign.ActivityId);
                            if (act != null)
                                currentDuration += act.Duration;
                        }
                    }
                    // Yeni eklenecek activity'nin süresini de ekle
                    var thisAct = allActivities.FirstOrDefault(x => x.Id == activity.Id);
                    if (thisAct != null)
                        currentDuration += thisAct.Duration;

                    if (currentDuration > constraint.MaximumHoursDaily)
                        return false;
                }
            }
            return true;
        }
    }
}
