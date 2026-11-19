using System.Data;

namespace Qa.ApiFramework.Db;

/// <summary>
/// ADO.NET helper for backend validation: after an API call, run SQL to confirm the data was
/// stored correctly. Works with any ADO.NET connection (SQL Server, MySQL, SQLite ...).
/// Values are always bound as parameters (@p0, @p1 ...) and never concatenated into the SQL.
/// </summary>
public sealed class DbValidator
{
    private readonly IDbConnection _connection;

    public DbValidator(IDbConnection connection)
    {
        _connection = connection;
    }

    /// <summary>Run a SELECT and return rows as column-name (lower case) to value maps.</summary>
    public List<Dictionary<string, object?>> Query(string sql, params object[] args)
    {
        using var command = BuildCommand(sql, args);
        using var reader = command.ExecuteReader();

        var rows = new List<Dictionary<string, object?>>();
        while (reader.Read())
        {
            var row = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i).ToLowerInvariant()] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            rows.Add(row);
        }
        return rows;
    }

    /// <summary>Return the single matching row, or fail if there are none or more than one.</summary>
    public Dictionary<string, object?> FindOne(string sql, params object[] args)
    {
        var rows = Query(sql, args);
        if (rows.Count != 1)
        {
            throw new InvalidOperationException($"Expected exactly 1 row but found {rows.Count} for: {sql}");
        }
        return rows[0];
    }

    /// <summary>Insert, update or delete. Returns the number of affected rows.</summary>
    public int Execute(string sql, params object[] args)
    {
        using var command = BuildCommand(sql, args);
        return command.ExecuteNonQuery();
    }

    private IDbCommand BuildCommand(string sql, object[] args)
    {
        var command = _connection.CreateCommand();
        command.CommandText = sql;
        for (var i = 0; i < args.Length; i++)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = $"@p{i}";
            parameter.Value = args[i];
            command.Parameters.Add(parameter);
        }
        return command;
    }
}
