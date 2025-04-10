namespace Module2.Toolbox;

public abstract class Scene {
    public virtual void Enter() {}
    public virtual void Exit() {}
    public abstract void Update(float dt);
    public abstract void Draw();
}

public class SceneService : ServiceLocator<SceneService, Type, Scene> {
    private Scene? _current;

    public Scene Current => _current!;

    public void Register(Scene scene) {
        Register(scene.GetType(), scene);
    }

    public void GoTo<T>() where T : Scene {
        if (!Has(typeof(T))) return;

        _current?.Exit();
        _current = Get(typeof(T));
        _current.Enter();
    }
}
