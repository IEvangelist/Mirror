# Mirror
Magic mirror application. **UWP** app, written with **C# .NET**. For presentation @ <a href='www.mkedotnet.com' target='_blank'>MKE DOT NET</a> 2016.
##Configuration
If you fork this repository, you will need to configure the application in order for it to function as you would expect. The `Settings.cs` class and corresponding `Configuration.resw` file contain the various configuration that the application relies on to execute.

 - `AzureEmotionApiKey` visit [Microsoft Cognitive Services, Emotion API](https://www.microsoft.com/cognitive-services/en-us/subscriptions?productId=/products/5639d8afca73072154c1ce88) to get register for **free** and get your own subscription key.
 - `OpenWeatherApiKey` visit [Open Weather API](https://home.openweathermap.org/users/sign_up) to sign up for **free** and get your subscription key.
 - `WeatherUom` desired UOM, `imperial` or `metric`.
 - `City` the city in which to query weather for, i.e.; for me this is `Pewaukee` since that is where I live.
 - `Calendars` this is a complex object that is stored as **JSON** and serialized / deserialized as a `List<CalendarConfig>` (where credentials are optional). The only requirement is that the _URL_ is an accessible endpoint that returns valid `iCal` (`*.ics`) formatting -- http://icalendar.org/. 

###Note
To ensure your settings and API keys remain safe and secure, execute the following command to remove the local changes you make to the configuration from `git` ever watching eye.
 ```
 git update-index --assume-unchanged "[file path]\Mirror\Configuration.resw"
 ```
###Details

I took the time to [blog](https://ievangelist.github.io/blog/building-a-magic-mirror/) about the entire process, enjoy!
