using System;
using System.Threading;

namespace AsyncResiliencyPatterns
{
    internal class SemaphoreSuperSlim
    {
        private int maximumAllowed;
        private int currentCount;

        public SemaphoreSuperSlim(int maximumAllowed)
        {
            if (maximumAllowed <= 0 || maximumAllowed > 1000) throw new ArgumentOutOfRangeException("maximumAllowed");
            this.maximumAllowed = maximumAllowed;
        }

        public int AvailableCount
        {
            get { return this.maximumAllowed - this.currentCount; }
        }

        public int CurrentCount
        {
            get { return this.currentCount; }
        }

        public bool Acquire()
        {
            return this.AddToCount(1);
        }

        public void Release()
        {
            this.AddToCount(-1);
        }

        private bool AddToCount(int incrementValue)
        {
            int initialValue, computedValue;
            do
            {
                initialValue = this.currentCount;
                computedValue = initialValue + incrementValue;

                if (computedValue > this.maximumAllowed) return false;
                if (computedValue < 0) computedValue = 0;
            } while (Interlocked.CompareExchange(ref this.currentCount, computedValue, initialValue) != initialValue);

            return true;
        }
    }
}
