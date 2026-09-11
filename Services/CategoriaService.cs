using Biblioteca.Models;
using Npgsql;

namespace Biblioteca.Services;

public class CategoriaService : ICategoriaService
{
    private readonly string _connectionString;

    public CategoriaService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public IEnumerable<Categoria> GetAll()
    {
        var list = new List<Categoria>();

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        const string sql = "SELECT id, nombre, descripcion FROM categorias ORDER BY id ASC";
        using var command = new NpgsqlCommand(sql, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new Categoria
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                Descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("descripcion"))
            });
        }

        return list;
    }

    public Categoria? GetById(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        const string sql = "SELECT id, nombre, descripcion FROM categorias WHERE id = @id";
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new Categoria
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                Descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("descripcion"))
            };
        }

        return null;
    }

    public void Create(Categoria categoria)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
            INSERT INTO categorias (nombre, descripcion)
            VALUES (@nombre, @descripcion)
            RETURNING id";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@nombre", categoria.Nombre);
        command.Parameters.AddWithValue("@descripcion", (object?)categoria.Descripcion ?? DBNull.Value);

        categoria.Id = Convert.ToInt32(command.ExecuteScalar());
    }

    public bool Update(int id, Categoria categoria)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        const string sql = @"
            UPDATE categorias
            SET nombre = @nombre,
                descripcion = @descripcion
            WHERE id = @id";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@nombre", categoria.Nombre);
        command.Parameters.AddWithValue("@descripcion", (object?)categoria.Descripcion ?? DBNull.Value);

        var rowsAffected = command.ExecuteNonQuery();
        return rowsAffected > 0;
    }

    public bool Delete(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        const string sql = "DELETE FROM categorias WHERE id = @id";
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        var rowsAffected = command.ExecuteNonQuery();
        return rowsAffected > 0;
    }
}
