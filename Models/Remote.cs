namespace remoteWebMos.Models
{
	public class Remote
	{
        public int uid { get; set; }
        public int uidMachine { get; set; }
		public string section { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
		public bool isExpired { get; set; }
		public bool isSection { get; set; }
        public string IdRemote { get; set; }
		public string userRemote { get; set; }
		public string passRemote { get; set; }
		
	}
}
