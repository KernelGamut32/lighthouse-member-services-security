using System.Globalization;
using Lighthouse.Api.Models;
using Microsoft.Data.Sqlite;

namespace Lighthouse.Api.Services;

public sealed class MemberService(string connectionString)
{
    public Member? Find(string memberId)
    {
        using SqliteConnection connection = new(connectionString);
        connection.Open();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT MemberId,FullName,Segment,JoinedDate,IsActive FROM Members WHERE MemberId=$id";
        command.Parameters.AddWithValue("$id", memberId);
        using SqliteDataReader reader = command.ExecuteReader();
        return reader.Read() ? ReadMember(reader) : null;
    }

    public Member GetRequiredActiveMember(string memberId)
    {
        Member member = Find(memberId) ?? throw new KeyNotFoundException("Member not found.");
        MemberValidator.Validate(member);
        return member;
    }

    public IReadOnlyList<Member> SearchByName(string name)
    {
        using SqliteConnection connection = new(connectionString);
        connection.Open();
        using SqliteCommand command = connection.CreateCommand();
        // Query text is fixed; untrusted search text is passed as data.
        command.CommandText = "SELECT MemberId,FullName,Segment,JoinedDate,IsActive FROM Members WHERE FullName LIKE $name ORDER BY MemberId LIMIT 20";
        command.Parameters.AddWithValue("$name", "%" + name + "%");
        using SqliteDataReader reader = command.ExecuteReader();
        List<Member> members = [];
        while (reader.Read()) members.Add(ReadMember(reader));
        return members;
    }

    private static Member ReadMember(SqliteDataReader reader) => new(reader.GetString(0), reader.GetString(1),
        reader.GetString(2), DateOnly.ParseExact(reader.GetString(3), "yyyy-MM-dd", CultureInfo.InvariantCulture), reader.GetBoolean(4));
}
