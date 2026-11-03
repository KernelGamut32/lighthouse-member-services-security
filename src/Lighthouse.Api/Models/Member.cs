namespace Lighthouse.Api.Models;

public sealed record Member(string MemberId, string FullName, string Segment, DateOnly JoinedDate, bool IsActive);
