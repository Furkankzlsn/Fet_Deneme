using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Fet_Deneme.Models
{
    [Serializable]
    [XmlRoot("Subgroup")]
    public class Subgroup
    {
        [XmlElement("Name")]
        public string? Name { get; set; }

        [XmlElement("Number_of_Students")]
        public int NumberOfStudents { get; set; }

        [XmlElement("Comments")]
        public string? Comments { get; set; }
    }

    [Serializable]
    [XmlRoot("Group")]
    public class Group
    {
        [XmlElement("Name")]
        public string? Name { get; set; }

        [XmlElement("Number_of_Students")]
        public int NumberOfStudents { get; set; }

        [XmlElement("Comments")]
        public string? Comments { get; set; }

        [XmlElement("Subgroup")]
        public List<Subgroup>? Subgroups { get; set; }
    }

    [Serializable]
    [XmlRoot("Year")]
    public class Year
    {
        [XmlElement("Name")]
        public string? Name { get; set; }

        [XmlElement("Number_of_Students")]
        public int NumberOfStudents { get; set; }

        [XmlElement("Comments")]
        public string? Comments { get; set; }

        [XmlElement("Number_of_Categories")]
        public int NumberOfCategories { get; set; }

        [XmlElement("Separator")]
        public string? Separator { get; set; }

        [XmlElement("Group")]
        public List<Group>? Groups { get; set; }
    }
}
