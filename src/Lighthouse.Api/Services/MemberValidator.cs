using Lighthouse.Api.Models;

namespace Lighthouse.Api.Services;

internal static class MemberValidator
{
    internal static void Validate(Member member)
    {
        ArgumentNullException.ThrowIfNull(member);
        if (string.IsNullOrWhiteSpace(member.MemberId)) throw new ArgumentException("Member ID is required.", nameof(member));
        if (!member.IsActive) throw new InvalidOperationException("Member is inactive.");
    }
}
