using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class PlayerUpdateEventArgs : InterceptedEventArgs, IHEntity
    {
        public int Index { get; }
        public string Motto { get; }
        public HGender Gender { get; }
        public string FigureId { get; }

        public PlayerUpdateEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Index = packet.ReadInteger();
            FigureId = packet.ReadString();
            Gender = SKore.ToGender(packet.ReadString());
            Motto = packet.ReadString();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Index)}: {Index}, " +
            $"{nameof(FigureId)}: {FigureId}, {nameof(Gender)}: {Gender}, {nameof(Motto)}: {Motto}";
    }
}