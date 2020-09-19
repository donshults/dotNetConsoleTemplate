using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace dotNetConsole.Services
{
    public class Utilities
    {
        public static IConfigurationRoot LoadAppSettings(ILogger log)
        {
            log.LogInformation("Entering LoadAppSettings");
            var appConfig = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();
            log.LogInformation("Build appConfig");
            log.LogInformation("Exiting App Config");
            return appConfig;
        }

        /// <summary>
        /// Convert SharePoint/Graph DateTimeOffset to DateTime
        /// This is a UTC Value..
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ConvertFromDateTimeOffset(DateTimeOffset dateTime)
        {
            if (dateTime.Offset.Equals(TimeSpan.Zero))
                return dateTime.UtcDateTime;
            else if (dateTime.Offset.Equals(TimeZoneInfo.Local.GetUtcOffset(dateTime.DateTime)))
                return DateTime.SpecifyKind(dateTime.DateTime, DateTimeKind.Local);
            else
                return dateTime.DateTime;
        }


    }
}
