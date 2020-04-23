namespace BitfinexTrader
{
    public class MovingAverage
    {
        private readonly double[] numbers;
        private int index;
        private int count;
        private double value;
        private bool needsUpdate;

        public MovingAverage(int count)
        {
            numbers = new double[count];
        }

        /// <summary>
        ///     Pushed a value to the array.
        /// </summary>
        /// <param name="value"></param>
        public void Push(double value)
        {
            numbers[index % numbers.Length] = value;
            index++;
            count++;
            if (count > numbers.Length)
            {
                count = numbers.Length;
            }

            needsUpdate = true;
        }

        /// <summary>
        ///     Updates the moving average value.
        /// </summary>
        private void Update()
        {
            double sum = 0;
            for (var i = 0; i < count; i++)
            {
                sum += numbers[i];
            }

            value = sum / count;
        }

        /// <summary>
        ///     Gets the average value of the array.
        /// </summary>
        public double Value
        {
            get
            {
                if (needsUpdate)
                {
                    Update();
                }

                return value;
            }
        }
    }
}