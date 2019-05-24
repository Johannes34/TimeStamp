using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeStamp.Tests
{
    [TestClass]
    public class StampTests
    {
        private Stamp GetStamp(bool hasOpenEnd = false)
        {
            var stamp = new Stamp();
            stamp.Day = new DateTime(2019, 05, 21);
            stamp.WorkingHours = 8;

            stamp.Begin = new TimeSpan(09, 17, 00);
            stamp.End = hasOpenEnd ? default(TimeSpan) : new TimeSpan(16, 51, 00);
            stamp.Pause = new TimeSpan(00, 00, 00);

            return stamp;
        }

        private Stamp GetOpenEndStamp()
        {
            return GetStamp(true);
        }

        private void AddSingleActivity(Stamp stamp)
        {
            stamp.ActivityRecords.Add(new ActivityRecord() { Activity = "Product Development", Begin = stamp.Begin, End = (stamp.End == default(TimeSpan) ? null : (TimeSpan?)stamp.End) });
        }

        private TimeManager GetManager()
        {
            return new TimeManager(new TimeSettings() { IgnoreStampFile = true, DisablePopupNotifications = true });
        }

        [TestMethod]
        public void TestSetBeginEarlierOnOpenEndStampSetsActivityStartEarlier()
        {
            // Arrange:
            var tm = GetManager();
            var stamp = GetOpenEndStamp();
            AddSingleActivity(stamp);

            var newStart = new TimeSpan(09, 05, 00);

            // Act:
            tm.SetBegin(stamp, newStart);

            // Assert:
            Assert.AreEqual(newStart, stamp.Begin);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.AreEqual(newStart, stamp.ActivityRecords[0].Begin.Value);
        }

        [TestMethod]
        public void TestSetBeginLaterOnOpenEndStampSetsActivityStartLater()
        {
            // Arrange:
            var tm = GetManager();
            var stamp = GetOpenEndStamp();
            AddSingleActivity(stamp);

            var newStart = new TimeSpan(09, 30, 00);

            // Act:
            tm.SetBegin(stamp, newStart);

            // Assert:
            Assert.AreEqual(newStart, stamp.Begin);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.AreEqual(newStart, stamp.ActivityRecords[0].Begin.Value);
        }

        [TestMethod]
        public void TestSetBeginLaterOnOpenEndStampSetsActivityStartLaterAndCutsOffActivity()
        {
            // Arrange:
            var tm = GetManager();
            var stamp = GetOpenEndStamp();

            stamp.ActivityRecords.Add(new ActivityRecord() { Activity = "Product Support", Begin = stamp.Begin, End = new TimeSpan(09, 25, 00) });
            stamp.ActivityRecords.Add(new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(09, 25, 00), End = (stamp.End == default(TimeSpan) ? null : (TimeSpan?)stamp.End) });

            var newStart = new TimeSpan(09, 30, 00);

            // Act:
            tm.SetBegin(stamp, newStart);

            // Assert:
            Assert.AreEqual(newStart, stamp.Begin);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.AreEqual(newStart, stamp.ActivityRecords[0].Begin.Value);
        }



        [TestMethod]
        public void TestSetEndEarlierOnOpenEndStampSetsActivityEndEarlier()
        {
            // Arrange:
            var tm = GetManager();
            var stamp = GetOpenEndStamp();
            AddSingleActivity(stamp);

            var newEnd = new TimeSpan(16, 40, 00);

            // Act:
            tm.SetEnd(stamp, newEnd);

            // Assert:
            Assert.AreEqual(newEnd, stamp.End);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.AreEqual(newEnd, stamp.ActivityRecords[0].End.Value);
        }

        [TestMethod]
        public void TestSetEndEarlierOnStampSetsActivityEndEarlier()
        {
            // Arrange:
            var tm = GetManager();
            var stamp = GetStamp();
            AddSingleActivity(stamp);

            var newEnd = new TimeSpan(16, 40, 00);

            // Act:
            tm.SetEnd(stamp, newEnd);

            // Assert:
            Assert.AreEqual(newEnd, stamp.End);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.AreEqual(newEnd, stamp.ActivityRecords[0].End.Value);
        }

        [TestMethod]
        public void TestSetEndEarlierOnOpenEndStampSetsActivityEndEarlierAndCutsOffActivity()
        {
            // Arrange:
            var tm = GetManager();
            var stamp = GetOpenEndStamp();

            stamp.ActivityRecords.Add(new ActivityRecord() { Activity = "Product Support", Begin = stamp.Begin, End = new TimeSpan(16, 40, 00) });
            stamp.ActivityRecords.Add(new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(16, 40, 00), End = null });

            var newEnd = new TimeSpan(16, 30, 00);

            // Act:
            tm.SetEnd(stamp, newEnd);

            // Assert:
            Assert.AreEqual(newEnd, stamp.End);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.AreEqual(newEnd, stamp.ActivityRecords[0].End.Value);
        }

        [TestMethod]
        public void TestSetEndEarlierOnStampSetsActivityEndEarlierAndCutsOffActivity()
        {
            // Arrange:
            var tm = GetManager();
            var stamp = GetStamp();

            stamp.ActivityRecords.Add(new ActivityRecord() { Activity = "Product Support", Begin = stamp.Begin, End = new TimeSpan(16, 40, 00) });
            stamp.ActivityRecords.Add(new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(16, 40, 00), End = stamp.End });

            var newEnd = new TimeSpan(16, 30, 00);

            // Act:
            tm.SetEnd(stamp, newEnd);

            // Assert:
            Assert.AreEqual(newEnd, stamp.End);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.AreEqual(newEnd, stamp.ActivityRecords[0].End.Value);
        }

        [TestMethod]
        public void TestSetEndLaterOnStampSetsActivityEndLater()
        {
            // Arrange:
            var tm = GetManager();
            var stamp = GetStamp();
            AddSingleActivity(stamp);

            var newEnd = new TimeSpan(17, 20, 00);

            // Act:
            tm.SetEnd(stamp, newEnd);

            // Assert:
            Assert.AreEqual(newEnd, stamp.End);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.AreEqual(newEnd, stamp.ActivityRecords[0].End.Value);
        }



        [TestMethod]
        public void TestSetPauseOnStampSetsSingleActivityEnd()
        {
            // Arrange:
            var tm = GetManager();
            var stamp = GetStamp();
            AddSingleActivity(stamp);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, tm.Total(stamp.ActivityRecords[0]));

            var newPause = new TimeSpan(00, 10, 00);

            // Act: Set Pause from zero to actual value
            tm.SetPause(stamp, newPause);

            // Assert: expect activity ends earlier
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);

            Assert.AreEqual(stamp.End - newPause, stamp.ActivityRecords[0].End.Value);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, tm.Total(stamp.ActivityRecords[0]));


            // Arrange:
            newPause = new TimeSpan(00, 00, 00);

            // Act: Set Pause from actual value to shorter value
            tm.SetPause(stamp, newPause);

            // Assert: expect activity ends later
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);

            Assert.AreEqual(stamp.End, stamp.ActivityRecords[0].End.Value);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, tm.Total(stamp.ActivityRecords[0]));
        }

        [TestMethod]
        public void TestSetPauseOnOpenEndStampSetsSingleActivityBegin()
        {
            // Arrange:
            var tm = GetManager();
            var stamp = GetOpenEndStamp();
            AddSingleActivity(stamp);

            var newPause = new TimeSpan(00, 10, 00);

            // Act: Set Pause from zero to actual value
            tm.SetPause(stamp, newPause);

            // Assert: expect activity begins later
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);

            Assert.AreEqual(stamp.Begin + newPause, stamp.ActivityRecords[0].Begin.Value);


            // Arrange:
            newPause = new TimeSpan(00, 00, 00);

            // Act: Set Pause from actual value to zero
            tm.SetPause(stamp, newPause);

            // Assert: expect activity begins earlier
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);

            Assert.AreEqual(stamp.Begin, stamp.ActivityRecords[0].Begin.Value);
        }

        [TestMethod]
        public void TestSetPauseOnStampSetsInterruptedActivityEnd()
        {
            // Arrange:
            var tm = GetManager();
            var stamp = GetStamp();
            stamp.Pause = new TimeSpan(00, 20, 00);
            var morning = new ActivityRecord() { Activity = "Product Support", Begin = stamp.Begin, End = new TimeSpan(12, 10, 00) };
            var afternoon = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(12, 30, 00), End = stamp.End };
            stamp.ActivityRecords.Add(morning);
            stamp.ActivityRecords.Add(afternoon);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(a => tm.Total(a).TotalMinutes)));

            var newPause = new TimeSpan(00, 65, 00);

            // Act: Set Pause from actual value to higher value
            tm.SetPause(stamp, newPause);

            // Assert: expect interruption is longer
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(2, stamp.ActivityRecords.Count);

            Assert.AreEqual(newPause, afternoon.Begin.Value - morning.End.Value);
            Assert.AreEqual(new TimeSpan(11, 25, 00), morning.End.Value);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(a => tm.Total(a).TotalMinutes)));


            // Arrange:
            newPause = new TimeSpan(00, 00, 00);

            // Act: Set Pause from actual value to zero
            tm.SetPause(stamp, newPause);

            // Assert: expect interruption is zero
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(2, stamp.ActivityRecords.Count);

            Assert.AreEqual(newPause, afternoon.Begin.Value - morning.End.Value);
            Assert.AreEqual(morning.End.Value, afternoon.Begin.Value);
            Assert.AreEqual(new TimeSpan(12, 30, 00), morning.End.Value);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(a => tm.Total(a).TotalMinutes)));
        }

        [TestMethod]
        public void TestSetPauseOnStampSetsInterruptedActivityEndAndCutsOffActivity()
        {
            // Arrange:
            var tm = GetManager();
            var stamp = GetStamp();
            stamp.Pause = new TimeSpan(00, 20, 00);
            var morning = new ActivityRecord() { Activity = "Paid Requirements", Begin = stamp.Begin, End = new TimeSpan(12, 00, 00) };
            var morning2 = new ActivityRecord() { Activity = "Product Support", Begin = new TimeSpan(12, 00, 00), End = new TimeSpan(12, 10, 00) };
            var afternoon = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(12, 30, 00), End = stamp.End };
            stamp.ActivityRecords.Add(morning);
            stamp.ActivityRecords.Add(morning2);
            stamp.ActivityRecords.Add(afternoon);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(a => tm.Total(a).TotalMinutes)));


            // Arrange:
            var newPause = new TimeSpan(00, 05, 00);

            // Act: Set Pause from actual value to lower value
            tm.SetPause(stamp, newPause);

            // Assert: expect interruption is zero
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(3, stamp.ActivityRecords.Count);

            Assert.AreEqual(newPause, afternoon.Begin.Value - morning2.End.Value);
            Assert.AreEqual(new TimeSpan(12, 25, 00), morning2.End.Value);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(a => tm.Total(a).TotalMinutes)));


            // Arrange:
            newPause = new TimeSpan(00, 65, 00);

            // Act: Set Pause from actual value to higher value
            tm.SetPause(stamp, newPause);

            // Assert: expect interruption is longer, fully overlapping activity has been removed
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(2, stamp.ActivityRecords.Count);
            Assert.IsFalse(stamp.ActivityRecords.Contains(morning2));

            Assert.AreEqual(newPause, afternoon.Begin.Value - morning.End.Value);
            Assert.AreEqual(new TimeSpan(11, 25, 00), morning.End.Value);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(a => tm.Total(a).TotalMinutes)));


            // Arrange:
            newPause = new TimeSpan(00, 00, 00);

            // Act: Set Pause from actual value to zero
            tm.SetPause(stamp, newPause);

            // Assert: expect interruption is zero
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(2, stamp.ActivityRecords.Count);

            Assert.AreEqual(newPause, afternoon.Begin.Value - morning.End.Value);
            Assert.AreEqual(morning.End.Value, afternoon.Begin.Value);
            Assert.AreEqual(new TimeSpan(12, 30, 00), morning.End.Value);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(a => tm.Total(a).TotalMinutes)));


            // Arrange:
            newPause = new TimeSpan(00, 10, 00);

            // Act: Set Pause from zero to actual value
            tm.SetPause(stamp, newPause);

            // Assert: expect last activity to end earlier (because the interruption gap is closed from previous pause == 0, and therefore will not be found any more)
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(2, stamp.ActivityRecords.Count);

            Assert.AreEqual(morning.End.Value, afternoon.Begin.Value);
            Assert.AreEqual(stamp.End - stamp.Pause, afternoon.End.Value);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(a => tm.Total(a).TotalMinutes)));


        }

        [TestMethod]
        public void TestSetPauseOnOpenEndStampSetsInterruptedActivityEndAndCutsOffActivity()
        {
            // Arrange:
            var tm = GetManager();
            var stamp = GetOpenEndStamp();
            stamp.Pause = new TimeSpan(00, 20, 00);
            var morning0 = new ActivityRecord() { Activity = "Product Support", Begin = stamp.Begin, End = new TimeSpan(09, 40, 00) };
            var morning = new ActivityRecord() { Activity = "Paid Requirements", Begin = new TimeSpan(09, 40, 00), End = new TimeSpan(12, 00, 00) };
            var morning2 = new ActivityRecord() { Activity = "Product Support", Begin = new TimeSpan(12, 00, 00), End = new TimeSpan(12, 10, 00) };
            var afternoon = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(12, 30, 00), End = null };
            stamp.ActivityRecords.Add(morning0);
            stamp.ActivityRecords.Add(morning);
            stamp.ActivityRecords.Add(morning2);
            stamp.ActivityRecords.Add(afternoon);

            #region similar to previous test run

            // Arrange:
            var newPause = new TimeSpan(00, 05, 00);

            // Act: Set Pause from actual value to lower value
            tm.SetPause(stamp, newPause);

            // Assert: expect interruption is zero
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(4, stamp.ActivityRecords.Count);

            Assert.AreEqual(newPause, afternoon.Begin.Value - morning2.End.Value);
            Assert.AreEqual(new TimeSpan(12, 25, 00), morning2.End.Value);


            // Arrange:
            newPause = new TimeSpan(00, 65, 00);

            // Act: Set Pause from actual value to higher value
            tm.SetPause(stamp, newPause);

            // Assert: expect interruption is longer, fully overlapping activity has been removed
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(3, stamp.ActivityRecords.Count);
            Assert.IsFalse(stamp.ActivityRecords.Contains(morning2));

            Assert.AreEqual(newPause, afternoon.Begin.Value - morning.End.Value);
            Assert.AreEqual(new TimeSpan(11, 25, 00), morning.End.Value);


            // Arrange:
            newPause = new TimeSpan(00, 00, 00);

            // Act: Set Pause from actual value to zero
            tm.SetPause(stamp, newPause);

            // Assert: expect interruption is zero
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(3, stamp.ActivityRecords.Count);

            Assert.AreEqual(newPause, afternoon.Begin.Value - morning.End.Value);
            Assert.AreEqual(morning.End.Value, afternoon.Begin.Value);
            Assert.AreEqual(new TimeSpan(12, 30, 00), morning.End.Value);

            #endregion

            // Arrange:
            newPause = new TimeSpan(00, 10, 00);

            // Act: Set Pause from zero to actual value
            tm.SetPause(stamp, newPause);

            // Assert: expect first activity to start later (because the interruption gap is closed from previous pause == 0, and therefore will not be found any more, plus the open end prevents editing the last end)
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(3, stamp.ActivityRecords.Count);

            Assert.AreEqual(new TimeSpan(09, 27, 00), morning0.Begin.Value);


            // Arrange:
            newPause = new TimeSpan(00, 60, 00);

            // Act: Set Pause to higher value
            tm.SetPause(stamp, newPause);

            // Assert: expect first activity be removed and the next activity to start later
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(2, stamp.ActivityRecords.Count);
            Assert.IsFalse(stamp.ActivityRecords.Contains(morning0));

            Assert.AreEqual(new TimeSpan(10, 17, 00), morning.Begin.Value);

        }
    }
}
