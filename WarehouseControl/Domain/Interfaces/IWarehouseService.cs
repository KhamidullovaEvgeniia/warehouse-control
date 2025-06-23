using Domain.Models;

namespace Domain.Interfaces;

public interface IWarehouseService
{
    /// <summary>
    /// Группирует паллеты по сроку годности, сортирует группы по возрастанию срока,
    /// а внутри групп — по возрастанию веса.
    /// </summary>
    Task<IEnumerable<IGrouping<DateTime, Pallet>>> GroupAndSortPalletsByExpiry(CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает указанное колчиество паллет, содержащие коробки с максимальными сроками годности.
    /// Сортирует их по возрастанию объема.
    /// </summary>
    Task<List<Pallet>> GetTopPalletsWithMaxBoxExpiry(int countTopPallet, CancellationToken cancellationToken);
}