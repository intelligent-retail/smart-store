using System.Net.Http;
using System.Threading.Tasks;

namespace BoxManagementService.Models.Repositories.Http
{
    public class UserRepository
    {
        private readonly HttpClient _client;

        public UserRepository(HttpClient client)
        {
            this._client = client;
        }

        public async Task<(RepositoryResult result, UserInformation info)> AuthenticateUserAsync(string userId = "A1427BBA-25A8-4A23-926C-FE5C78BD8CC9")
        {
            return
                await Task.FromResult((
                    RepositoryResult.OK,
                    new UserInformation
                    {
                        UserId = userId,
                        UserName = "テストユーザー"
                    }));

            // 認証エラー時 "User is unauthorized."
        }
    }
}
