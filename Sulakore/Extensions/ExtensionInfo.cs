namespace Sulakore.Extensions
{
    public class ExtensionInfo
    {
        public string Hash { get; }
        public string FileLocation { get; }
        public Contractor Contractor { get; }


        public ExtensionInfo(string fileLocation, string hash, Contractor contractor)
        {
            Hash = hash;
            Contractor = contractor;
            FileLocation = fileLocation;
        }
    }
}