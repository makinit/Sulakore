/* Copyright

    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    .NET library for creating Habbo Hotel related desktop applications.
    Copyright (C) 2015 ArachisH

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

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
            if (FurnitureLoad != null)
            {
                var args = new FurnitureLoadEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(FurnitureLoad, args, e);
            }
        }

        public event EventHandler<FurnitureDropEventArgs> FurnitureDrop;
        public void RaiseOnFurnitureDrop(InterceptedEventArgs e)
        {
            if (FurnitureDrop != null)
            {
                var args = new FurnitureDropEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(FurnitureDrop, args, e);
            }
        }

        public event EventHandler<FurnitureMoveEventArgs> FurnitureMove;
        public void RaiseOnFurnitureMove(InterceptedEventArgs e)
        {
            if (FurnitureMove != null)
            {
                var args = new FurnitureMoveEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(FurnitureMove, args, e);
            }
        }

        public event EventHandler<EntityLoadEventArgs> EntityLoad;
        public void RaiseOnEntityLoad(InterceptedEventArgs e)
        {
            if (EntityLoad != null)
            {
                var args = new EntityLoadEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(EntityLoad, args, e);
            }
        }

        public event EventHandler<EntityActionEventArgs> EntityAction;
        public void RaiseOnEntityAction(InterceptedEventArgs e)
        {
            if (EntityAction != null)
            {
                var args = new EntityActionEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(EntityAction, args, e);
            }
        }

        public event EventHandler<PlayerKickHostEventArgs> PlayerKickHost;
        public void RaiseOnPlayerKickHost(InterceptedEventArgs e)
        {
            if (PlayerKickHost != null)
            {
                var args = new PlayerKickHostEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(PlayerKickHost, args, e);
            }
        }

        public event EventHandler<PlayerUpdateEventArgs> PlayerUpdate;
        public void RaiseOnPlayerUpdate(InterceptedEventArgs e)
        {
            if (PlayerUpdate != null)
            {
                var args = new PlayerUpdateEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(PlayerUpdate, args, e);
            }
        }

        public event EventHandler<PlayerDanceEventArgs> PlayerDance;
        public void RaiseOnPlayerDance(InterceptedEventArgs e)
        {
            if (PlayerDance != null)
            {
                var args = new PlayerDanceEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(PlayerDance, args, e);
            }
        }

        public event EventHandler<PlayerGestureEventArgs> PlayerGesture;
        public void RaiseOnPlayerGesture(InterceptedEventArgs e)
        {
            if (PlayerGesture != null)
            {
                var args = new PlayerGestureEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(PlayerGesture, args, e);
            }
        }
        #endregion
        #region Outgoing Game Event Handlers
        public event EventHandler<HostBanPlayerEventArgs> HostBanPlayer;
        public void RaiseOnHostBanPlayer(InterceptedEventArgs e)
        {
            if (HostBanPlayer != null)
            {
                var args = new HostBanPlayerEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(HostBanPlayer, args, e);
            }
        }

        public event EventHandler<HostUpdateClothesEventArgs> HostUpdateClothes;
        public void RaiseOnHostUpdateClothes(InterceptedEventArgs e)
        {
            if (HostUpdateClothes != null)
            {
                var args = new HostUpdateClothesEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(HostUpdateClothes, args, e);
            }
        }

        public event EventHandler<HostUpdateMottoEventArgs> HostUpdateMotto;
        public void RaiseOnHostUpdateMotto(InterceptedEventArgs e)
        {
            if (HostUpdateMotto != null)
            {
                var args = new HostUpdateMottoEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(HostUpdateMotto, args, e);
            }
        }

        public event EventHandler<HostUpdateStanceEventArgs> HostUpdateStance;
        public void RaiseOnHostUpdateStance(InterceptedEventArgs e)
        {
            if (HostUpdateStance != null)
            {
                var args = new HostUpdateStanceEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(HostUpdateStance, args, e);
            }
        }

        public event EventHandler<HostClickPlayerEventArgs> HostClickPlayer;
        public void RaiseOnHostClickPlayer(InterceptedEventArgs e)
        {
            if (HostClickPlayer != null)
            {
                var args = new HostClickPlayerEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(HostClickPlayer, args, e);
            }
        }

        public event EventHandler<HostDanceEventArgs> HostDance;
        public void RaiseOnHostDance(InterceptedEventArgs e)
        {
            if (HostDance != null)
            {
                var args = new HostDanceEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(HostDance, args, e);
            }
        }

        public event EventHandler<HostGestureEventArgs> HostGesture;
        public void RaiseOnHostGesture(InterceptedEventArgs e)
        {
            if (HostGesture != null)
            {
                var args = new HostGestureEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(HostGesture, args, e);
            }
        }

        public event EventHandler<HostKickPlayerEventArgs> HostKickPlayer;
        public void RaiseOnHostKickPlayer(InterceptedEventArgs e)
        {
            if (HostKickPlayer != null)
            {
                var args = new HostKickPlayerEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(HostKickPlayer, args, e);
            }
        }

        public event EventHandler<HostMoveFurnitureEventArgs> HostMoveFurniture;
        public void RaiseOnHostMoveFurniture(InterceptedEventArgs e)
        {
            if (HostMoveFurniture != null)
            {
                var args = new HostMoveFurnitureEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(HostMoveFurniture, args, e);
            }
        }

        public event EventHandler<HostMutePlayerEventArgs> HostMutePlayer;
        public void RaiseOnHostMutePlayer(InterceptedEventArgs e)
        {
            if (HostMutePlayer != null)
            {
                var args = new HostMutePlayerEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(HostMutePlayer, args, e);
            }
        }

        public event EventHandler<HostRaiseSignEventArgs> HostRaiseSign;
        public void RaiseOnHostRaiseSign(InterceptedEventArgs e)
        {
            if (HostRaiseSign != null)
            {
                var args = new HostRaiseSignEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(HostRaiseSign, args, e);
            }
        }

        public event EventHandler<HostExitRoomEventArgs> HostExitRoom;
        public void RaiseOnHostExitRoom(InterceptedEventArgs e)
        {
            if (HostExitRoom != null)
            {
                var args = new HostExitRoomEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(HostExitRoom, args, e);
            }
        }

        public event EventHandler<HostNavigateRoomEventArgs> HostNavigateRoom;
        public void RaiseOnHostNavigateRoom(InterceptedEventArgs e)
        {
            if (HostNavigateRoom != null)
            {
                var args = new HostNavigateRoomEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(HostNavigateRoom, args, e);
            }
        }

        public event EventHandler<HostSayEventArgs> HostSay;
        public void RaiseOnHostSay(InterceptedEventArgs e)
        {
            if (HostSay != null)
            {
                var args = new HostSayEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(HostSay, args, e);
            }
        }

        public event EventHandler<HostShoutEventArgs> HostShout;
        public void RaiseOnHostShout(InterceptedEventArgs e)
        {
            if (HostShout != null)
            {
                var args = new HostShoutEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(HostShout, args, e);
            }
        }

        public event EventHandler<HostTradeEventArgs> HostTradePlayer;
        public void RaiseOnHostTradePlayer(InterceptedEventArgs e)
        {
            if (HostTradePlayer != null)
            {
                var args = new HostTradeEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(HostTradePlayer, args, e);
            }
        }

        public event EventHandler<HostWalkEventArgs> HostWalk;
        public void RaiseOnHostWalk(InterceptedEventArgs e)
        {
            if (HostWalk != null)
            {
                var args = new HostWalkEventArgs(e.Continuation, e.Step, e.Packet);
                OnGameEvent(HostWalk, args, e);
            }
        }
        #endregion

        private readonly Stack<HMessage> _outPrevious, _inPrevious;
        private readonly IDictionary<ushort, Action<InterceptedEventArgs>> _outLocked, _inLocked;
        private readonly IDictionary<ushort, EventHandler<InterceptedEventArgs>> _inAttaches, _outAttaches;

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
            DetectOutgoing = DetectIncoming = isDetecting;

            IncomingDetected = new Incoming();
            OutgoingDetected = new Outgoing();

            _inPrevious = new Stack<HMessage>();
            _outPrevious = new Stack<HMessage>();

            _inLocked = new Dictionary<ushort, Action<InterceptedEventArgs>>();
            _outLocked = new Dictionary<ushort, Action<InterceptedEventArgs>>();
            _inAttaches = new Dictionary<ushort, EventHandler<InterceptedEventArgs>>();
            _outAttaches = new Dictionary<ushort, EventHandler<InterceptedEventArgs>>();
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
        public void OutAttach(ushort header, EventHandler<InterceptedEventArgs> callback)
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
        public void InAttach(ushort header, EventHandler<InterceptedEventArgs> callback)
        {
            _inAttaches[header] = callback;
        }

        public void HandleOutgoing(InterceptedEventArgs e)
        {
            if (e.Packet == null || e.Packet.IsCorrupted)
                return;

            e.Packet.Position = 0;
            bool ignoreCurrent = false;
            try
            {
                if (_outAttaches.ContainsKey(e.Packet.Header))
                    _outAttaches[e.Packet.Header](this, e);

                if (DetectOutgoing && _outPrevious.Count > 0)
                {
                    e.Packet.Position = 0;
                    HMessage previous = _outPrevious.Pop();

                    if (!_outLocked.ContainsKey(e.Packet.Header) &&
                        !_outLocked.ContainsKey(previous.Header))
                        ignoreCurrent = HandleOutgoing(e.Packet, previous);
                    else ignoreCurrent = true;

                    if (ignoreCurrent)
                    {
                        e.Packet.Position = 0;
                        previous.Position = 0;

                        if (_outLocked.ContainsKey(e.Packet.Header))
                            _outLocked[e.Packet.Header](e);
                        else if (_outLocked.ContainsKey(previous.Header))
                        {
                            var args = new InterceptedEventArgs(null, 0, previous);
                            _outLocked[previous.Header](args);
                        }
                    }
                }
            }
            finally
            {
                e.Packet.Position = 0;

                if (!ignoreCurrent && DetectOutgoing)
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
            return false;
        }

        public void HandleIncoming(InterceptedEventArgs e)
        {
            if (e.Packet == null || e.Packet.IsCorrupted)
                return;

            e.Packet.Position = 0;
            bool ignoreCurrent = false;
            try
            {
                if (_inAttaches.ContainsKey(e.Packet.Header))
                    _inAttaches[e.Packet.Header](this, e);

                if (DetectIncoming && _inPrevious.Count > 0)
                {
                    e.Packet.Position = 0;
                    HMessage previous = _inPrevious.Pop();

                    if (!_inLocked.ContainsKey(e.Packet.Header) &&
                        !_inLocked.ContainsKey(previous.Header))
                        ignoreCurrent = HandleIncoming(e.Packet, previous);
                    else ignoreCurrent = true;

                    if (ignoreCurrent)
                    {
                        e.Packet.Position = 0;
                        previous.Position = 0;

                        if (_inLocked.ContainsKey(e.Packet.Header))
                            _inLocked[e.Packet.Header](e);
                        else if (_inLocked.ContainsKey(previous.Header))
                        {
                            var args = new InterceptedEventArgs(null, 0, previous);
                            _inLocked[previous.Header](args);
                        }
                    }
                }
            }
            finally
            {
                e.Packet.Position = 0;

                if (!ignoreCurrent && DetectIncoming)
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
            return false;
        }

        protected virtual void OnGameEvent<TEventArgs>(EventHandler<TEventArgs> gameEvent,
            TEventArgs gameArguments, InterceptedEventArgs toUpdate) where TEventArgs : InterceptedEventArgs
        {
            try { gameEvent?.Invoke(this, gameArguments); }
            catch
            {
                // TODO: Notify subscriber(s) that an exception has been thrown?
            }
            finally
            {
                toUpdate.IsBlocked = gameArguments.IsBlocked;
                toUpdate.Replacement = gameArguments.Replacement;
                toUpdate.WasContinued = gameArguments.WasContinued;
            }
        }

        private bool TryHandleHostExitRoom(HMessage current, HMessage previous)
        {
            if (previous.Length != 2 || current.ReadInteger(0) != -1)
                return false;

            OutgoingDetected.HostExitRoom = previous.Header;
            _outLocked[previous.Header] = RaiseOnHostExitRoom;
            return true;
        }
        private bool TryHandleHostRaiseSign(HMessage current, HMessage previous)
        {
            bool isHostRaiseSign = false;
            if (current.CanRead<string>(22) && current.ReadString(22) == "sign")
            {
                OutgoingDetected.RaiseSign = previous.Header;
                _outLocked[previous.Header] = RaiseOnHostRaiseSign;

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
                _inLocked[current.Header] = RaiseOnPlayerKickHost;
            }
            return isPlayerKickHost;
        }
        private bool TryHandleAvatarMenuClick(HMessage current, HMessage previous)
        {
            if (!previous.CanRead<string>(22)) return false;
            switch (previous.ReadString(22))
            {
                case "sit":
                case "stand":
                {
                    OutgoingDetected.UpdateStance = current.Header;
                    _outLocked[current.Header] = RaiseOnHostUpdateStance;
                    return true;
                }

                case "dance_stop":
                case "dance_start":
                {
                    OutgoingDetected.Dance = current.Header;
                    _outLocked[current.Header] = RaiseOnHostDance;
                    return true;
                }

                case "wave":
                case "idle":
                case "laugh":
                case "blow_kiss":
                {
                    OutgoingDetected.Gesture = current.Header;
                    _outLocked[current.Header] = RaiseOnHostGesture;
                    return true;
                }
            }
            return false;
        }
        private bool TryHandleHostNavigateRoom(HMessage current, HMessage previous)
        {
            if (previous.Length >= 12 &&
                current.CanRead<string>(0) &&
                current.ReadString() == "Navigation")
            {
                current.ReadString();
                if (current.ReadString() != "go.official") return false;

                if (previous.ReadInteger(0).ToString() == current.ReadString())
                {
                    OutgoingDetected.NavigateRoom = previous.Header;

                    _outLocked[previous.Header] = RaiseOnHostNavigateRoom;
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

                _inLocked.Clear();
                _outLocked.Clear();

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