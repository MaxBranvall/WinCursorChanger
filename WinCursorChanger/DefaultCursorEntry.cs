namespace WinCursorChanger
{
    class DefaultCursorEntry
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public DefaultCursorEntry(string name, string path)
        {
            this.Name = name;
            this.Path = path;
        }

    }
}
