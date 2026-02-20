using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IVillaRepository VillaRepository { get; private set; }

    public IVillaNumberRepository VillaNumberRepository { get; private set; }

    public IAmenityRepository AmenityRepository { get; private set; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        VillaRepository = new VillaRepository(_context);
        VillaNumberRepository = new VillaNumberRepository(_context);
        AmenityRepository = new AmenityRepository(_context);
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}
