using Sulakore.Habbo.Web;
using Sulakore.Communication;

namespace Sulakore.Extensions
{
    public class ExtensionInfo
    {
        public string Hash { get; }
        public HHotel Hotel { get; }
        public HGameData GameData { get; }
        public string FileLocation { get; }
        public IHConnection Connection { get; }

        public ExtensionInfo(string fileLocation, string hash,
            HGameData gameData, HHotel hotel, IHConnection connection)
        {
            Hash = hash;
            Hotel = hotel;
            GameData = gameData;
            Connection = connection;
            FileLocation = fileLocation;
        }
    }
}