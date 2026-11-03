using Lighthouse.Api.Models;
using Microsoft.Data.Sqlite;

namespace Lighthouse.Api.Services;

public sealed class AccountService(string connectionString)
{
    public IReadOnlyList<Account> ForMember(string memberId)
    {
        using SqliteConnection connection = new(connectionString);
        connection.Open();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT AccountId,MemberId,AccountType,BalanceCents,IsOpen FROM Accounts WHERE MemberId=$id ORDER BY AccountId";
        command.Parameters.AddWithValue("$id", memberId);
        using SqliteDataReader reader = command.ExecuteReader();
        List<Account> accounts = [];
        while (reader.Read()) accounts.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetInt64(3) / 100m, reader.GetBoolean(4)));
        return accounts;
    }
}
