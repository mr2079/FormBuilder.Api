using System.Data;
using DynamicTable.Api.Data;
using DynamicTable.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DynamicTable.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TableController(AppDbContext context) : ControllerBase
{
    [HttpPost("[action]", Name = "CreateTable")]
    public async Task<IActionResult> Create([FromBody] CreateTableDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.JsonStringModel)
            || string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest();

        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(dto.JsonStringModel);

        var columns = dictionary?.Keys.ToList();

        var columnsSql = "id INT IDENTITY(1,1),";

        columns?.ForEach(c => columnsSql += $"[{c}] NVARCHAR(MAX),");

        columnsSql = columnsSql.Substring(0, columnsSql.Length - 1);

        try
        {
            int result;

            await using (var connection = new SqlConnection(DbConstants.ConnectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand($"CREATE TABLE [{dto.Name}] ({columnsSql});", connection);
                result = command.ExecuteNonQuery();
                await connection.CloseAsync();
            }

            if (result == 0) return StatusCode(StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }

        return Ok();
    }

    [HttpPost("[action]", Name = "InsertToTable")]
    public async Task<IActionResult> Insert([FromBody] InsertTableDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.JsonStringModel))
            return BadRequest();

        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(dto.JsonStringModel);

        var columns = dictionary?.Keys.ToList();
        var values = dictionary?.Values.ToList();

        var columnsSql = string.Empty;
        var valuesSql = string.Empty;

        columns?.ForEach(c => columnsSql += $"[{c}],");

        values?.ForEach(v => valuesSql += $"N'{v}',");

        columnsSql = columnsSql.Substring(0, columnsSql.Length - 1);

        valuesSql = valuesSql.Substring(0, valuesSql.Length - 1);

        try
        {
            int result;

            await using (var connection = new SqlConnection(DbConstants.ConnectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand($"INSERT INTO [{dto.Name}] ({columnsSql}) VALUES ({valuesSql});", connection);
                result = command.ExecuteNonQuery();
                await connection.CloseAsync();
            }

            if (result == 0) return StatusCode(StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }

        return Ok();
    }

    [HttpGet("{tableName}", Name = "GetTableSchema")]
    public async Task<IActionResult> GetTable(string tableName)
    {
        var dictionary = new Dictionary<string, string?>();

        var adapter = new SqlDataAdapter($"SELECT * FROM {tableName} WHERE id = 1;", DbConstants.ConnectionString);
        adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
        var table = new DataTable();
        adapter.Fill(table);

        await using var reader = new DataTableReader(table);
        var schema = reader.GetSchemaTable();

        foreach (DataRow row in schema.Rows)
        {
            if (Equals(row["ColumnName"].ToString(),"id")) continue;

            dictionary.Add(row["ColumnName"].ToString()!, string.Empty);
        }

        var jsonStringModel = JsonConvert.SerializeObject(dictionary);

        return Ok(jsonStringModel);
    }

    [HttpGet("{tableName}/{dataId}", Name = "GetDataFromTable")]
    public async Task<IActionResult> GetData(string tableName, int dataId)
    {
        var dictionary = new Dictionary<string, string?>();

        var adapter = new SqlDataAdapter($"SELECT * FROM {tableName} WHERE id = {dataId};", DbConstants.ConnectionString);
        adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
        var table = new DataTable();
        adapter.Fill(table);

        await using var reader = new DataTableReader(table);
        var schema = reader.GetSchemaTable();

        // TODO: get row value from reader & add to dictionary
        //var index = 1;
        //foreach (DataRow row in schema.Rows)
        //{
        //    if (Equals(row["ColumnName"].ToString(), "id")) continue;
        //    dictionary.Add(row["ColumnName"].ToString()!, reader.GetString(index));
        //    index++;
        //}

        var jsonStringModel = JsonConvert.SerializeObject(dictionary);

        return Ok(jsonStringModel);
    }
}