using System;

namespace GNServerLib
{
    internal interface IYieldCondition
    {
        bool IsFinished();
    }

    internal class WaitForSeconds : IYieldCondition
    {
        private DateTime _start;
        private TimeSpan _duration;

        public bool IsFinished() => DateTime.Now - _start > _duration;

        public WaitForSeconds(double duration)
        {
            _start = DateTime.Now;
            _duration = TimeSpan.FromSeconds(duration);
        }
    }

    internal class WaitUntil : IYieldCondition
    {
        private Func<bool> _func;

        public bool IsFinished() => _func();

        public WaitUntil(Func<bool> func)
        {
            _func = func;
        }
    }
}
