namespace BupaTest.Models
{
    public class VehicleData
    {
        public string Registration { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string FirstUsedDate { get; set; }
        public string FuelType { get; set; }
        public string PrimaryColour { get; set; }
        public List<MotTest> MotTests { get; set; }
    }
}
