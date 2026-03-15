namespace Dotori.Registry.Api.Dtos;

public sealed class CollaboratorDto
{
    public required string Username { get; init; }
    public required string Role { get; init; }
    public DateTime AddedAt { get; init; }
}

public sealed class AddCollaboratorRequestDto
{
    public required string Username { get; init; }
    public string Role { get; init; } = "collaborator";
}

public sealed class TransferOwnershipRequestDto
{
    public required string NewOwner { get; init; }
}
