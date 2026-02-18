namespace WhiteLagoon.Application.Common.Interfaces;

public interface IUnitOfWork
{
    IVillaRepository VillaRepository { get; }
    IVillaNumberRepository VillaNumberRepository { get; }
    void Save();
}
