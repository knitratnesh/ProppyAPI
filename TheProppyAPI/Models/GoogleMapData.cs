﻿namespace TheProppyAPI.Models
{
    public class GoogleMapData
    {
    }
    public class Prediction
    {
        public string description { get; set; }
        public List<MatchedSubstring> matched_substrings { get; set; }
        public string place_id { get; set; }
        public string reference { get; set; }
        public StructuredFormatting structured_formatting { get; set; }
        public List<Term> terms { get; set; }
        public List<string> types { get; set; }
    }
    public class MainTextMatchedSubstring
    {
        public int length { get; set; }
        public int offset { get; set; }
    }

    public class MatchedSubstring
    {
        public int length { get; set; }
        public int offset { get; set; }
    }
    public class Root
    {
        public List<Prediction> predictions { get; set; }
        public string status { get; set; }
    }

    public class StructuredFormatting
    {
        public string main_text { get; set; }
        public List<MainTextMatchedSubstring> main_text_matched_substrings { get; set; }
        public string secondary_text { get; set; }
    }

    public class Term
    {
        public int offset { get; set; }
        public string value { get; set; }
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class AddressComponent
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        public List<string> types { get; set; }
    }

    public class Bounds
    {
        public Northeast northeast { get; set; }
        public Southwest southwest { get; set; }
    }

    public class Geometry
    {
        public Bounds bounds { get; set; }
        public Location location { get; set; }
        public string location_type { get; set; }
        public Viewport viewport { get; set; }
    }

    public class Location
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Northeast
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Result
    {
        public List<AddressComponent> address_components { get; set; }
        public string formatted_address { get; set; }
        public Geometry geometry { get; set; }
        public string place_id { get; set; }
        public List<string> types { get; set; }
    }

    public class LatRoot
    {
        public List<Result> results { get; set; }
        public string status { get; set; }
    }

    public class Southwest
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Viewport
    {
        public Northeast northeast { get; set; }
        public Southwest southwest { get; set; }
    }


}
