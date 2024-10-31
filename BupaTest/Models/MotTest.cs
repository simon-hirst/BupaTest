namespace BupaTest.Models
{
    public class MotTest
    {
        public string CompletedDate { get; set; }
        public string TestResult { get; set; }
        public string ExpiryDate { get; set; }
        public string OdometerValue { get; set; }
        public string OdometerUnit { get; set; }
        public string MotTestNumber { get; set; }
        public List<RfrAndComment> RfrAndComments { get; set; }
    }
}
