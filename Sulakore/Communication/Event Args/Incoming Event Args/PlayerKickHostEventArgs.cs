﻿using System;
using System.Threading.Tasks;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class PlayerKickHostEventArgs : DataInterceptedEventArgs
    {
        public PlayerKickHostEventArgs(HMessage packet, int step, Func<Task> continuation)
            : base(packet, step, continuation)
        { }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}";
    }
}