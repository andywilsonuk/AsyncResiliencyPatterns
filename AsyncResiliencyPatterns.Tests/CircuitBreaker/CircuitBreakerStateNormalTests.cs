using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Moq;
using System.Threading;

namespace AsyncResiliencyPatterns.Tests
{
    [TestClass]
    public class CircuitBreakerStateNormalTests
    {
        private Mock<CircuitBreakerStateMachine> stateMachine;
        private CircuitBreakerSettings settings;
        private ResiliencyCommandInvoker invoker;
        private CircuitBreakerStateParameters parameters;

        [TestInitialize]
        public void TestInitialize()
        {
            this.stateMachine = new Mock<CircuitBreakerStateMachine>();
            this.settings = new CircuitBreakerSettings();
            this.invoker = new InnerCommandInvoker();
            this.parameters = new CircuitBreakerStateParameters
            {
                stateMachine = this.stateMachine.Object,
                settings = this.settings,
                invoker = this.invoker
            };
        }

        [TestMethod]
        public void Normal()
        {
            var state = new CircuitBreakerStateNormal(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);
            int counter = 0;

            Task t1 = state.ExecuteAsync<bool>(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                counter++;
                return true;
            });

            t1.Wait();

            Assert.AreEqual(1, counter);
            this.stateMachine.VerifySet(m => m.State = It.IsAny<CircuitBreakerStateInternal>(), Times.Never());
        }

        [TestMethod]
        public void NormalFailNoTrip()
        {
            var state = new CircuitBreakerStateNormal(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);

            Task task = state.ExecuteAsync<bool>(() =>
            {
                throw new ApplicationException();
            });

            Assert.AreEqual(1, state.FailureCount);
            Assert.IsTrue(task.IsFaulted);
            Assert.IsNotNull(task.Exception);
            Assert.IsNotNull(task.Exception.InnerException);
            this.stateMachine.VerifySet(m => m.State = It.IsAny<CircuitBreakerStateInternal>(), Times.Never());
        }

        [TestMethod]
        public void NestedInvoker()
        {
            PassThroughInvoker passThrough = new PassThroughInvoker();
            this.parameters = new CircuitBreakerStateParameters
            {
                stateMachine = this.parameters.stateMachine,
                settings = this.parameters.settings,
                invoker = passThrough
            };
            var state = new CircuitBreakerStateNormal(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);

            int counter = 0;
            Task t = state.ExecuteAsync<bool>(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                counter++;
                return true;
            });

            t.Wait();

            Assert.AreEqual(1, counter);
            Assert.IsTrue(passThrough.Verify);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void NormalToTripped()
        {
            this.settings.FailureThreshold = 1;
            var state = new CircuitBreakerStateNormal(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);

            Task task = state.ExecuteAsync<bool>(() =>
            {
                throw new ApplicationException();
            });

            Assert.IsTrue(task.IsFaulted);
            Assert.IsNotNull(task.Exception);
            Assert.IsNotNull(task.Exception.InnerException);
            this.stateMachine.VerifySet(m => m.State = It.IsAny<CircuitBreakerStateTripped>(), Times.Once());
            throw task.Exception.InnerException;
        }

        [TestMethod]
        public void NormalToTrippedMultiple()
        {
            this.settings.FailureThreshold = 2;
            var state = new CircuitBreakerStateNormal(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);

            Task t1 = state.ExecuteAsync<bool>(() =>
            {
                throw new ApplicationException();
            });
            this.stateMachine.VerifySet(m => m.State = It.IsAny<CircuitBreakerStateInternal>(), Times.Never());
            Task t2 = state.ExecuteAsync<bool>(() =>
            {
                throw new ApplicationException();
            });

            this.stateMachine.VerifySet(m => m.State = It.IsAny<CircuitBreakerStateTripped>(), Times.Once());
        }

        [TestMethod]
        public void NormalFailureReset()
        {
            var state = new CircuitBreakerStateNormal(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);
            Task t1 = state.ExecuteAsync<bool>(() =>
            {
                throw new ApplicationException();
            });
            this.stateMachine.VerifySet(m => m.State = It.IsAny<CircuitBreakerStateInternal>(), Times.Never());
            Assert.AreEqual(1, state.FailureCount);

            state.ResetCallback(null);

            Assert.AreEqual(0, state.FailureCount);
        }

        [TestMethod]
        public void NormalToNormal()
        {
            var state = new CircuitBreakerStateNormal(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);

            state.TransitionToNormal();

            this.stateMachine.VerifySet(m => m.State = It.IsAny<CircuitBreakerStateInternal>(), Times.Never());
        }

        [TestMethod]
        public void NormalToAttempt()
        {
            var state = new CircuitBreakerStateNormal(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);

            state.TransitionToAttempt();

            this.stateMachine.VerifySet(m => m.State = It.IsAny<CircuitBreakerStateInternal>(), Times.Never());
        }

        [TestMethod]
        public void NormalFailureResetUsingTimer()
        {
            this.settings.FailureExpiryPeriod = TimeSpan.FromMilliseconds(1);
            var state = new CircuitBreakerStateNormal(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);
            Task t1 = state.ExecuteAsync<bool>(() =>
            {
                throw new ApplicationException();
            });

            try
            {
                t1.Wait();
            }
            catch
            {
            }

            Thread.Sleep(TimeSpan.FromMilliseconds(50));

            Assert.AreEqual(0, state.FailureCount);
        }

        [TestMethod]
        public void NormalFailExceptionFilter()
        {
            this.settings.ExceptionTypes.Add(typeof(ApplicationException));
            var state = new CircuitBreakerStateNormal(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);

            Task task = state.ExecuteAsync<bool>(() =>
            {
                throw new ApplicationException();
            });

            Assert.AreEqual(1, state.FailureCount);
        }

        [TestMethod]
        public void NormalFailNoRecordExceptionFilter()
        {
            this.settings.ExceptionTypes.Add(typeof(ArgumentNullException));
            var state = new CircuitBreakerStateNormal(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);

            Task task = state.ExecuteAsync<bool>(() =>
            {
                throw new ApplicationException();
            });

            Assert.AreEqual(0, state.FailureCount);
        }

        [TestMethod]
        [ExpectedException(typeof(DivideByZeroException))]
        public void StateHasBeenDisposedHoweverFailedRequestComesIn()
        {
            var state = new CircuitBreakerStateNormal(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);
            bool exit = false;

            Task t1 = state.ExecuteAsync<bool>(async () =>
            {
                while (!exit)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
                throw new DivideByZeroException("Fail");
            });

            state.Dispose();
            exit = true;

            try
            {
                t1.Wait();
            }
            catch(AggregateException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
