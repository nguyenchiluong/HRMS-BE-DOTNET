using EmployeeApi.Data;
using EmployeeApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly AppDbContext _context;

    public AttendanceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceRecord?> GetTodayAttendanceAsync(long employeeId)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.AttendanceRecords
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);
    }

    public async Task<AttendanceRecord> CheckInAsync(AttendanceRecord record)
    {
        _context.AttendanceRecords.Add(record);
        await _context.SaveChangesAsync();
        return record;
    }

    public async Task<AttendanceRecord> CheckOutAsync(AttendanceRecord record)
    {
        _context.AttendanceRecords.Update(record);
        await _context.SaveChangesAsync();
        return record;
    }

    public async Task<List<AttendanceRecord>> GetAttendanceHistoryAsync(
        long? employeeId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int page = 1,
        int limit = 20)
    {
        var query = _context.AttendanceRecords
            .Include(a => a.Employee)
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(a => a.Date >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(a => a.Date <= dateTo.Value);
        }

        return await query
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.CheckInTime)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> GetAttendanceCountAsync(
        long? employeeId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var query = _context.AttendanceRecords.AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(a => a.Date >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(a => a.Date <= dateTo.Value);
        }

        return await query.CountAsync();
    }

    public async Task<AttendanceRecord?> GetAttendanceByIdAsync(long id)
    {
        return await _context.AttendanceRecords
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
}
