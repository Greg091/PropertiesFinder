using Interfaces;
using Models;
using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Application.Sample
{
    public class Integration : IWebSiteIntegration
    {
        public WebPage WebPage { get; }
        public IDumpsRepository DumpsRepository { get; }

        public IEqualityComparer<Entry> EntriesComparer { get; }

        public Integration(IDumpsRepository dumpsRepository,
            IEqualityComparer<Entry> equalityComparer)
        {
            DumpsRepository = dumpsRepository;
            EntriesComparer = equalityComparer;
            WebPage = new WebPage
            {
                Url = "https://gwenieruchomosci.pl/",
                Name = "GWE nieruchomosci",
                WebPageFeatures = new WebPageFeatures
                {
                    HomeSale = true,
                    HomeRental = false,
                    HouseSale = false,
                    HouseRental = false
                }
            };
        }
        /* klasa przechowuje dane potrzebne do utworzenia nowego Entry */
        public class propertyInfo
        {
            public string email { get; set; }
            public string phone { get; set; }
            public string description { get; set; }
            public long price { get; set; }
            public int price_per_m { get; set; }
            public PolishCity city { get; set; }
            public string street { get; set; }
            public float area { get; set; }
            public int floor { get; set; }
            public int rooms { get; set; }
            public int year { get; set; }
            public int balcony { get; set; }
            public bool basement { get; set; }
            public string parking { get; set; }
            public string district { get; set; }
            public int garage { get; set; }
            public string url { get; set; }
            public propertyInfo() { }
        }
        public Dump GenerateDump()
        {
            List<string> offers = getAllPagesWithOffer();
            var entries = new List<Entry>();
            foreach(string offer in offers)
            {
                entries.Add(newEntry(parseOffer(offer)));
            }

            var dump = new Dump
            {
                DateTime = DateTime.Now,
                WebPage = WebPage,
                Entries = entries,
            };

            return dump;
        }

        /*  getHtmlDocument - pobranie zawartości strony internetowej*/
        private HtmlDocument getHtmlDocument(String url)
        {
            var htmlWeb = new HtmlWeb();
            var lastStatusCode = HttpStatusCode.OK;

            htmlWeb.PostResponse = (request, response) =>
            {
                if (response != null)
                {
                    lastStatusCode = response.StatusCode;
                }
            };

            var doc = htmlWeb.Load(url);
            if (lastStatusCode == HttpStatusCode.OK)
            {
                return doc;
            }

            return null;
        }

        public static string URL_FLAT_SALE = "https://gwenieruchomosci.pl/oferty?oferta=sprzedaz&nieruchomosc=mieszkanie&strona=";

        /* getAllPagesWithOffer - pobranie wszystkich adresów stron zawierających ofertę sprzedaży mieszkania */
        public List<string> getAllPagesWithOffer()
        {
            List<string> pagesWithOffer = new List<string>();
            int pageIndex = 1;
            string url = URL_FLAT_SALE + pageIndex.ToString();
            var page = getHtmlDocument(url);
            while (page.DocumentNode.SelectNodes("//a[@class='property-img']") != null)
            {
                var offers = page.DocumentNode.SelectNodes("//a[@class='property-img']");
                foreach (var offer in offers)
                {
                    pagesWithOffer.Add(offer.GetAttributeValue("href", string.Empty));
                }
                pageIndex++;
                url = URL_FLAT_SALE + pageIndex.ToString();
                page = getHtmlDocument(url);
            }
            return pagesWithOffer;
        }

        /*
         * parseOffer - parsuje niebędne dane ze strony z ofertą sprzedaży
         * mieszkania, i zwraca je w postaci obiektu kalsy propertyInfo
         */
        public propertyInfo parseOffer(string url_offer)
        {
            propertyInfo p_info = new propertyInfo();
            p_info.url = url_offer;

            var offer = getHtmlDocument(url_offer);
            p_info.description = offer.DocumentNode.SelectSingleNode(
                "//div[@class='properties-description mrg-btm-40 ']").InnerText.Trim().TrimEnd();

            /* pobranie i parsowanie części strony zawierającej dane kontakowe */
            var contact_info_nodes = offer.DocumentNode.SelectNodes(
                "//div[@class='sidebar-widget helping-box clearfix']");
            string contact_info = "";
            foreach (var i in contact_info_nodes)
            {
                if (i.InnerText.Contains("Opiekun  oferty"))
                    contact_info = i.InnerText;
            }
            var list_info = contact_info.Split('\n');
            foreach (var i in list_info)
            {
                if (Regex.IsMatch(i.Trim().TrimEnd(), "^[0-9]+$"))
                {
                    p_info.phone = i.Trim().TrimEnd();
                }
                else if (i.Trim().TrimEnd().Contains('@'))
                {
                    p_info.email = i.Trim().TrimEnd();
                }
            }

            /* pobranie i parsowanie części strony zawierającej dane opisujące mieszkanie */
            var elements = offer.DocumentNode.SelectNodes("//ul[@class='condition']");
            foreach (var a in elements)
            {
                var list = a.InnerText.Split('\n');
                foreach(var i in list)
                {
                    string property = i.Trim().TrimEnd();
                    if (property.Contains("Cena:"))
                    {
                        string pattern = "([0-9]*[0-9\\s]+[0-9]*)";
                        Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                        Match m = r.Match(property);
                        string temp = m.ToString().Replace(" ", "");
                        p_info.price = Convert.ToInt64(temp);
                    }
                    else if (property.Contains("Cena za m"))
                    {
                        string pattern = "(m2:\\s)([0-9]*[\\s]*[0-9]*)";
                        Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                        Match m = r.Match(property);
                        string temp = m.Groups[2].ToString().Replace(" ", "");
                        p_info.price_per_m = Int32.Parse(temp);
                    }
                    else if (property.Contains("Lokalizacja"))
                    {
                        string pattern = ":\\s*([\\S\\s]*)";
                        Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                        Match m = r.Match(property);
                        string address = m.Groups[1].ToString();
                        string[] add_list = address.Split(',');
                        p_info.street = add_list[0];
                        p_info.city = parseCity(add_list[1].Trim());
                        p_info.district = parseDistrict(add_list[1].Trim());
                    }
                    else if (property.Contains("Rok budowy"))
                    {
                        string pattern = ":\\s*([0-9]*)";
                        Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                        Match m = r.Match(property);
                        p_info.year = Int32.Parse(m.Groups[1].ToString());
                    }
                    else if (property.Contains("Liczba pokoi"))
                    {
                        string pattern = ":\\s*([0-9]*)";
                        Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                        Match m = r.Match(property);
                        p_info.rooms = Int32.Parse(m.Groups[1].ToString());
                    }
                    else if (property.Contains("Balkon"))
                    {
                        if (property.Contains("nie"))
                            p_info.balcony = 0;
                        else if (property.Contains("tak"))
                            p_info.balcony = 1;
                        else
                        {
                            string pattern = ":\\s*([0-9]*)";
                            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                            Match m = r.Match(property);
                            p_info.balcony = Int32.Parse(m.Groups[1].ToString());
                        }

                    }
                    else if (property.Contains("Piwnica"))
                    {
                        p_info.basement = false;
                        if (property.Contains("tak"))
                            p_info.basement = true;
                    }
                    else if (property.Contains("Piętro:"))
                    {
                        if (property.Contains("arter"))
                        {
                            p_info.floor = 0;
                        }
                        else
                        {
                            string pattern = ":\\s*([0-9]*)";
                            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                            Match m = r.Match(property);
                            p_info.floor = Int32.Parse(m.Groups[1].ToString());
                        }
                    }
                    else if (property.Contains("Powierzchnia"))
                    {
                        string pattern = ":\\s*([0-9,]*)";
                        Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                        Match m = r.Match(property);
                        p_info.area = float.Parse(m.Groups[1].ToString());
                    }
                    else if (property.Contains("Garaż"))
                    {
                        if (property.Contains("nie"))
                            p_info.garage = 0;
                        else
                        {
                            string pattern = ":\\s*([0-9]*)";
                            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                            Match m = r.Match(property);
                            p_info.garage = Int32.Parse(m.Groups[1].ToString());
                        }
                    }
                    else if (property.Contains("Parking"))
                    {
                        string[] parking_info = property.Split(' ');
                        p_info.parking = parking_info[1].Trim();
                    }
                }
            }
            return p_info;
        }

        /* newEntry - stworzenie noewego Entry */
        private Entry newEntry(propertyInfo pi)
        {
            var entry = new Entry
            {
                OfferDetails = new OfferDetails
                {
                    Url = pi.url,
                    CreationDateTime = DateTime.Now,
                    OfferKind = OfferKind.SALE,
                    SellerContact = new SellerContact
                    {
                        Email = pi.email,
                        Telephone = pi.phone
                    },
                    IsStillValid = true
                },
                RawDescription = translatePolishSigns(pi.description),
                PropertyPrice = new PropertyPrice
                {
                    PricePerMeter = (decimal)pi.price_per_m,
                    TotalGrossPrice = (decimal)pi.price,
                    ResidentalRent = null //brak danych o czynszu
                },
                PropertyAddress = new PropertyAddress
                {
                    City = pi.city,
                    StreetName = translatePolishSigns(pi.street),
                    DetailedAddress = "",
                    District = pi.district
                },
                PropertyDetails = new PropertyDetails
                {
                    Area = (decimal)pi.area,
                    FloorNumber = pi.floor,
                    NumberOfRooms = pi.rooms,
                    YearOfConstruction = pi.year
                },
                PropertyFeatures = new PropertyFeatures
                {
                    Balconies = pi.balcony,
                    BasementArea = null, // tylko informacja czy jest piwnica
                    GardenArea = null, //brak danych
                    IndoorParkingPlaces = pi.garage, //
                    OutdoorParkingPlaces = null // tylko informacja opisowa
                }
            };
            return entry;
        }
        /* 
         * parseCity - parsuje lokazlizację i zwraca nazwę miasta,
         * w razie niepowodzenia ustawia nazwę na BRAK
         */
        public PolishCity parseCity(string loc)
        {
            PolishCity result;
            string city = loc.Split(' ')[0].Trim().ToUpper();
            if (Enum.TryParse(city, out result))
            {
                return result;
            }
            return PolishCity.BRAK;
        }
        /* parseDistrict - parsuje lokalizację i zwraca nazwę dzielnicy */
        public string parseDistrict(string loc)
        {
            string district_name = "";
            foreach (string name in loc.Split(' ').Skip(1))
            {
                district_name += name + " "; 
            }
            return district_name;
        }

        /* translatePolishSigns - usuwa znaki polskie i zamienia na zwykłe */
        public string translatePolishSigns(String toTranslate)
        {
            var after = toTranslate.Replace('Ą', 'A');
            after = toTranslate.Replace('Ć', 'C');
            after = toTranslate.Replace('Ę', 'E');
            after = toTranslate.Replace('Ł', 'L');
            after = toTranslate.Replace('Ó', 'O');
            after = toTranslate.Replace('Ń', 'N');
            after = toTranslate.Replace('Ś', 'S');
            after = toTranslate.Replace('Ż', 'Z');
            after = toTranslate.Replace('Ź', 'Z');
            after = toTranslate.Replace('ą', 'a');
            after = toTranslate.Replace('ć', 'c');
            after = toTranslate.Replace('ę', 'e');
            after = toTranslate.Replace('ł', 'l');
            after = toTranslate.Replace('ó', 'o');
            after = toTranslate.Replace('ń', 'n');
            after = toTranslate.Replace('ś', 's');
            after = toTranslate.Replace('ż', 'z');
            after = toTranslate.Replace('ź', 'z');

            return after;
        }
    }
}
