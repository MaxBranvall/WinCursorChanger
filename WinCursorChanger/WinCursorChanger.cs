using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace WinCursorChanger
{
    public class CursorChanger
    {

#pragma warning disable CA1416 // Validate platform compatibility

        #region DllImports

        [DllImport("user32.dll")]
        private static extern bool SetSystemCursor(IntPtr hcur, uint id);        

        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursorFromFile(string lpFileName);

        #endregion

        /// <summary>
        /// Path to the cursor that will replace the system cursor.
        /// </summary>
        private string _newCursorFilePath { get; set; }

        /// <summary>
        /// Path to the users CursorDirectory.
        /// Attention: Not used anymore. Will probably remove in the future.
        /// </summary>
        private string _rootCursorDirectoryPath { get; init; }

        /// <summary>
        /// Cursors subkey in the Registry.
        /// </summary>
        private string _cursorSubKey { get; set; }

        /// <summary>
        /// Path to the default cursors json file.
        /// </summary>
        private string _defaultCursorSettingsPath { get; init; }

        /// <summary>
        /// Determines whether or not a force reset of the default cursor
        /// settings file is required.
        /// </summary>
        private bool _forceReset { get; init; } = false;

        /// <summary>
        /// Stores the id of system cursors.
        /// </summary>
        public enum Cursors
        {
            AppStarting = 32650,
            Arrow = 32512,
            Crosshair = 32515,
            Hand = 32649,
            Help = 32651,
            IBeam = 32513,
            No = 32648,
            SizeAll = 32646,
            SizeNESW = 32643,
            SizeNS = 32645,
            SizeNWSE = 32642,
            SizeWE = 32644,
            UpArrow = 32516,
            Wait = 32514
        }

        private String[] _keysToIgnore = { "NWPen", "Person", "Pin", "Scheme Source", "GestureVisualization", "CursorBaseSize", "ContactVisualization" };

        public CursorChanger()
        {
            this._newCursorFilePath = null;
            this._cursorSubKey = @"Control Panel\Cursors";
            this._rootCursorDirectoryPath = _getCursorDirectoryPath();
            this._defaultCursorSettingsPath = _getDefaultCursorSettingsPath();
        }

        /// <summary>
        /// Constructor which takes in the new cursor's file path.
        /// </summary>
        /// <param name="newCursorFilePath">File path to the cursor that will replace selected system cursors.</param>
        public CursorChanger(string newCursorFilePath)
        {
            this._newCursorFilePath = newCursorFilePath;
            this._cursorSubKey = @"Control Panel\Cursors";
            this._rootCursorDirectoryPath = _getCursorDirectoryPath();
            this._defaultCursorSettingsPath = _getDefaultCursorSettingsPath();
        }

        /// <summary>
        /// Takes in the new cursor's file path, as well as a force reset bool.
        /// </summary>
        /// <param name="newCursorFilePath">File path to the cursor that will replace selected system cursors.</param>
        /// <param name="forceReset">True will force a reset to the defaultCursors file. False is the default behavior.</param>
        public CursorChanger(string newCursorFilePath, bool forceReset)
        {
            this._forceReset = forceReset;
            this._newCursorFilePath = newCursorFilePath;
            this._cursorSubKey = @"Control Panel\Cursors";
            this._rootCursorDirectoryPath = _getCursorDirectoryPath();
            this._defaultCursorSettingsPath = _getDefaultCursorSettingsPath();
        }

        /// <summary>
        /// Replaces common cursors (Arrow, Hand, IBeam) with the new cursor.
        /// </summary>
        /// <returns>Boolean: True if successful, false otherwise.</returns>
        public bool replaceCommonCursors()
        {
            Cursors[] cursorsToReplace = { Cursors.IBeam, Cursors.Arrow, Cursors.Hand };

            return this._setCursors(cursorsToReplace);
        }

        /// <summary>
        /// Replaces the link select cursor (hand) with the new cursor.
        /// </summary>
        /// <returns>Boolean: True if successful, false otherwise.</returns>
        public bool replaceLinkSelectCursor()
        {
            Cursors[] cursorsToReplace = { Cursors.Hand };

            return this._setCursors(cursorsToReplace);
        }

        /// <summary>
        /// Replaces all system cursors with the new cursor.
        /// </summary>
        /// <returns>Boolean: True if successful, false otherwise.</returns>
        public bool replaceAllCursors()
        {
            /// Create an array of each Cursor in Cursors.
            var tmp = Enum.GetValues(typeof(Cursors));

            // Create an array and fill it with Cursors objects.
            Cursors[] cursorsToReplace = new Cursors[tmp.Length];
            tmp.CopyTo(cursorsToReplace, 0);

            return this._setCursors(cursorsToReplace);

        }

        /// <summary>
        /// Replaces all cursors specified in array with
        /// the new cursor file specified in the constructor.
        /// </summary>
        /// <param name="cursorsToReplace">Array of Cursors to be replaced.</param>
        /// <returns>Boolean: True if successful, false otherwise.</returns>
        public bool replaceAnyCursors(Cursors[] cursorsToReplace)
        {
            return this._setCursors(cursorsToReplace);
        }

        /// <summary>
        /// Restores default cursors from backup file made
        /// on first startup.
        /// </summary>
        /// <returns>Bool: True if successful, false otherwise.</returns>
        public bool restoreAllDefaultCursors()
        {

            CursorEntryList jsonOutput;

            // Deserialsize settings file into object

            try
            {
                jsonOutput = JsonConvert.DeserializeObject<CursorEntryList>(File.ReadAllText(this._defaultCursorSettingsPath));
            } catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deserializing the settings file: {ex}");
                return false;
            }

            return this._setCursors(jsonOutput.defaultCursorEntries);
        }

        /// <summary>
        /// Provides abstraction to public methods. Calls the private methods
        /// to update the registry and perform interops.
        /// </summary>
        /// <param name="cursorsToReplace">Array of cursors to replace.</param>
        /// <returns>Boolean: true if successful, false otherwise</returns>
        private bool _setCursors(Cursors[] cursorsToReplace)
        {

            bool registry = false;
            bool system = false;

            try
            {

                // If our new cursor was never specified, do not proceed.
                if (String.IsNullOrEmpty(this._newCursorFilePath))
                {
                    throw new ArgumentNullException("_newCursorFilePath", "Error: New cursor was not specified!");
                }

                registry = this._updateRegistry(cursorsToReplace);
                system = this._updateSystemCursorsWithoutRestart(cursorsToReplace);
            } catch (ArgumentNullException ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            return (registry && system) ? true : false;
        }

        /// <summary>
        /// Provides abstraction to public methods. Calls the private methods
        /// to update the registry and perform interops.
        /// </summary>
        /// <param name="cursorsToReplace">List of cursors to replace.</param>
        /// <returns>Boolean: true if successful, false otherwise</returns>
        private bool _setCursors(List<DefaultCursorEntry> cursorsToReplace)
        {
            bool registry = this._updateRegistry(cursorsToReplace);
            bool system = this._updateSystemCursorsWithoutRestart(cursorsToReplace);

            return (registry && system) ? true : false;
        }

        /// <summary>
        /// Updates the registry of all specified cursors.
        /// </summary>
        /// <param name="cursorsToReplace">Array of cursors to replace.</param>
        /// <returns>Bool: True - Updated Successfully, False - Otherwise</returns>
        private bool _updateRegistry(Cursors[] cursorsToReplace)
        {
            try
            {
                using (var cursorRegistryKey = Registry.CurrentUser.OpenSubKey(this._cursorSubKey, true))
                {

                    foreach(var cursor in cursorsToReplace)
                    {
                        cursorRegistryKey.SetValue(cursor.ToString(), this._newCursorFilePath);
                    }                    
                }
            } catch (Exception ex)
            {
                Console.WriteLine("There was an issue setting the cursor: " + ex);
                return false;
            }

            return true;
        }
        /// <summary>
        /// Used to reset default cursors.
        /// </summary>
        /// <param name="cursorsToReplace">List of cursors to replace.</param>
        /// <returns>Bool: True - Updated Successfully, False - Otherwise</returns>
        private bool _updateRegistry(List<DefaultCursorEntry> cursorsToReplace)
        {
            try
            {
                using (var cursorRegistryKey = Registry.CurrentUser.OpenSubKey(this._cursorSubKey, true))
                {
                    foreach (var cursor in cursorsToReplace)
                    {
                        cursorRegistryKey.SetValue(cursor.Name, cursor.Path);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an issue setting the cursor: " + ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Uses interops to update the cursor immediately. This allows the cursor
        /// changes to be seen without restarting or logging out of the system first.
        /// </summary>
        /// <param name="cursorsToReplace">Array of cursors to replace.</param>
        /// <returns>Boolean: True if successful, false otherwise.</returns>
        private bool _updateSystemCursorsWithoutRestart(Cursors[] cursorsToReplace)
        {
            foreach(var cursor in cursorsToReplace)
            {
                try
                {
                    IntPtr newCursorFilePtr = LoadCursorFromFile(this._newCursorFilePath);
                    SetSystemCursor(newCursorFilePtr, (uint)cursor.GetHashCode());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"There was an error updating cursor ({cursor}). Error: {ex}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Used to reset system cursors to default. 
        /// Uses interops to update the cursor immediately. This allows the cursor
        /// changes to be seen without restarting or logging out of the system first.
        /// </summary>
        /// <param name="cursorsToReplace">List of cursors to replace.</param>
        /// <returns>Boolean: True if successful, false otherwise.</returns>
        private bool _updateSystemCursorsWithoutRestart(List<DefaultCursorEntry> cursorsToReplace)
        {
            foreach (var cursor in cursorsToReplace)
            {
                try
                {
                    IntPtr newCursorFilePtr = LoadCursorFromFile(cursor.Path);
                    SetSystemCursor(newCursorFilePtr, (uint)cursor.ID);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"There was an error updating cursor ({cursor}). Error: {ex}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get the cursor directory path
        /// </summary>
        /// <returns>String - Cursor directory path.</returns>
        private string _getCursorDirectoryPath()
        {
            return Path.Combine(Environment.GetEnvironmentVariable("systemroot"), "Cursors");
        }

        /// <summary>
        /// Gets the path to the default cursor settings JSON file created
        /// by the application.
        /// </summary>
        /// <returns>String: Path to the file, Null if an error occurred.</returns>
        private string _getDefaultCursorSettingsPath()
        {
            // Path to AppData/Roaming
            string appDataRoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            
            // Full path to defaultCursors directory
            string fullDirectoryPath = Path.Combine(appDataRoamingPath, @"WinCursorChanger/defaultCursors");

            // Full path to defaultCursors.json
            string fullFilePath = Path.Combine(fullDirectoryPath, "defaultCursors.json");


            // If the file exits, return the file path,
            // otherwise, try to save the default cursors.

            if (File.Exists(fullFilePath) && !this._forceReset)
            {
                return fullFilePath;
            }

            if (!Directory.Exists(fullDirectoryPath))
            {
                Directory.CreateDirectory(fullDirectoryPath);
            }

            return _saveDefaultCursors(fullFilePath) ? fullFilePath : null;

        }

        /// <summary>
        /// Saves default cursors from registry to a JSON file
        /// Path of file: C:\Users\(user)\AppData\Roaming\WinCursorChanger\defaultCursors
        /// </summary>
        /// <param name="filePath">Path to JSON file.</param>
        private bool _saveDefaultCursors(string filePath)
        {

            CursorEntryList cursorEntryList = new CursorEntryList();

            try
            {
                using (var cursorRegistryKey = Registry.CurrentUser.OpenSubKey(this._cursorSubKey, true))
                {
                    var names = cursorRegistryKey.GetValueNames();

                    foreach (var name in names)
                    {
                        // Only save cursors whose values are not of type 'DWORD', not empty, and not in keys to ignore.
                        if (!(name == "DWORD" || String.IsNullOrEmpty(name) || this._keysToIgnore.Contains(name)))
                        {
                            var path = cursorRegistryKey.GetValue(name);
                            var entry = new DefaultCursorEntry(this._getID(name), name, path.ToString());
                            cursorEntryList.defaultCursorEntries.Add(entry);
                        }
                    }
                }

            } catch (Exception ex)
            {
                Console.WriteLine($"Caught exception while retrieving registry items: {ex}");
                return false;
            }

            try
            {
                string defaultCursorJson = JsonConvert.SerializeObject(cursorEntryList);

                using (StreamWriter sw = File.CreateText(filePath))
                {
                    sw.WriteLine(defaultCursorJson);
                }

            } catch (Exception ex)
            {
                Console.WriteLine($"Caught exception while writing default cursor settings: {ex}");
                return false;
            }

            return true;

        }

        /// <summary>
        /// Get the hash code of a cursor from Cursors.
        /// </summary>
        /// <param name="name">Name of cursor.</param>
        /// <returns>Boolean: true if successful, false otherwise.</returns>
        private int _getID(string name)
        {
            var cursorsList = Enum.GetValues(typeof(Cursors));

            foreach (Cursors cursor in cursorsList)
            {
                if (cursor.ToString().Equals(name))
                {
                    return cursor.GetHashCode();
                }
            }

            return -1;

        }
    }
}
