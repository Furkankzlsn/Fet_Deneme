using System;
using System.Xml.Serialization;

namespace Fet_Deneme.Models
{
    [Serializable]
    [XmlRoot("Subject")]
    public class Subject
    {
        [XmlElement("Name")]
        public string? Name { get; set; }

        [XmlElement("Comments")]
        public string? Comments { get; set; }

        [XmlElement("Long_Name")]
        public string? LongName { get; set; }

        [XmlElement("Code")]
        public string? Code { get; set; }
    }
}
