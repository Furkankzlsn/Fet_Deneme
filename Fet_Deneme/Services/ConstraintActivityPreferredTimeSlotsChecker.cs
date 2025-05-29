using Fet_Deneme.Models;
using System.Collections.Generic;
using System.Linq;

namespace Fet_Deneme.Services
{
    public class ConstraintActivityPreferredTimeSlotsChecker
    {
        private readonly List<ConstraintActivityPreferredTimeSlots> _constraints;

        public ConstraintActivityPreferredTimeSlotsChecker(List<ConstraintActivityPreferredTimeSlots> constraints)
        {
            _constraints = constraints;
        }

        // Belirli bir activity için, verilen gün-saat slotu tercih edilenler arasında mı?
        public bool IsAllowed(int activityId, string day, string hour)
        {
            var constraint = _constraints.FirstOrDefault(c => c.ActivityId == activityId && c.Active);
            if (constraint == null || constraint.PreferredTimeSlots == null || constraint.PreferredTimeSlots.Count == 0)
                return true; // Kısıt yoksa veya aktif değilse serbest
            return constraint.PreferredTimeSlots.Any(slot => slot.Day == day && slot.Hour == hour);
        }
    }
}
