using System;
using System.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InfoAPI : Controller
    {
        public static string ReadSetting(string key)
        {
            var appSettings = ConfigurationManager.AppSettings;
            string result = appSettings[key] ?? "";
            return result;
        }
        public IActionResult Get()
        {
            string name = ReadSetting("name");
            if (string.IsNullOrEmpty(name))
                name = "Grzegorz";
            string webService = ReadSetting("webService");
            if (string.IsNullOrEmpty(webService))
                webService = "gwenieruchomosci";
            string connectionStringValue = ReadSetting("connectionString");
            if (string.IsNullOrEmpty(connectionStringValue))
                connectionStringValue = @"Data Source=DESKTOP-GOB4QRS\SQLEXPRESS;Initial Catalog=PropertyFinder;Integrated Security=True";
            int index;
            bool ret = Int32.TryParse(ReadSetting("index"), out index);
            if (ret == false)
                index = 133000;
            var info = new
            {
                connectionString = connectionStringValue,
                integrationName = webService,
                studentName = name,
                studentIndex = index
            };

            return Ok(info);
        }
    }
}
