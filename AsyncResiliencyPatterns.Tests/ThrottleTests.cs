using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Moq;

namespace AsyncResiliencyPatterns.Tests
{
    [TestClass]
    public class ThrottleTests
    {
        [TestMethod]
        public void AllowedRequest()
        {
            Throttle throttle = new Throttle(1);
            int counter = 0;
            Task t = throttle.ExecuteAsync(async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    counter++;
                });

            t.Wait();

            Assert.AreEqual(1, counter);
            Assert.AreEqual(1, throttle.InstancesRemaining);
        }

        [TestMethod]
        public void AllowedRequestWithResult()
        {
            Throttle throttle = new Throttle(1);
            Task<int> t = throttle.ExecuteAsync<int>(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                return 1;
            });

            t.Wait();
            int counter = t.Result;

            Assert.AreEqual(1, counter);
            Assert.AreEqual(1, throttle.InstancesRemaining);
        }

        [TestMethod]
        [ExpectedException(typeof(ThrottleLimitException))]
        public void ThrottledRequest()
        {
            Throttle throttle = new Throttle(1);
            Task t1 = throttle.ExecuteAsync(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));
                });

            Task t2 = throttle.ExecuteAsync(async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                });

            Assert.IsTrue(t2.IsFaulted);
            Assert.IsNotNull(t2.Exception);
            Assert.IsNotNull(t2.Exception.InnerException);
            throw t2.Exception.InnerException;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InvalidThrottleLimit()
        {
            new Throttle(0);
        }

        [TestMethod]
        public void ThrottleReleasedOnError()
        {
            Throttle throttle = new Throttle(1);
            try
            {
                Task t = throttle.ExecuteAsync(() =>
                {
                    throw new Exception();
                });

                t.Wait();
            }
            catch (AggregateException)
            {
            }

            Assert.AreEqual(1, throttle.InstancesRemaining);
        }

        [TestMethod]
        public void NestedInvoker()
        {
            PassThroughInvoker passThrough = new PassThroughInvoker();
            Throttle throttle = new Throttle(1, passThrough);
            int counter = 0;
            Task t = throttle.ExecuteAsync(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                counter++;
            });

            t.Wait();

            Assert.AreEqual(1, counter);
            Assert.IsTrue(passThrough.Verify);
        }

        [TestMethod]
        [ExpectedException(typeof(ThrottleLimitException))]
        public void ThrottledNestedInvoker()
        {
            PassThroughInvoker passThrough = new PassThroughInvoker();
            Throttle throttle = new Throttle(1, passThrough);
            int counter = 0;
            Task t1 = throttle.ExecuteAsync(async () =>
            {
                counter++;
                await Task.Delay(TimeSpan.FromSeconds(10));
            });

            Task t2 = throttle.ExecuteAsync(async () =>
            {
                counter++;
                await Task.Delay(TimeSpan.FromMilliseconds(1));
            });

            Assert.AreEqual(1, counter);
            Assert.IsTrue(t2.IsFaulted);
            Assert.IsNotNull(t2.Exception);
            Assert.IsNotNull(t2.Exception.InnerException);
            Assert.IsTrue(passThrough.Verify);
            throw t2.Exception.InnerException;
        }

        [TestMethod]
        public void ThrottledWithNotification()
        {
            Throttle throttle = new Throttle(1);
            bool wasCalled = false;

            throttle.Throttled += (sender, e) => {
                wasCalled = true;
            };

            Task t1 = throttle.ExecuteAsync(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
            });

            Task t2 = throttle.ExecuteAsync(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1));
            });

            try
            {
                t2.Wait();
            }
            catch
            {
            }

            Assert.IsTrue(wasCalled);
        }
    }
}
