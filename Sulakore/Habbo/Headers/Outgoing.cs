using System.IO;
using System.Runtime.Serialization.Json;

namespace Sulakore.Habbo.Headers
{
    public class Outgoing
    {
        private static readonly DataContractJsonSerializer _serializer =
            new DataContractJsonSerializer(typeof(Outgoing));

        private static readonly Outgoing _global = new Outgoing();
        public static Outgoing Global
        {
            get { return _global; }
        }

        public const ushort CLIENT_CONNECT = 4000;

        public ushort InitiateHandshake { get; set; }
        public ushort ClientPublicKey { get; set; }
        public ushort FlashClientUrl { get; set; }
        public ushort ClientSsoTicket { get; set; }

        public ushort PetScratch { get; set; }
        public ushort PlayerEffect { get; set; }

        public ushort BanPlayer { get; set; }
        public ushort KickPlayer { get; set; }
        public ushort MutePlayer { get; set; }
        public ushort TradePlayer { get; set; }
        public ushort ClickPlayer { get; set; }

        public ushort UpdateMotto { get; set; }
        public ushort UpdateStance { get; set; }
        public ushort UpdateClothes { get; set; }

        public ushort Walk { get; set; }
        public ushort Dance { get; set; }
        public ushort Gesture { get; set; }
        public ushort RaiseSign { get; set; }

        public ushort HostExitRoom { get; set; }
        public ushort NavigateRoom { get; set; }
        public ushort MoveFurniture { get; set; }
        public ushort ShopObjectGet { get; set; }

        public ushort Say { get; set; }
        public ushort Shout { get; set; }
        public ushort Whisper { get; set; }

        public void Save(string path)
        {
            using (var fileStream = File.Open(path, FileMode.Create))
                _serializer.WriteObject(fileStream, this);
        }
        public static Outgoing Load(string path)
        {
            using (var fileStream = File.Open(path, FileMode.Open))
                return (Outgoing)_serializer.ReadObject(fileStream);
        }
    }
}