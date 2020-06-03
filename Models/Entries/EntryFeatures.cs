namespace Models
{
    public class PropertyFeatures
    {

        /// <summary>
        /// Wielkość ogródka w metrach. "0" w przypadku gdy oferta nie zawiera ogródka
        /// "null" w przypadku braku danych
        /// </summary>
        public decimal? GardenArea { get; set; }

        /// <summary>
        /// Ilość balkonów. "0" w przypadku braku balkonu, "null" w przypadku braku danych
        /// </summary>
        public int? Balconies { get; set; }

        /// Wielkość piwnicy w metrach. "0" w przypadku gdy oferta nie zawiera ogródka
        /// "null" w przypadku braku danych.
        /// Komórkę lokatorską można potraktować jak piwnicę.
        public decimal? BasementArea { get; set; }

        /// <summary>
        /// Ilość miejsc prakingowych naziemnych. "0" w przypadku braku miejsc, "null" w przypadku braku danych
        /// </summary>
        public int? OutdoorParkingPlaces { get; set; }
        /// <summary>
        /// Ilość miejsc prakingowych podziemnych/garażowych. "0" w przypadku braku miejsc, "null" w przypadku braku danych
        /// </summary>
        public int? IndoorParkingPlaces { get; set; }
        public int ID { get; set; }
    }
}