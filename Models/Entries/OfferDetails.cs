using System;

namespace Models
{
    public class OfferDetails
    { 
        /// <summary>
        /// Adres do konkretnej oferty
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Data stworzenia oferty
        /// </summary>
        public DateTime CreationDateTime { get; set; }

        /// <summary>
        /// Data ostatniej aktualizacji oferty
        /// </summary>
        public DateTime? LastUpdateDateTime { get; set; }

        /// <summary>
        /// Rodzaj oferty - wynajem czy sprzedaż
        /// </summary>
        public OfferKind OfferKind { get; set; }

        /// <summary>
        /// Kontakt do sprzedawcy. Którekolwiek z property wewnątrz obiektu musi zostać wypełnione
        /// </summary>
        public SellerContact SellerContact { get; set; }

        /// <summary>
        /// Czy oferta jest aktualna
        /// </summary>
        public bool IsStillValid { get; set; }
        public int ID { get; set; }
    }
}