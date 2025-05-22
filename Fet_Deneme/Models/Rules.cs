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
}
