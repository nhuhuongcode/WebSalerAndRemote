namespace remoteWebMos.Models
{
    public class AccountModel
    {
        public int Id { get; set; }
        public DateTime Expired { get; set; }
        public int Attempt { get; set; }
        public string login_prefix { get; set; }
        public string ExamLimits { get; set; }
        public string login_key { get; set; }

    }
}
