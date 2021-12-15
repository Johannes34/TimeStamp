using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
            var stamp = new Stamp(8);
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

        private void GetEndedStampWithThreeActivitiesWithoutPause(out Stamp stamp, out TimeManager manager, out ActivityRecord first, out ActivityRecord middle, out ActivityRecord last)
        {
            stamp = GetStamp();
            stamp.ActivityRecords.Add(first = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(09, 17, 00), End = new TimeSpan(11, 00, 00) });
            stamp.ActivityRecords.Add(middle = new ActivityRecord() { Activity = "Meeting", Begin = new TimeSpan(11, 00, 00), End = new TimeSpan(14, 30, 00) });
            stamp.ActivityRecords.Add(last = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(14, 30, 00), End = new TimeSpan(16, 51, 00) });

            manager = GetManager();
            manager.StampList = new List<Stamp>() { stamp };
            manager.CurrentShown = stamp;
        }

        private void GetEndedStampWithThreeActivitiesAndPause(out Stamp stamp, out TimeManager manager, out ActivityRecord first, out ActivityRecord middle, out ActivityRecord last)
        {
            stamp = GetStamp();
            stamp.ActivityRecords.Add(first = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(09, 17, 00), End = new TimeSpan(11, 00, 00) });
            stamp.ActivityRecords.Add(middle = new ActivityRecord() { Activity = "Meeting", Begin = new TimeSpan(11, 00, 00), End = new TimeSpan(12, 30, 00) });
            stamp.ActivityRecords.Add(last = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(13, 30, 00), End = new TimeSpan(16, 51, 00) });
            stamp.Pause = new TimeSpan(1, 0, 0);

            manager = GetManager();
            manager.StampList = new List<Stamp>() { stamp };
            manager.CurrentShown = stamp;
        }

        private void GetOpenStampWithThreeActivitiesAndPause(out Stamp stamp, out TimeManager manager, out ActivityRecord first, out ActivityRecord middle, out ActivityRecord last)
        {
            stamp = GetStamp(true);
            stamp.ActivityRecords.Add(first = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(09, 17, 00), End = new TimeSpan(11, 00, 00) });
            stamp.ActivityRecords.Add(middle = new ActivityRecord() { Activity = "Meeting", Begin = new TimeSpan(11, 00, 00), End = new TimeSpan(12, 30, 00) });
            stamp.ActivityRecords.Add(last = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(13, 30, 00), End = null });
            stamp.Pause = new TimeSpan(1, 0, 0);

            manager = GetManager();
            manager.StampList = new List<Stamp>() { stamp };
            manager.CurrentShown = stamp;
        }

        private TimeManager GetManager()
        {
            var man = new TimeManager(new TimeSettings() { IgnoreStampFile = true, DisablePopupNotifications = true });
            PopupDialog.Manager = man;
            return man;
        }

        private void SetNowTime(DateTime now)
        {
            var mockTime = new Mock<TimeProvider>();
            mockTime.SetupGet(t => t.Today).Returns(now.Date);
            mockTime.SetupGet(t => t.Now).Returns(now);
            TimeManager.Time = mockTime.Object;
        }


        #region Test Stamp SetBegin

        [TestMethod]
        public void TestSetBeginEarlierOnOpenEndStampSetsActivityStartEarlier()
        {
            // Arrange:
            var stamp = GetOpenEndStamp();
            AddSingleActivity(stamp);

            var newStart = new TimeSpan(09, 05, 00);

            // Act:
            TimeManager.SetBegin(stamp, newStart);

            // Assert:
            Assert.AreEqual(newStart, stamp.Begin);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.AreEqual(newStart, stamp.ActivityRecords[0].Begin.Value);
        }

        [TestMethod]
        public void TestSetBeginLaterOnOpenEndStampSetsActivityStartLater()
        {
            // Arrange:
            var stamp = GetOpenEndStamp();
            AddSingleActivity(stamp);

            var newStart = new TimeSpan(09, 30, 00);

            // Act:
            TimeManager.SetBegin(stamp, newStart);

            // Assert:
            Assert.AreEqual(newStart, stamp.Begin);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.AreEqual(newStart, stamp.ActivityRecords[0].Begin.Value);
        }

        [TestMethod]
        public void TestSetBeginLaterOnOpenEndStampSetsActivityStartLaterAndCutsOffActivity()
        {
            // Arrange:
            var stamp = GetOpenEndStamp();

            stamp.ActivityRecords.Add(new ActivityRecord() { Activity = "Product Support", Begin = stamp.Begin, End = new TimeSpan(09, 25, 00) });
            stamp.ActivityRecords.Add(new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(09, 25, 00), End = (stamp.End == default(TimeSpan) ? null : (TimeSpan?)stamp.End) });

            var newStart = new TimeSpan(09, 30, 00);

            // Act:
            TimeManager.SetBegin(stamp, newStart);

            // Assert:
            Assert.AreEqual(newStart, stamp.Begin);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.AreEqual(newStart, stamp.ActivityRecords[0].Begin.Value);
        }

        #endregion

        #region Test Stamp SetEnd

        [TestMethod]
        public void TestSetEndEarlierOnOpenEndStampSetsActivityEndEarlier()
        {
            // Arrange:
            var stamp = GetOpenEndStamp();
            AddSingleActivity(stamp);

            var newEnd = new TimeSpan(16, 40, 00);

            // Act:
            TimeManager.SetEnd(stamp, newEnd);

            // Assert:
            Assert.AreEqual(newEnd, stamp.End);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.AreEqual(newEnd, stamp.ActivityRecords[0].End.Value);
        }

        [TestMethod]
        public void TestSetEndEarlierOnStampSetsActivityEndEarlier()
        {
            // Arrange:
            var stamp = GetStamp();
            AddSingleActivity(stamp);

            var newEnd = new TimeSpan(16, 40, 00);

            // Act:
            TimeManager.SetEnd(stamp, newEnd);

            // Assert:
            Assert.AreEqual(newEnd, stamp.End);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.AreEqual(newEnd, stamp.ActivityRecords[0].End.Value);
        }

        [TestMethod]
        public void TestSetEndEarlierOnOpenEndStampSetsActivityEndEarlierAndCutsOffActivity()
        {
            // Arrange:
            var stamp = GetOpenEndStamp();

            stamp.ActivityRecords.Add(new ActivityRecord() { Activity = "Product Support", Begin = stamp.Begin, End = new TimeSpan(16, 40, 00) });
            stamp.ActivityRecords.Add(new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(16, 40, 00), End = null });

            var newEnd = new TimeSpan(16, 30, 00);

            // Act:
            TimeManager.SetEnd(stamp, newEnd);

            // Assert:
            Assert.AreEqual(newEnd, stamp.End);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.AreEqual(newEnd, stamp.ActivityRecords[0].End.Value);
        }

        [TestMethod]
        public void TestSetEndEarlierOnStampSetsActivityEndEarlierAndCutsOffActivity()
        {
            // Arrange:
            var stamp = GetStamp();

            stamp.ActivityRecords.Add(new ActivityRecord() { Activity = "Product Support", Begin = stamp.Begin, End = new TimeSpan(16, 40, 00) });
            stamp.ActivityRecords.Add(new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(16, 40, 00), End = stamp.End });

            var newEnd = new TimeSpan(16, 30, 00);

            // Act:
            TimeManager.SetEnd(stamp, newEnd);

            // Assert:
            Assert.AreEqual(newEnd, stamp.End);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.AreEqual(newEnd, stamp.ActivityRecords[0].End.Value);
        }

        [TestMethod]
        public void TestSetEndLaterOnStampSetsActivityEndLater()
        {
            // Arrange:
            var stamp = GetStamp();
            AddSingleActivity(stamp);

            var newEnd = new TimeSpan(17, 20, 00);

            // Act:
            TimeManager.SetEnd(stamp, newEnd);

            // Assert:
            Assert.AreEqual(newEnd, stamp.End);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.AreEqual(newEnd, stamp.ActivityRecords[0].End.Value);
        }

        #endregion

        #region Test Stamp SetPause

        [TestMethod]
        public void TestSetPauseOnStampWhenDayHasNoPauseYetSetsSingleActivityStart()
        {
            // Arrange:
            var tm = GetManager();
            var stamp = GetStamp();
            AddSingleActivity(stamp);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeManager.Total(stamp.ActivityRecords[0]));

            var newPause = new TimeSpan(00, 10, 00);

            // Act: Set Pause from zero to actual value
            tm.SetPause(stamp, newPause);

            // Assert: expect activity begins later
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);

            Assert.AreEqual(stamp.Begin + newPause, stamp.ActivityRecords[0].Begin.Value);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeManager.Total(stamp.ActivityRecords[0]));


            // Arrange:
            newPause = new TimeSpan(00, 00, 00);

            // Act: Set Pause from actual value to shorter value
            tm.SetPause(stamp, newPause);

            // Assert: expect activity begins earlier
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(1, stamp.ActivityRecords.Count);

            Assert.AreEqual(stamp.Begin, stamp.ActivityRecords[0].Begin.Value);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeManager.Total(stamp.ActivityRecords[0]));
        }

        [TestMethod]
        public void TestSetPauseOnStampWhenDayHasNoPauseYetAndSplitModeSplitsSingleActivity()
        {
            // Arrange:
            var tm = GetManager();
            var stamp = GetStamp();
            AddSingleActivity(stamp);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeManager.Total(stamp.ActivityRecords[0]));

            var newPause = new TimeSpan(00, 10, 00);

            // Act: Set Pause from zero to actual value
            tm.SetPause(stamp, newPause, true);

            // Assert: expect activity begins later
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(2, stamp.ActivityRecords.Count);

            Assert.AreEqual(stamp.Begin, stamp.ActivityRecords[0].Begin.Value);
            Assert.AreEqual(new TimeSpan(12, 59, 0), stamp.ActivityRecords[0].End.Value);

            Assert.AreEqual(new TimeSpan(13, 09, 0), stamp.ActivityRecords[1].Begin.Value);
            Assert.AreEqual(stamp.End, stamp.ActivityRecords[1].End.Value);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, tm.DayTime(stamp));


            // Arrange:
            newPause = new TimeSpan(00, 00, 00);

            // Act: Set Pause from actual value to shorter value
            tm.SetPause(stamp, newPause);

            // Assert: expect activity begins earlier
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(2, stamp.ActivityRecords.Count);

            Assert.AreEqual(stamp.Begin, stamp.ActivityRecords[0].Begin.Value);
            Assert.AreEqual(new TimeSpan(13, 09, 0), stamp.ActivityRecords[0].End.Value);

            Assert.AreEqual(new TimeSpan(13, 09, 0), stamp.ActivityRecords[1].Begin.Value);
            Assert.AreEqual(stamp.End, stamp.ActivityRecords[1].End.Value);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, tm.DayTime(stamp));
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

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(a => TimeManager.Total(a).TotalMinutes)));

            var newPause = new TimeSpan(00, 65, 00);

            // Act: Set Pause from actual value to higher value
            tm.SetPause(stamp, newPause);

            // Assert: expect interruption is longer
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(2, stamp.ActivityRecords.Count);

            Assert.AreEqual(newPause, afternoon.Begin.Value - morning.End.Value);
            Assert.AreEqual(new TimeSpan(11, 25, 00), morning.End.Value);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(a => TimeManager.Total(a).TotalMinutes)));


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

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(a => TimeManager.Total(a).TotalMinutes)));
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

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(a => TimeManager.Total(a).TotalMinutes)));


            // Arrange:
            var newPause = new TimeSpan(00, 05, 00);

            // Act: Set Pause from actual value to lower value
            tm.SetPause(stamp, newPause);

            // Assert: expect interruption is zero
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(3, stamp.ActivityRecords.Count);

            Assert.AreEqual(newPause, afternoon.Begin.Value - morning2.End.Value);
            Assert.AreEqual(new TimeSpan(12, 25, 00), morning2.End.Value);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(a => TimeManager.Total(a).TotalMinutes)));


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

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(a => TimeManager.Total(a).TotalMinutes)));

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

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(a => TimeManager.Total(a).TotalMinutes)));


            // Arrange:
            newPause = new TimeSpan(00, 10, 00);

            // Act: Set Pause from zero to actual value
            tm.SetPause(stamp, newPause);

            // Assert: expect last activity to end earlier (because the interruption gap is closed from previous pause == 0, and therefore will not be found any more)
            Assert.AreEqual(newPause, stamp.Pause);
            Assert.AreEqual(2, stamp.ActivityRecords.Count);

            Assert.AreEqual(morning.End.Value, afternoon.Begin.Value);
            Assert.AreEqual(stamp.End, afternoon.End.Value);
            Assert.AreEqual(stamp.Begin + stamp.Pause, morning.Begin.Value);

            Assert.AreEqual((stamp.End - stamp.Begin) - stamp.Pause, TimeSpan.FromMinutes(stamp.ActivityRecords.Sum(a => TimeManager.Total(a).TotalMinutes)));

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

        #endregion

        #region Test Automatic Pause Recognition 

        [TestMethod]
        public void TestAutomaticPauseRecognition_InLockWhenLeavingMode()
        {
            var tm = TestAutomaticPauseRecognition(true, 22, 22);

            Assert.AreEqual(TimeSpan.FromMinutes(17), tm.StampList.Single().Pause);

            Assert.AreEqual(2, tm.StampList.Single().ActivityRecords.Count);

            Assert.AreEqual(new TimeSpan(12, 20, 00), tm.StampList.Single().ActivityRecords.ElementAt(0).Begin);
            Assert.AreEqual(new TimeSpan(12, 22, 00), tm.StampList.Single().ActivityRecords.ElementAt(0).End);
            Assert.IsNull(tm.StampList.Single().ActivityRecords.ElementAt(0).Comment);
            Assert.AreEqual("Product Development", tm.StampList.Single().ActivityRecords.ElementAt(0).Activity);

            Assert.AreEqual(new TimeSpan(12, 39, 00), tm.StampList.Single().ActivityRecords.ElementAt(1).Begin);
            Assert.IsNull(tm.StampList.Single().ActivityRecords.ElementAt(1).End);
            Assert.IsTrue(String.IsNullOrEmpty(tm.StampList.Single().ActivityRecords.ElementAt(1).Comment));
            Assert.AreEqual("Product Development", tm.StampList.Single().ActivityRecords.ElementAt(1).Activity);

            Assert.IsFalse(tm.IsInPauseTimeRecognitionMode);
            Assert.IsFalse(tm.IsQualifiedPauseBreak);
        }

        [TestMethod]
        public void TestAutomaticPauseRecognition_WithIdleBeforeLogOut_InLockWhenLeavingMode()
        {
            var tm = TestAutomaticPauseRecognition(true, 22, 25);

            Assert.AreEqual(TimeSpan.FromMinutes(14), tm.StampList.Single().Pause);

            Assert.AreEqual(2, tm.StampList.Single().ActivityRecords.Count);

            Assert.AreEqual(new TimeSpan(12, 20, 00), tm.StampList.Single().ActivityRecords.ElementAt(0).Begin);
            Assert.AreEqual(new TimeSpan(12, 25, 00), tm.StampList.Single().ActivityRecords.ElementAt(0).End);
            Assert.IsNull(tm.StampList.Single().ActivityRecords.ElementAt(0).Comment);
            Assert.AreEqual("Product Development", tm.StampList.Single().ActivityRecords.ElementAt(0).Activity);

            Assert.AreEqual(new TimeSpan(12, 39, 00), tm.StampList.Single().ActivityRecords.ElementAt(1).Begin);
            Assert.IsNull(tm.StampList.Single().ActivityRecords.ElementAt(1).End);
            Assert.IsTrue(String.IsNullOrEmpty(tm.StampList.Single().ActivityRecords.ElementAt(1).Comment));
            Assert.AreEqual("Product Development", tm.StampList.Single().ActivityRecords.ElementAt(1).Activity);

            Assert.IsFalse(tm.IsInPauseTimeRecognitionMode);
            Assert.IsFalse(tm.IsQualifiedPauseBreak);
        }

        [TestMethod]
        public void TestAutomaticPauseRecognition_WithMovementAfterLogOut_InLockWhenLeavingMode()
        {
            var tm = TestAutomaticPauseRecognition(true, 24, 21);

            Assert.AreEqual(TimeSpan.FromMinutes(18), tm.StampList.Single().Pause);

            Assert.AreEqual(2, tm.StampList.Single().ActivityRecords.Count);

            Assert.AreEqual(new TimeSpan(12, 20, 00), tm.StampList.Single().ActivityRecords.ElementAt(0).Begin);
            Assert.AreEqual(new TimeSpan(12, 21, 00), tm.StampList.Single().ActivityRecords.ElementAt(0).End);
            Assert.IsNull(tm.StampList.Single().ActivityRecords.ElementAt(0).Comment);
            Assert.AreEqual("Product Development", tm.StampList.Single().ActivityRecords.ElementAt(0).Activity);

            Assert.AreEqual(new TimeSpan(12, 39, 00), tm.StampList.Single().ActivityRecords.ElementAt(1).Begin);
            Assert.IsNull(tm.StampList.Single().ActivityRecords.ElementAt(1).End);
            Assert.IsTrue(String.IsNullOrEmpty(tm.StampList.Single().ActivityRecords.ElementAt(1).Comment));
            Assert.AreEqual("Product Development", tm.StampList.Single().ActivityRecords.ElementAt(1).Activity);

            Assert.IsFalse(tm.IsInPauseTimeRecognitionMode);
            Assert.IsFalse(tm.IsQualifiedPauseBreak);
        }

        [TestMethod]
        public void TestAutomaticPauseRecognition_NotLockWhenLeavingMode()
        {
            var tm = TestAutomaticPauseRecognition(false, 22, 22);

            Assert.AreEqual(TimeSpan.FromMinutes(17), tm.StampList.Single().Pause);

            Assert.AreEqual(2, tm.StampList.Single().ActivityRecords.Count);

            Assert.AreEqual(new TimeSpan(12, 20, 00), tm.StampList.Single().ActivityRecords.ElementAt(0).Begin);
            Assert.AreEqual(new TimeSpan(12, 22, 00), tm.StampList.Single().ActivityRecords.ElementAt(0).End);
            Assert.IsNull(tm.StampList.Single().ActivityRecords.ElementAt(0).Comment);
            Assert.AreEqual("Product Development", tm.StampList.Single().ActivityRecords.ElementAt(0).Activity);

            Assert.AreEqual(new TimeSpan(12, 39, 00), tm.StampList.Single().ActivityRecords.ElementAt(1).Begin);
            Assert.IsNull(tm.StampList.Single().ActivityRecords.ElementAt(1).End);
            Assert.IsTrue(String.IsNullOrEmpty(tm.StampList.Single().ActivityRecords.ElementAt(1).Comment));
            Assert.AreEqual("Product Development", tm.StampList.Single().ActivityRecords.ElementAt(1).Activity);

            Assert.IsFalse(tm.IsInPauseTimeRecognitionMode);
            Assert.IsFalse(tm.IsQualifiedPauseBreak);
        }

        [TestMethod]
        public void TestAutomaticPauseRecognition_WithIdleBeforeLogOut_NotLockWhenLeavingMode()
        {
            var tm = TestAutomaticPauseRecognition(false, 22, 25);

            Assert.AreEqual(TimeSpan.FromMinutes(17), tm.StampList.Single().Pause);

            Assert.AreEqual(2, tm.StampList.Single().ActivityRecords.Count);

            Assert.AreEqual(new TimeSpan(12, 20, 00), tm.StampList.Single().ActivityRecords.ElementAt(0).Begin);
            Assert.AreEqual(new TimeSpan(12, 22, 00), tm.StampList.Single().ActivityRecords.ElementAt(0).End);
            Assert.IsNull(tm.StampList.Single().ActivityRecords.ElementAt(0).Comment);
            Assert.AreEqual("Product Development", tm.StampList.Single().ActivityRecords.ElementAt(0).Activity);

            Assert.AreEqual(new TimeSpan(12, 39, 00), tm.StampList.Single().ActivityRecords.ElementAt(1).Begin);
            Assert.IsNull(tm.StampList.Single().ActivityRecords.ElementAt(1).End);
            Assert.IsTrue(String.IsNullOrEmpty(tm.StampList.Single().ActivityRecords.ElementAt(1).Comment));
            Assert.AreEqual("Product Development", tm.StampList.Single().ActivityRecords.ElementAt(1).Activity);

            Assert.IsFalse(tm.IsInPauseTimeRecognitionMode);
            Assert.IsFalse(tm.IsQualifiedPauseBreak);
        }

        [TestMethod]
        public void TestAutomaticPauseRecognition_WithMovementAfterLogOut_NotLockWhenLeavingMode()
        {
            var tm = TestAutomaticPauseRecognition(false, 24, 21);

            Assert.AreEqual(TimeSpan.FromMinutes(15), tm.StampList.Single().Pause);

            Assert.AreEqual(2, tm.StampList.Single().ActivityRecords.Count);

            Assert.AreEqual(new TimeSpan(12, 20, 00), tm.StampList.Single().ActivityRecords.ElementAt(0).Begin);
            Assert.AreEqual(new TimeSpan(12, 24, 00), tm.StampList.Single().ActivityRecords.ElementAt(0).End);
            Assert.IsNull(tm.StampList.Single().ActivityRecords.ElementAt(0).Comment);
            Assert.AreEqual("Product Development", tm.StampList.Single().ActivityRecords.ElementAt(0).Activity);

            Assert.AreEqual(new TimeSpan(12, 39, 00), tm.StampList.Single().ActivityRecords.ElementAt(1).Begin);
            Assert.IsNull(tm.StampList.Single().ActivityRecords.ElementAt(1).End);
            Assert.IsTrue(String.IsNullOrEmpty(tm.StampList.Single().ActivityRecords.ElementAt(1).Comment));
            Assert.AreEqual("Product Development", tm.StampList.Single().ActivityRecords.ElementAt(1).Activity);

            Assert.IsFalse(tm.IsInPauseTimeRecognitionMode);
            Assert.IsFalse(tm.IsQualifiedPauseBreak);
        }

        private TimeManager TestAutomaticPauseRecognition(bool lockWhenLeavingMode, int lastMouseMoveMinute, int logOffMinute)
        {
            var tm = new TimeManager(new TimeSettings()
            {
                AutomaticPauseRecognition = true,
                AutomaticPauseRecognitionStartTime = new TimeSpan(11, 30, 00),
                AutomaticPauseRecognitionStopTime = new TimeSpan(13, 00, 00),
                AutomaticPauseRecognitionMinPauseTime = 10,
                IsLockingComputerWhenLeaving = lockWhenLeavingMode,
                DisablePopupNotifications = true,
                IgnoreStampFile = true,
            });

            PopupDialog.Manager = tm;

            var mockTime = new Mock<TimeProvider>();
            mockTime.SetupGet(t => t.Today).Returns(new DateTime(2019, 05, 24));
            mockTime.SetupGet(t => t.Now).Returns(new DateTime(2019, 05, 24, 12, 20, 00));

            TimeManager.Time = mockTime.Object;

            tm.Initialize();

            // assert initial state:
            Assert.AreEqual(1, tm.StampList.Count);

            Assert.AreEqual(new DateTime(2019, 05, 24), tm.StampList.Single().Day);
            Assert.AreEqual(8, tm.StampList.Single().WorkingHours);
            Assert.AreEqual(new TimeSpan(12, 20, 00), tm.StampList.Single().Begin);
            Assert.AreEqual(TimeSpan.Zero, tm.StampList.Single().End);
            Assert.AreEqual(TimeSpan.Zero, tm.StampList.Single().Pause);

            Assert.AreEqual(1, tm.StampList.Single().ActivityRecords.Count);
            Assert.AreEqual(new TimeSpan(12, 20, 00), tm.StampList.Single().ActivityRecords.Single().Begin);
            Assert.IsNull(tm.StampList.Single().ActivityRecords.Single().End);
            Assert.IsNull(tm.StampList.Single().ActivityRecords.Single().Comment);
            Assert.AreEqual("Product Development", tm.StampList.Single().ActivityRecords.Single().Activity);

            Assert.IsTrue(tm.IsInPauseTimeRecognitionMode);
            Assert.IsFalse(tm.IsQualifiedPauseBreak);

            // simulate last mouse moved:

            tm.LastUserAction = new DateTime(2019, 05, 24, 12, lastMouseMoveMinute, 00);
            mockTime.SetupGet(t => t.Now).Returns(new DateTime(2019, 05, 24, 12, logOffMinute, 00));


            Assert.IsTrue(tm.IsInPauseTimeRecognitionMode);
            Assert.IsFalse(tm.IsQualifiedPauseBreak);

            // simulate sleep and time change:
            tm.SuspendStamping();

            mockTime.SetupGet(t => t.Now).Returns(new DateTime(2019, 05, 24, 12, 39, 00));

            Assert.IsTrue(tm.IsInPauseTimeRecognitionMode);
            Assert.IsTrue(tm.IsQualifiedPauseBreak);

            // simulate resuming after pause:

            tm.ResumeStamping();


            // assert new state:
            Assert.AreEqual(1, tm.StampList.Count);

            Assert.AreEqual(new DateTime(2019, 05, 24), tm.StampList.Single().Day);
            Assert.AreEqual(8, tm.StampList.Single().WorkingHours);
            Assert.AreEqual(new TimeSpan(12, 20, 00), tm.StampList.Single().Begin);
            Assert.AreEqual(TimeSpan.Zero, tm.StampList.Single().End);

            return tm;
        }

        #endregion

        #region Test Activity SetBegin

        [TestMethod]
        public void TestSetActivityStartEarlierUpdatesPreviousActivityEnd()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesWithoutPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityBegin(stamp, middle, new TimeSpan(10, 30, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(10, 30, 00), first.End);

            Assert.AreEqual(new TimeSpan(10, 30, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(14, 30, 00), middle.End);

            Assert.AreEqual(new TimeSpan(14, 30, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestSetActivityStartLaterLeavesPreviousActivityEndAndGeneratesPauseTime()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesWithoutPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityBegin(stamp, middle, new TimeSpan(11, 30, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00/*11, 30, 00*/), first.End);

            Assert.AreEqual(new TimeSpan(11, 30, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(14, 30, 00), middle.End);

            Assert.AreEqual(new TimeSpan(14, 30, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 30, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestSetActivityStartEarlierRemovesOverlaidPreviousActivityAndUpdatesStampBegin()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesWithoutPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityBegin(stamp, middle, new TimeSpan(08, 25, 00));

            // Assert:

            Assert.AreEqual(2, stamp.ActivityRecords.Count);
            Assert.IsFalse(stamp.ActivityRecords.Contains(first));

            Assert.AreEqual(new TimeSpan(08, 25, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(14, 30, 00), middle.End);

            Assert.AreEqual(new TimeSpan(14, 30, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(08, 25, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestSetActivityStartEarlierRemovesAllOverlaidPreviousActivitiesAndUpdatesStampBegin()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesWithoutPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityBegin(stamp, last, new TimeSpan(06, 00, 00));

            // Assert:

            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.IsTrue(stamp.ActivityRecords.Contains(last));

            Assert.AreEqual(new TimeSpan(06, 00, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(06, 00, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestSetActivityStartEarlierRemovesOverlaidPreviousActivityAndUpdatesPreviousActivityEnd()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesWithoutPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityBegin(stamp, last, new TimeSpan(10, 00, 00));

            // Assert:

            Assert.AreEqual(2, stamp.ActivityRecords.Count);
            Assert.IsFalse(stamp.ActivityRecords.Contains(middle));

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(10, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(10, 00, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestSetActivityStartEarlierUpdatesStampBegin()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesWithoutPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityBegin(stamp, first, new TimeSpan(09, 10, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 10, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(14, 30, 00), middle.End);

            Assert.AreEqual(new TimeSpan(14, 30, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(09, 10, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestSetActivityStartLaterUpdatesStampBegin()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesWithoutPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityBegin(stamp, first, new TimeSpan(09, 50, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 50, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(14, 30, 00), middle.End);

            Assert.AreEqual(new TimeSpan(14, 30, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(09, 50, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }



        [TestMethod]
        public void TestEndedDaySetActivityStartEarlierShortensPause()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var middle, out var last);

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(12, 30, 00), middle.End);

            Assert.AreEqual(new TimeSpan(13, 30, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(1, 0, 0), stamp.Pause);

            // Act:

            TimeManager.SetActivityBegin(stamp, last, new TimeSpan(13, 10, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(12, 30, 00), middle.End);

            Assert.AreEqual(new TimeSpan(13, 10, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 40, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestEndedDaySetActivityStartLaterIncreasesPause()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityBegin(stamp, last, new TimeSpan(13, 55, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(12, 30, 00), middle.End);

            Assert.AreEqual(new TimeSpan(13, 55, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(1, 25, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestEndedDaySetActivityStartEarlierClearsPauseAndUpdatesPreviousActivityEnd()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityBegin(stamp, last, new TimeSpan(12, 00, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(12, 00, 00), middle.End);

            Assert.AreEqual(new TimeSpan(12, 00, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestEndedDaySetActivityStartEarlierClearsPauseAndRemovesAllOverlaidPreviousActivitiesAndUpdatesStampBegin()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityBegin(stamp, last, new TimeSpan(08, 00, 00));

            // Assert:

            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.IsTrue(stamp.ActivityRecords.Contains(last));

            Assert.AreEqual(new TimeSpan(08, 00, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(08, 00, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }



        [TestMethod]
        public void TestOpenDaySetActivityStartEarlierShortensPause()
        {
            // Arrange:

            GetOpenStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var middle, out var last);

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(12, 30, 00), middle.End);

            Assert.AreEqual(new TimeSpan(13, 30, 00), last.Begin);
            Assert.AreEqual(null, last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.End);
            Assert.AreEqual(new TimeSpan(1, 0, 0), stamp.Pause);

            // Act:

            TimeManager.SetActivityBegin(stamp, last, new TimeSpan(13, 10, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(12, 30, 00), middle.End);

            Assert.AreEqual(new TimeSpan(13, 10, 00), last.Begin);
            Assert.AreEqual(null, last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 40, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestOpenDaySetActivityStartLaterIncreasesPause()
        {
            // Arrange:

            GetOpenStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityBegin(stamp, last, new TimeSpan(13, 55, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(12, 30, 00), middle.End);

            Assert.AreEqual(new TimeSpan(13, 55, 00), last.Begin);
            Assert.AreEqual(null, last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.End);
            Assert.AreEqual(new TimeSpan(1, 25, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestOpenDaySetActivityStartEarlierClearsPauseAndUpdatesPreviousActivityEnd()
        {
            // Arrange:

            GetOpenStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityBegin(stamp, last, new TimeSpan(12, 00, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(12, 00, 00), middle.End);

            Assert.AreEqual(new TimeSpan(12, 00, 00), last.Begin);
            Assert.AreEqual(null, last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestOpenDaySetActivityStartEarlierClearsPauseAndRemovesAllOverlaidPreviousActivitiesAndUpdatesStampBegin()
        {
            // Arrange:

            GetOpenStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityBegin(stamp, last, new TimeSpan(08, 00, 00));

            // Assert:

            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.IsTrue(stamp.ActivityRecords.Contains(last));

            Assert.AreEqual(new TimeSpan(08, 00, 00), last.Begin);
            Assert.AreEqual(null, last.End);

            Assert.AreEqual(new TimeSpan(08, 00, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }

        #endregion

        #region Test Activity SetEnd

        [TestMethod]
        public void TestSetActivityEndLaterUpdatesFollowingActivityBegin()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesWithoutPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityEnd(stamp, middle, new TimeSpan(14, 55, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(14, 55, 00), middle.End);

            Assert.AreEqual(new TimeSpan(14, 55, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestSetActivityEndEarlierLeavesFollowingActivityBeginAndGeneratesPauseTime()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesWithoutPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityEnd(stamp, middle, new TimeSpan(14, 12, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(14, 12, 00), middle.End);

            Assert.AreEqual(new TimeSpan(14, 30, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 18, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestSetActivityEndLaterRemovesOverlaidFollowingActivityAndUpdatesStampEnd()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesWithoutPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityEnd(stamp, middle, new TimeSpan(17, 25, 00));

            // Assert:

            Assert.AreEqual(2, stamp.ActivityRecords.Count);
            Assert.IsFalse(stamp.ActivityRecords.Contains(last));

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(17, 25, 00), middle.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(17, 25, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestSetActivityEndLaterRemovesAllOverlaidFollowingActivitiesAndUpdatesStampEnd()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesWithoutPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityEnd(stamp, first, new TimeSpan(19, 00, 00));

            // Assert:

            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.IsTrue(stamp.ActivityRecords.Contains(first));

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(19, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(19, 00, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestSetActivityEndLaterRemovesOverlaidFollowingActivityAndUpdatesFollowingActivityBegin()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesWithoutPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityEnd(stamp, first, new TimeSpan(15, 10, 00));

            // Assert:

            Assert.AreEqual(2, stamp.ActivityRecords.Count);
            Assert.IsFalse(stamp.ActivityRecords.Contains(middle));

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(15, 10, 00), first.End);

            Assert.AreEqual(new TimeSpan(15, 10, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestSetActivityEndEarlierUpdatesStampEnd()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesWithoutPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityEnd(stamp, last, new TimeSpan(16, 00, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(14, 30, 00), middle.End);

            Assert.AreEqual(new TimeSpan(14, 30, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 00, 00), last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 00, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestSetActivityEndLaterUpdatesStampEnd()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesWithoutPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityEnd(stamp, last, new TimeSpan(18, 20, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(14, 30, 00), middle.End);

            Assert.AreEqual(new TimeSpan(14, 30, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(18, 20, 00), last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(18, 20, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }



        [TestMethod]
        public void TestEndedDaySetActivityEndEarlierIncreasesPause()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var middle, out var last);

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(12, 30, 00), middle.End);

            Assert.AreEqual(new TimeSpan(13, 30, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(1, 0, 0), stamp.Pause);

            // Act:

            TimeManager.SetActivityEnd(stamp, middle, new TimeSpan(12, 15, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(12, 15, 00), middle.End);

            Assert.AreEqual(new TimeSpan(13, 30, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(1, 15, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestEndedDaySetActivityEndLaterShortensPause()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityEnd(stamp, middle, new TimeSpan(13, 00, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(13, 00, 00), middle.End);

            Assert.AreEqual(new TimeSpan(13, 30, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 30, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestEndedDaySetActivityEndLaterClearsPauseAndUpdatesFollowingActivityBegin()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityEnd(stamp, middle, new TimeSpan(14, 00, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(14, 00, 00), middle.End);

            Assert.AreEqual(new TimeSpan(14, 00, 00), last.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(16, 51, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestEndedDaySetActivityEndLaterClearsPauseAndRemovesAllOverlaidFollowingActivitiesAndUpdatesStampEnd()
        {
            // Arrange:

            GetEndedStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityEnd(stamp, first, new TimeSpan(19, 00, 00));

            // Assert:

            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.IsTrue(stamp.ActivityRecords.Contains(first));

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(19, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(19, 00, 00), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }



        [TestMethod]
        public void TestOpenDaySetActivityEndLaterShortensPause()
        {
            // Arrange:

            GetOpenStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var middle, out var last);

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(12, 30, 00), middle.End);

            Assert.AreEqual(new TimeSpan(13, 30, 00), last.Begin);
            Assert.AreEqual(null, last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.End);
            Assert.AreEqual(new TimeSpan(1, 0, 0), stamp.Pause);

            // Act:

            TimeManager.SetActivityEnd(stamp, middle, new TimeSpan(12, 50, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(12, 50, 00), middle.End);

            Assert.AreEqual(new TimeSpan(13, 30, 00), last.Begin);
            Assert.AreEqual(null, last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 40, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestOpenDaySetActivityEndEarlierIncreasesPause()
        {
            // Arrange:

            GetOpenStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityEnd(stamp, middle, new TimeSpan(12, 00, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(12, 00, 00), middle.End);

            Assert.AreEqual(new TimeSpan(13, 30, 00), last.Begin);
            Assert.AreEqual(null, last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.End);
            Assert.AreEqual(new TimeSpan(1, 30, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestOpenDaySetActivityEndLaterClearsPauseAndUpdatesFollowingOpenEndActivityBegin()
        {
            // Arrange:

            GetOpenStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var middle, out var last);

            var mockTime = new Mock<TimeProvider>();
            mockTime.SetupGet(t => t.Today).Returns(new DateTime(2019, 05, 21));
            mockTime.SetupGet(t => t.Now).Returns(new DateTime(2019, 05, 21, 16, 20, 00)); // 'now' is later than 14:30
            TimeManager.Time = mockTime.Object;

            // Act:

            TimeManager.SetActivityEnd(stamp, middle, new TimeSpan(14, 30, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(14, 30, 00), middle.End);

            Assert.AreEqual(new TimeSpan(14, 30, 00), last.Begin);
            Assert.AreEqual(null, last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestOpenDaySetActivityEndLaterClearsPauseAndRemovesFollowingOpenEndActivityAndSetDaysEnd()
        {
            // Arrange:

            GetOpenStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var middle, out var last);

            var mockTime = new Mock<TimeProvider>();
            mockTime.SetupGet(t => t.Today).Returns(new DateTime(2019, 05, 21));
            mockTime.SetupGet(t => t.Now).Returns(new DateTime(2019, 05, 21, 14, 00, 00)); // 'now' is before 14:30
            TimeManager.Time = mockTime.Object;

            // Act:

            TimeManager.SetActivityEnd(stamp, middle, new TimeSpan(14, 30, 00));

            // Assert:

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(11, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(11, 00, 00), middle.Begin);
            Assert.AreEqual(new TimeSpan(14, 30, 00), middle.End);

            Assert.AreEqual(new TimeSpan(14, 30, 00), last.Begin);
            Assert.AreEqual(null, last.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(14, 30, 0), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }

        [TestMethod]
        public void TestOpenDaySetActivityEndLaterClearsPauseAndRemovesAllOverlaidFollowingActivitiesAndUpdatesStampEnd()
        {
            // Arrange:

            GetOpenStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var middle, out var last);

            // Act:

            TimeManager.SetActivityEnd(stamp, first, new TimeSpan(19, 00, 00));

            // Assert:

            Assert.AreEqual(1, stamp.ActivityRecords.Count);
            Assert.IsTrue(stamp.ActivityRecords.Contains(first));

            Assert.AreEqual(new TimeSpan(09, 17, 00), first.Begin);
            Assert.AreEqual(new TimeSpan(19, 00, 00), first.End);

            Assert.AreEqual(new TimeSpan(09, 17, 00), stamp.Begin);
            Assert.AreEqual(new TimeSpan(19, 00, 0), stamp.End);
            Assert.AreEqual(new TimeSpan(0, 0, 0), stamp.Pause);
        }




        #endregion

        #region Test Delete Activity

        [TestMethod]
        public void TestCanDeleteActivity()
        {
            var manager = GetManager();

            var stamp = GetStamp();

            Assert.IsFalse(manager.CanDeleteActivity(stamp, stamp.ActivityRecords.FirstOrDefault()));

            stamp.ActivityRecords.Add(new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(09, 17, 00), End = new TimeSpan(11, 00, 00) });

            Assert.IsFalse(manager.CanDeleteActivity(stamp, stamp.ActivityRecords.FirstOrDefault()));

            stamp.ActivityRecords.Add(new ActivityRecord() { Activity = "Meeting", Begin = new TimeSpan(11, 00, 00), End = new TimeSpan(12, 30, 00) });

            Assert.IsTrue(manager.CanDeleteActivity(stamp, stamp.ActivityRecords.FirstOrDefault()));

            stamp.ActivityRecords.Add(new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(13, 30, 00), End = null });

            Assert.IsTrue(manager.CanDeleteActivity(stamp, stamp.ActivityRecords.FirstOrDefault()));
        }

        [TestMethod]
        public void TestDeleteFirstActivitySetsDaysBegin()
        {
            // Arrange:
            var manager = GetManager();
            var stamp = GetStamp(true);
            var first = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(09, 17, 00), End = new TimeSpan(11, 00, 00) };
            stamp.ActivityRecords.Add(first);
            var middle = new ActivityRecord() { Activity = "Meeting", Begin = new TimeSpan(11, 00, 00), End = new TimeSpan(12, 30, 00) };
            stamp.ActivityRecords.Add(middle);
            var last = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(13, 30, 00), End = null };
            stamp.ActivityRecords.Add(last);

            manager.CurrentShown = stamp;
            manager.TodayCurrentActivity = last;
            stamp.Pause = new TimeSpan(1, 0, 0);

            // Act:
            manager.DeleteActivity(stamp, first);

            // Assert:
            // deleted:
            Assert.AreEqual(2, stamp.ActivityRecords.Count);
            Assert.IsFalse(stamp.ActivityRecords.Contains(first));

            // updated:
            Assert.AreEqual(middle.Begin.Value, stamp.Begin);

            // still same:
            Assert.AreEqual(1d, stamp.Pause.TotalHours);
            Assert.AreEqual(default(TimeSpan), stamp.End);
        }

        [TestMethod]
        public void TestDeleteFirstActivityFollowedByPauseSetsDaysBeginAndShortensPauseTime()
        {
            // Arrange:
            var manager = GetManager();
            var stamp = GetStamp(true);
            var first = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(09, 17, 00), End = new TimeSpan(11, 00, 00) };
            stamp.ActivityRecords.Add(first);
            var middle = new ActivityRecord() { Activity = "Meeting", Begin = new TimeSpan(12, 00, 00), End = new TimeSpan(12, 30, 00) };
            stamp.ActivityRecords.Add(middle);
            var last = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(12, 30, 00), End = null };
            stamp.ActivityRecords.Add(last);

            manager.CurrentShown = stamp;
            manager.TodayCurrentActivity = last;
            stamp.Pause = new TimeSpan(1, 0, 0);

            // Act:
            manager.DeleteActivity(stamp, first);

            // Assert:
            // deleted:
            Assert.AreEqual(2, stamp.ActivityRecords.Count);
            Assert.IsFalse(stamp.ActivityRecords.Contains(first));

            // updated:
            Assert.AreEqual(middle.Begin.Value, stamp.Begin);
            Assert.AreEqual(0d, stamp.Pause.TotalHours);

            // still same:
            Assert.AreEqual(default(TimeSpan), stamp.End);
        }

        [TestMethod]
        public void TestDeleteLastActivitySetsDaysEnd()
        {
            // Arrange:
            var manager = GetManager();
            var stamp = GetStamp();
            var first = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(09, 17, 00), End = new TimeSpan(11, 00, 00) };
            stamp.ActivityRecords.Add(first);
            var middle = new ActivityRecord() { Activity = "Meeting", Begin = new TimeSpan(12, 00, 00), End = new TimeSpan(12, 30, 00) };
            stamp.ActivityRecords.Add(middle);
            var last = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(12, 30, 00), End = new TimeSpan(16, 51, 00) };
            stamp.ActivityRecords.Add(last);

            manager.CurrentShown = stamp;
            stamp.Pause = new TimeSpan(1, 0, 0);

            // Act:
            manager.DeleteActivity(stamp, last);

            // Assert:
            // deleted:
            Assert.AreEqual(2, stamp.ActivityRecords.Count);
            Assert.IsFalse(stamp.ActivityRecords.Contains(last));

            // updated:
            Assert.AreEqual(middle.End.Value, stamp.End);

            // still same:
            Assert.AreEqual(first.Begin.Value, stamp.Begin);
            Assert.AreEqual(1d, stamp.Pause.TotalHours);
        }

        [TestMethod]
        public void TestDeleteLastActivityFollowingPauseSetsDaysEndAndShortensPauseTime()
        {
            // Arrange:
            var manager = GetManager();
            var stamp = GetStamp();
            var first = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(09, 17, 00), End = new TimeSpan(11, 00, 00) };
            stamp.ActivityRecords.Add(first);
            var middle = new ActivityRecord() { Activity = "Meeting", Begin = new TimeSpan(11, 00, 00), End = new TimeSpan(12, 30, 00) };
            stamp.ActivityRecords.Add(middle);
            var last = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(13, 30, 00), End = new TimeSpan(16, 51, 00) };
            stamp.ActivityRecords.Add(last);

            manager.CurrentShown = stamp;
            stamp.Pause = new TimeSpan(1, 0, 0);

            // Act:
            manager.DeleteActivity(stamp, last);

            // Assert:
            // deleted:
            Assert.AreEqual(2, stamp.ActivityRecords.Count);
            Assert.IsFalse(stamp.ActivityRecords.Contains(last));

            // updated:
            Assert.AreEqual(middle.End.Value, stamp.End);
            Assert.AreEqual(0d, stamp.Pause.TotalHours);

            // still same:
            Assert.AreEqual(first.Begin.Value, stamp.Begin);
        }

        [TestMethod]
        public void TestDeleteCurrentActivitySetsDaysEnd()
        {
            // Arrange:
            var manager = GetManager();
            var stamp = GetStamp(true);
            var first = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(09, 17, 00), End = new TimeSpan(11, 00, 00) };
            stamp.ActivityRecords.Add(first);
            var middle = new ActivityRecord() { Activity = "Meeting", Begin = new TimeSpan(12, 00, 00), End = new TimeSpan(12, 30, 00) };
            stamp.ActivityRecords.Add(middle);
            var last = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(12, 30, 00), End = null };
            stamp.ActivityRecords.Add(last);

            manager.CurrentShown = stamp;
            manager.TodayCurrentActivity = last;
            stamp.Pause = new TimeSpan(1, 0, 0);

            // Act:
            manager.DeleteActivity(stamp, last);

            // Assert:
            // deleted:
            Assert.AreEqual(2, stamp.ActivityRecords.Count);
            Assert.IsFalse(stamp.ActivityRecords.Contains(last));
            Assert.IsNull(manager.TodayCurrentActivity);

            // updated:
            Assert.AreEqual(middle.End.Value, stamp.End);

            // still same:
            Assert.AreEqual(first.Begin.Value, stamp.Begin);
            Assert.AreEqual(1d, stamp.Pause.TotalHours);
        }

        [TestMethod]
        public void TestDeleteCurrentActivityFollowingPauseSetsDaysEndAndShortensPauseTime()
        {
            // Arrange:
            var manager = GetManager();
            var stamp = GetStamp(true);
            var first = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(09, 17, 00), End = new TimeSpan(11, 00, 00) };
            stamp.ActivityRecords.Add(first);
            var middle = new ActivityRecord() { Activity = "Meeting", Begin = new TimeSpan(11, 00, 00), End = new TimeSpan(12, 30, 00) };
            stamp.ActivityRecords.Add(middle);
            var last = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(13, 30, 00), End = null };
            stamp.ActivityRecords.Add(last);

            manager.CurrentShown = stamp;
            manager.TodayCurrentActivity = last;
            stamp.Pause = new TimeSpan(1, 0, 0);

            // Act:
            manager.DeleteActivity(stamp, last);

            // Assert:
            // deleted:
            Assert.AreEqual(2, stamp.ActivityRecords.Count);
            Assert.IsFalse(stamp.ActivityRecords.Contains(last));
            Assert.IsNull(manager.TodayCurrentActivity);

            // updated:
            Assert.AreEqual(middle.End.Value, stamp.End);
            Assert.AreEqual(0d, stamp.Pause.TotalHours);

            // still same:
            Assert.AreEqual(first.Begin.Value, stamp.Begin);
        }

        [TestMethod]
        public void TestDeleteActivityInBetweenSetsDaysPause()
        {
            // Arrange:
            var manager = GetManager();
            var stamp = GetStamp();
            var first = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(09, 17, 00), End = new TimeSpan(11, 00, 00) };
            stamp.ActivityRecords.Add(first);
            var middle = new ActivityRecord() { Activity = "Meeting", Begin = new TimeSpan(12, 00, 00), End = new TimeSpan(12, 30, 00) };
            stamp.ActivityRecords.Add(middle);
            var last = new ActivityRecord() { Activity = "Product Development", Begin = new TimeSpan(12, 30, 00), End = new TimeSpan(16, 51, 00) };
            stamp.ActivityRecords.Add(last);

            manager.CurrentShown = stamp;
            stamp.Pause = new TimeSpan(1, 0, 0);

            // Act:
            manager.DeleteActivity(stamp, middle);

            // Assert:
            // deleted:
            Assert.AreEqual(2, stamp.ActivityRecords.Count);
            Assert.IsFalse(stamp.ActivityRecords.Contains(middle));

            // updated:
            Assert.AreEqual(1.5d, stamp.Pause.TotalHours);

            // still same:
            Assert.AreEqual(first.Begin.Value, stamp.Begin);
            Assert.AreEqual(last.End.Value, stamp.End);
        }

        #endregion

        #region Test LastMouseMove-Timestamp upon delayed 'Lock'

        [TestMethod]
        public void TestDellLaptopDelayedLockEventStillSetsCorrectEndTime()
        {
            GetOpenStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var second, out var last);

            SetNowTime(new DateTime(2019, 05, 21, 16, 31, 00));

            manager.ResumeStamping();

            // last mouse move at:
            manager.LastUserAction = new DateTime(2019, 05, 21, 16, 30, 00);

            // trash DELL laptop computer is now 'locked' (e.g. sleep button on keyboard is pressed), but the trash system does not fire the locked event (yet)...

            // at some point later, the event is fired:

            SetNowTime(new DateTime(2019, 05, 21, 21, 55, 00));

            manager.SuspendStamping(considerLastMouseMove: true);

            // assert correct end time:
            Assert.AreEqual(1, manager.StampList.Count);

            Assert.AreEqual(new DateTime(2019, 05, 21), manager.StampList.Single().Day);
            Assert.AreEqual(8, manager.StampList.Single().WorkingHours);
            Assert.AreEqual(new TimeSpan(09, 17, 00), manager.StampList.Single().Begin);
            Assert.AreEqual(new TimeSpan(16, 30, 00), manager.StampList.Single().End);
            //Assert.IsTrue(manager.HasMatchingActivityTimestamps(manager.StampList.Single(), out string err), err);
        }

        [TestMethod]
        public void TestDellLaptopDelayedLockEventStillSetsCorrectEndTimeOnNextDay()
        {
            GetOpenStampWithThreeActivitiesAndPause(out var stamp, out var manager, out var first, out var second, out var last);

            SetNowTime(new DateTime(2019, 05, 21, 16, 31, 00));

            manager.ResumeStamping();

            // last mouse move at:
            manager.LastUserAction = new DateTime(2019, 05, 21, 16, 30, 00);

            // trash DELL laptop computer is now 'locked' (e.g. sleep button on keyboard is pressed), but the trash system does not fire the locked event (yet)...

            // it may even fire the next day after:
            /*
                16.08.2019 08:46 SessionUnlock... <- friday, working day
                16.08.2019 12:04 SessionLock...
                16.08.2019 12:26 SessionUnlock...
                16.08.2019 12:46 SessionLock...
                16.08.2019 13:39 SessionUnlock... <- actually, then locked out and finished around 17:00
                17.08.2019 11:32 SessionLock... <- delayed lock reported lock event on the next day in the morning
                19.08.2019 09:27 SessionUnlock... <- monday, working day
            */

            SetNowTime(new DateTime(2019, 05, 22, 11, 05, 00));

            manager.SuspendStamping(considerLastMouseMove: true);

            // assert correct end time:
            Assert.AreEqual(1, manager.StampList.Count);

            Assert.AreEqual(new DateTime(2019, 05, 21), manager.StampList.Single().Day);
            Assert.AreEqual(8, manager.StampList.Single().WorkingHours);
            Assert.AreEqual(new TimeSpan(09, 17, 00), manager.StampList.Single().Begin);
            Assert.AreEqual(new TimeSpan(16, 30, 00), manager.StampList.Single().End);
            //Assert.IsTrue(manager.HasMatchingActivityTimestamps(manager.StampList.Single(), out string err), err);
        }

        #endregion



        #region Integration Test User Stories

        [TestMethod]
        public void SetEndTimeAndSuspendComputerAndResumeNextDay()
        {
            // Story:
            // Set end time to now + 10 minutes and suspend computer. Resume on next day.

            // Result:
            // new day started when resumed,
            // manually inserted end time is set for previous day as well as for previous activity.

            var tm = new TimeManager(new TimeSettings() { IgnoreStampFile = true, DisablePopupNotifications = true });
            PopupDialog.Manager = tm;

            SetNowTime(new DateTime(2019, 05, 24, 20, 25, 11));

            tm.Initialize();

            tm.LastUserAction = TimeManager.Time.Now;

            Assert.IsNotNull(tm.TodayCurrentActivity);
            Assert.IsNotNull(tm.CurrentShown);
            Assert.IsNotNull(tm.Today);
            Assert.AreEqual(TimeManager.GetNowTime(), tm.Today.Begin);
            Assert.AreEqual(TimeSpan.Zero, tm.Today.End);
            Assert.AreEqual(1, tm.Today.ActivityRecords.Count);
            Assert.AreEqual(tm.TodayCurrentActivity, tm.Today.ActivityRecords.Single());

            var currAct = tm.TodayCurrentActivity;
            var today = tm.Today;
            var manualEnd = TimeManager.GetNowTime() + TimeSpan.FromMinutes(10);
            TimeManager.SetEnd(tm.Today, manualEnd);

            // simulate sleep:
            tm.SuspendStamping();

            // simulate day change:
            SetNowTime(new DateTime(2019, 05, 25, 08, 30, 48));

            // resume:
            tm.ResumeStamping();

            Assert.AreNotEqual(today, tm.Today);
            Assert.AreNotEqual(currAct, tm.TodayCurrentActivity);
            Assert.AreEqual(manualEnd, today.End);
            Assert.AreEqual(1, today.ActivityRecords.Count);
            Assert.AreEqual(manualEnd, today.ActivityRecords.Single().End);
        }

        //public void CorrectTrackedActivityWhichWasNotEnded()
        //{
        //    // User started a meeting tracking but forgot to end it / switch back to development and want to correct this by 'breaking up' the current activity

        //    // general initialization:
        //    var tm = new TimeManager(new TimeSettings() { IgnoreStampFile = true, DisablePopupNotifications = true });
        //    PopupDialog.Manager = tm;

        //    var mockTime = new Mock<TimeProvider>();
        //    mockTime.SetupGet(t => t.Today).Returns(new DateTime(2019, 05, 24));
        //    mockTime.SetupGet(t => t.Now).Returns(new DateTime(2019, 05, 24, 16, 20, 11));

        //    tm.Time = mockTime.Object;

        //    var stamp = new Stamp(new DateTime(2019, 05, 24), new TimeSpan(08, 30, 00), 8) { Pause = TimeSpan.FromMinutes(60) };
        //    tm.StampList.Add(stamp);

        //    var development1 = new ActivityRecord() { Activity = "Product Development", Begin = stamp.Begin, End = new TimeSpan(12, 30, 00) };
        //    stamp.ActivityRecords.Add(development1);

        //    var meeting1 = new ActivityRecord() { Activity = "Meeting", Begin = new TimeSpan(13, 30, 00) };
        //    stamp.ActivityRecords.Add(meeting1);

        //    tm.Initialize();

        //    Assert.AreEqual("Meeting", tm.TodayCurrentActivity?.Activity);
        //    Assert.AreEqual(stamp, tm.CurrentShown);
        //    Assert.AreEqual(stamp, tm.Today);

        //    Assert.AreEqual(2, tm.Today.ActivityRecords.Count);
        //    Assert.AreEqual(TimeSpan.Zero, tm.Today.End);
        //    Assert.AreEqual(60, tm.Today.Pause.TotalMinutes);

        //    // Act:

        //    meeting1.End = new TimeSpan(14, 40, 00);

        //    var development2 = new ActivityRecord() { Activity = "Product Development", Begin = meeting1.End };
        //    stamp.ActivityRecords.Add(development2);


        //}

        #endregion


        #region Regression Tests

        [TestMethod]
        public void TestLastActivityTagsAreAppliedCorrectly()
        {
            // Arrange:
            GetOpenStampWithThreeActivitiesAndPause(out Stamp stamp, out TimeManager manager, out ActivityRecord first, out ActivityRecord middle, out ActivityRecord last);
            last.Tags.Add("Test");
            manager.Today = stamp;
            manager.TodayCurrentActivity = last;

            SetNowTime(new DateTime(2019, 05, 21, 13, 50, 00));

            // Act:
            manager.StartNewActivity("Product Development", manager.Today.GetLastActivity());

            // Assert:
            Assert.AreEqual(4, stamp.ActivityRecords.Count);
            var newLast = stamp.GetLastActivity();
            Assert.AreEqual("Product Development", newLast.Activity);
            Assert.AreEqual(1, newLast.Tags.Count);
            Assert.AreEqual("Test", newLast.Tags[0]);
            Assert.AreEqual(new TimeSpan(13, 50, 00), newLast.Begin.Value);
        }


        [TestMethod]
        public void TestResumeStampingLastActivityTagsAreAppliedCorrectly()
        {
            // Arrange:
            GetEndedStampWithThreeActivitiesAndPause(out Stamp stamp, out TimeManager manager, out ActivityRecord first, out ActivityRecord middle, out ActivityRecord last);
            last.Tags.Add("Test");
            manager.Today = stamp;
            // TodayCurrentActivity should be null

            SetNowTime(new DateTime(2019, 05, 21, 16, 59, 00)); // must be > 7 minutes from last end time

            // Act:
            manager.ResumeStamping();

            // Assert:
            Assert.AreEqual(5, stamp.ActivityRecords.Count); // 1 filled into the pause, 1 started and running
            var pause = stamp.ActivityRecords.ElementAt(3);
            Assert.AreEqual("Product Development", pause.Activity);
            Assert.AreEqual(1, pause.Tags.Count);
            Assert.AreEqual("Test", pause.Tags[0]);
            Assert.AreEqual(new TimeSpan(16, 51, 00), pause.Begin.Value);
            Assert.AreEqual(new TimeSpan(16, 59, 00), pause.End.Value);

            var started = stamp.ActivityRecords.ElementAt(4);
            Assert.AreEqual("Product Development", started.Activity);
            Assert.AreEqual(1, started.Tags.Count);
            Assert.AreEqual("Test", started.Tags[0]);
            Assert.AreEqual(new TimeSpan(16, 59, 00), started.Begin.Value);
            Assert.IsNull(started.End);
            Assert.AreEqual(started, manager.TodayCurrentActivity);
        }
        #endregion
    }
}
