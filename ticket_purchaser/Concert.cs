using System.Text.Json;
using System.Text.Json.Serialization;
using res;

namespace ticket_purchaser
{
    [method: JsonConstructor]
    public class Concert(string artist, string location, DateTime date, int price, int tickets) 
    {
        [JsonInclude]
        [JsonPropertyName("id")]
        public int Id;
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

    public class ConcertMangager
    {
        private List<Concert> Concerts = [];
        private readonly string Filepath;

        public ConcertMangager(string filepath)
        {
            Filepath = filepath;
            LoadConcerts();
        }

        private void LoadConcerts()
        {
            try
            {
                string json = File.ReadAllText(Filepath);
                Concerts = JsonSerializer.Deserialize<List<Concert>>(json) ?? [];
            }
            catch (FileNotFoundException)
            {
                Concerts = [];
            }
            catch (JsonException e)
            {
                Console.WriteLine("Error loading concerts from file. Initializing empty concerts list.");
                Console.WriteLine(e.Message);
                Concerts = [];
            }
        }
        
        private void SaveConcerts()
        {
            string json = JsonSerializer.Serialize(Concerts);
            File.WriteAllText(Filepath, json);
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
