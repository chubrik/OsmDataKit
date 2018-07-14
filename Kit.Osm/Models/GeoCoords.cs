namespace Kit.Osm
{
    public class GeoCoords : IGeoCoords
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public GeoCoords(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
