using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Fet_Deneme.Models
{
    [Serializable]
    [XmlRoot("fet")]
    public class FetRoot
    {
        [XmlElement("Mode")]
        public string? Mode { get; set; }

        [XmlElement("Institution_Name")]
        public string? InstitutionName { get; set; }

        [XmlElement("Comments")]
        public string? Comments { get; set; }

        [XmlArray("Days_List")]
        [XmlArrayItem("Day")]
        public List<Day>? Days { get; set; }

        [XmlArray("Hours_List")]
        [XmlArrayItem("Hour")]
        public List<Hour>? Hours { get; set; }

        [XmlArray("Subjects_List")]
        [XmlArrayItem("Subject")]
        public List<Subject>? Subjects { get; set; }

        [XmlArray("Teachers_List")]
        [XmlArrayItem("Teacher")]
        public List<Teacher>? Teachers { get; set; }

        [XmlArray("Students_List")]
        [XmlArrayItem("Year")]
        public List<Year>? Years { get; set; }

        [XmlArray("Rooms_List")]
        [XmlArrayItem("Room")]
        public List<Room>? Rooms { get; set; }

        [XmlArray("Activities_List")]
        [XmlArrayItem("Activity")]
        public List<Activity>? Activities { get; set; }
    }

    [Serializable]
    public class Day
    {
        [XmlElement("Name")]
        public string? Name { get; set; }
    }

    [Serializable]
    public class Hour
    {
        [XmlElement("Name")]
        public string? Name { get; set; }
    }

    // Constraint: Activity Preferred Time Slots
    [Serializable]
    public class ConstraintActivityPreferredTimeSlots
    {
        [XmlElement("Weight_Percentage")] public int WeightPercentage { get; set; } = 100;
        [XmlElement("Activity_Id")] public int ActivityId { get; set; }
        [XmlArray("Preferred_Starting_Times")]
        [XmlArrayItem("Preferred_Starting_Time")]
        public List<PreferredStartingTime>? PreferredStartingTimes { get; set; }
        [XmlElement("Active")] public bool Active { get; set; } = true;
        [XmlElement("Comments")] public string? Comments { get; set; }
    }
    [Serializable]
    public class PreferredStartingTime
    {
        [XmlElement("Day")] public string? Day { get; set; }
        [XmlElement("Hour")] public string? Hour { get; set; }
    }

    // Constraint: Students Max Hours Daily
    [Serializable]
    public class ConstraintStudentsMaxHoursDaily
    {
        [XmlElement("Weight_Percentage")] public int WeightPercentage { get; set; } = 100;
        [XmlElement("Maximum_Hours_Daily")] public int MaximumHoursDaily { get; set; }
        [XmlElement("Active")] public bool Active { get; set; } = true;
        [XmlElement("Comments")] public string? Comments { get; set; }
    }

    // Constraint: Students Max Hours Continuously
    [Serializable]
    public class ConstraintStudentsMaxHoursContinuously
    {
        [XmlElement("Weight_Percentage")] public int WeightPercentage { get; set; } = 100;
        [XmlElement("Maximum_Hours_Continuously")] public int MaximumHoursContinuously { get; set; }
        [XmlElement("Active")] public bool Active { get; set; } = true;
        [XmlElement("Comments")] public string? Comments { get; set; }
    }

    // Constraint: Min Days Between Activities
    [Serializable]
    public class ConstraintMinDaysBetweenActivities
    {
        [XmlElement("Weight_Percentage")] public int WeightPercentage { get; set; } = 100;
        [XmlElement("Consecutive_If_Same_Day")] public bool ConsecutiveIfSameDay { get; set; } = false;
        [XmlElement("Number_of_Activities")] public int NumberOfActivities { get; set; }
        [XmlElement("Activity_Id")]
        public List<int>? ActivityIds { get; set; }
        [XmlElement("MinDays")] public int MinDays { get; set; }
        [XmlElement("Active")] public bool Active { get; set; } = true;
        [XmlElement("Comments")] public string? Comments { get; set; }
    }

    // Constraint: Activities Not Overlapping
    [Serializable]
    public class ConstraintActivitiesNotOverlapping
    {
        [XmlElement("Weight_Percentage")] public int WeightPercentage { get; set; } = 100;
        [XmlElement("Number_of_Activities")] public int NumberOfActivities { get; set; }
        [XmlElement("Activity_Id")]
        public List<int>? ActivityIds { get; set; }
        [XmlElement("Active")] public bool Active { get; set; } = true;
        [XmlElement("Comments")] public string? Comments { get; set; }
    }

    // Constraint: Break Times
    [Serializable]
    public class ConstraintBreakTimes
    {
        [XmlElement("Weight_Percentage")] public int WeightPercentage { get; set; } = 100;
        [XmlElement("Number_of_Break_Times")] public int NumberOfBreakTimes { get; set; }
        [XmlElement("Break_Time")]
        public List<BreakTime>? BreakTimes { get; set; }
        [XmlElement("Active")] public bool Active { get; set; } = true;
        [XmlElement("Comments")] public string? Comments { get; set; }
    }
    [Serializable]
    public class BreakTime
    {
        [XmlElement("Day")] public string? Day { get; set; }
        [XmlElement("Hour")] public string? Hour { get; set; }
    }
}
