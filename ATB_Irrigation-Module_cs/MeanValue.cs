namespace local
{
    public class MeanValue
    {
        private int num = 0;
        private double sum = 0;

        public double value
        {
            get
            {
                if (num == 0) return 0;
                return sum / num;
            }
        }

        public void Add(double value)
        {
            sum += value;
            num++;
        }
    }
}
