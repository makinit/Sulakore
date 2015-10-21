/*
    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    This file is part of the Sulakore library.
    Copyright (C) 2015 ArachisH
    
    This code is licensed under the GNU General Public License.
    See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;

using Sulakore.Protocol;
using Sulakore.Habbo.Headers;

namespace Sulakore.Communication
{
    public class HTriggers : IDisposable
    {
        #region Incoming Game Event Handlers
        public event EventHandler<FurnitureLoadEventArgs> FurnitureLoad;
        public void RaiseOnFurnitureLoad(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(FurnitureLoad, e);
        }

        public event EventHandler<FurnitureDropEventArgs> FurnitureDrop;
        public void RaiseOnFurnitureDrop(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(FurnitureDrop, e);
        }

        public event EventHandler<FurnitureMoveEventArgs> FurnitureMove;
        public void RaiseOnFurnitureMove(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(FurnitureMove, e);
        }

        public event EventHandler<EntityLoadEventArgs> EntityLoad;
        public void RaiseOnEntityLoad(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(EntityLoad, e);
        }

        public event EventHandler<EntityActionEventArgs> EntityAction;
        public void RaiseOnEntityAction(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(EntityAction, e);
        }

        public event EventHandler<PlayerKickHostEventArgs> PlayerKickHost;
        public void RaiseOnPlayerKickHost(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(PlayerKickHost, e);
        }

        public event EventHandler<PlayerUpdateEventArgs> PlayerUpdate;
        public void RaiseOnPlayerUpdate(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(PlayerUpdate, e);
        }

        public event EventHandler<PlayerDanceEventArgs> PlayerDance;
        public void RaiseOnPlayerDance(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(PlayerDance, e);
        }

        public event EventHandler<PlayerGestureEventArgs> PlayerGesture;
        public void RaiseOnPlayerGesture(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(PlayerGesture, e);
        }
        #endregion
        #region Outgoing Game Event Handlers
        public event EventHandler<HostBanPlayerEventArgs> HostBanPlayer;
        public void RaiseOnHostBanPlayer(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostBanPlayer, e);
        }

        public event EventHandler<HostUpdateClothesEventArgs> HostUpdateClothes;
        public void RaiseOnHostUpdateClothes(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostUpdateClothes, e);
        }

        public event EventHandler<HostUpdateMottoEventArgs> HostUpdateMotto;
        public void RaiseOnHostUpdateMotto(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostUpdateMotto, e);
        }

        public event EventHandler<HostUpdateStanceEventArgs> HostUpdateStance;
        public void RaiseOnHostUpdateStance(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostUpdateStance, e);
        }

        public event EventHandler<HostClickPlayerEventArgs> HostClickPlayer;
        public void RaiseOnHostClickPlayer(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostClickPlayer, e);
        }

        public event EventHandler<HostDanceEventArgs> HostDance;
        public void RaiseOnHostDance(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostDance, e);
        }

        public event EventHandler<HostGestureEventArgs> HostGesture;
        public void RaiseOnHostGesture(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostGesture, e);
        }

        public event EventHandler<HostKickPlayerEventArgs> HostKickPlayer;
        public void RaiseOnHostKickPlayer(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostKickPlayer, e);
        }

        public event EventHandler<HostMoveFurnitureEventArgs> HostMoveFurniture;
        public void RaiseOnHostMoveFurniture(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostMoveFurniture, e);
        }

        public event EventHandler<HostMutePlayerEventArgs> HostMutePlayer;
        public void RaiseOnHostMutePlayer(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostMutePlayer, e);
        }

        public event EventHandler<HostRaiseSignEventArgs> HostRaiseSign;
        public void RaiseOnHostRaiseSign(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostRaiseSign, e);
        }

        public event EventHandler<HostExitRoomEventArgs> HostExitRoom;
        public void RaiseOnHostExitRoom(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostExitRoom, e);
        }

        public event EventHandler<HostNavigateRoomEventArgs> HostNavigateRoom;
        public void RaiseOnHostNavigateRoom(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostNavigateRoom, e);
        }

        public event EventHandler<HostSayEventArgs> HostSay;
        public void RaiseOnHostSay(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostSay, e);
        }

        public event EventHandler<HostShoutEventArgs> HostShout;
        public void RaiseOnHostShout(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostShout, e);
        }

        public event EventHandler<HostTradeEventArgs> HostTradePlayer;
        public void RaiseOnHostTradePlayer(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostTradePlayer, e);
        }

        public event EventHandler<HostWalkEventArgs> HostWalk;
        public void RaiseOnHostWalk(InterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostWalk, e);
        }
        #endregion

        private readonly Stack<HMessage> _outPrevious, _inPrevious;
        private readonly IDictionary<ushort, Action<InterceptedEventArgs>> _inAttaches, _outAttaches;

        protected IDictionary<ushort, Action<InterceptedEventArgs>> InDetected { get; }
        protected IDictionary<ushort, Action<InterceptedEventArgs>> OutDetected { get; }

        /// <summary>
        /// Gets or sets a value that determines whether to begin identifying outgoing events.
        /// </summary>
        public bool DetectOutgoing { get; set; }
        /// <summary>
        /// Gets or sets a value that determines whether to begin identifying incoming events.
        /// </summary>
        public bool DetectIncoming { get; set; }
        public bool IsDisposed { get; private set; }

        public Outgoing OutgoingDetected { get; set; }
        public Incoming IncomingDetected { get; set; }

        public HTriggers(bool isDetecting)
        {
            DetectOutgoing =
                DetectIncoming = isDetecting;

            IncomingDetected = new Incoming();
            OutgoingDetected = new Outgoing();

            InDetected = new Dictionary<ushort, Action<InterceptedEventArgs>>();
            OutDetected = new Dictionary<ushort, Action<InterceptedEventArgs>>();

            _inPrevious = new Stack<HMessage>();
            _outPrevious = new Stack<HMessage>();
            _inAttaches = new Dictionary<ushort, Action<InterceptedEventArgs>>();
            _outAttaches = new Dictionary<ushort, Action<InterceptedEventArgs>>();
        }

        public void OutDetach()
        {
            _outAttaches.Clear();
        }
        public void OutDetach(ushort header)
        {
            if (_outAttaches.ContainsKey(header))
                _outAttaches.Remove(header);
        }
        public void OutAttach(ushort header, Action<InterceptedEventArgs> callback)
        {
            _outAttaches[header] = callback;
        }

        public void InDetach()
        {
            _inAttaches.Clear();
        }
        public void InDetach(ushort header)
        {
            if (_inAttaches.ContainsKey(header))
                _inAttaches.Remove(header);
        }
        public void InAttach(ushort header, Action<InterceptedEventArgs> callback)
        {
            _inAttaches[header] = callback;
        }

        public void HandleOutgoing(InterceptedEventArgs e)
        {
            bool ignoreCurrent = true;
            try
            {
                e.Packet.Position = 0;
                if (_outAttaches.ContainsKey(e.Packet.Header))
                    _outAttaches[e.Packet.Header](e);

                if (DetectOutgoing)
                {
                    e.Packet.Position = 0;
                    HMessage previous = _outPrevious.Count > 0 ?
                        _outPrevious.Pop() : e.Packet;

                    bool currentDetected = OutDetected.ContainsKey(e.Packet.Header);
                    bool previousDetected = OutDetected.ContainsKey(previous.Header);

                    if (!currentDetected && !previousDetected)
                        ignoreCurrent = HandleOutgoing(e.Packet, previous);

                    if (ignoreCurrent)
                    {
                        e.Packet.Position =
                            previous.Position = 0;

                        if (OutDetected.ContainsKey(e.Packet.Header))
                        {
                            OutDetected[e.Packet.Header](e);
                        }
                        else if (OutDetected.ContainsKey(previous.Header))
                        {
                            var args = new InterceptedEventArgs(null, e.Step - 1, previous);
                            OutDetected[previous.Header](args);
                        }
                    }
                }
            }
            finally
            {
                e.Packet.Position = 0;

                if (DetectOutgoing && !ignoreCurrent)
                    _outPrevious.Push(e.Packet);
            }
        }
        protected virtual bool HandleOutgoing(HMessage current, HMessage previous)
        {
            if (current.Length == 6)
            {
                // Range: 6
                if (TryHandleAvatarMenuClick(current, previous)) return true;
                if (TryHandleHostExitRoom(current, previous)) return true;
            }
            else if (current.Length >= 36 && current.Length <= 50)
            {
                //Range: 36 - 50
                if (TryHandleHostRaiseSign(current, previous)) return true;
                if (TryHandleHostNavigateRoom(current, previous)) return true;
            }

            return OutDetected.ContainsKey(current.Header) ||
                OutDetected.ContainsKey(previous.Header);
        }

        public void HandleIncoming(InterceptedEventArgs e)
        {
            bool ignoreCurrent = true;
            try
            {
                e.Packet.Position = 0;
                if (_inAttaches.ContainsKey(e.Packet.Header))
                    _inAttaches[e.Packet.Header](e);

                if (DetectIncoming)
                {
                    e.Packet.Position = 0;
                    HMessage previous = _inPrevious.Count > 0 ?
                        _inPrevious.Pop() : e.Packet;

                    bool currentDetected = InDetected.ContainsKey(e.Packet.Header);
                    bool previousDetected = InDetected.ContainsKey(previous.Header);

                    if (!currentDetected && !previousDetected)
                        ignoreCurrent = HandleIncoming(e.Packet, previous);

                    if (ignoreCurrent)
                    {
                        e.Packet.Position =
                            previous.Position = 0;

                        if (InDetected.ContainsKey(e.Packet.Header))
                        {
                            InDetected[e.Packet.Header](e);
                        }
                        else if (InDetected.ContainsKey(previous.Header))
                        {
                            var args = new InterceptedEventArgs(null, e.Step - 1, previous);
                            InDetected[previous.Header](args);
                        }
                    }
                }
            }
            finally
            {
                e.Packet.Position = 0;

                if (DetectIncoming && !ignoreCurrent)
                    _inPrevious.Push(e.Packet);
            }
        }
        protected virtual bool HandleIncoming(HMessage current, HMessage previous)
        {
            if (current.Length == 6)
            {
                // Range: 6
                if (TryHandlePlayerKickHost(current, previous)) return true;
            }
            else if (current.Length >= 35)
            {
                if (TryHandleEntityAction(current, previous)) return true;
            }

            return InDetected.ContainsKey(current.Header) ||
                InDetected.ContainsKey(previous.Header);
        }

        protected virtual void RaiseOnGameEvent<T>(EventHandler<T> handler, InterceptedEventArgs e) where T : InterceptedEventArgs
        {
            if (handler != null)
            {
                var args = (T)Activator.CreateInstance(typeof(T),
                    e.Continuation, e.Step, e.Packet);

                OnGameEvent(handler, args, e);
            }
        }
        protected virtual void OnGameEvent<T>(EventHandler<T> handler, T arguments, InterceptedEventArgs e) where T : InterceptedEventArgs
        {
            try { handler?.Invoke(this, arguments); }
            finally
            {
                e.IsBlocked = arguments.IsBlocked;
                e.Replacement = arguments.Replacement;
                e.WasContinued = arguments.WasContinued;
            }
        }

        private bool TryHandleHostExitRoom(HMessage current, HMessage previous)
        {
            if (previous.Length != 2 ||
                current.ReadInteger(0) != -1)
            {
                return false;
            }

            OutgoingDetected.HostExitRoom = previous.Header;
            OutDetected[previous.Header] = RaiseOnHostExitRoom;
            return true;
        }
        private bool TryHandleEntityAction(HMessage current, HMessage previous)
        {
            if (!current.CanReadString(16)) return false;

            int position = 16;
            string z = current.ReadString(ref position);

            if (position + 8 >= current.Length) return false;
            int d1 = current.ReadInteger(ref position);
            int d2 = current.ReadInteger(ref position);

            if (d1 < 0 || d2 < 0) return false;
            if (d1 > 7 || d2 > 7) return false;

            if (!current.CanReadString(position)) return false;
            string action = current.ReadString(ref position);

            if ((position + 2) == current.Length)
            {
                InDetected[current.Header] = RaiseOnEntityAction;
                return true;
            }
            else return false;
        }
        private bool TryHandleHostRaiseSign(HMessage current, HMessage previous)
        {
            bool isHostRaiseSign = false;
            if (current.CanReadString(22) && current.ReadString(22) == "sign")
            {
                OutgoingDetected.RaiseSign = previous.Header;
                OutDetected[previous.Header] = RaiseOnHostRaiseSign;

                isHostRaiseSign = true;
            }
            return isHostRaiseSign;
        }
        private bool TryHandlePlayerKickHost(HMessage current, HMessage previous)
        {
            bool isPlayerKickHost = (current.ReadInteger(0) == 4008);
            if (isPlayerKickHost)
            {
                IncomingDetected.PlayerKickHost = current.Header;
                InDetected[current.Header] = RaiseOnPlayerKickHost;
            }
            return isPlayerKickHost;
        }
        private bool TryHandleAvatarMenuClick(HMessage current, HMessage previous)
        {
            if (!previous.CanReadString(22)) return false;
            switch (previous.ReadString(22))
            {
                case "sit":
                case "stand":
                {
                    OutgoingDetected.UpdateStance = current.Header;
                    OutDetected[current.Header] = RaiseOnHostUpdateStance;
                    return true;
                }

                case "dance_stop":
                case "dance_start":
                {
                    OutgoingDetected.Dance = current.Header;
                    OutDetected[current.Header] = RaiseOnHostDance;
                    return true;
                }

                case "wave":
                case "idle":
                case "laugh":
                case "blow_kiss":
                {
                    OutgoingDetected.Gesture = current.Header;
                    OutDetected[current.Header] = RaiseOnHostGesture;
                    return true;
                }
            }
            return false;
        }
        private bool TryHandleHostNavigateRoom(HMessage current, HMessage previous)
        {
            if (previous.Length >= 12 &&
                current.CanReadString(0) &&
                current.ReadString() == "Navigation")
            {
                current.ReadString();
                if (current.ReadString() != "go.official") return false;

                if (previous.ReadInteger(0).ToString() == current.ReadString())
                {
                    OutgoingDetected.NavigateRoom = previous.Header;

                    OutDetected[previous.Header] = RaiseOnHostNavigateRoom;
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            if (disposing)
            {
                _inPrevious.Clear();
                _outPrevious.Clear();

                InDetected.Clear();
                OutDetected.Clear();

                _inAttaches.Clear();
                _outAttaches.Clear();

                #region Unsubscribe Game Events
                SKore.Unsubscribe(ref HostBanPlayer);
                SKore.Unsubscribe(ref HostUpdateClothes);
                SKore.Unsubscribe(ref HostUpdateMotto);
                SKore.Unsubscribe(ref HostUpdateStance);
                SKore.Unsubscribe(ref HostClickPlayer);
                SKore.Unsubscribe(ref HostDance);
                SKore.Unsubscribe(ref HostGesture);
                SKore.Unsubscribe(ref HostKickPlayer);
                SKore.Unsubscribe(ref HostMoveFurniture);
                SKore.Unsubscribe(ref HostMutePlayer);
                SKore.Unsubscribe(ref HostRaiseSign);
                SKore.Unsubscribe(ref HostExitRoom);
                SKore.Unsubscribe(ref HostNavigateRoom);
                SKore.Unsubscribe(ref HostSay);
                SKore.Unsubscribe(ref HostShout);
                SKore.Unsubscribe(ref HostTradePlayer);
                SKore.Unsubscribe(ref HostWalk);

                SKore.Unsubscribe(ref FurnitureLoad);
                SKore.Unsubscribe(ref EntityAction);
                SKore.Unsubscribe(ref PlayerUpdate);
                SKore.Unsubscribe(ref PlayerDance);
                SKore.Unsubscribe(ref EntityLoad);
                SKore.Unsubscribe(ref FurnitureDrop);
                SKore.Unsubscribe(ref PlayerGesture);
                SKore.Unsubscribe(ref PlayerKickHost);
                SKore.Unsubscribe(ref FurnitureMove);
                #endregion
            }
            IsDisposed = true;
        }
    }
}