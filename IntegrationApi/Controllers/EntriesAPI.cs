using Microsoft.AspNetCore.Mvc;
using Application.Sample;
using Models;
using DatabaseConnection;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;

namespace IntegrationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntryController : ControllerBase
    {
        private DataBaseCtx dbCtx;
        public EntryController()
        {
            dbCtx = new DataBaseCtx();
        }
        ~EntryController()
        {
            dbCtx.Dispose();
        }

        /*
         * Zapytanie zwraca wszystkie Entry z bazy danych
         * Uzycie: GET: api/Entry
         */
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get()
        {
            var entries = dbCtx.Entries.Include(x => x.PropertyPrice)
                .Include(x => x.PropertyFeatures)
                .Include(x => x.PropertyDetails)
                .Include(x => x.PropertyAddress)
                .Include(x => x.OfferDetails).ThenInclude(x => x.SellerContact);
            if (entries.Any())
                return Ok(entries);
            else
                return NotFound();
        }

        /*
         * Zapytanie zwraca Entry o podanycm ID z bazy danych
         * Uzycie: GET: api/Entry/Entry_id
         */
        [HttpGet("{id}", Name = "Get2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(int id)
        {
            var entries = dbCtx.Entries.Include(x => x.PropertyPrice)
                .Include(x => x.PropertyFeatures)
                .Include(x => x.PropertyDetails)
                .Include(x => x.PropertyAddress)
                .Include(x => x.OfferDetails).ThenInclude(x => x.SellerContact).Where(e => e.ID == id);
            if (entries.Any())
                return Ok(entries);
            else
                return NotFound();
        }

        /*
         * Zapytanie zwraca Entry o podanycm ID z bazy danych
         * Uzycie: GET: api/Entry/Rozmiar_strony/Numer_strony
         */
        [HttpGet("{pagesize}/{pagenumber}", Name = "Get3")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(int pagesize, int pagenumber)
        {
            var begin = (pagenumber - 1) * pagesize;
            var end = pagenumber * pagesize + 1;
            var entries = dbCtx.Entries.Include(x => x.PropertyPrice)
                .Include(x => x.PropertyFeatures)
                .Include(x => x.PropertyDetails)
                .Include(x => x.PropertyAddress)
                .Include(x => x.OfferDetails).ThenInclude(x => x.SellerContact).Where(e => e.ID > begin && e.ID < end);
            if (entries.Any())
                return Ok(entries);
            else
                return NotFound();
        }

        /*
         * Zapytanie zapisuje wszystkie Entry pobrane z serwisu
         * "gwenieruchomosci" do bazy danych
         * Uzycie: POST: api/Entry
         */
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Post()
        {
            var entries = Integration.GetEntries();
            if (!entries.Any())
                return NotFound();
            dbCtx.Entries.AddRange(entries);
            dbCtx.SaveChanges();

            return Ok(entries); ;
        }

        /*
         * Zapytanie zapisuje wszystkie Entry pobrane z serwisu
         * "gwenieruchomosci" z wybranej strony do bazy danych
         * Uzycie: POST: api/Entry/Numer_strony
         */
        [HttpPost("{pageNumber}", Name = "Post2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Post(int pageNumber)
        {
            var entries = Integration.GetEntriesFromPage(pageNumber);
            if (!entries.Any())
                return NotFound();
            dbCtx.Entries.AddRange(entries);
            dbCtx.SaveChanges();

            return Ok(entries);
        }

        /*
         * Zapytanie aktualizuje wybrane pole w określonym przez ID Entry
         * w bazie danych
         * Uzycie: PUT: api/Entry/Entry_ID/Atrybut/Wartosc
         */
        [HttpPut("{id}/{field}/{value}", Name = "Put")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Put(int id, string field, string value)
        {
            var toUpdate = dbCtx.Entries.Include(x => x.PropertyPrice)
                .Include(x => x.PropertyFeatures)
                .Include(x => x.PropertyDetails)
                .Include(x => x.PropertyAddress)
                .Include(x => x.OfferDetails).ThenInclude(x => x.SellerContact).Where(e => e.ID == id);
            if (field.ToLower() == "telephone")
            {
                toUpdate.Single<Entry>().OfferDetails.SellerContact.Telephone = value;
            }
            else if (field.ToLower() == "rawdescription")
            {
                toUpdate.Single<Entry>().RawDescription = value;
            }
            else if (field.ToLower() == "url")
            {
                toUpdate.Single<Entry>().RawDescription = value;
            }
            else if (field.ToLower() == "city")
            {
                PolishCity result;
                if (Enum.TryParse(value.ToUpper(), out result))
                {
                    toUpdate.Single<Entry>().PropertyAddress.City = result;
                }
                else
                {
                    toUpdate.Single<Entry>().PropertyAddress.City = PolishCity.BRAK;
                }
            }
            else if (field.ToLower() == "district")
            {
                toUpdate.Single<Entry>().PropertyAddress.District = value;
            }
            else if (field.ToLower() == "streetname")
            {
                toUpdate.Single<Entry>().PropertyAddress.StreetName = value;
            }
            else if (field.ToLower() == "area")
            {
                decimal area;
                bool ret = decimal.TryParse(value.Replace('.', ','), out area);
                if (ret == false)
                    return BadRequest();
                toUpdate.Single<Entry>().PropertyDetails.Area = area;
            }
            else if (field.ToLower() == "numberofrooms")
            {
                int rooms;
                bool ret = Int32.TryParse(value, out rooms);
                if (ret == false)
                    return BadRequest();
                toUpdate.Single<Entry>().PropertyDetails.NumberOfRooms = rooms;
            }
            else if (field.ToLower() == "floornumber")
            {
                int floor;
                bool ret = Int32.TryParse(value, out floor);
                if (ret == false)
                    return BadRequest();
                toUpdate.Single<Entry>().PropertyDetails.FloorNumber = floor;
            }
            else if (field.ToLower() == "yearofconstruction")
            {
                int year;
                bool ret = Int32.TryParse(value, out year);
                if (ret == false)
                    return BadRequest();
                toUpdate.Single<Entry>().PropertyDetails.YearOfConstruction = year;
            }
            else if (field.ToLower() == "gardenarea")
            {
                decimal garden;
                bool ret = decimal.TryParse(value.Replace('.', ','), out garden);
                if (ret == false)
                    return BadRequest();
                toUpdate.Single<Entry>().PropertyFeatures.GardenArea = garden;
            }
            else if (field.ToLower() == "balconies")
            {
                int balcony;
                bool ret = Int32.TryParse(value, out balcony);
                if (ret == false)
                    return BadRequest();
                toUpdate.Single<Entry>().PropertyFeatures.Balconies = balcony;
            }
            else if (field.ToLower() == "basementarea")
            {
                decimal basement;
                bool ret = decimal.TryParse(value.Replace('.', ','), out basement);
                if (ret == false)
                    return BadRequest();
                toUpdate.Single<Entry>().PropertyFeatures.BasementArea = basement;
            }
            else if (field.ToLower() == "outdoorparkingplaces")
            {
                int parking;
                bool ret = Int32.TryParse(value, out parking);
                if (ret == false)
                    return BadRequest();
                toUpdate.Single<Entry>().PropertyFeatures.OutdoorParkingPlaces = parking;
            }
            else if (field.ToLower() == "indoorparkingplaces")
            {
                int garage;
                bool ret = Int32.TryParse(value, out garage);
                if (ret == false)
                    return BadRequest();
                toUpdate.Single<Entry>().PropertyFeatures.OutdoorParkingPlaces = garage;
            }
            else if (field.ToLower() == "totalgrossprice")
            {
                decimal price;
                bool ret = decimal.TryParse(value.Replace('.', ','), out price);
                if (ret == false)
                    return BadRequest();
                toUpdate.Single<Entry>().PropertyPrice.TotalGrossPrice = price;
            }
            else if (field.ToLower() == "pricepermeter")
            {
                decimal pricePerMeter;
                bool ret = decimal.TryParse(value.Replace('.', ','), out pricePerMeter);
                if (ret == false)
                    return BadRequest();
                toUpdate.Single<Entry>().PropertyPrice.PricePerMeter = pricePerMeter;
            }
            else if (field.ToLower() == "email")
            {
                toUpdate.Single<Entry>().OfferDetails.SellerContact.Email = value;
            }
            else
                return BadRequest();

            dbCtx.SaveChanges();
            return Ok(toUpdate);
        }

        /*
         * Zapytanie aktualizuje Entry w bazie danych określone przez
         * ID w request body. Wszytskie pola są aktualizowane na te
         * określone w request body.
         * Uzycie: PUT: api/Entry
         */
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Put(Entry requestEntry)
        {
            var toUpdate = dbCtx.Entries.Include(x => x.PropertyPrice)
                .Include(x => x.PropertyFeatures)
                .Include(x => x.PropertyDetails)
                .Include(x => x.PropertyAddress)
                .Include(x => x.OfferDetails).ThenInclude(x => x.SellerContact).Where(e => e.ID == requestEntry.ID);

            toUpdate.Single<Entry>().OfferDetails.Url = requestEntry.OfferDetails.Url;
            toUpdate.Single<Entry>().OfferDetails.CreationDateTime = requestEntry.OfferDetails.CreationDateTime;
            toUpdate.Single<Entry>().OfferDetails.LastUpdateDateTime = requestEntry.OfferDetails.LastUpdateDateTime;
            toUpdate.Single<Entry>().OfferDetails.OfferKind = requestEntry.OfferDetails.OfferKind;
            toUpdate.Single<Entry>().OfferDetails.IsStillValid = requestEntry.OfferDetails.IsStillValid;

            toUpdate.Single<Entry>().OfferDetails.SellerContact.Email = requestEntry.OfferDetails.SellerContact.Email;
            toUpdate.Single<Entry>().OfferDetails.SellerContact.Telephone = requestEntry.OfferDetails.SellerContact.Telephone;
            toUpdate.Single<Entry>().OfferDetails.SellerContact.Name = requestEntry.OfferDetails.SellerContact.Name;

            toUpdate.Single<Entry>().PropertyPrice.TotalGrossPrice = requestEntry.PropertyPrice.TotalGrossPrice;
            toUpdate.Single<Entry>().PropertyPrice.PricePerMeter = requestEntry.PropertyPrice.PricePerMeter;
            toUpdate.Single<Entry>().PropertyPrice.ResidentalRent = requestEntry.PropertyPrice.ResidentalRent;

            toUpdate.Single<Entry>().PropertyDetails.Area = requestEntry.PropertyDetails.Area;
            toUpdate.Single<Entry>().PropertyDetails.NumberOfRooms = requestEntry.PropertyDetails.NumberOfRooms;
            toUpdate.Single<Entry>().PropertyDetails.FloorNumber = requestEntry.PropertyDetails.FloorNumber;
            toUpdate.Single<Entry>().PropertyDetails.YearOfConstruction = requestEntry.PropertyDetails.YearOfConstruction;

            toUpdate.Single<Entry>().PropertyAddress.City = requestEntry.PropertyAddress.City;
            toUpdate.Single<Entry>().PropertyAddress.District = requestEntry.PropertyAddress.District;
            toUpdate.Single<Entry>().PropertyAddress.StreetName = requestEntry.PropertyAddress.StreetName;
            toUpdate.Single<Entry>().PropertyAddress.DetailedAddress = requestEntry.PropertyAddress.DetailedAddress;

            toUpdate.Single<Entry>().PropertyFeatures.GardenArea = requestEntry.PropertyFeatures.GardenArea;
            toUpdate.Single<Entry>().PropertyFeatures.Balconies = requestEntry.PropertyFeatures.Balconies;
            toUpdate.Single<Entry>().PropertyFeatures.BasementArea = requestEntry.PropertyFeatures.BasementArea;
            toUpdate.Single<Entry>().PropertyFeatures.OutdoorParkingPlaces = requestEntry.PropertyFeatures.OutdoorParkingPlaces;
            toUpdate.Single<Entry>().PropertyFeatures.IndoorParkingPlaces = requestEntry.PropertyFeatures.IndoorParkingPlaces;

            toUpdate.Single<Entry>().RawDescription = requestEntry.RawDescription;

            dbCtx.SaveChanges();
            return Ok(toUpdate);
        }
    }
}
