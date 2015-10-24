using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostUpdateClothesEventArgs : InterceptedEventArgs
    {
        public HGender Gender { get; }
        public string FigureId { get; }

        public HostUpdateClothesEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Gender = SKore.ToGender(packet.ReadString());
            FigureId = packet.ReadString();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, " +
            $"{nameof(Gender)}: {Gender}, {nameof(FigureId)}: {FigureId}";
    }
}