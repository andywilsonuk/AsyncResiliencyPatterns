using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Moq;

namespace AsyncResiliencyPatterns.Tests
{
    [TestClass]
    public class CircuitBreakerStateAttemptTests
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
        [ExpectedException(typeof(ApplicationException))]
        public void AttemptToTripped()
        {
            var state = new CircuitBreakerStateAttempt(this.parameters);

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
        public void AttemptToNormal()
        {
            var state = new CircuitBreakerStateAttempt(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);

            Task task = state.ExecuteAsync<bool>(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                return true;
            });
            task.Wait();

            this.stateMachine.VerifySet(m => m.State = It.IsAny<CircuitBreakerStateNormal>(), Times.Once());
        }

        [TestMethod]
        public void AttemptToAttempt()
        {
            var state = new CircuitBreakerStateAttempt(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);

            state.TransitionToAttempt();

            this.stateMachine.VerifySet(m => m.State = It.IsAny<CircuitBreakerStateInternal>(), Times.Never());
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void AttemptTripDueToExceptionFilter()
        {
            this.settings.ExceptionTypes.Add(typeof(ApplicationException));
            var state = new CircuitBreakerStateAttempt(this.parameters);

            Task task = state.ExecuteAsync<bool>(() =>
            {
                throw new ApplicationException();
            });

            Assert.IsTrue(task.IsFaulted);
            this.stateMachine.VerifySet(m => m.State = It.IsAny<CircuitBreakerStateTripped>(), Times.Once());
            throw task.Exception.InnerException;
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void AttemptNoTripDueToExceptionFilter()
        {
            this.settings.ExceptionTypes.Add(typeof(ArgumentNullException));
            var state = new CircuitBreakerStateAttempt(this.parameters);

            Task task = state.ExecuteAsync<bool>(() =>
            {
                throw new ApplicationException();
            });

            Assert.IsTrue(task.IsFaulted);
            this.stateMachine.VerifySet(m => m.State = It.IsAny<CircuitBreakerStateInternal>(), Times.Never());
            throw task.Exception.InnerException;
        }

        [TestMethod]
        [ExpectedException(typeof(CircuitBreakerTrippedException))]
        public void SecondAttemptTripped()
        {
            var state = new CircuitBreakerStateAttempt(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);

            Task t1 = state.ExecuteAsync<bool>(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                return true;
            });

            Task t2 = state.ExecuteAsync<bool>(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                return true;
            });

            Assert.IsTrue(t2.IsFaulted);
            Assert.IsNotNull(t2.Exception);
            Assert.IsNotNull(t2.Exception.InnerException);
            throw t2.Exception.InnerException;
        }

        [TestMethod]
        public void AttemptTrippedExecuteEventFired()
        {
            var state = new CircuitBreakerStateAttempt(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);
            bool wasEventFired = false;
            state.ShortCircuited += delegate(object sender, EventArgs e)
            {
                wasEventFired = true;
            };

            Task t1 = state.ExecuteAsync<bool>(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                return true;
            });

            try
            {
                Task task = state.ExecuteAsync<bool>(async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    return true;
                });
                task.Wait();
            }
            catch (AggregateException ex)
            {
                Assert.IsInstanceOfType(ex.InnerException, typeof(CircuitBreakerTrippedException));
            }

            Assert.IsTrue(wasEventFired);
        }
    }
}
