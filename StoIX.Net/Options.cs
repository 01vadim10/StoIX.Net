using CommandLine;

namespace StoIX.Net
{
	public class Options
	{
		[Option('s', "server", Required = false, HelpText = "Enter a server name")]
		public string Server { get; set; }
		[Option('d', "database", Required = false, DefaultValue = "Galaktika.HCM.DemoRF_new", HelpText = "Enter a database name")]
		public string Database { get; set; }
		[Option('u', "username", Required = false, HelpText = "Enter a username")]
		public string Username { get; set; }
		[Option('p', "password", Required = false, HelpText = "Enter a password")]
		public string Password { get; set; }
		[Option("datasource", Required = false, DefaultValue = "(local)", HelpText = "Enter a password")]
		public string DataSource { get; set; }
		[Option('f', "filename", Required = true, HelpText = "Enter a filename")]
		public string Filename { get; set; }
	}
}
