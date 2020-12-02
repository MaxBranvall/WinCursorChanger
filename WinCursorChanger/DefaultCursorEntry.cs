namespace WinCursorChanger
{
    class DefaultCursorEntry
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

        public DefaultCursorEntry(int id, string name, string path)
        {
            this.ID = id;
            this.Name = name;
            this.Path = path;
        }

    }
}
