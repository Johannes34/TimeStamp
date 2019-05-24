using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TimeStamp
{
    public class TimeSettings
    {

        public bool AutomaticPauseRecognition { get; set; } = true;
        public TimeSpan AutomaticPauseRecognitionStartTime { get; set; } = new TimeSpan(11, 30, 0);
        public TimeSpan AutomaticPauseRecognitionStopTime { get; set; } = new TimeSpan(13, 30, 0);
        public int AutomaticPauseRecognitionMinPauseTime { get; set; } = 12;


        public List<string> TrackedActivities { get; set; }
        public string AlwaysStartNewDayWithActivity { get; set; } = "Product Development";


        public StatisticTypes StatisticType { get; set; } = StatisticTypes.TimeInLieu;
        public StatisticRanges StatisticRange { get; set; } = StatisticRanges.Ever;

        public enum StatisticTypes
        {
            TimeInLieu,
            Activities,
            WeeklyActivities,
        };

        public enum StatisticRanges
        {
            Ever,

            RecentYear,
            RecentTerm,
            RecentQuarter,
            RecentMonth,
            RecentWeek,

            SelectedYear,
            SelectedMonth,
            SelectedWeek,
            SelectedDay,
        };


        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }


        public bool DisablePopupNotifications { get; set; } = false;

        public bool IgnoreStampFile { get; set; } = false;


        public static Regex HHMM { get; } = new Regex("[0-9]{2}[:]{1}[0-9]{2}");
        public static Regex DDMMYYYY { get; } = new Regex("[0-9]{2}[.]{1}[0-9]{2}[.]{1}[0-9]{4}");
        public static Regex Integer { get; } = new Regex("[^0-9]+");


        public void LoadSettings()
        {
            AutomaticPauseRecognition = GetKey("AutomaticPauseRecognition", true);

            StatisticType = (StatisticTypes)GetKey("StatisticsTypeIndex", 0);
            StatisticRange = (StatisticRanges)GetKey("StatisticsTimeIndex", 0);

            WindowWidth = GetKey("WindowWidth", WindowWidth);
            WindowHeight = GetKey("WindowHeight", WindowHeight);

            TrackedActivities = GetKey("TrackedActivities", String.Empty).Split(new[] { ";;;" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!TrackedActivities.Any())
            {
                // default:
                TrackedActivities = new List<string>()
                {
                    "Paid Requirements",
                    "Product Development",
                    "Product Support",
                    "Meetings",
                    "Documentation"
                };
            }

            AlwaysStartNewDayWithActivity = GetKey("AlwaysStartNewDayWithActivity", (string)null);
            if (AlwaysStartNewDayWithActivity == String.Empty)
                AlwaysStartNewDayWithActivity = null;
        }

        private T GetKey<T>(string name, T defaultValue)
        {
            var key = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", name, defaultValue);
            if (key != null && key is T)
            {
                return (T)key;
            }
            return defaultValue;
        }

        public void SaveSettings()
        {
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "AutomaticPauseRecognition", AutomaticPauseRecognition);
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "StatisticsTypeIndex", (int)StatisticType);
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "StatisticsTimeIndex", (int)StatisticRange);

            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "WindowWidth", WindowWidth);
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "WindowHeight", WindowHeight);

            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "TrackedActivities", String.Join(";;;", TrackedActivities));
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "AlwaysStartNewDayWithActivity", AlwaysStartNewDayWithActivity ?? String.Empty);
        }


    }
}
