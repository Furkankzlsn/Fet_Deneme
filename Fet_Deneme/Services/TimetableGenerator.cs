﻿using Fet_Deneme.Models;
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
        private ConstraintStudentsMaxHoursContinuouslyChecker? _maxHoursContinuouslyChecker;
        private Dictionary<int, TimetableActivity>? _activityMap;
        private List<Activity>? _allActivities;
        private List<ConstraintStudentsMaxHoursDaily>? _constraintStudentsMaxHoursDaily;
        private List<ConstraintStudentsMaxHoursContinuously>? _constraintStudentsMaxHoursContinuously;
        private List<ConstraintBreakTimes>? _constraintBreakTimes;
        private ConstraintBreakTimesChecker? _breakTimesChecker;
        private List<ConstraintActivityPreferredTimeSlots>? _constraintActivityPreferredTimeSlots;
        private ConstraintActivityPreferredTimeSlotsChecker? _activityPreferredTimeSlotsChecker;
        private List<ConstraintActivitiesNotOverlapping>? _constraintActivitiesNotOverlapping;
        private ConstraintActivitiesNotOverlappingChecker? _activitiesNotOverlappingChecker;
        private List<ConstraintMinDaysBetweenActivities>? _constraintMinDaysBetweenActivities;
        private ConstraintMinDaysBetweenActivitiesChecker? _minDaysBetweenActivitiesChecker;
        private List<ConstraintMaxStudentsInSelectedTimeSlot>? _constraintMaxStudentsInSelectedTimeSlot;
        private ConstraintMaxStudentsInSelectedTimeSlotChecker? _maxStudentsInSelectedTimeSlotChecker;

        public TimetableGenerator(FetRoot data, string? rawXml = null)
        {
            _data = data;
            _rawXml = rawXml;
            // TimeConstraints listesinden ilgili kısıtları ayıkla
            if (_data != null && _data.TimeConstraints != null)
            {
                _constraintStudentsMaxHoursDaily = _data.TimeConstraints.OfType<ConstraintStudentsMaxHoursDaily>().ToList();
                _constraintStudentsMaxHoursContinuously = _data.TimeConstraints.OfType<ConstraintStudentsMaxHoursContinuously>().ToList();
                _constraintBreakTimes = _data.TimeConstraints.OfType<ConstraintBreakTimes>().ToList();
                _constraintActivityPreferredTimeSlots = _data.TimeConstraints.OfType<ConstraintActivityPreferredTimeSlots>().ToList();
                _constraintActivitiesNotOverlapping = _data.TimeConstraints.OfType<ConstraintActivitiesNotOverlapping>().ToList();
                _constraintMinDaysBetweenActivities = _data.TimeConstraints.OfType<ConstraintMinDaysBetweenActivities>().ToList();
                _constraintMaxStudentsInSelectedTimeSlot = _data.TimeConstraints.OfType<ConstraintMaxStudentsInSelectedTimeSlot>().ToList();
            }
            if (_data != null && _data.Days != null && _data.Activities != null)
            {
                _maxHoursDailyChecker = new ConstraintStudentsMaxHoursDailyChecker(_constraintStudentsMaxHoursDaily ?? new List<ConstraintStudentsMaxHoursDaily>(), _data.Days);
                _maxHoursContinuouslyChecker = new ConstraintStudentsMaxHoursContinuouslyChecker(_constraintStudentsMaxHoursContinuously ?? new List<ConstraintStudentsMaxHoursContinuously>(), _data.Hours!);
                _breakTimesChecker = new ConstraintBreakTimesChecker(_constraintBreakTimes ?? new List<ConstraintBreakTimes>());
                _activityPreferredTimeSlotsChecker = new ConstraintActivityPreferredTimeSlotsChecker(_constraintActivityPreferredTimeSlots ?? new List<ConstraintActivityPreferredTimeSlots>());
                _activitiesNotOverlappingChecker = new ConstraintActivitiesNotOverlappingChecker(_constraintActivitiesNotOverlapping ?? new List<ConstraintActivitiesNotOverlapping>());
                _minDaysBetweenActivitiesChecker = new ConstraintMinDaysBetweenActivitiesChecker(_constraintMinDaysBetweenActivities ?? new List<ConstraintMinDaysBetweenActivities>(), _data.Days);
                _maxStudentsInSelectedTimeSlotChecker = new ConstraintMaxStudentsInSelectedTimeSlotChecker(_constraintMaxStudentsInSelectedTimeSlot ?? new List<ConstraintMaxStudentsInSelectedTimeSlot>());
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
            // Break time kontrolü
            if (_breakTimesChecker != null && _breakTimesChecker.IsBreak(day, hour))
                return false;
            // Activity Preferred Time Slots kontrolü
            if (_activityPreferredTimeSlotsChecker != null && !_activityPreferredTimeSlotsChecker.IsAllowed(activity.Id, day, hour))
                return false;
            // Activities Not Overlapping kontrolü
            if (_activitiesNotOverlappingChecker != null)
            {
                if (_activityMap == null)
                    _activityMap = allActivities.ToDictionary(a => a.Id, a => a);
                if (!_activitiesNotOverlappingChecker.IsValid(activity, day, hour, assignments, _activityMap))
                    return false;
            }
            // ConstraintMinDaysBetweenActivities kontrolü
            if (_minDaysBetweenActivitiesChecker != null)
            {
                if (_activityMap == null)
                    _activityMap = allActivities.ToDictionary(a => a.Id, a => a);
                if (_allActivities != null && !_minDaysBetweenActivitiesChecker.IsValid(assignments, activity, day, _activityMap, _allActivities))
                    return false;
            }
            // ConstraintMaxStudentsInSelectedTimeSlot kontrolü
            if (_maxStudentsInSelectedTimeSlotChecker != null)
            {
                if (_activityMap == null)
                    _activityMap = allActivities.ToDictionary(a => a.Id, a => a);
                if (!_maxStudentsInSelectedTimeSlotChecker.IsValid(activity, day, hour, assignments, _activityMap))
                    return false;
            }

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
            // ConstraintStudentsMaxHoursContinuously kontrolü
            if (_maxHoursContinuouslyChecker != null)
            {
                if (_activityMap == null)
                {
                    _activityMap = allActivities.ToDictionary(a => a.Id, a => a);
                }
                if (_allActivities != null && !_maxHoursContinuouslyChecker.IsValid(assignments, activity, day, hour, _activityMap, _allActivities))
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
