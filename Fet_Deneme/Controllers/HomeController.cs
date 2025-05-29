using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Fet_Deneme.Models;
using ActivityModel = Fet_Deneme.Models.Activity;
using System.Text.Json;
using Fet_Deneme.Services;
using System.Xml.Serialization;
using ClosedXML.Excel; // dosyanın en üstüne ekle

namespace Fet_Deneme.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Simulate current project state (in-memory, for demo)
        private static string? CurrentFileName = null;
        private static string? CurrentXmlContent = null;
        private static string? CurrentFilePath = null;

        [HttpPost]
        public JsonResult NewProject()
        {
            // Varsayılan FET XML (dolu)
            var defaultXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<fet version=\"7.2.0\">\n  <Mode>Official</Mode>\n  <Institution_Name>Varsayılan Kurum</Institution_Name>\n  <Comments>Varsayılan Açıklamalar</Comments>\n  <Days_List>\n    <Number_of_Days>5</Number_of_Days>\n    <Day>\n      <Name>D1</Name>\n      <Long_Name>Pazartesi</Long_Name>\n    </Day>\n    <Day>\n      <Name>D2</Name>\n      <Long_Name>Salı</Long_Name>\n    </Day>\n    <Day>\n      <Name>D3</Name>\n      <Long_Name>Çarşamba</Long_Name>\n    </Day>\n    <Day>\n      <Name>D4</Name>\n      <Long_Name>Perşembe</Long_Name>\n    </Day>\n    <Day>\n      <Name>D5</Name>\n      <Long_Name>Cuma</Long_Name>\n    </Day>\n  </Days_List>\n  <Hours_List>\n    <Number_of_Hours>12</Number_of_Hours>\n    <Hour>\n      <Name>H1</Name>\n      <Long_Name>08:00</Long_Name>\n    </Hour>\n    <Hour>\n      <Name>H2</Name>\n      <Long_Name>09:00</Long_Name>\n    </Hour>\n    <Hour>\n      <Name>H3</Name>\n      <Long_Name>10:00</Long_Name>\n    </Hour>\n    <Hour>\n      <Name>H4</Name>\n      <Long_Name>11:00</Long_Name>\n    </Hour>\n    <Hour>\n      <Name>H5</Name>\n      <Long_Name>12:00</Long_Name>\n    </Hour>\n    <Hour>\n      <Name>H6</Name>\n      <Long_Name>13:00</Long_Name>\n    </Hour>\n    <Hour>\n      <Name>H7</Name>\n      <Long_Name>14:00</Long_Name>\n    </Hour>\n    <Hour>\n      <Name>H8</Name>\n      <Long_Name>15:00</Long_Name>\n    </Hour>\n    <Hour>\n      <Name>H9</Name>\n      <Long_Name>16:00</Long_Name>\n    </Hour>\n    <Hour>\n      <Name>H10</Name>\n      <Long_Name>17:00</Long_Name>\n    </Hour>\n    <Hour>\n      <Name>H11</Name>\n      <Long_Name>18:00</Long_Name>\n    </Hour>\n    <Hour>\n      <Name>H12</Name>\n      <Long_Name>19:00</Long_Name>\n    </Hour>\n  </Hours_List>\n  <Subjects_List>\n  </Subjects_List>\n  <Activity_Tags_List>\n  </Activity_Tags_List>\n  <Teachers_List>\n  </Teachers_List>\n  <Students_List>\n  </Students_List>\n  <Activities_List>\n  </Activities_List>\n  <Buildings_List>\n  </Buildings_List>\n  <Rooms_List>\n  </Rooms_List>\n  <Time_Constraints_List>\n    <ConstraintBasicCompulsoryTime>\n      <Weight_Percentage>100</Weight_Percentage>\n      <Active>true</Active>\n      <Comments></Comments>\n    </ConstraintBasicCompulsoryTime>\n  </Time_Constraints_List>\n  <Space_Constraints_List>\n    <ConstraintBasicCompulsorySpace>\n      <Weight_Percentage>100</Weight_Percentage>\n      <Active>true</Active>\n      <Comments></Comments>\n    </ConstraintBasicCompulsorySpace>\n  </Space_Constraints_List>\n  <Timetable_Generation_Options_List>\n  </Timetable_Generation_Options_List>\n</fet>";
            CurrentFileName = "untitled.fet";
            CurrentXmlContent = defaultXml;
            var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);
            CurrentFilePath = Path.Combine(appDataPath, CurrentFileName);
            return Json(new { success = true, fileName = CurrentFileName, xmlContent = defaultXml });
        }

        [HttpPost]
        public JsonResult OpenProject([FromBody] string xmlContent, [FromQuery] string fileName, [FromQuery] string? filePath = null)
        {
            // Set current project from uploaded XML
            CurrentFileName = fileName;
            CurrentXmlContent = xmlContent;
            if (!string.IsNullOrEmpty(filePath))
            {
                CurrentFilePath = filePath;
            }
            else
            {
                var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
                if (!Directory.Exists(appDataPath))
                    Directory.CreateDirectory(appDataPath);
                CurrentFilePath = Path.Combine(appDataPath, CurrentFileName);
            }
            return Json(new { success = true, fileName = CurrentFileName });
        }

        [HttpPost]
        public JsonResult SaveProject()
        {
            if (string.IsNullOrEmpty(CurrentFileName) || string.IsNullOrEmpty(CurrentXmlContent))
                return Json(new { success = false, message = "No project loaded." });
            if (string.IsNullOrEmpty(CurrentFilePath))
            {
                var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
                if (!Directory.Exists(appDataPath))
                    Directory.CreateDirectory(appDataPath);
                CurrentFilePath = Path.Combine(appDataPath, CurrentFileName);
            }
            System.IO.File.WriteAllText(CurrentFilePath, CurrentXmlContent);
            return Json(new { success = true, fileName = CurrentFileName, path = CurrentFilePath });
        }

        [HttpPost]
        public JsonResult SaveProjectAs([FromBody] SaveAsRequest req)
        {
            if (string.IsNullOrEmpty(req.XmlContent) || string.IsNullOrEmpty(req.FilePath))
                return Json(new { success = false, message = "Invalid parameters." });
            try
            {
                System.IO.File.WriteAllText(req.FilePath, req.XmlContent);
                CurrentFileName = Path.GetFileName(req.FilePath);
                CurrentXmlContent = req.XmlContent;
                CurrentFilePath = req.FilePath;
                return Json(new { success = true, fileName = CurrentFileName, path = req.FilePath });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class SaveAsRequest
        {
            public string? XmlContent { get; set; }
            public string? FilePath { get; set; }
        }

        public class ImportSubjectsCsvRequest
        {
            public string? CsvContent { get; set; }
            public string? FileName { get; set; }
        }

        [HttpPost]
        public JsonResult ImportSubjectsCsv([FromBody] ImportSubjectsCsvRequest req)
        {
            if (string.IsNullOrEmpty(req.CsvContent))
                return Json(new { success = false, message = "CSV içeriği boş." });
            if (string.IsNullOrEmpty(CurrentXmlContent))
                return Json(new { success = false, message = "Önce bir proje açın veya oluşturun." });

            try
            {
                // 1. CSV'yi satır satır parse et
                var lines = req.CsvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0)
                    return Json(new { success = false, message = "CSV'de ders bulunamadı." });

                // 2. Başlık satırı otomatik algıla ve mapping yap
                var header = lines[0].Trim().ToLower();
                int startIdx = 0;
                int nameIdx = 0, commentsIdx = -1;
                var headerParts = lines[0].Split(',');
                if (header.Contains("subject") || header.Contains("name"))
                {
                    for (int i = 0; i < headerParts.Length; i++)
                    {
                        var h = headerParts[i].Trim().ToLower();
                        if (h == "subject" || h == "name") nameIdx = i;
                        if (h == "comments" || h == "comment") commentsIdx = i;
                    }
                    startIdx = 1; // başlık satırı var
                }

                // 3. Mevcut XML'i yükle
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(CurrentXmlContent);
                var subjectsList = doc.SelectSingleNode("/fet/Subjects_List");
                if (subjectsList == null)
                    return Json(new { success = false, message = "XML'de Subjects_List bulunamadı." });
                subjectsList.RemoveAll();

                // 4. Var olan subject adlarını set olarak tut (tekrar eklememek için)
                var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // 5. Her subject için FET algoritmasına uygun ekle
                int added = 0;
                for (int i = startIdx; i < lines.Length; i++)
                {
                    var parts = lines[i].Split(',');
                    if (parts.Length <= nameIdx) continue;
                    var name = parts[nameIdx].Trim();
                    if (string.IsNullOrEmpty(name)) continue;
                    if (existingNames.Contains(name)) continue;
                    existingNames.Add(name);

                    var subjectNode = doc.CreateElement("Subject");
                    var nameNode = doc.CreateElement("Name");
                    nameNode.InnerText = name;
                    subjectNode.AppendChild(nameNode);

                    var longNameNode = doc.CreateElement("Long_Name");
                    longNameNode.InnerText = string.Empty;
                    subjectNode.AppendChild(longNameNode);

                    var codeNode = doc.CreateElement("Code");
                    codeNode.InnerText = string.Empty;
                    subjectNode.AppendChild(codeNode);

                    var commentsNode = doc.CreateElement("Comments");
                    if (commentsIdx >= 0 && parts.Length > commentsIdx)
                        commentsNode.InnerText = parts[commentsIdx].Trim();
                    else
                        commentsNode.InnerText = string.Empty;
                    subjectNode.AppendChild(commentsNode);

                    subjectNode.AppendChild(longNameNode);
                    subjectNode.AppendChild(codeNode);
                    subjectNode.AppendChild(commentsNode);
                    subjectsList.AppendChild(subjectNode);
                    added++;
                }

                // 6. XML'i güncelle ve dosyaya yaz
                using (var sw = new System.IO.StringWriter())
                using (var xw = new System.Xml.XmlTextWriter(sw))
                {
                    xw.Formatting = System.Xml.Formatting.Indented;
                    doc.WriteTo(xw);
                    xw.Flush();
                    // Sadece boş elemanlar için gereksiz satır sonlarını temizle
                    var xml = sw.ToString();
                    // Sadece <Tag>\n</Tag> => <Tag></Tag> olanları değiştir
                    xml = System.Text.RegularExpressions.Regex.Replace(xml, @"<([a-zA-Z0-9_]+)>\s*</\\1>", "<$1></$1>");
                    CurrentXmlContent = xml;
                }
                if (!string.IsNullOrEmpty(CurrentFilePath))
                {
                    System.IO.File.WriteAllText(CurrentFilePath, CurrentXmlContent);
                }

                return Json(new { success = true, xmlContent = CurrentXmlContent, added });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class ImportTeachersCsvRequest
        {
            public string? CsvContent { get; set; }
            public string? FileName { get; set; }
        }

        [HttpPost]
        public JsonResult ImportTeachersCsv([FromBody] ImportTeachersCsvRequest req)
        {
            if (string.IsNullOrEmpty(req.CsvContent))
                return Json(new { success = false, message = "CSV içeriği boş." });
            if (string.IsNullOrEmpty(CurrentXmlContent))
                return Json(new { success = false, message = "Önce bir proje açın veya oluşturun." });

            try
            {
                var lines = req.CsvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0)
                    return Json(new { success = false, message = "CSV'de öğretmen bulunamadı." });

                // Başlık satırı algıla ve mapping yap
                var header = lines[0].Trim().ToLower();
                int startIdx = 0;
                int nameIdx = 0, commentsIdx = -1;
                var headerParts = lines[0].Split(',');
                if (header.Contains("teacher") || header.Contains("name"))
                {
                    for (int i = 0; i < headerParts.Length; i++)
                    {
                        var h = headerParts[i].Trim().ToLower();
                        if (h == "teacher" || h == "name") nameIdx = i;
                        if (h == "comments" || h == "comment") commentsIdx = i;
                    }
                    startIdx = 1;
                }

                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(CurrentXmlContent);
                var teachersList = doc.SelectSingleNode("/fet/Teachers_List");
                if (teachersList == null)
                    return Json(new { success = false, message = "XML'de Teachers_List bulunamadı." });
                teachersList.RemoveAll();

                var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                int added = 0;
                for (int i = startIdx; i < lines.Length; i++)
                {
                    var parts = lines[i].Split(',');
                    if (parts.Length <= nameIdx) continue;
                    var name = parts[nameIdx].Trim();
                    if (string.IsNullOrEmpty(name)) continue;
                    if (existingNames.Contains(name)) continue;
                    existingNames.Add(name);

                    var teacherNode = doc.CreateElement("Teacher");
                    var nameNode = doc.CreateElement("Name");
                    nameNode.InnerText = name;
                    teacherNode.AppendChild(nameNode);

                    var targetHoursNode = doc.CreateElement("Target_Number_of_Hours");
                    targetHoursNode.InnerText = "0"; // FET örneklerinde hep 0
                    teacherNode.AppendChild(targetHoursNode);

                    var qualifiedSubjectsNode = doc.CreateElement("Qualified_Subjects");
                    qualifiedSubjectsNode.IsEmpty = false; // force multi-line
                    teacherNode.AppendChild(qualifiedSubjectsNode);

                    var commentsNode = doc.CreateElement("Comments");
                    if (commentsIdx >= 0 && parts.Length > commentsIdx && !string.IsNullOrWhiteSpace(parts[commentsIdx]))
                        commentsNode.InnerText = parts[commentsIdx].Trim();
                    else
                        commentsNode.InnerText = name; // FET örneklerinde default olarak Name kullanılıyor
                    teacherNode.AppendChild(commentsNode);

                    teachersList.AppendChild(teacherNode);
                    added++;
                }

                using (var sw = new System.IO.StringWriter())
                using (var xw = new System.Xml.XmlTextWriter(sw))
                {
                    xw.Formatting = System.Xml.Formatting.Indented;
                    doc.WriteTo(xw);
                    xw.Flush();
                    var xml = sw.ToString();
                    CurrentXmlContent = xml;
                }
                if (!string.IsNullOrEmpty(CurrentFilePath))
                {
                    System.IO.File.WriteAllText(CurrentFilePath, CurrentXmlContent);
                }

                return Json(new { success = true, xmlContent = CurrentXmlContent, added });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class ImportStudentsCsvRequest
        {
            public string? CsvContent { get; set; }
            public string? FileName { get; set; }
        }

        [HttpPost]
        public JsonResult ImportStudentsCsv([FromBody] ImportStudentsCsvRequest req)
        {
            if (string.IsNullOrEmpty(req.CsvContent))
                return Json(new { success = false, message = "CSV içeriği boş." });
            if (string.IsNullOrEmpty(CurrentXmlContent))
                return Json(new { success = false, message = "Önce bir proje açın veya oluşturun." });

            try
            {
                var lines = req.CsvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0)
                    return Json(new { success = false, message = "CSV'de öğrenci bulunamadı." });

                // Başlık satırı algıla ve mapping yap
                var header = lines[0].Trim().ToLower();
                int startIdx = 0;
                int yearIdx = 0, groupIdx = -1, subgroupIdx = -1, yearNumIdx = -1, groupNumIdx = -1, subgroupNumIdx = -1, commentsIdx = -1;
                var headerParts = lines[0].Split(',');
                if (header.Contains("year") || header.Contains("group") || header.Contains("subgroup"))
                {
                    for (int i = 0; i < headerParts.Length; i++)
                    {
                        var h = headerParts[i].Trim().ToLower();
                        if (h == "year") yearIdx = i;
                        if (h == "group") groupIdx = i;
                        if (h == "subgroup") subgroupIdx = i;
                        if (h == "number_of_students" || h == "numberofstudents")
                        {
                            if (groupIdx >= 0 && groupNumIdx == -1) groupNumIdx = i;
                            else if (subgroupIdx >= 0 && subgroupNumIdx == -1) subgroupNumIdx = i;
                            else yearNumIdx = i;
                        }
                        if (h == "comments" || h == "comment") commentsIdx = i;
                    }
                    startIdx = 1; // başlık satırı var
                }

                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(CurrentXmlContent);
                var studentsList = doc.SelectSingleNode("/fet/Students_List");
                if (studentsList == null)
                    return Json(new { success = false, message = "XML'de Students_List bulunamadı." });
                studentsList.RemoveAll();

                // Hiyerarşi: Year > Group > Subgroup
                var years = new Dictionary<string, System.Xml.XmlElement>(StringComparer.OrdinalIgnoreCase);
                var groupsInYear = new List<(string year, string group, System.Xml.XmlElement)>();
                var subgroupsInGroup = new List<(string year, string group, string subgroup)>();
                int added = 0;
                for (int i = startIdx; i < lines.Length; i++)
                {
                    var parts = lines[i].Split(',');
                    if (parts.Length <= yearIdx) continue;
                    var year = parts[yearIdx].Trim();
                    if (string.IsNullOrEmpty(year)) continue;
                    var group = groupIdx >= 0 && parts.Length > groupIdx ? parts[groupIdx].Trim() : null;
                    var subgroup = subgroupIdx >= 0 && parts.Length > subgroupIdx ? parts[subgroupIdx].Trim() : null;
                    var yearNum = yearNumIdx >= 0 && parts.Length > yearNumIdx ? parts[yearNumIdx].Trim() : "0";
                    var groupNum = groupNumIdx >= 0 && parts.Length > groupNumIdx ? parts[groupNumIdx].Trim() : "0";
                    var subgroupNum = subgroupNumIdx >= 0 && parts.Length > subgroupNumIdx ? parts[subgroupNumIdx].Trim() : "0";
                    var comments = commentsIdx >= 0 && parts.Length > commentsIdx ? parts[commentsIdx].Trim() : string.Empty;

                    // Year ekle
                    if (!years.ContainsKey(year))
                    {
                        var yearNode = doc.CreateElement("Year");
                        var nameNode = doc.CreateElement("Name");
                        nameNode.InnerText = year;
                        yearNode.AppendChild(nameNode);
                        var numNode = doc.CreateElement("Number_of_Students");
                        numNode.InnerText = yearNum;
                        yearNode.AppendChild(numNode);
                        var commentsNode = doc.CreateElement("Comments");
                        commentsNode.InnerText = comments;
                        yearNode.AppendChild(commentsNode);
                        var numCatNode = doc.CreateElement("Number_of_Categories");
                        numCatNode.InnerText = "0";
                        yearNode.AppendChild(numCatNode);
                        var sepNode = doc.CreateElement("Separator");
                        sepNode.InnerText = string.Empty;
                        yearNode.AppendChild(sepNode);
                        studentsList.AppendChild(yearNode);
                        years[year] = yearNode;
                        added++;
                    }
                    var yearElem = years[year];

                    // Group ekle
                    if (!string.IsNullOrEmpty(group))
                    {
                        var groupKey = (year.ToLowerInvariant(), group.ToLowerInvariant());
                        if (!groupsInYear.Exists(x => x.year.ToLowerInvariant() == groupKey.Item1 && x.group.ToLowerInvariant() == groupKey.Item2))
                        {
                            var groupNode = doc.CreateElement("Group");
                            var nameNode = doc.CreateElement("Name");
                            nameNode.InnerText = group;
                            groupNode.AppendChild(nameNode);
                            var numNode = doc.CreateElement("Number_of_Students");
                            numNode.InnerText = groupNum;
                            groupNode.AppendChild(numNode);
                            var commentsNode = doc.CreateElement("Comments");
                            commentsNode.InnerText = comments;
                            groupNode.AppendChild(commentsNode);
                            yearElem.AppendChild(groupNode);
                            groupsInYear.Add((year, group, groupNode));
                            added++;
                        }
                        var groupElem = groupsInYear.Find(x => x.year.ToLowerInvariant() == groupKey.Item1 && x.group.ToLowerInvariant() == groupKey.Item2).Item3;

                        // Subgroup ekle
                        if (!string.IsNullOrEmpty(subgroup))
                        {
                            var subKey = (year.ToLowerInvariant(), group.ToLowerInvariant(), subgroup.ToLowerInvariant());
                            if (!subgroupsInGroup.Exists(x => x.Item1 == subKey.Item1 && x.Item2 == subKey.Item2 && x.Item3 == subKey.Item3))
                            {
                                var subNode = doc.CreateElement("Subgroup");
                                var nameNode = doc.CreateElement("Name");
                                nameNode.InnerText = subgroup;
                                subNode.AppendChild(nameNode);
                                var numNode = doc.CreateElement("Number_of_Students");
                                numNode.InnerText = subgroupNum;
                                subNode.AppendChild(numNode);
                                var commentsNode = doc.CreateElement("Comments");
                                commentsNode.InnerText = "";
                                subNode.AppendChild(commentsNode);
                                groupElem.AppendChild(subNode);
                                subgroupsInGroup.Add((subKey.Item1, subKey.Item2, subKey.Item3));
                                added++;
                            }
                        }
                    }
                }

                using (var sw = new System.IO.StringWriter())
                using (var xw = new System.Xml.XmlTextWriter(sw))
                {
                    xw.Formatting = System.Xml.Formatting.Indented;
                    doc.WriteTo(xw);
                    xw.Flush();
                    var xml = sw.ToString();
                    CurrentXmlContent = xml;
                }
                if (!string.IsNullOrEmpty(CurrentFilePath))
                {
                    System.IO.File.WriteAllText(CurrentFilePath, CurrentXmlContent);
                }

                return Json(new { success = true, xmlContent = CurrentXmlContent, added });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class ImportActivitiesCsvRequest
        {
            public string? CsvContent { get; set; }
            public string? FileName { get; set; }
        }

        [HttpPost]
        public JsonResult ImportActivitiesCsv([FromBody] ImportActivitiesCsvRequest req)
        {
            if (string.IsNullOrEmpty(req.CsvContent))
                return Json(new { success = false, message = "CSV içeriği boş." });
            if (string.IsNullOrEmpty(CurrentXmlContent))
                return Json(new { success = false, message = "Önce bir proje açın veya oluşturun." });

            try
            {
                var lines = req.CsvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0)
                    return Json(new { success = false, message = "CSV'de etkinlik bulunamadı." });

                // Başlık satırı algıla ve mapping yap
                var header = lines[0].Trim().ToLower();
                int startIdx = 0;
                int studentsIdx = -1, subjectIdx = -1, teachersIdx = -1, tagsIdx = -1, totalDurationIdx = -1, splitDurationIdx = -1, minDaysIdx = -1, minDaysWeightIdx = -1, minDaysConsecutiveIdx = -1, commentsIdx = -1;
                var headerParts = lines[0].Split(',');
                for (int i = 0; i < headerParts.Length; i++)
                {
                    var h = headerParts[i].Trim().ToLower();
                    if (h.Contains("students")) studentsIdx = i;
                    if (h.Contains("subject")) subjectIdx = i;
                    if (h.Contains("teacher")) teachersIdx = i;
                    if (h.Contains("tag")) tagsIdx = i;
                    if (h.Contains("total duration")) totalDurationIdx = i;
                    if (h.Contains("split duration")) splitDurationIdx = i;
                    if (h.Contains("min days")) minDaysIdx = i;
                    if (h.Contains("weight")) minDaysWeightIdx = i;
                    if (h.Contains("consecutive")) minDaysConsecutiveIdx = i;
                    if (h.Contains("comment")) commentsIdx = i;
                }
                if (studentsIdx >= 0 && subjectIdx >= 0 && teachersIdx >= 0 && totalDurationIdx >= 0)
                    startIdx = 1;
                else
                    return Json(new { success = false, message = "CSV başlığı eksik veya hatalı (Students, Subject, Teachers, Total Duration zorunlu)." });

                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(CurrentXmlContent);
                var activitiesList = doc.SelectSingleNode("/fet/Activities_List");
                if (activitiesList == null)
                    return Json(new { success = false, message = "XML'de Activities_List bulunamadı." });
                activitiesList.RemoveAll();

                int added = 0;
                int activityId = 1;
                for (int i = startIdx; i < lines.Length; i++)
                {
                    var parts = lines[i].Split(',');
                    if (parts.Length <= Math.Max(Math.Max(studentsIdx, subjectIdx), teachersIdx)) continue;
                    var students = parts[studentsIdx].Trim();
                    var subject = parts[subjectIdx].Trim();
                    var teachers = parts[teachersIdx].Trim();
                    var tags = tagsIdx >= 0 && parts.Length > tagsIdx ? parts[tagsIdx].Trim() : string.Empty;
                    var totalDuration = totalDurationIdx >= 0 && parts.Length > totalDurationIdx ? parts[totalDurationIdx].Trim() : "1";
                    var splitDuration = splitDurationIdx >= 0 && parts.Length > splitDurationIdx ? parts[splitDurationIdx].Trim() : totalDuration;
                    var minDays = minDaysIdx >= 0 && parts.Length > minDaysIdx ? parts[minDaysIdx].Trim() : "1";
                    var minDaysWeight = minDaysWeightIdx >= 0 && parts.Length > minDaysWeightIdx ? parts[minDaysWeightIdx].Trim() : "95";
                    var minDaysConsecutive = minDaysConsecutiveIdx >= 0 && parts.Length > minDaysConsecutiveIdx ? parts[minDaysConsecutiveIdx].Trim() : "no";
                    var comments = commentsIdx >= 0 && parts.Length > commentsIdx ? parts[commentsIdx].Trim() : string.Empty;

                    // FET: Her activity için bir Activity node'u oluştur
                    var activityNode = doc.CreateElement("Activity");

                    // Öğretmenleri doğrudan Activity altına ekle
                    foreach (var tGroup in teachers.Split('|'))
                    {
                        foreach (var t in tGroup.Split('+'))
                        {
                            if (!string.IsNullOrWhiteSpace(t))
                            {
                                var teacherNode = doc.CreateElement("Teacher");
                                teacherNode.InnerText = t.Trim();
                                activityNode.AppendChild(teacherNode);
                            }
                        }
                    }

                    var subjectNode = doc.CreateElement("Subject");
                    subjectNode.InnerText = subject;
                    activityNode.AppendChild(subjectNode);

                    // Öğrencileri doğrudan Activity altına ekle
                    foreach (var sGroup in students.Split('|'))
                    {
                        foreach (var s in sGroup.Split('+'))
                        {
                            if (!string.IsNullOrWhiteSpace(s))
                            {
                                var studentNode = doc.CreateElement("Students");
                                studentNode.InnerText = s.Trim();
                                activityNode.AppendChild(studentNode);
                            }
                        }
                    }

                    // <Duration>
                    var durationNode = doc.CreateElement("Duration");
                    durationNode.InnerText = "1"; // veya splitDuration'dan ilki, FET örneklerinde genellikle Total_Duration ile aynı
                    activityNode.AppendChild(durationNode);

                    // <Total_Duration>
                    var totalDurationNode = doc.CreateElement("Total_Duration");
                    totalDurationNode.InnerText = "1";
                    activityNode.AppendChild(totalDurationNode);

                    // <Id>
                    var idNode = doc.CreateElement("Id");
                    idNode.InnerText = activityId.ToString();
                    activityNode.AppendChild(idNode);

                    // <Activity_Group_Id>
                    var groupIdNode = doc.CreateElement("Activity_Group_Id");
                    groupIdNode.InnerText = "0";
                    activityNode.AppendChild(groupIdNode);

                    // <Active>
                    var activeNode = doc.CreateElement("Active");
                    activeNode.InnerText = "true";
                    activityNode.AppendChild(activeNode);

                    // <Comments>
                    var commentsNode = doc.CreateElement("Comments");
                    commentsNode.InnerText = "";
                    activityNode.AppendChild(commentsNode);

                    activitiesList.AppendChild(activityNode);
                    added++;
                    activityId++;
                }

                using (var sw = new System.IO.StringWriter())
                using (var xw = new System.Xml.XmlTextWriter(sw))
                {
                    xw.Formatting = System.Xml.Formatting.Indented;
                    doc.WriteTo(xw);
                    xw.Flush();
                    var xml = sw.ToString();
                    CurrentXmlContent = xml;
                }
                if (!string.IsNullOrEmpty(CurrentFilePath))
                {
                    System.IO.File.WriteAllText(CurrentFilePath, CurrentXmlContent);
                }

                return Json(new { success = true, xmlContent = CurrentXmlContent, added });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }        [HttpGet]
        public JsonResult ShowData(string dataType)
        {
            if (string.IsNullOrEmpty(CurrentXmlContent))
                return Json(new { success = false, message = "Aktif proje bulunamadı" });
                
            try
            {
                var xml = new System.Xml.XmlDocument();
                xml.LoadXml(CurrentXmlContent);
                
                List<object> dataList = new List<object>();
                
                switch(dataType.ToLower())
                {
                    case "subjects":
                        var subjectNodes = xml.SelectNodes("//fet/Subjects_List/Subject");
                        if (subjectNodes != null)
                        {
                            foreach (System.Xml.XmlNode node in subjectNodes)
                            {
                                var name = node.SelectSingleNode("Name")?.InnerText;
                                if (!string.IsNullOrEmpty(name))
                                {
                                    dataList.Add(new Subject {
                                        Name = name,
                                        Comments = node.SelectSingleNode("Comments")?.InnerText,
                                        LongName = node.SelectSingleNode("Long_Name")?.InnerText,
                                        Code = node.SelectSingleNode("Code")?.InnerText
                                    });
                                }
                            }
                        }
                        break;
                        
                    case "teachers":
                        var teacherNodes = xml.SelectNodes("//fet/Teachers_List/Teacher");
                        if (teacherNodes != null)
                        {
                            foreach (System.Xml.XmlNode node in teacherNodes)
                            {
                                var name = node.SelectSingleNode("Name")?.InnerText;
                                if (!string.IsNullOrEmpty(name))
                                {
                                    var teacher = new Teacher {
                                        Name = name,
                                        Comments = node.SelectSingleNode("Comments")?.InnerText,
                                        TargetNumberOfHours = int.TryParse(node.SelectSingleNode("Target_Number_of_Hours")?.InnerText, out int hours) ? hours : 0,
                                        QualifiedSubjects = new List<string>()
                                    };
                                    
                                    var qualifiedNodes = node.SelectNodes("Qualified_Subjects/Qualified_Subject");
                                    if (qualifiedNodes != null)
                                    {
                                        foreach (System.Xml.XmlNode qualNode in qualifiedNodes)
                                        {
                                            if (!string.IsNullOrEmpty(qualNode.InnerText))
                                                teacher.QualifiedSubjects.Add(qualNode.InnerText);
                                        }
                                    }
                                    
                                    dataList.Add(teacher);
                                }
                            }
                        }
                        break;
                        
                    case "students":
                        var yearNodes = xml.SelectNodes("//fet/Students_List/Year");
                        if (yearNodes != null)
                        {
                            foreach (System.Xml.XmlNode yearNode in yearNodes)
                            {
                                var yearName = yearNode.SelectSingleNode("Name")?.InnerText;
                                if (!string.IsNullOrEmpty(yearName))
                                {
                                    var year = new Year {
                                        Name = yearName,
                                        Comments = yearNode.SelectSingleNode("Comments")?.InnerText,
                                        NumberOfStudents = int.TryParse(yearNode.SelectSingleNode("Number_of_Students")?.InnerText, out int students) ? students : 0,
                                        NumberOfCategories = int.TryParse(yearNode.SelectSingleNode("Number_of_Categories")?.InnerText, out int cats) ? cats : 0,
                                        Separator = yearNode.SelectSingleNode("Separator")?.InnerText,
                                        Groups = new List<Group>()
                                    };
                                    
                                    var groupNodes = yearNode.SelectNodes("Group");
                                    if (groupNodes != null)
                                    {
                                        foreach (System.Xml.XmlNode groupNode in groupNodes)
                                        {
                                            var groupName = groupNode.SelectSingleNode("Name")?.InnerText;
                                            if (!string.IsNullOrEmpty(groupName))
                                            {
                                                var group = new Group {
                                                    Name = groupName,
                                                    Comments = groupNode.SelectSingleNode("Comments")?.InnerText,
                                                    NumberOfStudents = int.TryParse(groupNode.SelectSingleNode("Number_of_Students")?.InnerText, out int groupStudents) ? groupStudents : 0,
                                                    Subgroups = new List<Subgroup>()
                                                };
                                                
                                                var subgroupNodes = groupNode.SelectNodes("Subgroup");
                                                if (subgroupNodes != null)
                                                {
                                                    foreach (System.Xml.XmlNode subgroupNode in subgroupNodes)
                                                    {
                                                        var subgroupName = subgroupNode.SelectSingleNode("Name")?.InnerText;
                                                        if (!string.IsNullOrEmpty(subgroupName))
                                                        {
                                                            group.Subgroups.Add(new Subgroup {
                                                                Name = subgroupName,
                                                                Comments = subgroupNode.SelectSingleNode("Comments")?.InnerText,
                                                                NumberOfStudents = int.TryParse(subgroupNode.SelectSingleNode("Number_of_Students")?.InnerText, out int subgroupStudents) ? subgroupStudents : 0
                                                            });
                                                        }
                                                    }
                                                }
                                                
                                                year.Groups.Add(group);
                                            }
                                        }
                                    }
                                    
                                    dataList.Add(year);
                                }
                            }
                        }
                        break;
                        
                    case "activities":
                        var activityNodes = xml.SelectNodes("//fet/Activities_List/Activity");
                        // Build a mapping of all student sets (years, groups, subgroups) to their student counts
                        var studentCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        var yearNodesAll = xml.SelectNodes("//fet/Students_List/Year");
                        if (yearNodesAll != null)
                        {
                            foreach (System.Xml.XmlNode yearNode in yearNodesAll)
                            {
                                var yearName = yearNode.SelectSingleNode("Name")?.InnerText?.Trim();
                                int yearCount = int.TryParse(yearNode.SelectSingleNode("Number_of_Students")?.InnerText, out int y) ? y : 0;
                                if (!string.IsNullOrEmpty(yearName)) studentCounts[yearName] = yearCount;
                                var groupNodes = yearNode.SelectNodes("Group");
                                if (groupNodes != null)
                                {
                                    foreach (System.Xml.XmlNode groupNode in groupNodes)
                                    {
                                        var groupName = groupNode.SelectSingleNode("Name")?.InnerText?.Trim();
                                        int groupCount = int.TryParse(groupNode.SelectSingleNode("Number_of_Students")?.InnerText, out int g) ? g : 0;
                                        if (!string.IsNullOrEmpty(groupName)) studentCounts[groupName] = groupCount;
                                        var subgroupNodes = groupNode.SelectNodes("Subgroup");
                                        if (subgroupNodes != null)
                                        {
                                            foreach (System.Xml.XmlNode subgroupNode in subgroupNodes)
                                            {
                                                var subgroupName = subgroupNode.SelectSingleNode("Name")?.InnerText?.Trim();
                                                int subgroupCount = int.TryParse(subgroupNode.SelectSingleNode("Number_of_Students")?.InnerText, out int s) ? s : 0;
                                                if (!string.IsNullOrEmpty(subgroupName)) studentCounts[subgroupName] = subgroupCount;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (activityNodes != null)
                        {
                            foreach (System.Xml.XmlNode node in activityNodes)
                            {
                                // FET format: multiple <Students> tags per activity
                                int totalCount = 0;
                                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // Avoid double-counting
                                var studentNodes = node.SelectNodes("Students");
                                string studentsRaw = "";
                                int studentSetCount = 0;
                                if (studentNodes != null)
                                {
                                    foreach (System.Xml.XmlNode stNode in studentNodes)
                                    {
                                        var stName = stNode.InnerText?.Trim();
                                        if (!string.IsNullOrEmpty(stName))
                                        {
                                            studentsRaw += (studentsRaw.Length > 0 ? ", " : "") + stName;
                                            if (!seen.Contains(stName))
                                            {
                                                seen.Add(stName);
                                                if (studentCounts.TryGetValue(stName, out int cnt))
                                                    totalCount += cnt;
                                                studentSetCount++;
                                            }
                                        }
                                    }
                                }
                                // Eğer toplam öğrenci sayısı 0 ise, öğrenci seti adedini göster
                                int displayCount = totalCount > 0 ? totalCount : studentSetCount;
                                dataList.Add(new {
                                    Teacher = node.SelectSingleNode("Teacher")?.InnerText,
                                    Subject = node.SelectSingleNode("Subject")?.InnerText,
                                    Students = studentsRaw,
                                    StudentCount = displayCount,
                                    Duration = int.TryParse(node.SelectSingleNode("Duration")?.InnerText, out int duration) ? duration : 0,
                                    TotalDuration = int.TryParse(node.SelectSingleNode("Total_Duration")?.InnerText, out int totalDuration) ? totalDuration : 0,
                                    Id = int.TryParse(node.SelectSingleNode("Id")?.InnerText, out int id) ? id : 0,
                                    ActivityGroupId = int.TryParse(node.SelectSingleNode("Activity_Group_Id")?.InnerText, out int groupId) ? groupId : 0,
                                    Active = node.SelectSingleNode("Active")?.InnerText?.ToLower() == "true",
                                    Comments = node.SelectSingleNode("Comments")?.InnerText
                                });
                            }
                        }
                        break;
                        
                    case "days":
    var dayNodes = xml.SelectNodes("//fet/Days_List/Day");
    if (dayNodes != null)
    {
        foreach (System.Xml.XmlNode node in dayNodes)
        {
            var name = node.SelectSingleNode("Name")?.InnerText;
            var longName = node.SelectSingleNode("Long_Name")?.InnerText;
            dataList.Add(new { name = name, longName = longName });
        }
    }
    break;
case "hours":
    var hourNodes = xml.SelectNodes("//fet/Hours_List/Hour");
    if (hourNodes != null)
    {
        foreach (System.Xml.XmlNode node in hourNodes)
        {
            var name = node.SelectSingleNode("Name")?.InnerText;
            var longName = node.SelectSingleNode("Long_Name")?.InnerText;
            dataList.Add(new { name = name, longName = longName });
        }
    }
    break;
                        
                    default:
                        return Json(new { success = false, message = "Geçersiz veri tipi" });
                }
                
                return Json(new { success = true, data = dataList });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "XML işleme hatası: " + ex.Message });
            }
        }

        // --- Day/Hour CRUD Models ---
        public class DayHourRequest
        {
            public int? Index { get; set; } // For edit/delete
            public string? Name { get; set; }
            public string? LongName { get; set; }
        }

        // --- Day CRUD ---
        [HttpPost]
        public JsonResult AddDay([FromBody] DayHourRequest req)
        {
            if (string.IsNullOrEmpty(CurrentXmlContent))
                return Json(new { success = false, message = "Önce bir proje açın veya oluşturun." });
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(CurrentXmlContent);
                var daysList = doc.SelectSingleNode("/fet/Days_List");
                if (daysList == null)
                    return Json(new { success = false, message = "XML'de Days_List bulunamadı." });
                var dayNode = doc.CreateElement("Day");
                var nameNode = doc.CreateElement("Name");
                nameNode.InnerText = req.Name ?? "";
                var longNameNode = doc.CreateElement("Long_Name");
                longNameNode.InnerText = req.LongName ?? req.Name ?? "";
                dayNode.AppendChild(nameNode);
                dayNode.AppendChild(longNameNode);
                daysList.AppendChild(dayNode);
                // Update number of days
                var numDaysNode = daysList.SelectSingleNode("Number_of_Days");
                if (numDaysNode != null)
                {
                    var dayNodes = daysList.SelectNodes("Day");
                    int n = dayNodes != null ? dayNodes.Count : 0;
                    numDaysNode.InnerText = n.ToString();
                }
                using (var sw = new System.IO.StringWriter())
                using (var xw = new System.Xml.XmlTextWriter(sw))
                {
                    xw.Formatting = System.Xml.Formatting.Indented;
                    doc.WriteTo(xw);
                    xw.Flush();
                    CurrentXmlContent = sw.ToString();
                }
                if (!string.IsNullOrEmpty(CurrentFilePath))
                    System.IO.File.WriteAllText(CurrentFilePath, CurrentXmlContent);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult EditDay([FromBody] DayHourRequest req)
        {
            if (string.IsNullOrEmpty(CurrentXmlContent))
                return Json(new { success = false, message = "Önce bir proje açın veya oluşturun." });
            if (req.Index == null)
                return Json(new { success = false, message = "Index gerekli." });
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(CurrentXmlContent);
                var daysList = doc.SelectSingleNode("/fet/Days_List");
                if (daysList == null)
                    return Json(new { success = false, message = "XML'de Days_List bulunamadı." });
                var dayNodes = daysList.SelectNodes("Day");
                if (dayNodes == null || req.Index < 0 || req.Index >= dayNodes.Count)
                    return Json(new { success = false, message = "Geçersiz index." });
                var dayNode = dayNodes[req.Index.Value];
                if (dayNode != null)
                {
                    var nameNode = dayNode.SelectSingleNode("Name");
                    var longNameNode = dayNode.SelectSingleNode("Long_Name");
                    if (nameNode != null)
                        nameNode.InnerText = req.Name ?? "";
                    if (longNameNode != null)
                        longNameNode.InnerText = req.LongName ?? req.Name ?? "";
                }
                using (var sw = new System.IO.StringWriter())
                using (var xw = new System.Xml.XmlTextWriter(sw))
                {
                    xw.Formatting = System.Xml.Formatting.Indented;
                    doc.WriteTo(xw);
                    xw.Flush();
                    CurrentXmlContent = sw.ToString();
                }
                if (!string.IsNullOrEmpty(CurrentFilePath))
                    System.IO.File.WriteAllText(CurrentFilePath, CurrentXmlContent);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteDay([FromBody] DayHourRequest req)
        {
            if (string.IsNullOrEmpty(CurrentXmlContent))
                return Json(new { success = false, message = "Önce bir proje açın veya oluşturun." });
            if (req.Index == null)
                return Json(new { success = false, message = "Index gerekli." });
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(CurrentXmlContent);
                var daysList = doc.SelectSingleNode("/fet/Days_List");
                if (daysList == null)
                    return Json(new { success = false, message = "XML'de Days_List bulunamadı." });
                var dayNodes = daysList.SelectNodes("Day");
                if (dayNodes == null || req.Index < 0 || req.Index >= dayNodes.Count)
                    return Json(new { success = false, message = "Geçersiz index." });
                var dayNode = dayNodes[req.Index.Value];
                if (dayNode != null)
                    daysList.RemoveChild(dayNode);
                // Update number of days
                var numDaysNode = daysList.SelectSingleNode("Number_of_Days");
                if (numDaysNode != null)
                {
                    var newDayNodes = daysList.SelectNodes("Day");
                    int n = newDayNodes != null ? newDayNodes.Count : 0;
                    numDaysNode.InnerText = n.ToString();
                }
                using (var sw = new System.IO.StringWriter())
                using (var xw = new System.Xml.XmlTextWriter(sw))
                {
                    xw.Formatting = System.Xml.Formatting.Indented;
                    doc.WriteTo(xw);
                    xw.Flush();
                    CurrentXmlContent = sw.ToString();
                }
                if (!string.IsNullOrEmpty(CurrentFilePath))
                    System.IO.File.WriteAllText(CurrentFilePath, CurrentXmlContent);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // --- Hour CRUD ---
        [HttpPost]
        public JsonResult AddHour([FromBody] DayHourRequest req)
        {
            if (string.IsNullOrEmpty(CurrentXmlContent))
                return Json(new { success = false, message = "Önce bir proje açın veya oluşturun." });
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(CurrentXmlContent);
                var hoursList = doc.SelectSingleNode("/fet/Hours_List");
                if (hoursList == null)
                    return Json(new { success = false, message = "XML'de Hours_List bulunamadı." });
                var hourNode = doc.CreateElement("Hour");
                var nameNode = doc.CreateElement("Name");
                nameNode.InnerText = req.Name ?? "";
                var longNameNode = doc.CreateElement("Long_Name");
                longNameNode.InnerText = req.LongName ?? req.Name ?? "";
                hourNode.AppendChild(nameNode);
                hourNode.AppendChild(longNameNode);
                hoursList.AppendChild(hourNode);
                // Update number of hours
                var numHoursNode = hoursList.SelectSingleNode("Number_of_Hours");
                if (numHoursNode != null)
                {
                    var hourNodes = hoursList.SelectNodes("Hour");
                    int n = hourNodes != null ? hourNodes.Count : 0;
                    numHoursNode.InnerText = n.ToString();
                }
                using (var sw = new System.IO.StringWriter())
                using (var xw = new System.Xml.XmlTextWriter(sw))
                {
                    xw.Formatting = System.Xml.Formatting.Indented;
                    doc.WriteTo(xw);
                    xw.Flush();
                    CurrentXmlContent = sw.ToString();
                }
                if (!string.IsNullOrEmpty(CurrentFilePath))
                    System.IO.File.WriteAllText(CurrentFilePath, CurrentXmlContent);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult EditHour([FromBody] DayHourRequest req)
        {
            if (string.IsNullOrEmpty(CurrentXmlContent))
                return Json(new { success = false, message = "Önce bir proje açın veya oluşturun." });
            if (req.Index == null)
                return Json(new { success = false, message = "Index gerekli." });
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(CurrentXmlContent);
                var hoursList = doc.SelectSingleNode("/fet/Hours_List");
                if (hoursList == null)
                    return Json(new { success = false, message = "XML'de Hours_List bulunamadı." });
                var hourNodes = hoursList.SelectNodes("Hour");
                if (hourNodes == null || req.Index < 0 || req.Index >= hourNodes.Count)
                    return Json(new { success = false, message = "Geçersiz index." });
                var hourNode = hourNodes[req.Index.Value];
                if (hourNode != null)
                {
                    var nameNode = hourNode.SelectSingleNode("Name");
                    var longNameNode = hourNode.SelectSingleNode("Long_Name");
                    if (nameNode != null)
                        nameNode.InnerText = req.Name ?? "";
                    if (longNameNode != null)
                        longNameNode.InnerText = req.LongName ?? req.Name ?? "";
                }
                using (var sw = new System.IO.StringWriter())
                using (var xw = new System.Xml.XmlTextWriter(sw))
                {
                    xw.Formatting = System.Xml.Formatting.Indented;
                    doc.WriteTo(xw);
                    xw.Flush();
                    CurrentXmlContent = sw.ToString();
                }
                if (!string.IsNullOrEmpty(CurrentFilePath))
                    System.IO.File.WriteAllText(CurrentFilePath, CurrentXmlContent);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteHour([FromBody] DayHourRequest req)
        {
            if (string.IsNullOrEmpty(CurrentXmlContent))
                return Json(new { success = false, message = "Önce bir proje açın veya oluşturun." });
            if (req.Index == null)
                return Json(new { success = false, message = "Index gerekli." });
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(CurrentXmlContent);
                var hoursList = doc.SelectSingleNode("/fet/Hours_List");
                if (hoursList == null)
                    return Json(new { success = false, message = "XML'de Hours_List bulunamadı." });
                var hourNodes = hoursList.SelectNodes("Hour");
                if (hourNodes == null || req.Index < 0 || req.Index >= hourNodes.Count)
                    return Json(new { success = false, message = "Geçersiz index." });
                var hourNode = hourNodes[req.Index.Value];
                if (hourNode != null)
                    hoursList.RemoveChild(hourNode);
                // Update number of hours
                var numHoursNode = hoursList.SelectSingleNode("Number_of_Hours");
                if (numHoursNode != null)
                {
                    var newHourNodes = hoursList.SelectNodes("Hour");
                    int n = newHourNodes != null ? newHourNodes.Count : 0;
                    numHoursNode.InnerText = n.ToString();
                }
                using (var sw = new System.IO.StringWriter())
                using (var xw = new System.Xml.XmlTextWriter(sw))
                {
                    xw.Formatting = System.Xml.Formatting.Indented;
                    doc.WriteTo(xw);
                    xw.Flush();
                    CurrentXmlContent = sw.ToString();
                }
                if (!string.IsNullOrEmpty(CurrentFilePath))
                    System.IO.File.WriteAllText(CurrentFilePath, CurrentXmlContent);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Gün ve saat listesini XML'den oku ve JSON olarak döndür
        [HttpGet]
        public JsonResult GetDaysAndHours()
        {
            if (string.IsNullOrEmpty(CurrentXmlContent))
                return Json(new { success = false, message = "Proje yüklü değil" });
            try
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(Fet_Deneme.Models.FetRoot));
                using var reader = new StringReader(CurrentXmlContent);
                var fetRoot = (Fet_Deneme.Models.FetRoot?)serializer.Deserialize(reader);
                var days = fetRoot?.Days?.Select(d => d.Name).Where(x => !string.IsNullOrEmpty(x)).ToList() ?? new List<string?>();
                var hours = fetRoot?.Hours?.Select(h => h.Name).Where(x => !string.IsNullOrEmpty(x)).ToList() ?? new List<string?>();
                return Json(new { success = true, days, hours });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Add a time constraint to the XML
        [HttpPost]
        public JsonResult AddConstraint([FromBody] ConstraintRequest req)
        {
            if (string.IsNullOrEmpty(CurrentXmlContent))
                return Json(new { success = false, message = "No project loaded." });
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(CurrentXmlContent);
                var constraintsList = doc.SelectSingleNode("/fet/Time_Constraints_List");
                if (constraintsList == null)
                    return Json(new { success = false, message = "Time_Constraints_List not found in XML." });

                System.Xml.XmlElement? newConstraint = null;
                switch (req.Type)
                {
                    case "ConstraintActivityPreferredTimeSlots":
                        var model1 = ConvertData<ConstraintActivityPreferredTimeSlotsModel>(req.Data);
                        newConstraint = CreateConstraintActivityPreferredTimeSlots(doc, model1);
                        break;
                    case "ConstraintStudentsMaxHoursDaily":
                        var model2 = ConvertData<ConstraintStudentsMaxHoursDailyModel>(req.Data);
                        newConstraint = CreateConstraintStudentsMaxHoursDaily(doc, model2);
                        break;
                    case "ConstraintStudentsMaxHoursContinuously":
                        var model3 = ConvertData<ConstraintStudentsMaxHoursContinuouslyModel>(req.Data);
                        newConstraint = CreateConstraintStudentsMaxHoursContinuously(doc, model3);
                        break;
                    case "ConstraintMinDaysBetweenActivities":
                        var model4 = ConvertData<ConstraintMinDaysBetweenActivitiesModel>(req.Data);
                        newConstraint = CreateConstraintMinDaysBetweenActivities(doc, model4);
                        break;
                    case "ConstraintActivitiesNotOverlapping":
                        var model5 = ConvertData<ConstraintActivitiesNotOverlappingModel>(req.Data);
                        newConstraint = CreateConstraintActivitiesNotOverlapping(doc, model5);
                        break;
                    case "ConstraintBreakTimes":
                        var model6 = ConvertData<ConstraintBreakTimesModel>(req.Data);
                        newConstraint = CreateConstraintBreakTimes(doc, model6);
                        break;
                    case "ConstraintMaxStudentsInSelectedTimeSlot":
                        var model7 = ConvertData<ConstraintMaxStudentsInSelectedTimeSlotModel>(req.Data);
                        newConstraint = CreateConstraintMaxStudentsInSelectedTimeSlot(doc, model7);
                        break;
                    default:
                        return Json(new { success = false, message = "Unknown constraint type." });
                }
                if (newConstraint == null)
                    return Json(new { success = false, message = "Constraint could not be created." });
                constraintsList.AppendChild(newConstraint);

                using (var sw = new System.IO.StringWriter())
                using (var xw = new System.Xml.XmlTextWriter(sw))
                {
                    xw.Formatting = System.Xml.Formatting.Indented;
                    doc.WriteTo(xw);
                    xw.Flush();
                    CurrentXmlContent = sw.ToString();
                }
                if (!string.IsNullOrEmpty(CurrentFilePath))
                    System.IO.File.WriteAllText(CurrentFilePath, CurrentXmlContent);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class ConstraintRequest
        {
            public string Type { get; set; } = string.Empty;
            public dynamic Data { get; set; } = null!;
        }

        // Helper methods for each constraint type
        private System.Xml.XmlElement CreateConstraintActivityPreferredTimeSlots(System.Xml.XmlDocument doc, ConstraintActivityPreferredTimeSlotsModel data)
        {
            var node = doc.CreateElement("ConstraintActivityPreferredTimeSlots");
            node.AppendChild(CreateTextElement(doc, "Weight_Percentage", data.WeightPercentage.ToString()));
            node.AppendChild(CreateTextElement(doc, "Activity_Id", data.ActivityId.ToString()));
            var slots = data.PreferredTimeSlots ?? new List<PreferredTimeSlot>();
            node.AppendChild(CreateTextElement(doc, "Number_of_Preferred_Time_Slots", slots.Count.ToString()));
            foreach (var t in slots)
            {
                var slot = doc.CreateElement("Preferred_Time_Slot");
                slot.AppendChild(CreateTextElement(doc, "Day", t.Day));
                slot.AppendChild(CreateTextElement(doc, "Hour", t.Hour));
                node.AppendChild(slot);
            }
            node.AppendChild(CreateTextElement(doc, "Active", (data.Active).ToString().ToLower()));
            node.AppendChild(CreateTextElement(doc, "Comments", data.Comments ?? ""));
            return node;
        }
        private System.Xml.XmlElement CreateConstraintStudentsMaxHoursDaily(System.Xml.XmlDocument doc, dynamic data)
        {
            var node = doc.CreateElement("ConstraintStudentsMaxHoursDaily");
            node.AppendChild(CreateTextElement(doc, "Weight_Percentage", data.WeightPercentage?.ToString() ?? "100"));
            node.AppendChild(CreateTextElement(doc, "Maximum_Hours_Daily", data.MaximumHoursDaily.ToString()));
            node.AppendChild(CreateTextElement(doc, "Active", (data.Active ?? true).ToString().ToLower()));
            node.AppendChild(CreateTextElement(doc, "Comments", data.Comments ?? ""));
            return node;
        }
        private System.Xml.XmlElement CreateConstraintStudentsMaxHoursContinuously(System.Xml.XmlDocument doc, dynamic data)
        {
            var node = doc.CreateElement("ConstraintStudentsMaxHoursContinuously");
            node.AppendChild(CreateTextElement(doc, "Weight_Percentage", data.WeightPercentage?.ToString() ?? "100"));
            node.AppendChild(CreateTextElement(doc, "Maximum_Hours_Continuously", data.MaximumHoursContinuously.ToString()));
            node.AppendChild(CreateTextElement(doc, "Active", (data.Active ?? true).ToString().ToLower()));
            node.AppendChild(CreateTextElement(doc, "Comments", data.Comments ?? ""));
            return node;
        }
        private System.Xml.XmlElement CreateConstraintMinDaysBetweenActivities(System.Xml.XmlDocument doc, dynamic data)
        {
            var node = doc.CreateElement("ConstraintMinDaysBetweenActivities");
            node.AppendChild(CreateTextElement(doc, "Weight_Percentage", data.WeightPercentage?.ToString() ?? "100"));
            node.AppendChild(CreateTextElement(doc, "Consecutive_If_Same_Day", (data.ConsecutiveIfSameDay ?? false).ToString().ToLower()));
            // DÜZELTME: ActivityIds tipi int olmalı, object değil
            var ids = (data.ActivityIds as IEnumerable<int>) ??
                      (data.ActivityIds as IEnumerable<object>)?.Select(x => Convert.ToInt32(x)) ??
                      new List<int>();
            node.AppendChild(CreateTextElement(doc, "Number_of_Activities", ids.Count().ToString()));
            foreach (var id in ids)
                node.AppendChild(CreateTextElement(doc, "Activity_Id", id.ToString()));
            node.AppendChild(CreateTextElement(doc, "MinDays", data.MinDays.ToString()));
            node.AppendChild(CreateTextElement(doc, "Active", (data.Active ?? true).ToString().ToLower()));
            node.AppendChild(CreateTextElement(doc, "Comments", data.Comments ?? ""));
            return node;
        }
        private System.Xml.XmlElement CreateConstraintActivitiesNotOverlapping(System.Xml.XmlDocument doc, dynamic data)
        {
            var node = doc.CreateElement("ConstraintActivitiesNotOverlapping");
            node.AppendChild(CreateTextElement(doc, "Weight_Percentage", data.WeightPercentage?.ToString() ?? "100"));
            // DÜZELTME: ActivityIds tipi int olmalı, object değil
            var ids = (data.ActivityIds as IEnumerable<int>) ??
                      (data.ActivityIds as IEnumerable<object>)?.Select(x => Convert.ToInt32(x)) ??
                      new List<int>();
            node.AppendChild(CreateTextElement(doc, "Number_of_Activities", ids.Count().ToString()));
            foreach (var id in ids)
                node.AppendChild(CreateTextElement(doc, "Activity_Id", id.ToString()));
            node.AppendChild(CreateTextElement(doc, "Active", (data.Active ?? true).ToString().ToLower()));
            node.AppendChild(CreateTextElement(doc, "Comments", data.Comments ?? ""));
            return node;
        }
        private System.Xml.XmlElement CreateConstraintBreakTimes(System.Xml.XmlDocument doc, dynamic data)
        {
            var node = doc.CreateElement("ConstraintBreakTimes");
            node.AppendChild(CreateTextElement(doc, "Weight_Percentage", data.WeightPercentage?.ToString() ?? "100"));
            var breaks = data.BreakTimes as IEnumerable<object> ?? new List<object>();
            node.AppendChild(CreateTextElement(doc, "Number_of_Break_Times", breaks.Count().ToString()));
            foreach (var b in breaks)
            {
                var breakNode = doc.CreateElement("Break_Time");
                var day = b.GetType().GetProperty("Day")?.GetValue(b, null)?.ToString() ?? string.Empty;
                var hour = b.GetType().GetProperty("Hour")?.GetValue(b, null)?.ToString() ?? string.Empty;
                breakNode.AppendChild(CreateTextElement(doc, "Day", day));
                breakNode.AppendChild(CreateTextElement(doc, "Hour", hour));
                node.AppendChild(breakNode);
            }
            node.AppendChild(CreateTextElement(doc, "Active", (data.Active ?? true).ToString().ToLower()));
            node.AppendChild(CreateTextElement(doc, "Comments", data.Comments ?? ""));
            return node;
        }
        private System.Xml.XmlElement CreateConstraintMaxStudentsInSelectedTimeSlot(System.Xml.XmlDocument doc, ConstraintMaxStudentsInSelectedTimeSlotModel data)
        {
            var node = doc.CreateElement("ConstraintMaxStudentsInSelectedTimeSlot");
            node.AppendChild(CreateTextElement(doc, "Weight_Percentage", data.WeightPercentage.ToString()));
            var slots = data.SelectedTimeSlots ?? new List<PreferredTimeSlot>();
            node.AppendChild(CreateTextElement(doc, "Number_of_Selected_Time_Slots", slots.Count.ToString()));
            foreach (var t in slots)
            {
                var slot = doc.CreateElement("Selected_Time_Slot");
                slot.AppendChild(CreateTextElement(doc, "Day", t.Day));
                slot.AppendChild(CreateTextElement(doc, "Hour", t.Hour));
                node.AppendChild(slot);
            }
            node.AppendChild(CreateTextElement(doc, "Max_Students", data.MaxStudents.ToString()));
            node.AppendChild(CreateTextElement(doc, "Active", (data.Active).ToString().ToLower()));
            node.AppendChild(CreateTextElement(doc, "Comments", data.Comments ?? ""));
            return node;
        }
        private System.Xml.XmlElement CreateTextElement(System.Xml.XmlDocument doc, string name, string? value)
        {
            var el = doc.CreateElement(name);
            el.InnerText = value ?? string.Empty;
            return el;
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View(new ErrorViewModel { RequestId = requestId });
        }

        [HttpGet]
        public IActionResult ListConstraints(string type)
        {
            if (string.IsNullOrEmpty(CurrentXmlContent))
                return Json(new { list = new List<object>() });
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(CurrentXmlContent);
            var constraintsList = doc.SelectSingleNode("/fet/Time_Constraints_List");
            if (constraintsList == null)
                return Json(new { list = new List<object>() });
            var result = new List<object>();
            foreach (System.Xml.XmlNode node in constraintsList.ChildNodes)
            {
                if (node.Name != type) continue;
                // Kısa özet ve detay üret
                string summary = GetConstraintSummary(node);
                string details = GetConstraintDetails(node);
                result.Add(new { summary, details });
            }
            return Json(new { list = result });
        }

        private string GetConstraintSummary(System.Xml.XmlNode node)
        {
            var type = node.Name;
            var weight = node.SelectSingleNode("Weight_Percentage")?.InnerText ?? "";
            var summary = type + ", AY:" + weight + "%";
            if (type == "ConstraintActivitiesNotOverlapping" || type == "ConstraintMinDaysBetweenActivities")
            {
                var ids = node.SelectNodes("Activity_Id");
                if (ids != null && ids.Count > 0)
                {
                    summary += ", Id:" + string.Join(", Id:", ids.Cast<System.Xml.XmlNode>().Select(x => x.InnerText));
                }
            }
            if (type == "ConstraintActivityPreferredTimeSlots")
            {
                var id = node.SelectSingleNode("Activity_Id")?.InnerText;
                summary += ", Id:" + id;
            }
            if (type == "ConstraintBreakTimes")
            {
                var n = node.SelectSingleNode("Number_of_Break_Times")?.InnerText;
                summary += ", NA:" + n;
            }
            if (type == "ConstraintMaxStudentsInSelectedTimeSlot")
            {
                var n = node.SelectSingleNode("Number_of_Selected_Time_Slots")?.InnerText;
                summary += ", NSlots:" + n;
                var max = node.SelectSingleNode("Max_Students")?.InnerText;
                summary += ", MaxStudents:" + max;
            }
            return summary;
        }

        private string GetConstraintDetails(System.Xml.XmlNode node)
        {
            var type = node.Name;
            var details = $"<b>Zaman kısıtı</b><br>";
            if (type == "ConstraintActivitiesNotOverlapping")
            {
                details += "Dersler Ardışık Olmamalı<br>";
                details += $"Weight (percentage)={node.SelectSingleNode("Weight_Percentage")?.InnerText ?? ""}<br>";
                details += $"Ders sayısı={node.SelectSingleNode("Number_of_Activities")?.InnerText ?? ""}<br>";
                var ids = node.SelectNodes("Activity_Id");
                if (ids != null && ids.Count > 0)
                {
                    details += "Activity with id=" + string.Join(",", ids.Cast<System.Xml.XmlNode>().Select(x => x.InnerText)) + "<br>";
                }
            }
            else if (type == "ConstraintMaxStudentsInSelectedTimeSlot")
            {
                details += "Seçili slotlarda maksimum öğrenci kısıtı<br>";
                details += $"Weight (percentage)={node.SelectSingleNode("Weight_Percentage")?.InnerText ?? ""}<br>";
                details += $"Slot sayısı={node.SelectSingleNode("Number_of_Selected_Time_Slots")?.InnerText ?? ""}<br>";
                details += $"Max Students={node.SelectSingleNode("Max_Students")?.InnerText ?? ""}<br>";
                var slots = node.SelectNodes("Selected_Time_Slot");
                if (slots != null && slots.Count > 0)
                {
                    details += "<b>Slotlar:</b><br>";
                    foreach (System.Xml.XmlNode slot in slots)
                    {
                        var day = slot.SelectSingleNode("Day")?.InnerText;
                        var hour = slot.SelectSingleNode("Hour")?.InnerText;
                        details += $"&nbsp;&nbsp;{day} - {hour}<br>";
                    }
                }
            }
            else
            {
                // Tüm alt elemanları başlık:değer şeklinde sırala
                foreach (System.Xml.XmlNode child in node.ChildNodes)
                {
                    if (child.HasChildNodes && child.ChildNodes.Count > 1 && child.FirstChild.NodeType == System.Xml.XmlNodeType.Element)
                    {
                        // Alt elemanları listele
                        details += $"<b>{child.Name}</b>:<br>";
                        foreach (System.Xml.XmlNode sub in child.ChildNodes)
                        {
                            details += $"&nbsp;&nbsp;{sub.Name}: {sub.InnerText}<br>";
                        }
                    }
                    else
                    {
                        details += $"{child.Name}: {child.InnerText}<br>";
                    }
                }
            }
            return details;
        }

        public class EditConstraintRequest
        {
            public string Type { get; set; } = string.Empty;
            public int Index { get; set; } // ListConstraints ile dönen listedeki sıra
            public dynamic Data { get; set; } = null!;
        }

        [HttpPost]
        public JsonResult EditConstraint([FromBody] EditConstraintRequest req)
        {
            if (string.IsNullOrEmpty(CurrentXmlContent))
                return Json(new { success = false, message = "No project loaded." });
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(CurrentXmlContent);
                var constraintsList = doc.SelectSingleNode("/fet/Time_Constraints_List");
                if (constraintsList == null)
                    return Json(new { success = false, message = "Time_Constraints_List not found in XML." });
                // İlgili tipteki constraint'leri bul
                var nodes = constraintsList.SelectNodes(req.Type);
                if (nodes == null || req.Index < 0 || req.Index >= nodes.Count)
                    return Json(new { success = false, message = "Constraint not found." });
                var oldNode = nodes[req.Index];
                if (oldNode == null)
                    return Json(new { success = false, message = "Constraint node not found." });
                // Yeni constraint node'u oluştur
                System.Xml.XmlElement? newConstraint = null;
                switch (req.Type)
                {
                    case "ConstraintActivityPreferredTimeSlots":
                        newConstraint = CreateConstraintActivityPreferredTimeSlots(doc, req.Data);
                        break;
                    case "ConstraintStudentsMaxHoursDaily":
                        newConstraint = CreateConstraintStudentsMaxHoursDaily(doc, req.Data);
                        break;
                    case "ConstraintStudentsMaxHoursContinuously":
                        newConstraint = CreateConstraintStudentsMaxHoursContinuously(doc, req.Data);
                        break;
                    case "ConstraintMinDaysBetweenActivities":
                        newConstraint = CreateConstraintMinDaysBetweenActivities(doc, req.Data);
                        break;
                    case "ConstraintActivitiesNotOverlapping":
                        newConstraint = CreateConstraintActivitiesNotOverlapping(doc, req.Data);
                        break;
                    case "ConstraintBreakTimes":
                        newConstraint = CreateConstraintBreakTimes(doc, req.Data);
                        break;
                    case "ConstraintMaxStudentsInSelectedTimeSlot":
                        newConstraint = CreateConstraintMaxStudentsInSelectedTimeSlot(doc, req.Data);
                        break;
                    default:
                        return Json(new { success = false, message = "Unknown constraint type." });
                }
                if (newConstraint == null)
                    return Json(new { success = false, message = "Constraint could not be created." });
                // Eski node'u yenisiyle değiştir
                constraintsList.ReplaceChild(newConstraint, oldNode);
                using (var sw = new System.IO.StringWriter())
                using (var xw = new System.Xml.XmlTextWriter(sw))
                {
                    xw.Formatting = System.Xml.Formatting.Indented;
                    doc.WriteTo(xw);
                    xw.Flush();
                    CurrentXmlContent = sw.ToString();
                }
                if (!string.IsNullOrEmpty(CurrentFilePath))
                    System.IO.File.WriteAllText(CurrentFilePath, CurrentXmlContent);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class DeleteConstraintRequest
        {
            public string Type { get; set; } = string.Empty;
            public int Index { get; set; } // ListConstraints ile dönen listedeki sıra
        }

        [HttpPost]
        public JsonResult DeleteConstraint([FromBody] DeleteConstraintRequest req)
        {
            if (string.IsNullOrEmpty(CurrentXmlContent))
                return Json(new { success = false, message = "No project loaded." });
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(CurrentXmlContent);
                var constraintsList = doc.SelectSingleNode("/fet/Time_Constraints_List");
                if (constraintsList == null)
                    return Json(new { success = false, message = "Time_Constraints_List not found in XML." });
                var nodes = constraintsList.SelectNodes(req.Type);
                if (nodes == null || req.Index < 0 || req.Index >= nodes.Count)
                    return Json(new { success = false, message = "Constraint not found." });
                var nodeToRemove = nodes[req.Index];
                if (nodeToRemove == null)
                    return Json(new { success = false, message = "Constraint node not found." });
                constraintsList.RemoveChild(nodeToRemove);
                using (var sw = new System.IO.StringWriter())
                using (var xw = new System.Xml.XmlTextWriter(sw))
                {
                    xw.Formatting = System.Xml.Formatting.Indented;
                    doc.WriteTo(xw);
                    xw.Flush();
                    CurrentXmlContent = sw.ToString();
                }
                if (!string.IsNullOrEmpty(CurrentFilePath))
                    System.IO.File.WriteAllText(CurrentFilePath, CurrentXmlContent);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Generate()
        {
            if (string.IsNullOrEmpty(CurrentXmlContent))
                return Json(new { success = false, message = "Henüz yüklenmiş bir .fet içeriği yok." });

            using var stringReader = new StringReader(CurrentXmlContent);
            var serializer = new XmlSerializer(typeof(FetRoot));
            var fetData = (FetRoot)serializer.Deserialize(stringReader)!;

            // TimetableGenerator'a rawXml parametresi de gönder
            var generator = new TimetableGenerator(fetData, CurrentXmlContent);
            var result = generator.Generate();

            if (!result.Success)
            {
                return Json(new
                {
                    success = false,
                    message = $"Çakışma: Activity ID {result.FailedActivityId} için uygun zaman bulunamadı.",
                    failedActivityId = result.FailedActivityId
                });
            }

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Çizelge");

            worksheet.Cell(1, 1).Value = "Activity ID";
            worksheet.Cell(1, 2).Value = "Day";
            worksheet.Cell(1, 3).Value = "Hour";

            int row = 2;
            foreach (var assign in result.Assignments)
            {
                worksheet.Cell(row, 1).Value = assign.ActivityId;
                worksheet.Cell(row, 2).Value = assign.Day;
                worksheet.Cell(row, 3).Value = assign.Hour;
                row++;
            }

            // wwwroot içinde dosyayı kaydet
            var fileName = $"timetable_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "outputs");
            Directory.CreateDirectory(filePath);
            var fullPath = Path.Combine(filePath, fileName);
            workbook.SaveAs(fullPath);

            // İstemciye JSON içinde dosya linkini döndür
            return Json(new
            {
                success = true,
                message = "Çizelge başarıyla oluşturuldu.",
                downloadUrl = $"/outputs/{fileName}"
            });
        }

        private T ConvertData<T>(object data)
        {
            if (data is JsonElement elem)
                return JsonSerializer.Deserialize<T>(elem.GetRawText())!;
            return (T)data;
        }

        public class ConstraintActivityPreferredTimeSlotsModel
        {
            public int WeightPercentage { get; set; }
            public int ActivityId { get; set; }
            public int NumberOfPreferredTimeSlots { get; set; }
            public List<PreferredTimeSlot> PreferredTimeSlots { get; set; } = new();
            public bool Active { get; set; }
            public string? Comments { get; set; }
        }
        public class PreferredTimeSlot
        {
            public string Day { get; set; } = string.Empty;
            public string Hour { get; set; } = string.Empty;
        }
        public class ConstraintStudentsMaxHoursDailyModel
        {
            public int WeightPercentage { get; set; }
            public int MaximumHoursDaily { get; set; }
            public bool Active { get; set; }
            public string? Comments { get; set; }
        }
        public class ConstraintStudentsMaxHoursContinuouslyModel
        {
            public int WeightPercentage { get; set; }
            public int MaximumHoursContinuously { get; set; }
            public bool Active { get; set; }
            public string? Comments { get; set; }
        }
        public class ConstraintMinDaysBetweenActivitiesModel
        {
            public int WeightPercentage { get; set; }
            public List<int> ActivityIds { get; set; } = new();
            public int MinDays { get; set; }
            public bool ConsecutiveIfSameDay { get; set; }
            public bool Active { get; set; }
            public string? Comments { get; set; }
        }
        public class ConstraintActivitiesNotOverlappingModel
        {
            public int WeightPercentage { get; set; }
            public List<int> ActivityIds { get; set; } = new();
            public bool Active { get; set; }
            public string? Comments { get; set; }
        }
        public class ConstraintBreakTimesModel
        {
            public int WeightPercentage { get; set; }
            public List<PreferredTimeSlot> BreakTimes { get; set; } = new();
            public bool Active { get; set; }
            public string? Comments { get; set; }
        }
        public class ConstraintMaxStudentsInSelectedTimeSlotModel
        {
            public int WeightPercentage { get; set; }
            public int NumberOfSelectedTimeSlots { get; set; }
            public List<PreferredTimeSlot> SelectedTimeSlots { get; set; } = new();
            public int MaxStudents { get; set; }
            public bool Active { get; set; }
            public string? Comments { get; set; }
        }
    }
}
