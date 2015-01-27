using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncResiliencyPatterns.Tests
{
    [TestClass]
    public class SemaphoreSuperSlimTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MaximumAllowedLessThanZero()
        {
            var semaphore = new SemaphoreSuperSlim(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MaximumAllowedZero()
        {
            var semaphore = new SemaphoreSuperSlim(0);
        }

        [TestMethod]
        public void Acquire()
        {
            var semaphore = new SemaphoreSuperSlim(1);
            Assert.AreEqual(0, semaphore.CurrentCount);

            bool acquired = semaphore.Acquire();

            Assert.IsTrue(acquired);
            Assert.AreEqual(1, semaphore.CurrentCount);
        }

        [TestMethod]
        public void Release()
        {
            var semaphore = new SemaphoreSuperSlim(1);
            semaphore.Acquire();

            semaphore.Release();

            Assert.AreEqual(0, semaphore.CurrentCount);
        }

        [TestMethod]
        public void ReleaseBelowZero()
        {
            var semaphore = new SemaphoreSuperSlim(1);

            semaphore.Release();

            Assert.AreEqual(0, semaphore.CurrentCount);
        }

        [TestMethod]
        public void AcquireFailed()
        {
            var semaphore = new SemaphoreSuperSlim(1);

            semaphore.Acquire();
            bool acquired = semaphore.Acquire();

            Assert.IsFalse(acquired);
            Assert.AreEqual(1, semaphore.CurrentCount);
        }
    }
}
