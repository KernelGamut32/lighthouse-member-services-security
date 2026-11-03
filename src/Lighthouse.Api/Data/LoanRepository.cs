using System.Globalization;
using Lighthouse.Api.Models;
using Microsoft.Data.Sqlite;

namespace Lighthouse.Api.Data;

public sealed class LoanRepository(string connectionString)
{
    public Loan? Find(string loanId)
    {
        using SqliteConnection connection = new(connectionString);
        connection.Open();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT LoanId,MemberId,PrincipalCents,AnnualRate,InterestAsOfDate,DueDate,IsClosed FROM Loans WHERE LoanId=$id";
        command.Parameters.AddWithValue("$id", loanId);
        using SqliteDataReader reader = command.ExecuteReader();
        if (!reader.Read()) return null;
        return new Loan(reader.GetString(0), reader.GetString(1), reader.GetInt64(2) / 100m,
            decimal.Parse(reader.GetString(3), CultureInfo.InvariantCulture),
            DateOnly.ParseExact(reader.GetString(4), "yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateOnly.ParseExact(reader.GetString(5), "yyyy-MM-dd", CultureInfo.InvariantCulture), reader.GetBoolean(6));
    }
}
