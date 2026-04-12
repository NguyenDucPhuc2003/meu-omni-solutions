using MeuOmni.Modules.SalesChannel.Domain.Shifts.Entities;

namespace MeuOmni.Modules.SalesChannel.Domain.Shifts.Repositories;

public interface IShiftRepository
{
    Task<Shift?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<Shift?> GetOpenShiftByCashierAsync(Guid cashierId, CancellationToken cancellationToken = default);
    
    Task<List<Shift>> GetShiftsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    Task AddAsync(Shift shift, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(Shift shift, CancellationToken cancellationToken = default);
}
