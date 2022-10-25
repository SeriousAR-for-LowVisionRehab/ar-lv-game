/// <summary>
/// State is an element of a Finite-State-Machine (FSM)
/// <a href="https://github.com/shamim-akhtar/fsm-generic">(Source: GitHub)</a>
/// </summary>
/// <typeparam name="T"></typeparam>
public class State<T>
{
    public string Name { get; set; }
    public T ID { get; private set; }

    public State(T id)
    {
        ID = id;
    }
    public State(string name, T id): this(id)
    {
        Name = name;
    }

    // Delegates to handle key function calls:
    public delegate void DelegateNoArg();
    public DelegateNoArg OnEnter;
    public DelegateNoArg OnExit;
    public DelegateNoArg OnUpdate;
    public DelegateNoArg OnFixedUpdate;

    public State(string name,
        T id, 
        DelegateNoArg onEnter, 
        DelegateNoArg onExit=null, 
        DelegateNoArg onUpdate=null, 
        DelegateNoArg onFixedUpdate=null) : this(name, id)
    {
        OnEnter = onEnter;
        OnExit = onExit;
        OnUpdate = onUpdate;
        OnFixedUpdate = onFixedUpdate;
    }

    public State(T id,
    DelegateNoArg onEnter,
    DelegateNoArg onExit = null,
    DelegateNoArg onUpdate = null,
    DelegateNoArg onFixedUpdate = null) : this(id)
    {
        OnEnter = onEnter;
        OnExit = onExit;
        OnUpdate = onUpdate;
        OnFixedUpdate = onFixedUpdate;
    }

    virtual public void Enter() 
    {
        OnEnter?.Invoke();
    }
    virtual public void Exit()
    {
        OnExit?.Invoke();
    }
    virtual public void Update() 
    {
        OnUpdate?.Invoke();
    }
    virtual public void FixedUpdate() 
    {
        OnFixedUpdate?.Invoke();
    }
}
