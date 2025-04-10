using System.Collections.ObjectModel;

namespace Module2.Toolbox;

/// <summary>
/// Dans les cas où je ne peux pas utiliser de struct, mais que
/// j'ai beaucoup d'instances du même type à recréer souvent,
/// j'utilse un object pool pour réutiliser des anciennes instances
/// afin de ne pas allouer de la mémoire inutilement (le garbage
/// collector ne passe jamais chez moi...).
/// </summary>
public class ObjectPool<T> where T : class, new() {
    // J'utilise une stack pour récupérer les objets encore "chauds" dans le cache CPU
    private readonly Stack<T> _free = [];
    private readonly List<T> _used = [];

    public ReadOnlyCollection<T> AllUsed => _used.AsReadOnly();

    public T Create() {
        var obj = _free.Count > 0 ? _free.Pop() : new T();
        _used.Add(obj);

        return obj;
    }

    public void Destroy(T obj) {
        _used.Remove(obj);
        _free.Push(obj);
    }

    public void DestroyAll() {
        foreach (var obj in _used) {
            _free.Push(obj);
        }

        _used.Clear();
    }
}
