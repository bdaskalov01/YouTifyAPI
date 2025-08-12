using PSQLModels.Tables;

namespace WebAPIProgram.v1.Controllers.Artist;
public interface IArtistService
{
    Task<IEnumerable<IdentityUserExtended>> GetByName(string name); }