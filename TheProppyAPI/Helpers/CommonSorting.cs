namespace TheProppyAPI.Helpers
{
    public class CommonSortFilter
    {

    }
    public class CommonFilter
    {
        public int[]? NoOfBeds { get; set; }
        public int[]? NoOfBaths { get; set; }
        public PriceRange? Price { get; set; }
        public string[]? Location { get; set; }
        public string[]? City { get; set; }
        public bool? Nofee { get; set; }
        public string? DealType { get; set; }
    }
    public class PriceRange
    {
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
    }
}