namespace expenses_tracker_pb.Server.DTOs;

public record WalletDto(
    long Id,
    string Name,
    long IconId,
    decimal AccountBalance,
    string UserId
);