namespace BoxManagementService.Models.Repositories
{
    public class RepositoryResult
    {
        public enum ResultStatus
        {
            Success,
            NotFound,
            Conflict,
            InvalidParameter,
        }

        public static readonly RepositoryResult OK = new RepositoryResult(ResultStatus.Success, null);

        public static RepositoryResult CreateNotFound(string errorMessage) => new RepositoryResult(ResultStatus.NotFound, errorMessage);

        public static RepositoryResult CreateConflict(string errorMessage) => new RepositoryResult(ResultStatus.Conflict, errorMessage);

        public static RepositoryResult CreateInvalidParameter(string errorMessage) => new RepositoryResult(ResultStatus.InvalidParameter, errorMessage);

        public bool IsSuccess => this.Status == ResultStatus.Success;

        public ResultStatus Status { get; protected set; }

        public string ErrorMessage { get; protected set; }

        protected RepositoryResult()
        {
        }

        protected RepositoryResult(ResultStatus status, string errorMessage)
        {
            this.Status = status;
            this.ErrorMessage = errorMessage;
        }
    }
}
