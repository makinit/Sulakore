﻿/*
    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    This file is part of the Sulakore library.
    Copyright (C) 2015 ArachisH
    
    This code is licensed under the GNU General Public License.
    See License.txt in the project root for license information.
*/

using System;
using System.Threading.Tasks;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostBanPlayerEventArgs : InterceptedEventArgs
    {
        public int Id { get; }
        public HBan Ban { get; }
        public int RoomId { get; }

        public HostBanPlayerEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Id = packet.ReadInteger();
            RoomId = packet.ReadInteger();
            Ban = SKore.ToBan(packet.ReadString());
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, {nameof(Id)}: {Id}, " +
            $"{nameof(RoomId)}: {RoomId}, {nameof(Ban)}: {Ban}";
    }
}