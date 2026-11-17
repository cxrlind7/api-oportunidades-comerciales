namespace Oportunidades.Api.Auth;

public static class Roles
{
    public const string Consulta = "Consulta";
    public const string Editor = "Editor";

    public static readonly IReadOnlySet<string> Todos = new HashSet<string> { Consulta, Editor };
}

public static class Politicas
{
    // GET: Consulta o Editor
    public const string Lectura = "Lectura";

    // POST y PATCH: solo Editor
    public const string Escritura = "Escritura";
}
