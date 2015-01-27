using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AsyncResiliencyPatterns.Tests
{
    [TestClass]
    public class CircuitBreakerStateMachineImplTests
    {
        private CircuitBreakerStateMachineImpl stateMachine;
        private Mock<CircuitBreakerStateInternal> initialState;

        [TestInitialize]
        public void TestInitialize()
        {
            this.initialState = new Mock<CircuitBreakerStateInternal>();
            this.stateMachine = new CircuitBreakerStateMachineImpl(initialState.Object);
        }

        [TestMethod]
        public void TransitionDisposed()
        {
            this.initialState = new Mock<CircuitBreakerStateInternal>();
            Mock<IDisposable> disposable = initialState.As<IDisposable>();
            this.stateMachine = new CircuitBreakerStateMachineImpl(initialState.Object);

            this.stateMachine.State = new Mock<CircuitBreakerStateInternal>().Object;

            disposable.Verify(m => m.Dispose(), Times.Once());
        }

        [TestMethod]
        public void StateChangedNotification()
        {
            var parameters = new CircuitBreakerStateParameters(this.stateMachine, new CircuitBreakerSettings(), new InnerCommandInvoker());
            var newState = new CircuitBreakerNormalState(parameters);
            bool wasCalled = false;
            this.stateMachine.StateChanged += (sender, e) =>
            {
                wasCalled = true;

                Assert.AreEqual(this.initialState.Object, e.Previous);
                Assert.AreEqual(newState, e.Current);
            };
            this.stateMachine.State = newState;

            Assert.IsTrue(wasCalled);
            Assert.AreEqual(newState, this.stateMachine.State);
        }

        [TestMethod]
        public void DoNotTransitionToSameState()
        {
            var newState = new Mock<CircuitBreakerStateInternal>().Object;
            this.stateMachine.State = newState;

            Assert.AreNotEqual(newState, this.stateMachine.State);
        }
    }
}
