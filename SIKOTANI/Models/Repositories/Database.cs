using Npgsql;

namespace SIKOTANI.Models.Repositories;

public static class Database
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=sikotani;" +
        "Username=postgres;Password=GeraldSangat33_";

    private static readonly NpgsqlDataSource DataSource =
        NpgsqlDataSource.Create(ConnectionString);

    public static NpgsqlConnection BukaKoneksi()
    {
        return DataSource.OpenConnection();
    }
}