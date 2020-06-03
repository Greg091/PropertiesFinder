namespace Models
{
    public class Entry
    {
        public OfferDetails OfferDetails { get; set; }

        public PropertyPrice PropertyPrice { get; set; }
    
        public PropertyDetails PropertyDetails { get; set; }

        public PropertyAddress PropertyAddress { get; set; }

        public PropertyFeatures PropertyFeatures { get; set; }

        /// <summary>
        /// Nieprzetworzony tekst z ogłoszenia
        /// </summary>
        public string RawDescription { get; set; }
        public int ID { get; set; }
    }
}