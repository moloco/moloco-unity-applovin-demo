#if UNITY_IOS || UNITY_IPHONE

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Moloco.Scripts.Editor
{
    public class PodfileEditor
    {
        [PostProcessBuild(45)] // Add to the Podfile after it's generated (40) but before "pod install" (50)
        public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.iOS)
            {
                return;
            }

            string podfilePath = Path.Combine(pathToBuiltProject, "Podfile");

            if (File.Exists(podfilePath))
            {
                var codeSigningStyle = PlayerSettings.iOS.appleEnableAutomaticSigning ? "Automatic" : "Manual";
                var teamId = PlayerSettings.iOS.appleDeveloperTeamID;
                var provisioningProfileId = PlayerSettings.iOS.iOSManualProvisioningProfileID;
                var provisioningProfileType = PlayerSettings.iOS.iOSManualProvisioningProfileType;
                
                string[] molocoTargets =
                {
                    "MolocoSDKiOS-MolocoSDK", 
                    "MolocoCustomAdapter-MolocoCustomAdapter", 
                    "MolocoCustomAdapterAppLovin-MolocoCustomAdapterAppLovin",
                    "MolocoCustomAdapterIronSource-MolocoCustomAdapterIronSource"
                };
                var molocoTargetsString = string.Join(", ", molocoTargets.Select(element => $"'{element}'"));

                using var sw = File.AppendText(podfilePath);
                sw.WriteLine("\n\n\npost_install do |installer|");
                sw.WriteLine("  installer.pods_project.targets.each do |target|");
                sw.WriteLine("    target.build_configurations.each do |config|");
                sw.WriteLine("      if [" + molocoTargetsString + "].include? target.name");
                sw.WriteLine("        config.build_settings['CODE_SIGN_STYLE'] = '" + codeSigningStyle + "'");
                sw.WriteLine("        config.build_settings['DEVELOPMENT_TEAM'] = '" + teamId + "'");
                if (!PlayerSettings.iOS.appleEnableAutomaticSigning)
                {
                    sw.WriteLine("        config.build_settings['PROVISIONING_PROFILE_APP'] = '" + provisioningProfileId + "\'");
                }
                sw.WriteLine("      end");
                sw.WriteLine("    end");
                sw.WriteLine("  end");
                sw.WriteLine("end");
            }
            else
            {
                Debug.LogWarning("Podfile not found in the Xcode project.");
            }
        }
    }
}

#endif