using Fet_Deneme.Models;
using System.Collections.Generic;
using System.Linq;

namespace Fet_Deneme.Services
{
    public class ConstraintMaxStudentsInSelectedTimeSlotChecker
    {
        private readonly List<ConstraintMaxStudentsInSelectedTimeSlot> _constraints;

        public ConstraintMaxStudentsInSelectedTimeSlotChecker(List<ConstraintMaxStudentsInSelectedTimeSlot> constraints)
        {
            _constraints = constraints;
        }

        // assignments: mevcut atamalar
        // activity: yeni eklenecek aktivite
        // day, hour: eklenecek slot
        // allActivities: id -> TimetableActivity
        public bool IsValid(TimetableActivity activity, string day, string hour, List<GeneratedAssignment> assignments, Dictionary<int, TimetableActivity> allActivities)
        {
            foreach (var constraint in _constraints)
            {
                if (!constraint.Active || constraint.SelectedTimeSlots == null || constraint.SelectedTimeSlots.Count == 0)
                    continue;
                // Seçili slotlar arasında bu gün+saat var mı?
                bool slotMatch = constraint.SelectedTimeSlots.Any(slot => slot.Day == day && slot.Hour == hour);
                if (!slotMatch)
                    continue;
                // Bu slotta atanmış tüm öğrencileri topla
                var studentsInSlot = new HashSet<string>();
                foreach (var assign in assignments.Where(a => a.Day == day && a.Hour == hour))
                {
                    if (allActivities.TryGetValue(assign.ActivityId, out var act))
                    {
                        foreach (var s in act.Students)
                            studentsInSlot.Add(s);
                    }
                }
                // Yeni eklenecek activity'nin öğrencilerini de ekle
                foreach (var s in activity.Students)
                    studentsInSlot.Add(s);
                if (studentsInSlot.Count > constraint.MaxStudents)
                    return false;
            }
            return true;
        }
    }
}
