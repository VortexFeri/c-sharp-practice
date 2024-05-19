public abstract class SerializableSingleton<TItem>
{
    private static Lazy<SerializableSingleton<TItem>>? lazy;
    private static string? _initialFilePath;
    private static bool isInitialized = false;

    public static SerializableSingleton<TItem> Instance
    {
        get
        {
            if (lazy == null || lazy.Value == null)
                throw new InvalidOperationException($"{typeof(SerializableSingleton<TItem>).Name} must be initialized with a file path before use.");
            return lazy.Value;
        }
    }

    protected List<TItem> _items = new List<TItem>();
    protected readonly string _filePath;

    protected SerializableSingleton(string filePath)
    {
        _filePath = filePath;
    }

    public static void Initialize(Func<string, SerializableSingleton<TItem>> factoryMethod, string filePath)
    {
        if (!isInitialized)
        {
            _initialFilePath = filePath;
            lazy = new Lazy<SerializableSingleton<TItem>>(() => factoryMethod(_initialFilePath!));
            isInitialized = true;
        }
        else
        {
            throw new InvalidOperationException($"{typeof(SerializableSingleton<TItem>).Name} is already initialized.");
        }
    }

    public List<TItem> GetItems()
    {
        return new List<TItem>(_items);
    }
}
