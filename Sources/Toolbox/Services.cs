namespace Module2.Toolbox;

/// <summary>
/// Comme j'ai plusieurs services locator de type différents,
/// je les fais hériter de cette classe abstraite qui possède
/// le dictionnaire et les méthodes Register et Get.
///
/// Comme l'héritage ne peut se produire dans un contexte statique,
/// il permet également d'en faire des singletons. Et c'est la
/// partie un peu tricky : pour pouvoir instancier un enfant depuis
/// un parent, je dois le passer en paramètre.
/// </summary>
public abstract class ServiceLocator<TSelf, TKey, TValue>
    where TSelf : ServiceLocator<TSelf, TKey, TValue>, new()
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _items = [];
    private static TSelf? _instance;
    
    public static TSelf Instance => _instance ??= new TSelf();

    public void Register(TKey key, TValue obj) {
        _items.Add(key, obj);
    }

    protected bool Has(TKey key) {
        return _items.ContainsKey(key);
    }

    public TValue Get(TKey key) {
        return _items[key];
    }
}
