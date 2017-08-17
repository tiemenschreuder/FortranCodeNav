namespace VSIntegration
{
    public class LocationInFile
    {
        public LocationInFile(int line, int offset, int absoluteOffset)
        {
            Line = line;
            Offset = offset;
            AbsoluteOffset = absoluteOffset;
        }

        public int Line { get; private set; }
        public int Offset { get; private set; }
        public int AbsoluteOffset { get; set; }
        public string LineStr { get; set; }

        public override string ToString()
        {
            return "line: " + Line + ", " + Offset;
        }
    }
}
