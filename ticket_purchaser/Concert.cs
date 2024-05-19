using System.Text.Json;
using System.Text.Json.Serialization;
using res;
using user_namespace;

namespace ticket_purchaser
{
    [method: JsonConstructor]
    public class Concert(int id, string artist, string location, DateTime date, int price, int tickets) 
    {
        [JsonInclude]
        [JsonPropertyName("id")]
        public readonly int _Id = id;

        [JsonInclude]
        [JsonPropertyName("artist")]
        public readonly string Artist = artist;

        [JsonInclude]
        [JsonPropertyName("loaction")]
        public readonly string Location = location;

        [JsonInclude]
        [JsonPropertyName("tickets")]
        public readonly int Tickets = tickets;

        [JsonInclude]
        [JsonPropertyName("price")]
        public readonly int  Price = price;

        [JsonInclude]
        [JsonPropertyName("date")]
        public readonly DateTime Date = date;
    }

    public class ConcertManager : SerializableSingleton<Concert>
    {
        public static int LastId { get; }

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
            string json = JsonSerializer.Serialize(_items);
            File.WriteAllText(_filePath, json);
        }

        public Result<Account, ResultError<PurchaseError>> BuyTicket(ref Account acc, ref Concert concert)
        {
            if (concert.Tickets <= 0)
            {
                return new(Error.OutOfStock);
            }

            if (acc.HasTicket(concert._Id))
            {
                return new(Error.ConcertAlreadyInInventory);
            }

            SaveConcerts();
            return new(acc);
        }
    }

    public enum PurchaseError
    {
        ConcertAlreadyInInventory,
        NotEnoughFunds,
        OutOfStock
    }
    public readonly struct Error
    {
        public static readonly ResultError<PurchaseError> NotEnoughFunds = new(PurchaseError.NotEnoughFunds, "Insufficient funds.");
        public static readonly ResultError<PurchaseError> OutOfStock = new(PurchaseError.OutOfStock, "This concert is sold out");
        public static readonly ResultError<PurchaseError> ConcertAlreadyInInventory = new(PurchaseError.ConcertAlreadyInInventory, "You may only buy one ticket for a concert.");
    }
}
