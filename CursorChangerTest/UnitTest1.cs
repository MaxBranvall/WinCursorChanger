using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using WinCursorChanger;

namespace CursorChangerTest
{
    [TestClass]
    public class UnitTest1
    {

        string cursorPath = Path.Combine(Directory.GetCurrentDirectory(), "Cursors\\_middleFinger.cur");

        [TestMethod]
        public void TestReplaceCommonCursors()
        {

            CursorChanger cursorChanger = new CursorChanger();

            bool result = cursorChanger.replaceAllCursors();
            Assert.IsFalse(result);

        }
    }
}
