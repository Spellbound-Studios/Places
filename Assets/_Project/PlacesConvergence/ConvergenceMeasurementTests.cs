// Copyright (c) Spellbound Studios. All rights reserved.

using System.IO;
using NUnit.Framework;

namespace PlacesConvergence.Tests {
    public sealed class ConvergenceMeasurementTests {
        [Test]
        public void EveryRunFinishesAndTheReportIsWritten() {
            var measurement = new ConvergenceMeasurement(ConvergenceFixtures.FromProject());

            var reportLines = measurement.Run();
            var reportPath = ConvergenceMeasurement.WriteReport(reportLines);

            Assert.That(measurement.CompletedRuns, Is.EqualTo(700));
            Assert.That(File.Exists(reportPath), Is.True);
            Assert.That(File.ReadAllLines(reportPath).Length, Is.EqualTo(15));
        }
    }
}
