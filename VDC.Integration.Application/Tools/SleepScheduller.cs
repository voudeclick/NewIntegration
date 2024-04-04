namespace VDC.Integration.Application.Tools
{
    public class SleepScheduller
    {
        private readonly int _maxSleep;
        private readonly int _initialSleep;
        private readonly int _sleepStep;
        private int _timeToSleep;

        public SleepScheduller(int maxSleep, int initialSleep, int sleepStep)
        {
            _maxSleep = maxSleep;
            _initialSleep = initialSleep;
            _sleepStep = sleepStep;
            _timeToSleep = 0;
        }

        public void Reset()
        {
            _timeToSleep = 0;
        }

        public int GetNextTime()
        {
            if (_timeToSleep == 0)
                _timeToSleep = _initialSleep;
            else
                _timeToSleep *= _sleepStep;

            if (_timeToSleep > _maxSleep)
                _timeToSleep = _maxSleep;

            return _timeToSleep;
        }
    }
}
