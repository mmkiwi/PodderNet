namespace MMKiwi.PodderNet.Database.Sqlite;

public static class Parameter
{
    public static Parameter<T1> Create<T1>(T1 param1) => new(param1);
    public static Parameter<T1, T2> Create<T1, T2>(T1 param1, T2 param2) => new(param1, param2);

    public static Parameter<T1, T2, T3> Create<T1, T2, T3>(T1 param1, T2 param2, T3 param3) =>
        new(param1, param2, param3);

    public static Parameter<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 param1, T2 param2, T3 param3, T4 param4) =>
        new(param1, param2, param3, param4);
}

public readonly struct Parameter<T1, T2>
{
    public Parameter(T1 param1, T2 param2) => (Param1, Param2) = (param1, param2);
    public T1 Param1 { get; }
    public T2 Param2 { get; }
}

public readonly struct Parameter<T1, T2, T3>
{
    public Parameter(T1 param1, T2 param2, T3 param3) => (Param1, Param2, Param3) = (param1, param2, param3);
    public T1 Param1 { get; }
    public T2 Param2 { get; }
    public T3 Param3 { get; }
}

public readonly struct Parameter<T1, T2, T3, T4>
{
    public Parameter(T1 param1, T2 param2, T3 param3, T4 param4) =>
        (Param1, Param2, Param3, Param4) = (param1, param2, param3, param4);

    public T1 Param1 { get; }
    public T2 Param2 { get; }
    public T3 Param3 { get; }

    public T4 Param4 { get; }
}

public readonly struct Parameter<T1>
{
    public Parameter(T1 param1) => Param1 = param1;
    public T1 Param1 { get; }
}