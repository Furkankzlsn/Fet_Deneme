using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Fet_Deneme.Models
{
    [Serializable]
    [XmlRoot("Teacher")]
    public class Teacher
    {
        [XmlElement("Name")]
        public string? Name { get; set; }

        [XmlElement("Target_Number_of_Hours")]
        public int TargetNumberOfHours { get; set; }

        [XmlArray("Qualified_Subjects")]
        [XmlArrayItem("Qualified_Subject")]
        public List<string>? QualifiedSubjects { get; set; }

        [XmlElement("Comments")]
        public string? Comments { get; set; }
    }
}
