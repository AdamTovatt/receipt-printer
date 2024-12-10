namespace ReceiptPrinter.Tests
{
    [TestClass]
    public class TimeBasedFunctionTests
    {
        [TestMethod]
        public void ReturnsOne_WhenPrintedWithinLastHour()
        {
            // Arrange
            int timeSinceLastPrint = 3500; // Less than an hour
            DateTime now = new DateTime(2024, 6, 1, 12, 0, 0);

            // Act
            double result = Worker.GetRefreshDelayMultiplier(now, timeSinceLastPrint);

            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void ReturnsTwo_OnWednesdayAt1900_WithLongTimeSinceLastPrint()
        {
            // Arrange
            int timeSinceLastPrint = 21600; // 6 hours (a long time since the last print)
            DateTime now = new DateTime(2024, 6, 5, 19, 0, 0); // Wednesday 19:00

            // Act
            double result = Worker.GetRefreshDelayMultiplier(now, timeSinceLastPrint);

            // Assert
            Assert.AreEqual(2, result, "Multiplier should be 2 for 19:00 on a Wednesday regardless of time since last print.");
        }

        [TestMethod]
        public void ReturnsTwo_OnWednesdayBetween1700And2000()
        {
            // Arrange
            int timeSinceLastPrint = 4000; // More than an hour
            DateTime now = new DateTime(2024, 6, 5, 17, 30, 0); // Wednesday 17:30

            // Act
            double result = Worker.GetRefreshDelayMultiplier(now, timeSinceLastPrint);

            // Assert
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void ReturnsTwo_OnFridayBetween2100And2300()
        {
            // Arrange
            int timeSinceLastPrint = 5000; // More than an hour
            DateTime now = new DateTime(2024, 6, 7, 21, 15, 0); // Friday 21:15

            // Act
            double result = Worker.GetRefreshDelayMultiplier(now, timeSinceLastPrint);

            // Assert
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void ReturnsMaxClamp24_ForEarlyMorningTimeBetween0100And0800()
        {
            // Arrange
            int timeSinceLastPrint = 20000; // Around 5.5 hours
            DateTime now = new DateTime(2024, 6, 2, 3, 0, 0); // 03:00 AM

            // Act
            double result = Worker.GetRefreshDelayMultiplier(now, timeSinceLastPrint);

            // Assert
            Assert.IsTrue(result <= 24);
        }

        [TestMethod]
        public void ReturnsMaxClamp12_ForDaytimeOutsideEarlyMorning()
        {
            // Arrange
            int timeSinceLastPrint = 20000; // Around 5.5 hours
            DateTime now = new DateTime(2024, 6, 2, 10, 0, 0); // 10:00 AM

            // Act
            double result = Worker.GetRefreshDelayMultiplier(now, timeSinceLastPrint);

            // Assert
            Assert.IsTrue(result <= 12);
        }

        [TestMethod]
        public void ReturnsCorrectMultiplier_ForEdgeCaseNearOneHour()
        {
            // Arrange
            int timeSinceLastPrint = 3601; // Just above one hour
            DateTime now = new DateTime(2024, 6, 3, 15, 0, 0);

            // Act
            double result = Worker.GetRefreshDelayMultiplier(now, timeSinceLastPrint);

            // Assert
            Assert.IsTrue(result > 1);
        }

        [TestMethod]
        public void ReturnsCorrectMultiplier_ForLargeTimeSinceLastPrint()
        {
            // Arrange
            int timeSinceLastPrint = 21600; // 6 hours
            DateTime now = new DateTime(2024, 6, 3, 14, 0, 0);

            // Act
            double result = Worker.GetRefreshDelayMultiplier(now, timeSinceLastPrint);

            // Assert
            Assert.IsTrue(result > 4 && result <= 24);
        }

        [TestMethod]
        public void ReturnsCorrectMultiplier_ForEarlyMorningLargeTimeSinceLastPrint()
        {
            // Arrange
            int timeSinceLastPrint = 21600; // 6 hours
            DateTime now = new DateTime(2024, 6, 2, 4, 0, 0); // 04:00 AM

            // Act
            double result = Worker.GetRefreshDelayMultiplier(now, timeSinceLastPrint);

            // Assert
            Assert.IsTrue(result == 24);
        }

        [TestMethod]
        public void ReturnsOne_ForExactOneHourSinceLastPrint()
        {
            // Arrange
            int timeSinceLastPrint = 3600; // Exactly 1 hour
            DateTime now = new DateTime(2024, 6, 1, 12, 0, 0);

            // Act
            double result = Worker.GetRefreshDelayMultiplier(now, timeSinceLastPrint);

            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void RefreshDelay_IsClampedCorrectly()
        {
            // Arrange
            int refreshDelay = 5000; // Base refresh delay in milliseconds
            DateTime now = new DateTime(2024, 6, 2, 4, 0, 0); // Example time: 04:00 AM (Early morning)

            // Case 1: Small delay (within normal range)
            int timeSinceLastPrint = 3500; // Less than an hour
            double multiplier1 = Worker.GetRefreshDelayMultiplier(now, timeSinceLastPrint);
            int expectedDelay1 = refreshDelay; // Should be clamped to the base refresh delay

            // Case 2: Large multiplier (close to the max clamp)
            timeSinceLastPrint = 21600; // 6 hours
            double multiplier2 = Worker.GetRefreshDelayMultiplier(now, timeSinceLastPrint);
            int expectedDelay2 = refreshDelay * 24; // Should be clamped to the max (24x refreshDelay)

            // Act
            int _refreshDelay1 = Math.Clamp((int)Math.Round(refreshDelay * multiplier1), refreshDelay, refreshDelay * 24);
            int _refreshDelay2 = Math.Clamp((int)Math.Round(refreshDelay * multiplier2), refreshDelay, refreshDelay * 24);

            // Assert
            Assert.AreEqual(expectedDelay1, _refreshDelay1, "Refresh delay should be clamped to the base delay for small multipliers.");
            Assert.AreEqual(expectedDelay2, _refreshDelay2, "Refresh delay should be clamped to 24 times the base delay for large multipliers.");
        }
    }
}
