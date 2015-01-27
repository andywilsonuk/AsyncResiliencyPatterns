using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Moq;

namespace AsyncResiliencyPatterns.Tests
{
    [TestClass]
    public class CircuitBreakerTests
    {
        private CircuitBreaker circuitBreaker;
        private Mock<CircuitBreakerStateMachine> stateMachine;
        private Mock<CircuitBreakerStateInternal> initialState;

        [TestInitialize]
        public void TestInitialize()
        {
            this.initialState = new Mock<CircuitBreakerStateInternal>();
            this.initialState.As<CircuitBreakerState>();
            this.stateMachine = new Mock<CircuitBreakerStateMachine>();
            this.stateMachine.SetupAllProperties();
            this.stateMachine.SetupGet(s => s.State).Returns(this.initialState.Object);
            this.circuitBreaker = new CircuitBreaker(this.stateMachine.Object);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            this.circuitBreaker.Dispose();
        }

        [TestMethod]
        public void Execute()
        {
            Task task = this.circuitBreaker.ExecuteAsync(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1));
            });
            task.Wait();

            this.initialState.Verify(m => m.ExecuteAsync(It.IsAny<Func<Task<bool>>>()), Times.Once());
        }

        [TestMethod]
        public void ExecuteWithResult()
        {
            Func<Task<int>> func = async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                return 1;
            };

            this.initialState.Setup(m => m.ExecuteAsync<int>(func)).Returns(func());

            Task<int> task = this.circuitBreaker.ExecuteAsync<int>(func);
            task.Wait();
            int counter = task.Result;

            this.initialState.Verify(m => m.ExecuteAsync(It.IsAny<Func<Task<int>>>()), Times.Once());
            Assert.AreEqual(1, counter);
        }

        [TestMethod]
        public void StateChangedNotification()
        {
            var newState = new Mock<CircuitBreakerState>();
            bool wasCalled = false;
            CircuitBreakerStateChangedEventArgs actualArgs = null;
            this.circuitBreaker.StateChanged += (sender, e) =>
            {
                wasCalled = true;
                actualArgs = e;

                Assert.AreEqual(this.initialState.Object, e.Previous);
                Assert.AreEqual(newState.Object, e.Current);
            };

            var eventArgs = new CircuitBreakerStateChangedEventArgs { Previous = this.initialState.Object, Current = newState.Object };

            this.stateMachine.Raise(m => m.StateChanged += null, eventArgs);

            Assert.IsTrue(wasCalled);
            Assert.AreEqual(eventArgs, actualArgs);
        }

        [TestMethod]
        public void StateGotFromStateMachine()
        {
            Assert.AreEqual(this.initialState.Object, this.circuitBreaker.State);
        }

        [TestMethod]
        public void Disposed()
        {
            this.circuitBreaker.Dispose();

            this.stateMachine.Verify(m => m.Dispose(), Times.Once());
        }
    }
}
