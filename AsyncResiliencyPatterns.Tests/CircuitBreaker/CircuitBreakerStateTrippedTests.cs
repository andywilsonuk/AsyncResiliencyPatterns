using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Moq;
using System.Threading;

namespace AsyncResiliencyPatterns.Tests
{
    [TestClass]
    public class CircuitBreakerStateTrippedTests
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
        public void TrippedToAttempt()
        {
            var state = new CircuitBreakerStateTripped(this.parameters);

            state.ResetCallback(null);

            this.stateMachine.VerifySet(m => m.State = It.IsAny<CircuitBreakerStateAttempt>(), Times.Once());
        }

        [TestMethod]
        public void TrippedToTripped()
        {
            var state = new CircuitBreakerStateTripped(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);

            state.TransitionToTripped();

            this.stateMachine.VerifySet(m => m.State = It.IsAny<CircuitBreakerStateInternal>(), Times.Never());
        }

        [TestMethod]
        public void TrippedToNormal()
        {
            var state = new CircuitBreakerStateTripped(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);

            state.TransitionToNormal();

            this.stateMachine.VerifySet(m => m.State = It.IsAny<CircuitBreakerStateInternal>(), Times.Never());
        }

        [TestMethod]
        [ExpectedException(typeof(CircuitBreakerTrippedException))]
        public void TrippedExecute()
        {
            var state = new CircuitBreakerStateTripped(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);

            Task task = state.ExecuteAsync<bool>(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                return true;
            });

            task.Wait();
        }

        [TestMethod]
        public void TrippedResetToAttemptUsingTimer()
        {
            this.settings.TripWaitPeriod = TimeSpan.FromMilliseconds(1);
            var state = new CircuitBreakerStateTripped(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);

            Thread.Sleep(TimeSpan.FromMilliseconds(50));

            this.stateMachine.VerifySet(m => m.State = It.IsAny<CircuitBreakerStateAttempt>(), Times.Once());
        }

        [TestMethod]
        public void TrippedExecuteEventFired()
        {
            var state = new CircuitBreakerStateTripped(this.parameters);
            this.stateMachine.SetupProperty(m => m.State, state);
            bool wasEventFired = false;
            state.ShortCircuited += delegate(object sender, EventArgs e)
            {
                wasEventFired = true;
            };
            
            try
            {
                Task task = state.ExecuteAsync<bool>(async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    return true;
                });
                task.Wait();
            }
            catch (CircuitBreakerTrippedException)
            {
            }

            Assert.IsTrue(wasEventFired);
        }
    }
}
