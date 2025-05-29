using Fet_Deneme.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Fet_Deneme.Services
{
    public class GenerationResult
    {
        public bool Success { get; set; }
        public int? FailedActivityId { get; set; }
        public List<GeneratedAssignment> Assignments { get; set; } = new();
    }

    public class GeneratedAssignment
    {
        public int ActivityId { get; set; }
        public string Day { get; set; } = "";
        public string Hour { get; set; } = "";
    }

    // Yeni activity modeli sadece timetable için
    public class TimetableActivity
    {
        public int Id { get; set; }
        public List<string> Teachers { get; set; } = new();
        public List<string> Students { get; set; } = new();
    }

    public class TimetableGenerator
    {
        private readonly FetRoot _data;
        private readonly string? _rawXml;
        private ConstraintStudentsMaxHoursDailyChecker? _maxHoursDailyChecker;
        private Dictionary<int, TimetableActivity>? _activityMap;
        private List<Activity>? _allActivities;

        public TimetableGenerator(FetRoot data, string? rawXml = null)
        {
            _data = data;
            _rawXml = rawXml;
            // Kısıtları ve checker'ı başlat
            if (_data != null && _data.Days != null && _data.Activities != null)
            {
                var maxHoursDailyConstraints = _data.ConstraintStudentsMaxHoursDaily ?? new List<ConstraintStudentsMaxHoursDaily>();
                _maxHoursDailyChecker = new ConstraintStudentsMaxHoursDailyChecker(maxHoursDailyConstraints, _data.Days);
                _allActivities = _data.Activities;
            }
        }

        public GenerationResult Generate()
        {
            var result = new GenerationResult();

            // Eğer XML içeriği varsa, orijinal XML'den tüm Teacher ve Students tag'larını topla
            List<TimetableActivity> parsedActivities;
            if (!string.IsNullOrEmpty(_rawXml))
            {
                var xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(_rawXml);
                var activityNodes = xmlDoc.SelectNodes("//fet/Activities_List/Activity");
                parsedActivities = new List<TimetableActivity>();
                if (activityNodes != null)
                {
                    foreach (System.Xml.XmlNode node in activityNodes)
                    {
                        var idNode = node.SelectSingleNode("Id");
                        int id = idNode != null && int.TryParse(idNode.InnerText, out int i) ? i : 0;
                        var teacherNodes = node.SelectNodes("Teacher");
                        var studentNodes = node.SelectNodes("Students");
                        var teachers = new List<string>();
                        var students = new List<string>();
                        if (teacherNodes != null)
                            foreach (System.Xml.XmlNode t in teacherNodes)
                                if (!string.IsNullOrWhiteSpace(t.InnerText))
                                    teachers.Add(t.InnerText.Trim().ToLower());
                        if (studentNodes != null)
                            foreach (System.Xml.XmlNode s in studentNodes)
                                if (!string.IsNullOrWhiteSpace(s.InnerText))
                                    students.Add(s.InnerText.Trim().ToLower());
                        parsedActivities.Add(new TimetableActivity
                        {
                            Id = id,
                            Teachers = teachers.Distinct().ToList(),
                            Students = students.Distinct().ToList()
                        });
                    }
                }
            }
            else
            {
                // Eski davranış: sadece string olarak Teacher/Students
                parsedActivities = _data.Activities!.Select(a => new TimetableActivity
                {
                    Id = a.Id,
                    Teachers = GetListSafe(a.Teacher),
                    Students = GetListSafe(a.Students)
                }).ToList();
            }

            int totalActivities = parsedActivities.Count;
            if (totalActivities == 0)
            {
                result.Success = true;
                return result;
            }

            int count = 0;
            foreach (var activity in parsedActivities)
            {
                count++;
                bool assigned = false;

                foreach (var day in _data.Days!)
                {
                    foreach (var hour in _data.Hours!)
                    {
                        if (IsSlotAvailable(activity, day.Name!, hour.Name!, result.Assignments, parsedActivities))
                        {
                            result.Assignments.Add(new GeneratedAssignment
                            {
                                ActivityId = activity.Id,
                                Day = day.Name!,
                                Hour = hour.Name!
                            });
                            assigned = true;
                            break;
                        }
                    }
                    if (assigned) break;
                }

                if (!assigned)
                {
                    result.Success = false;
                    result.FailedActivityId = activity.Id;
                    return result;
                }
            }

            result.Success = true;
            return result;
        }

        private bool IsSlotAvailable(TimetableActivity activity, string day, string hour, List<GeneratedAssignment> assignments, List<TimetableActivity> allActivities)
        {
            var sameSlotAssignments = assignments
                .Where(a => a.Day == day && a.Hour == hour)
                .ToList();

            foreach (var assign in sameSlotAssignments)
            {
                var otherActivity = allActivities.FirstOrDefault(a => a.Id == assign.ActivityId);
                if (otherActivity == null) continue;

                if (activity.Teachers.Intersect(otherActivity.Teachers).Any() ||
                    activity.Students.Intersect(otherActivity.Students).Any())
                    return false;
            }

            // ConstraintStudentsMaxHoursDaily kontrolü
            if (_maxHoursDailyChecker != null)
            {
                if (_activityMap == null)
                {
                    _activityMap = allActivities.ToDictionary(a => a.Id, a => a);
                }
                if (_allActivities != null && !_maxHoursDailyChecker.IsValid(assignments, activity, day, _activityMap, _allActivities))
                    return false;
            }

            return true;
        }

        private List<string> GetListSafe(object? raw)
        {
            if (raw is List<string> list)
                return list.Select(x => x.Trim().ToLower()).ToList();

            if (raw is string str)
            {
                return str
                    .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToLower())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct()
                    .ToList();
            }

            return new List<string>();
        }
    }
}
