﻿using System;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Application;
using JetBrains.DataFlow;
using JetBrains.UI.Updates;
using JetBrains.VSIntegration.Updates;

namespace TestCop.Plugin.Helper
{
    /* R# cache is %LOCALAPPDATA%\JetBrains\ReSharper\v7.1\UpdatesManager.xml */
    [ShellComponent]
    public class UpdatesNotifier
    {
        public UpdatesNotifier(Lifetime lifetime, UpdatesManager updatesManager)
        {
            var uri = new Uri("https://testcop.svn.codeplex.com/svn/Project/Src/TestCop.Plugin/updates.xslt");
            //uri = new Uri("file://d:/CodePlexLive/Project/Src/TestCop.Plugin/updates.xslt");
            
            var category = updatesManager.Categories.AddOrActivate("TestCopSupport", uri);
            
            category.CustomizeLocalEnvironmentInfo.Advise(lifetime, args =>
                {
                    // We can customise the local environment info that the xslt will be applied to                    
                    // It should be an instance of UpdateLocalEnvironmentInfo, bail out early if it's                    
                    // not. The only reason it wouldn't be is if someone has got hold of the "TestCopSupport"                    
                    // category and subscribed to the CustomizeLocalEnvironmentInfo signal. Unlikely.                    
                    if (!(args.Out is UpdateLocalEnvironmentInfoVs)) return;
                    // Set the data the xslt will be applied against. Pass in the current environment,  
                    // in case we ever need it, but really, we only care about the current version     
                    args.Out = new PluginLocalEnvironmentInfo
                                    {
                                        LocalEnvironment = args.Out as UpdateLocalEnvironmentInfoVs,
                                        PluginVersion = new UpdateLocalEnvironmentInfo.VersionSubInfo(GetThisVersion())
                                    };
                });
            RemoveStaleUpdateNotification(category);
        }

        private static Version GetThisVersion()
        {
            var assembly = typeof (UpdatesNotifier).Assembly;
            return assembly.GetName().Version;
        }

        private static void RemoveStaleUpdateNotification(UpdatesCategory category)
        {
            // ReSharper downloads and evaluates the xslt on a regular basis (every 24 hours),   
            // but doesn't re-evaluate it after an install (it doesn't know when something is    
            // installed!) so if there's a reminder to download this or an older version, remove it    
            var thisVersion = GetThisVersion();
            var updateInfo =
                category.UpdateInfos.FirstOrDefault(
                    container => new Version(container.Data.ProductVersion) <= thisVersion);
            if (updateInfo != null) category.UpdateInfos.Remove(updateInfo);
        }
    }

    [XmlType]
    [XmlRoot("PluginLocalInfo")]
    [Serializable]
    public class PluginLocalEnvironmentInfo
    {
        [XmlElement] 
        public UpdateLocalEnvironmentInfoVs LocalEnvironment = new UpdateLocalEnvironmentInfoVs();

        [XmlElement] 
        public UpdateLocalEnvironmentInfo.VersionSubInfo PluginVersion = new UpdateLocalEnvironmentInfo.VersionSubInfo();
    }
}
