﻿/*
    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    This file is part of the Sulakore library.
    Copyright (C) 2015 ArachisH
    
    This code is licensed under the GNU General Public License.
    See License.txt in the project root for license information.
*/

using System;
using System.Threading.Tasks;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostPrivateMessageEventArgs : InterceptedEventArgs
    {
        public int Id { get; }
        public string Message { get; }

        public HostPrivateMessageEventArgs(Func<Task> continuation, int step, HMessage packet)
            : base(continuation, step, packet)
        {
            Id = packet.ReadInteger();
            Message = packet.ReadString();
        }

        public override string ToString() =>
            $"{nameof(Packet.Header)}: {Packet.Header}, " +
            $"{nameof(Id)}: {Id}, {nameof(Message)}: {Message}";
    }
}