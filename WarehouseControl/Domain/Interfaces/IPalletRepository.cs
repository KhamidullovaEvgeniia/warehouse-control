using Domain.Models;

namespace Domain.Interfaces;

public interface IPalletRepository
{
    Task<List<Pallet?>> GetAllAsync(CancellationToken cancellationToken);
}