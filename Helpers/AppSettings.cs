namespace Users.Helpers {

    public class AppSettings {

        public string Audience { get; set; }
        public string ExpireTime { get; set; }
        public string Secret { get; set; }
        public string Site { get; set; }

        public string ClientId { get; set; }
        public string RefreshToken { get; set; }
        public string GrantType { get; set; }

        public string SendGridKey { get; set; }
        public string SendGridUser { get; set; }

    }

}