using System;
using System.IO;
using WinCursorChanger;

namespace WinCursorChangerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            CursorChanger cursorChanger = new CursorChanger();
            cursorChanger.restoreAllDefaultCursors();
        }
    }
}
