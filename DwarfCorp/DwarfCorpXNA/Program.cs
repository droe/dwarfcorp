// DwarfGame.cs
// 
//  Modified MIT License (MIT)
//  
//  Copyright (c) 2015 Completely Fair Games Ltd.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// The following content pieces are considered PROPRIETARY and may not be used
// in any derivative works, commercial or non commercial, without explicit 
// written permission from Completely Fair Games:
// 
// * Images (sprites, textures, etc.)
// * 3D Models
// * Sound Effects
// * Music
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Threading;
using ContentGenerator;
using DwarfCorp.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DwarfCorpCore
{
    
}

#if XNA_BUILD

namespace DwarfCorp
{

#if WINDOWS || XBOX
    internal static class Program
    {
        public static string Version = "18.09.23_XNA";
        public static string[] CompatibleVersions = { "18.09.23_XNA", "18.09.23_FNA", "18.09.16_XNA", "18.09.16_FNA", "18.09.03_XNA", "18.09.03_FNA"};
        public static string Commit = "UNKNOWN";
        public static char DirChar = Path.DirectorySeparatorChar;
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            try
            {
               var cwd = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                Directory.SetCurrentDirectory(new Uri(cwd).LocalPath);
                using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("DwarfCorp.version.txt"))
                using (StreamReader reader = new StreamReader(stream))
                    Commit = reader.ReadToEnd();

                Commit = Commit.Trim();
            }
            catch (Exception) { }

            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

#if !DEBUG
            try
#endif
            {
                fbDeprofiler.DeProfiler.Load();
                using (DwarfGame game = new DwarfGame())
                {
                    game.Run();
                }

                while (AssetManagement.Steam.Steam.HasTransaction(a => true))
                    AssetManagement.Steam.Steam.Update();

                SignalShutdown();
            }
#if !DEBUG
            catch (Exception exception)
            {
                WriteExceptionLog(exception);
                string report = "This was automatically reported to the devs to help us debug!";
                if (!GameSettings.Default.AllowReporting)
                {
                    report = "You have opted out of automatic crash reporting.";
                }
                System.Windows.Forms.MessageBox.Show(String.Format("An unhandled exception occurred in DwarfCorp. {1} \n {0}", exception.ToString(), report), "ERROR");
            }
#endif
        }

        public static void WriteExceptionLog(Exception exception)
        {
            SignalShutdown();
            DirectoryInfo worldDirectory = Directory.CreateDirectory(DwarfGame.GetGameDirectory() + Path.DirectorySeparatorChar + "Logging");
            StreamWriter file =
                new StreamWriter(worldDirectory.FullName + Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "_" + "Crashlog.txt", true);
            file.WriteLine("DwarfCorp Version " + Version);
            OperatingSystem os = Environment.OSVersion;
            file.WriteLine("OS Version: " + os.Version);
            file.WriteLine("OS Platform: " + os.Platform);
            file.WriteLine("OS SP: " + os.ServicePack);
            file.WriteLine("OS Version String: " + os.VersionString);
            
            if (GameState.Game != null && GameState.Game.GraphicsDevice != null)
            {
                GraphicsAdapter adapter = GameState.Game.GraphicsDevice.Adapter;
                file.WriteLine("Graphics Card: " + adapter.DeviceName + "->" + adapter.Description);
                file.WriteLine("Display Mode: " + adapter.CurrentDisplayMode.Width + "x" + adapter.CurrentDisplayMode.Height + " (" + adapter.CurrentDisplayMode.AspectRatio + ")");
                file.WriteLine("Supported display modes: ");

                foreach (var mode in adapter.SupportedDisplayModes)
                {
                    file.WriteLine(mode.Width + "x" + mode.Height + " (" + mode.AspectRatio + ")");
                }
            }
            
            file.WriteLine(exception.ToString());
            file.Close();
        }

        public static string CreatePath(params string[] args)
        {
            string toReturn = "";

            for(int i = 0; i < args.Length; i++)
            {
                toReturn += args[i];

                if(i < args.Length - 1)
                {
                    toReturn += DirChar;
                }
            }

            return toReturn;
        }

        public static ManualResetEvent ShutdownEvent = new ManualResetEvent(false);

        public static void SignalShutdown()
        {
            DwarfGame.ExitGame = true;
            ShutdownEvent.Set();
        }

    }
#endif
}
#endif