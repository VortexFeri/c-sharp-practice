namespace ticket_purchaser;

public abstract class SerializableSingleton<TItem>(string filePath) : FileInit
{
    private static Lazy<SerializableSingleton<TItem>>? _lazy;

    public static SerializableSingleton<TItem> Instance
    {
        get
        {
            if (_lazy == null || _lazy.Value == null)
                throw new InvalidOperationException($"{typeof(SerializableSingleton<TItem>).Name} must be initialized with a file path before use.");
            return _lazy.Value;
        }
    }

    protected List<TItem> Items = [];
    protected readonly string FilePath = filePath;

    public static void Initialize(Func<string, SerializableSingleton<TItem>> factoryMethod, string filePath)
    {
        if (!IsInitialized)
        {
            InitialFilePath = filePath;
            _lazy = new(() => factoryMethod(InitialFilePath!));
            IsInitialized = true;
        }
        else
        {
            throw new InvalidOperationException($"{typeof(SerializableSingleton<TItem>).Name} is already initialized.");
        }
    }

    public List<TItem> GetItems()
    {
        return [.. Items];
    }
}

public class FileInit
{
    protected static string? InitialFilePath { get; set; }

    protected static bool IsInitialized { get; set; }
}