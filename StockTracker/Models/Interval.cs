namespace StockTracker.Models
{
    public class Interval
    {
        public DateTime start {  get; set; }
        public DateTime peak { get; set; }
        public double startValue {  get; set; }
        public double peakValue { get; set; }
        public double improvement { get; set; }
    }
}
