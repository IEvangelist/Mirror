using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Storage;

namespace Mirror.Cortana
{
    public interface IVoiceService
    {
        Task IntializeCortanaAsync();
    }

    public class VoiceService : IVoiceService
    {
        async Task IVoiceService.IntializeCortanaAsync()
        {
            // You can't write to application files by default, so we need to create a 
            // secondary VCD file to dynamically write Cortana commands to.
            //var dynamicFile =
            //    await ApplicationData.Current
            //                         .RoamingFolder
            //                         .CreateFileAsync("VoiceCommands.xml", CreationCollisionOption.ReplaceExisting);

            // Load the base file and parse the PhraseList we want from it.
            var baseFile = 
                await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/VoiceCommands.xml"));

            //var xml = XDocument.Load(baseFile.Path);
            //var state = 
            //    xml.Descendants()
            //       .First(x => x.Name.LocalName == "PhraseList" &&
            //              null != x.Attribute("Label") && x.Attribute("Label").Value == "state");

            // A ColorMapping is a RGB and HSV compliant representation a system color.
            // ColorMapping.CreateAll() returns a ColorMapping for all system colors available to UWP apps.
            // For each ColorMapping, add it to the list of phrases Cortana knows.
            //foreach (HsbColor color in HsbColor.CreateAll())
            //{
            //    state.Add(new XElement("Item", color.Name));
            //}

            // Add the light names.
            //XElement names = 
            //    xml.Descendants()
            //       .First(x => x.Name.LocalName == "PhraseList" &&
            //              null != x.Attribute("Label") && x.Attribute("Label").Value == "name");

            //foreach (Light light in _lights)
            //{
            //    names.Add(new XElement("Item", light.Name));
            //}

            // Save the file, and then load so Cortana recognizes it.
            //using (var stream = await dynamicFile.OpenStreamForWriteAsync())
            //{
            //    xml.Save(stream);
            //}

            try
            {
                await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(baseFile);
            }
            catch (FileNotFoundException)
            {
                // Do nothing. This is a workaround for a spurious FileNotFoundException that 
                // is thrown even though dynamicFile exists on disk. 
            }
        }
    }
}