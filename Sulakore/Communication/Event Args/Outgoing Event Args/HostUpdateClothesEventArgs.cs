using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostUpdateClothesEventArgs : DataInterceptedEventArgs
    {
        public HGender Gender { get; }
        public string FigureId { get; }

        public HostUpdateClothesEventArgs(HMessage packet, int step, Func<Task> continuation)
            : base(packet, step, continuation)
        {
            Gender = SKore.ToGender(packet.ReadString());
            FigureId = packet.ReadString();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, " +
            $"{nameof(Gender)}: {Gender}, {nameof(FigureId)}: {FigureId}";
    }
}