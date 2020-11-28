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
        /// Stores the id of system cursors.
        /// </summary>
        private enum Cursors
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

        private Dictionary<Cursors, string> cursorDict = new Dictionary<Cursors, string>();

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
        /// Replaces common cursors (Arrow, Hand, IBeam) with the new cursor.
        /// </summary>
        /// <returns>Boolean: True if successful, false otherwise.</returns>
        public bool replaceCommonCursors()
        {
            Cursors[] cursorsToReplace = { Cursors.IBeam, Cursors.Arrow, Cursors.Hand };

            foreach (var cursor in cursorsToReplace)
            {
                try
                {
                    this._setCursors(cursor);

                }
                catch (Exception ex)
                {
                    throw new Exception();
                }
            }

            return true;
        }

        ///// <summary>
        ///// Replaces the link select cursor (hand) with the new cursor.
        ///// </summary>
        ///// <returns>Boolean: True if successful, false otherwise.</returns>
        public bool replaceLinkSelectCursor()
        {
            Cursors cursor = Cursors.Hand;

            return this._setCursors(cursor);
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

            foreach (var cursor in cursorsToReplace)
            {
                try
                {
                    this._setCursors(cursor);

                } catch(Exception ex)
                {
                    throw new Exception();
                }
            }

            return true;

        }

        /// <summary>
        /// Not implemented yet. Will restore cursors to default.
        /// </summary>
        /// <returns></returns>
        public bool resetDefaultCursors()
        {
            throw new NotImplementedException();
        }

        private bool _defaultCursors()
        {
            // Cursors.AppStarting = wait_r.cur
            // Cursors.Normal = arrow_r.cur
            // Cursors.Cross = cross_r.cur
            // Cursors.Hand = aero_link.cur
            // Cursors.Help = help_r.cur
            // Cursors.IBeam = //not yet
            throw new NotImplementedException();
        }

        private bool _setCursors(Cursors cursor)
        {
            bool registry = this._updateRegistry(cursor);
            bool system = this._updateSystemCursorWithoutRestart(cursor);

            return (registry && system) ? true : false;
        }

        /// <summary>
        /// Updates the registry of all specified cursors.
        /// </summary>
        /// <param name="cursorsToReplace">List of cursorsToReplace</param>
        /// <returns>Bool: True - Updated Successfully, False - Otherwise</returns>
        private bool _updateRegistry(Cursors cursor)
        {
            try
            {
                using (var cursorRegistryKey = Registry.CurrentUser.OpenSubKey(this._cursorSubKey, true))
                {
                    cursorRegistryKey.SetValue(cursor.ToString(), this._newCursorFilePath);
                }
            } catch (Exception ex)
            {
                Console.WriteLine("There was an issue setting the cursor: " + ex);
                return false;
            }

            return true;
        }

        private bool _resetRegistry(Cursors[] cursorsToReplace)
        {
            try
            {
                using (var cursorRegistryKey = Registry.CurrentUser.OpenSubKey(this._cursorSubKey, true))
                {
                    foreach (var cursor in cursorsToReplace)
                    {
                        cursorRegistryKey.SetValue(cursor.ToString(), cursor.GetHashCode());
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

        private bool _updateSystemCursorWithoutRestart(Cursors cursor)
        {

            try
            {
                IntPtr newCursorFilePtr = LoadCursorFromFile(this._newCursorFilePath);

                try
                {
                    SetSystemCursor(newCursorFilePtr, (uint)cursor.GetHashCode());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"There was an error updating cursor ({cursor}). Error: {ex}");
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"There was an error loading the cursorFile. Error: {ex}");
                return false;
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

            if (File.Exists(fullFilePath))
            {
                return fullFilePath;
            }

            Directory.CreateDirectory(fullDirectoryPath);

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

                        if (!(name == "DWORD" || String.IsNullOrEmpty(name) || this._keysToIgnore.Contains(name)))
                        {
                            var path = cursorRegistryKey.GetValue(name);
                            var entry = new DefaultCursorEntry(name, path.ToString());
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
    }
}
