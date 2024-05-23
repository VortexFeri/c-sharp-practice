using System.Text.Json;
using System.Text.Json.Serialization;
using res;
using user_namespace;

namespace ticket_purchaser
{
    [method: JsonConstructor]
    public class Concert(string artist, string location, DateOnly date, int price, int tickets, int id = 0)
    {
        [JsonInclude]
        [JsonPropertyName("id")]
        public readonly int Id = id == 0 ? ConcertManager.LastId + 1 : id;

        [JsonInclude]
        [JsonPropertyName("artist")]
        public readonly string Artist = artist;

        [JsonInclude]
        [JsonPropertyName("location")]
        public readonly string Location = location;

        [JsonInclude]
        [JsonPropertyName("tickets")]
        public int Tickets = tickets;

        [JsonInclude]
        [JsonPropertyName("price")]
        public readonly int Price = price;

        [JsonInclude]
        [JsonPropertyName("date")]
        public readonly DateOnly Date = date;

        internal string PrettyPrint(int headerLength)
        {
            string separator = new string('-', headerLength);
            string details = $"{Artist,-20}{Location,-20}" + $"{Date:yyyy-MM-dd}".PadRight(12) + $"{Price}".PadRight(12);

            return $"{separator}\n{details}";
        }
    }

    public class ConcertManager : SerializableSingleton<Concert>
    {
        public static int LastId { get; private set; }
        private ConcertManager(string filepath) : base(filepath)
        {
            LoadConcerts();
        }

        public static void Initialize(string filePath)
        {
            Initialize(path => new ConcertManager(path), filePath);
        }

        private void LoadConcerts()
        {
            try
            {
                string json = File.ReadAllText(_filePath);
                _items = JsonSerializer.Deserialize<List<Concert>>(json) ?? [];
            }
            catch (FileNotFoundException)
            {
                _items = [];
            }
            catch (JsonException e)
            {
                Console.WriteLine("Error loading concerts from file. Initializing empty concerts list.");
                Console.WriteLine(e.Message);
                _items = [];
            }
        }

        private void SaveConcerts()
        {
            _items = [.. _items.OrderBy(concert => concert.Date)];
            string json = JsonSerializer.Serialize(_items);
            File.WriteAllText(_filePath, json);
        }

        public Result<Concert, ResultError<PurchaseError>> GetConcertById(int id)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Id == id)
                {
                    return new(_items.ElementAt(i));
                }
            }
            return new(Error.ConcertNotFound);
        }

        public Result<Account, ResultError<PurchaseError>> BuyTicket(ref Account acc, int id)
        {

            var concert = GetConcertById(id);
            if (concert.Value == null)
            {
                return new(Error.ConcertNotFound);
            }
            if (concert.Value.Tickets <= 0)
            {
                return new(Error.OutOfStock);
            }
            if (acc.HasTicket((concert.Value.Id)))
            {
                return new(Error.ConcertAlreadyInInventory);
            }

            var userManager = (UserManager)UserManager.Instance;
            var res = userManager.BuyTicket(ref acc, concert.Value);

            if (res.IsSuccess)
            {
                concert.Value.Tickets--;
                SaveConcerts();
                return new(acc);
            }
            else
            {
                if (res.Error!.Code == UserOperationError.NotEnoughFunds)
                {
                    return new(Error.NotEnoughFunds);
                }
                else
                {
                    return new(new ResultError<PurchaseError>(PurchaseError.NotEnoughFunds, res.Error.Message));
                }
            }
        }

        public Result<Concert, ResultError<RegistrationError>> Register(string artist, string location, DateOnly date, int price, int tickets)
        {
            if (string.IsNullOrEmpty(artist))
                return new(new ResultError<RegistrationError>(RegistrationError.FormatError, "Artist field cannot be null"));
            if (string.IsNullOrEmpty(location))
                return new(new ResultError<RegistrationError>(RegistrationError.FormatError, "Location field cannot be null"));
            if (price <= 0)
                return new(Error.PriceError);
            if (tickets <= 0)
                return new(Error.NegativeOrZeroTickets);
            if (_items.Any(x => x.Artist == artist && x.Date == date))
                return new(Error.DateOverlap);

            Concert c = new(artist, location, date, price, tickets, LastId + 1);
            LastId++;
            _items.Add(c);
            SaveConcerts();
            return new(c);
        }
    }

    public enum PurchaseError
    {
        ConcertAlreadyInInventory,
        NotEnoughFunds,
        OutOfStock,
        ConcertNotFound,
        UnknownError
    }

    public enum RegistrationError
    {
        NegativeOrZeroTickets,
        DateOverlap, //TODO: date overlap check
        PriceError,
        FormatError
    }

    public readonly struct Error
    {
        public static readonly ResultError<PurchaseError> NotEnoughFunds = new(PurchaseError.NotEnoughFunds, "Insufficient funds.");
        public static readonly ResultError<PurchaseError> OutOfStock = new(PurchaseError.OutOfStock, "This concert is sold out");
        public static readonly ResultError<PurchaseError> ConcertAlreadyInInventory = new(PurchaseError.ConcertAlreadyInInventory, "You may only buy one ticket for a concert.");
        public static readonly ResultError<PurchaseError> ConcertNotFound = new(PurchaseError.ConcertNotFound, "This concert does not exist.");

        public static readonly ResultError<RegistrationError> NegativeOrZeroTickets = new(RegistrationError.NegativeOrZeroTickets, "Ticket amount should be a non-zero positive number.");
        public static readonly ResultError<RegistrationError> DateOverlap = new(RegistrationError.DateOverlap, "The artist already has a concert planned for that day");
        public static readonly ResultError<RegistrationError> PriceError = new(RegistrationError.PriceError, "The price should be a non-zero positive amount.");

    }
}
