using System.Diagnostics;
using Fet_Deneme.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;

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

                    subjectsList.AppendChild(subjectNode);
                    added++;
                }

                // 6. XML'i güncelle ve dosyaya yaz
                using (var sw = new System.IO.StringWriter())
                using (var xw = new System.Xml.XmlTextWriter(sw))
                {
                    xw.Formatting = System.Xml.Formatting.None;
                    doc.WriteTo(xw);
                    xw.Flush();
                    // Boş elemanlar için gereksiz satır sonlarını temizle
                    var xml = sw.ToString();
                    // <Tag>\n</Tag> => <Tag></Tag>
                    xml = System.Text.RegularExpressions.Regex.Replace(xml, @">\s*</([a-zA-Z0-9_]+)>", "></$1>");
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
