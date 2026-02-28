namespace WhiteLagoon.Application.Common.Interfaces;

public interface IUnitOfWork
{
    IVillaRepository VillaRepository { get; }
    IVillaNumberRepository VillaNumberRepository { get; }
    IAmenityRepository AmenityRepository { get; }
    IBookingRepository BookingRepository { get; }
    IApplicationUserRepository ApplicationUserRepository { get; }
    void Save();
}
