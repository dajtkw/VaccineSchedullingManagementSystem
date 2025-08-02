using Microsoft.EntityFrameworkCore;
using QuanLyTiemChung.Web.Data;
using QuanLyTiemChung.Web.Interfaces;
using QuanLyTiemChung.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Ensure this is only here once

namespace QuanLyTiemChung.Web.Repositories
{
    public class VaccineRepository : IVaccineRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VaccineRepository> _logger; // Declare logger field

        public VaccineRepository(ApplicationDbContext context, ILogger<VaccineRepository> logger) // Inject logger
        {
            _context = context;
            _logger = logger; // Assign logger
        }

        public async Task AddAsync(Vaccine vaccine)
        {
            await _context.Vaccines.AddAsync(vaccine);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Vaccine>> GetAllAsync()
        {
            return await _context.Vaccines
                .Include(v => v.VaccineCategory)
                .Include(v => v.Doses)
                .ToListAsync();
        }

        public async Task<Vaccine?> GetByIdAsync(int id)
        {
            return await _context.Vaccines
                .Include(v => v.VaccineCategory)
                .Include(v => v.Doses)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task UpdateAsync(Vaccine vaccine)
        {
            _context.Vaccines.Update(vaccine);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var hasAppointments = await _context.Appointments.AnyAsync(a => a.VaccineId == id);
            if (hasAppointments)
            {
                return false;
            }

            var vaccine = await _context.Vaccines.FindAsync(id);
            if (vaccine != null)
            {
                _context.Vaccines.Remove(vaccine);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<Vaccine?> GetByIdWithDosesAsync(int id)
        {
            return await _context.Vaccines
                .Include(v => v.VaccineCategory)
                .Include(v => v.Doses.OrderBy(d => d.DoseNumber))
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task AddVaccineWithDosesAsync(Vaccine vaccine)
        {
            vaccine.CreatedAt = DateTime.Now;
            _context.Vaccines.Add(vaccine);
            await _context.SaveChangesAsync();

            if (vaccine.Doses != null && vaccine.Doses.Any())
            {
                foreach (var dose in vaccine.Doses)
                {
                    dose.VaccineId = vaccine.Id;
                    _context.VaccineDoses.Add(dose);
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateVaccineWithDosesAsync(Vaccine vaccine)
        {
            var existingVaccine = await _context.Vaccines
                .Include(v => v.Doses)
                .FirstOrDefaultAsync(v => v.Id == vaccine.Id);

            if (existingVaccine == null)
            {
                _logger.LogWarning("Vaccine with ID {VaccineId} not found for update.", vaccine.Id);
                return; // Or throw an exception
            }

            _logger.LogInformation("Updating vaccine {VaccineId}. Existing doses count: {ExistingDosesCount}, Incoming doses count: {IncomingDosesCount}", 
                vaccine.Id, existingVaccine.Doses.Count, vaccine.Doses?.Count ?? 0);

            // Update basic vaccine info
            _context.Entry(existingVaccine).CurrentValues.SetValues(vaccine);

            // Track changes for doses
            var currentDoseIds = existingVaccine.Doses.Select(d => d.Id).ToHashSet();
            var incomingDoseIds = vaccine.Doses?.Where(d => d.Id > 0).Select(d => d.Id).ToHashSet() ?? new HashSet<int>();

            // Remove doses that are no longer present
            foreach (var existingDose in existingVaccine.Doses.ToList())
            {
                if (!incomingDoseIds.Contains(existingDose.Id))
                {
                    _logger.LogInformation("Removing dose {DoseId} from vaccine {VaccineId}", existingDose.Id, vaccine.Id);
                    _context.VaccineDoses.Remove(existingDose);
                }
            }

            // Add or update incoming doses
            if (vaccine.Doses != null)
            {
                foreach (var incomingDose in vaccine.Doses)
                {
                    if (incomingDose.Id > 0)
                    {
                        // Update existing dose
                        var existingDose = existingVaccine.Doses.FirstOrDefault(d => d.Id == incomingDose.Id);
                        if (existingDose != null)
                        {
                            _context.Entry(existingDose).CurrentValues.SetValues(incomingDose);
                        }
                        else
                        {
                            _logger.LogWarning("Incoming dose {DoseId} for vaccine {VaccineId} not found in existing doses. Adding as new.", incomingDose.Id, vaccine.Id);
                            incomingDose.VaccineId = vaccine.Id;
                            _context.VaccineDoses.Add(incomingDose);
                        }
                    }
                    else
                    {
                        // Add new dose
                        _logger.LogInformation("Adding new dose for vaccine {VaccineId}", vaccine.Id);
                        incomingDose.VaccineId = vaccine.Id;
                        _context.VaccineDoses.Add(incomingDose);
                    }
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Changes saved for vaccine {VaccineId}", vaccine.Id);
        }

        public async Task<Vaccine?> GetVaccineDetailsWithAllIncludesAsync(int id)
        {
            return await _context.Vaccines
                .Include(v => v.VaccineCategory)
                .Include(v => v.Doses.OrderBy(d => d.DoseNumber))
                .Include(v => v.Appointments.OrderByDescending(a => a.ScheduledDateTime))
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(v => v.Id == id);
        }
    }
}
