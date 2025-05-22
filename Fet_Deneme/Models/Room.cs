using System;
using System.Xml.Serialization;

namespace Fet_Deneme.Models
{
    [Serializable]
    [XmlRoot("Room")]
    public class Room
    {
        [XmlElement("Name")]
        public string? Name { get; set; }

        [XmlElement("Comments")]
        public string? Comments { get; set; }
    }
}
