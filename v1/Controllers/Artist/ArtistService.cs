using PSQLModels.Tables;
using WebAPIProgram.v1.Controllers.Auth;

namespace WebAPIProgram.v1.Controllers.Artist;
public class ArtistService: IArtistService
{
    private readonly IAuthRepository artistRepository;

    public ArtistService(IAuthRepository repository)
    {
        artistRepository = repository;
    }

    public async Task<IEnumerable<IdentityUserExtended>> GetByName(string name)
    {
        // TODO: Get by name from User repository
        throw new NotImplementedException();

    }
    
}