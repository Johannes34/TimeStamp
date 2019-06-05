using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeStamp.Tests
{
    [TestClass]
    public class UserStories
    {

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

            var mockTime = new Mock<TimeProvider>();
            mockTime.SetupGet(t => t.Today).Returns(new DateTime(2019, 05, 24));
            mockTime.SetupGet(t => t.Now).Returns(new DateTime(2019, 05, 24, 20, 25, 11));

            tm.Time = mockTime.Object;

            tm.Initialize();

            Assert.IsNotNull(tm.TodayCurrentActivity);
            Assert.IsNotNull(tm.CurrentShown);
            Assert.IsNotNull(tm.Today);
            Assert.AreEqual(tm.GetNowTime(), tm.Today.Begin);
            Assert.AreEqual(TimeSpan.Zero, tm.Today.End);
            Assert.AreEqual(1, tm.Today.ActivityRecords.Count);
            Assert.AreEqual(tm.TodayCurrentActivity, tm.Today.ActivityRecords.Single());

            var currAct = tm.TodayCurrentActivity;
            var today = tm.Today;
            var manualEnd = tm.GetNowTime() + TimeSpan.FromMinutes(10);
            tm.SetEnd(tm.Today, manualEnd);

            // simulate sleep:
            tm.SuspendStamping();

            // simulate day change:
            mockTime.SetupGet(t => t.Today).Returns(new DateTime(2019, 05, 25));
            mockTime.SetupGet(t => t.Now).Returns(new DateTime(2019, 05, 25, 08, 30, 48));

            // resume:
            tm.ResumeStamping();

            Assert.AreNotEqual(today, tm.Today);
            Assert.AreNotEqual(currAct, tm.TodayCurrentActivity);
            Assert.AreEqual(manualEnd, today.End);
            Assert.AreEqual(1, today.ActivityRecords.Count);
            Assert.AreEqual(manualEnd, today.ActivityRecords.Single().End);
        }



    }
}
