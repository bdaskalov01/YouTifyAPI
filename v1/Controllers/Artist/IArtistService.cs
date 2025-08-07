using WebAPIProgram.Models.Database.Tables;

namespace WebAPIProgram.Services;

public interface IArtistService
{
    Task<IEnumerable<IdentityUserExtended>> GetByName(string name); }