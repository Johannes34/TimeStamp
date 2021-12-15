using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TimeStamp
{
    public class TimeSettings : INotifyPropertyChanged
    {

        // PAUSE RECOGNITION:


        private bool m_AutomaticPauseRecognition = true;
        public bool AutomaticPauseRecognition
        {
            get
            {
                return m_AutomaticPauseRecognition;
            }
            set
            {
                if (m_AutomaticPauseRecognition != value)
                {
                    m_AutomaticPauseRecognition = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(AutomaticPauseRecognition)));
                }
            }
        }

        private TimeSpan m_AutomaticPauseRecognitionStartTime = new TimeSpan(11, 30, 0);
        public TimeSpan AutomaticPauseRecognitionStartTime
        {
            get
            {
                return m_AutomaticPauseRecognitionStartTime;
            }
            set
            {
                if (m_AutomaticPauseRecognitionStartTime != value)
                {
                    m_AutomaticPauseRecognitionStartTime = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(AutomaticPauseRecognitionStartTime)));
                }
            }
        }

        private TimeSpan m_AutomaticPauseRecognitionStopTime = new TimeSpan(13, 30, 0);
        public TimeSpan AutomaticPauseRecognitionStopTime
        {
            get
            {
                return m_AutomaticPauseRecognitionStopTime;
            }
            set
            {
                if (m_AutomaticPauseRecognitionStopTime != value)
                {
                    m_AutomaticPauseRecognitionStopTime = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(AutomaticPauseRecognitionStopTime)));
                }
            }
        }

        private int m_AutomaticPauseRecognitionMinPauseTime = 12;
        public int AutomaticPauseRecognitionMinPauseTime
        {
            get
            {
                return m_AutomaticPauseRecognitionMinPauseTime;
            }
            set
            {
                if (m_AutomaticPauseRecognitionMinPauseTime != value)
                {
                    m_AutomaticPauseRecognitionMinPauseTime = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(AutomaticPauseRecognitionMinPauseTime)));
                }
            }
        }

        private bool m_IsLockingComputerWhenLeaving = true;
        /// <summary>
        /// Determines, how the pause 'start' time is being detected: If the property is true, it is assumed that the computer will not be left behind unlocked, so the last 'log off' time is used for pause calculation, independently of any mouse movements.
        /// On the other hand, if this property is false, the pause times are calculated by the last mouse move time stamp. This makes sense, when the computer is not being locked when leaving it (auto-log-off time stamp is then ignored).
        /// </summary>
        public bool IsLockingComputerWhenLeaving
        {
            get
            {
                return m_IsLockingComputerWhenLeaving;
            }
            set
            {
                if (m_IsLockingComputerWhenLeaving != value)
                {
                    m_IsLockingComputerWhenLeaving = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsLockingComputerWhenLeaving)));
                }
            }
        }



        // ACTIVITY REMINDER NOTIFICATIONS:

        private bool m_RemindCurrentActivityWhenChangingVPN = true;
        public bool RemindCurrentActivityWhenChangingVPN
        {
            get
            {
                return m_RemindCurrentActivityWhenChangingVPN;
            }
            set
            {
                if (m_RemindCurrentActivityWhenChangingVPN != value)
                {
                    m_RemindCurrentActivityWhenChangingVPN = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(RemindCurrentActivityWhenChangingVPN)));
                }
            }
        }

        private string m_RemindCurrentActivityWhenChangingVPNWithName;
        public string RemindCurrentActivityWhenChangingVPNWithName
        {
            get
            {
                return m_RemindCurrentActivityWhenChangingVPNWithName;
            }
            set
            {
                if (m_RemindCurrentActivityWhenChangingVPNWithName != value)
                {
                    m_RemindCurrentActivityWhenChangingVPNWithName = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(RemindCurrentActivityWhenChangingVPNWithName)));
                }
            }
        }





        public List<string> TrackedActivities { get; set; }
        public List<string> TrackedActivitiesComments { get; set; }
        public string AlwaysStartNewDayWithActivity { get; set; } = "Product Development";


        public Dictionary<string, List<string>> Tags { get; set; }


        private int m_DefaultWorkingHours = 8;
        public int DefaultWorkingHours
        {
            get
            {
                return m_DefaultWorkingHours;
            }
            set
            {
                if (m_DefaultWorkingHours != value)
                {
                    m_DefaultWorkingHours = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(DefaultWorkingHours)));
                }
            }
        }

        // Working Hours on SA/SO by default automatically 0:

        private int m_DefaultWorkingHoursSaturday = 0;
        public int DefaultWorkingHoursSaturday
        {
            get
            {
                return m_DefaultWorkingHoursSaturday;
            }
            set
            {
                if (m_DefaultWorkingHoursSaturday != value)
                {
                    m_DefaultWorkingHoursSaturday = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(DefaultWorkingHoursSaturday)));
                }
            }
        }

        private int m_DefaultWorkingHoursSunday = 0;
        public int DefaultWorkingHoursSunday
        {
            get
            {
                return m_DefaultWorkingHoursSunday;
            }
            set
            {
                if (m_DefaultWorkingHoursSunday != value)
                {
                    m_DefaultWorkingHoursSunday = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(DefaultWorkingHoursSunday)));
                }
            }
        }

        public int GetDefaultWorkingHours(DateTime day)
        {
            switch (day.DayOfWeek)
            {
                case DayOfWeek.Monday:
                case DayOfWeek.Tuesday:
                case DayOfWeek.Wednesday:
                case DayOfWeek.Thursday:
                case DayOfWeek.Friday:
                default:
                    return DefaultWorkingHours;

                case DayOfWeek.Saturday:
                    return DefaultWorkingHoursSaturday;

                case DayOfWeek.Sunday:
                    return DefaultWorkingHoursSunday;
            }
        }

        public StatisticTypes StatisticType { get; set; } = StatisticTypes.TimeInLieu;
        public StatisticRanges StatisticRange { get; set; } = StatisticRanges.Ever;

        public enum StatisticTypes
        {
            TimeInLieu,
            Activities,
            //WeeklyActivities,
            //ActivityComments
        };

        public enum StatisticRanges
        {
            Ever,

            RecentYear,
            RecentTerm,
            RecentQuarter,
            RecentMonth,
            RecentFortnight,
            RecentWeek,

            SelectedYear,
            SelectedMonth,
            SelectedWeek,
            SelectedDay,
        };

        public string StatisticActivityFilter { get; set; }
        public Dictionary<string, string> StatisticTagCategoryFilter { get; } = new Dictionary<string, string>();


        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }



        public bool DisablePopupNotifications { get; set; } = false;

        public bool IgnoreStampFile { get; set; } = false;


        // Hard-coded:

        public static Regex HHMM { get; } = new Regex("[0-9]{1,2}[:]{1}[0-9]{1,2}");
        public static Regex DDMMYYYY { get; } = new Regex("[0-9]{2}[.]{1}[0-9]{2}[.]{1}[0-9]{4}");
        public static Regex Integer { get; } = new Regex("[^0-9]+");


        public event PropertyChangedEventHandler PropertyChanged = delegate { };


        public void LoadSettings()
        {
            AutomaticPauseRecognition = GetKey("AutomaticPauseRecognition", true);

            StatisticType = (StatisticTypes)GetKey("StatisticsTypeIndex", 0);
            StatisticRange = (StatisticRanges)GetKey("StatisticsTimeIndex", 0);

            WindowWidth = GetKey("WindowWidth", WindowWidth);
            WindowHeight = GetKey("WindowHeight", WindowHeight);

            TrackedActivities = GetKey("TrackedActivities2", String.Empty).Split(new[] { ";;;" }, StringSplitOptions.RemoveEmptyEntries).ToList();
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

            Tags = GetKeyNames()?.Where(k => k.StartsWith("Tags_")).ToDictionary(k => k.Substring("Tags_".Length), k => GetKey<string>(k, String.Empty).Split(new[] { ";;;" }, StringSplitOptions.RemoveEmptyEntries).ToList()) ?? new Dictionary<string, List<string>>();

            AlwaysStartNewDayWithActivity = GetKey("AlwaysStartNewDayWithActivity", (string)null);
            if (AlwaysStartNewDayWithActivity == String.Empty)
                AlwaysStartNewDayWithActivity = null;



            RemindCurrentActivityWhenChangingVPN = GetKey("RemindCurrentActivityWhenChangingVPN", true);
            RemindCurrentActivityWhenChangingVPNWithName = GetKey("RemindCurrentActivityWhenChangingVPNWithName", (string)null);
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

        private string[] GetKeyNames()
        {
            return RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey("Software").OpenSubKey("TimeStamp")?.GetValueNames();
        }

        public void SaveSettings()
        {
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "AutomaticPauseRecognition", AutomaticPauseRecognition);
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "StatisticsTypeIndex", (int)StatisticType);
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "StatisticsTimeIndex", (int)StatisticRange);

            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "WindowWidth", WindowWidth);
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "WindowHeight", WindowHeight);

            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "TrackedActivities2", String.Join(";;;", TrackedActivities));
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "AlwaysStartNewDayWithActivity", AlwaysStartNewDayWithActivity ?? String.Empty);

            foreach (var category in Tags)
            {
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", $"Tags_{category.Key}", String.Join(";;;", category.Value));
            }

            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "RemindCurrentActivityWhenChangingVPN", RemindCurrentActivityWhenChangingVPN);
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TimeStamp\\", "RemindCurrentActivityWhenChangingVPNWithName", RemindCurrentActivityWhenChangingVPNWithName ?? String.Empty);

        }


    }
}
