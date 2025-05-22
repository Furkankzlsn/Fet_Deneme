using System;
using System.Xml.Serialization;

namespace Fet_Deneme.Models
{
    [Serializable]
    [XmlRoot("Activity")]
    public class Activity
    {
        [XmlElement("Teacher")]
        public string? Teacher { get; set; }

        [XmlElement("Subject")]
        public string? Subject { get; set; }

        [XmlElement("Students")]
        public string? Students { get; set; }

        [XmlElement("Duration")]
        public int Duration { get; set; }

        [XmlElement("Total_Duration")]
        public int TotalDuration { get; set; }

        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("Activity_Group_Id")]
        public int ActivityGroupId { get; set; }

        [XmlElement("Active")]
        public bool Active { get; set; }

        [XmlElement("Comments")]
        public string? Comments { get; set; }
    }
}
