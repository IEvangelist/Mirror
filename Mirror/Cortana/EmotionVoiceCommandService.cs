using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Storage;


namespace Mirror.Cortana
{
    public sealed class EmotionVoiceCommandService : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        VoiceCommandServiceConnection _voiceServiceConnection;
        VoiceCommand _voiceCommand;
        

        async void IBackgroundTask.Run(IBackgroundTaskInstance taskInstance)
        {
            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            if (triggerDetails?.Name == nameof(EmotionVoiceCommandService))
            {
                _deferral = taskInstance.GetDeferral();
                taskInstance.Canceled += OnTaskInstanceCanceled;

                if (await InitializeAsync(triggerDetails) != true)
                {
                    return;
                }

                // These command phrases are coded in the VoiceCommands.xml file.
                switch (_voiceCommand.CommandName)
                {
                    case "lookAtMe": await ChangeLightStateAsync(); break;
                    case "howDoILook": await SelectColorAsync(); break;
                    //case "changeLightStateByName": await ChangeSpecificLightStateAsync(); break;
                    default:
                        await _voiceServiceConnection.RequestAppLaunchAsync(
                   CreateCortanaResponse("Launching HueLightController")); break;
                }

                // keep alive for 1 second to ensure all HTTP requests sent.
                await Task.Delay(1000);

                _deferral.Complete();
            }
        }

        void OnTaskInstanceCanceled(IBackgroundTaskInstance sender, 
                                    BackgroundTaskCancellationReason reason)
        {
            _deferral?.Complete();
        }

        ///// <summary>
        ///// Handles the command to change the state of a specific light.
        ///// </summary>
        //private async Task ChangeSpecificLightStateAsync()
        //{
        //    string name = _voiceCommand.Properties["name"][0];
        //    string state = _voiceCommand.Properties["state"][0];
        //    Light light = _lights.FirstOrDefault(x =>
        //        x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        //    if (null != light)
        //    {
        //        await ExecutePhrase(light, state);
        //        var response = CreateCortanaResponse($"Turned {name} {state}.");
        //        await _voiceServiceConnection.ReportSuccessAsync(response);
        //    }
        //}

        /// <summary>
        /// Handles the command to change the state of all the lights.
        /// </summary>
        private async Task ChangeLightStateAsync()
        {
            string phrase = _voiceCommand.Properties["state"][0];
            //foreach (Light light in _lights)
            //{
            //    await ExecutePhrase(light, phrase);
            //}
            var response = CreateCortanaResponse($"Turned your lights {phrase.ToLower()}.");
            await _voiceServiceConnection.ReportSuccessAsync(response);
        }

        /// <summary>
        /// Handles an interaction with Cortana where the user selects 
        /// from randomly chosen colors to change the lights to.
        /// </summary>
        private async Task SelectColorAsync()
        {
            var userPrompt = new VoiceCommandUserMessage();
            userPrompt.DisplayMessage = userPrompt.SpokenMessage =
                "Here's some colors you can choose from.";

            var userReprompt = new VoiceCommandUserMessage();
            userReprompt.DisplayMessage = userReprompt.SpokenMessage =
                "Sorry, didn't catch that. What color would you like to use?";

            //// Randomly select 6 colors for Cortana to show
            //var random = new Random();
            //var colorContentTiles = _colors.Select(x => new VoiceCommandContentTile
            //{
            //    ContentTileType = VoiceCommandContentTileType.TitleOnly,
            //    Title = x.Value.Name
            //}).OrderBy(x => random.Next()).Take(6);

            //var colorResponse = VoiceCommandResponse.CreateResponseForPrompt(
            //    userPrompt, userReprompt, colorContentTiles);
            //var disambiguationResult = await
            //    _voiceServiceConnection.RequestDisambiguationAsync(colorResponse);
            //if (null != disambiguationResult)
            //{
            //    var selectedColor = disambiguationResult.SelectedItem.Title;
            //    foreach (Light light in _lights)
            //    {
            //        await ExecutePhrase(light, selectedColor);
            //    }
            //    var response = CreateCortanaResponse($"Turned your lights {selectedColor}.");
            //    await _voiceServiceConnection.ReportSuccessAsync(response);
            //}
        }

        /// <summary>
        /// Converts a phrase to a light command and executes it.
        /// </summary>
        private async Task ExecutePhrase(string phrase)
        {
            //if (phrase == "On")
            //{
            //    light.State.On = true;
            //}
            //else if (phrase == "Off")
            //{
            //    light.State.On = false;
            //}
            //else if (_colors.ContainsKey(phrase))
            //{
            //    light.State.Hue = _colors[phrase].H;
            //    light.State.Saturation = _colors[phrase].S;
            //    light.State.Brightness = _colors[phrase].B;
            //}
            //else
            {
                var response = CreateCortanaResponse("Launching HueLightController");
                await _voiceServiceConnection.RequestAppLaunchAsync(response);
            }
        }

        /// <summary>
        /// Helper method for initalizing the voice service, bridge, and lights. Returns if successful. 
        /// </summary>
        private async Task<bool> InitializeAsync(AppServiceTriggerDetails triggerDetails)
        {
            _voiceServiceConnection =
                VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);
            _voiceServiceConnection.VoiceCommandCompleted += (s, e) => _deferral.Complete();

            _voiceCommand = await _voiceServiceConnection.GetVoiceCommandAsync();

            var localStorage = ApplicationData.Current.LocalSettings.Values;
            //_bridge = new Bridge(
            //    localStorage["bridgeIp"].ToString(), localStorage["userId"].ToString());
            //try
            //{
            //    _lights = await _bridge.GetLightsAsync();
            //}
            //catch (Exception)
            //{
            //    var response = CreateCortanaResponse("Sorry, I couldn't connect to your bridge.");
            //    await _voiceServiceConnection.ReportFailureAsync(response);
            //    return false;
            //}
            //if (!_lights.Any())
            //{
            //    var response = CreateCortanaResponse("Sorry, I couldn't find any lights.");
            //    await _voiceServiceConnection.ReportFailureAsync(response);
            //    return false;
            //}
            return true;
        }

        /// <summary>
        /// Helper method for creating a message for Cortana to speak and write to the user.
        /// </summary>
        private VoiceCommandResponse CreateCortanaResponse(string message)
        {
            var userMessage = new VoiceCommandUserMessage()
            {
                DisplayMessage = message,
                SpokenMessage = message
            };
            var response = VoiceCommandResponse.CreateResponse(userMessage);
            return response;
        }
    }
}