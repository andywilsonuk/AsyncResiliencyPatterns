using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AsyncResiliencyPatterns.Tests
{
    [TestClass]
    public class CircuitBreakerSettingsExtensionsTests
    {
        private CircuitBreakerSettings settings;

        [TestInitialize]
        public void TestInitialize()
        {
            this.settings = new CircuitBreakerSettings();
        }


        [TestMethod]
        public void ShouldFailureBeRecordedTrueWithExceptionFilter()
        {
            this.settings.ExceptionTypes.Add(typeof(ApplicationException));
            Exception ex = new ApplicationException();

            bool actual = this.settings.ShouldFailureBeRecorded(ex);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void ShouldFailureBeRecordedFalseWithExceptionFilter()
        {
            this.settings.ExceptionTypes.Add(typeof(ArgumentNullException));
            Exception ex = new ApplicationException();

            bool actual = this.settings.ShouldFailureBeRecorded(ex);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void ShouldFailureBeRecordedInheritedTrueWithExceptionFilter()
        {
            this.settings.ExceptionTypes.Add(typeof(SystemException));
            Exception ex = new NotImplementedException();

            bool actual = this.settings.ShouldFailureBeRecorded(ex);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void ShouldFailureBeRecordedNoExceptionFilter()
        {
            Exception ex = new ApplicationException();

            bool actual = this.settings.ShouldFailureBeRecorded(ex);

            Assert.IsTrue(actual);
        }
    }
}
