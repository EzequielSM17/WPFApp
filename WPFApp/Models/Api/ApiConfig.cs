namespace Api
{
    public class ApiConfig
    {
        public string BaseUrl { get; set; } = "";
        public string PersistenceEndpoint { get; set; } = "";
        public string GamesEndpoint { get; set; } = "";
        public string AuthLoginEndpoint { get; set; } = "";
        public string AuthRegisterEndpoint { get; set; } = "";
    }
}
