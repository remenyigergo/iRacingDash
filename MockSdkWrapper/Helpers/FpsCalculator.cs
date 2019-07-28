namespace MockSdkWrapper.Helpers
{
    public static class FpsCalculator
    {
        public static int GetPeriodFromFPS(double fps)
        {
            return (int)(1000 / fps);
        }

        public static int GetFpsFromPeriod(int period)
        {
            return 1000 / period;
        }
    }
}