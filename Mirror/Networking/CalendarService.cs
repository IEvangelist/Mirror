using Mirror.Calendar;
using Mirror.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static Mirror.Calendar.Calendar;


namespace Mirror.Networking
{
    public interface ICalendarService
    {
        Task<IEnumerable<Calendar.Calendar>> GetCalendarsAsync();
    }

    public class CalendarService : ICalendarService
    {
        async Task<IEnumerable<Calendar.Calendar>> ICalendarService.GetCalendarsAsync()
        {
            var settings = Settings.Instance;

            var getCalendarTasks =
                settings
                    .Calendars
                   ?.Select(
                       cal =>
                       GetCalendarAsync(cal.Url,
                                        cal.IsUsingCredentials
                                            ? () => new HttpClient(new HttpClientHandler { Credentials = new NetworkCredential(cal.Username, cal.Password) })
                                            : null as Func<HttpClient>));

            return await Task.WhenAll(getCalendarTasks);
        }

        static async Task<Calendar.Calendar> GetCalendarAsync(string url, Func<HttpClient> getClient = null)
        {
            try
            {
                var response = await ApiClient.GetRawAsync(url, getClient);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var calendar = Parser.FromString(response);
                    return calendar;
                }
            }
            catch (Exception ex) when (DebugHelper.IsHandled<CalendarService>(ex))
            {
                // Do nothing...
            }

            return await Task.FromResult(Empty);
        }
    }
}