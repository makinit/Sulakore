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
        public void RaiseOnFurnitureLoad(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(FurnitureLoad, e);
        }

        public event EventHandler<FurnitureDropEventArgs> FurnitureDrop;
        public void RaiseOnFurnitureDrop(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(FurnitureDrop, e);
        }

        public event EventHandler<FurnitureMoveEventArgs> FurnitureMove;
        public void RaiseOnFurnitureMove(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(FurnitureMove, e);
        }

        public event EventHandler<EntityLoadEventArgs> EntityLoad;
        public void RaiseOnEntityLoad(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(EntityLoad, e);
        }

        public event EventHandler<EntityActionEventArgs> EntityAction;
        public void RaiseOnEntityAction(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(EntityAction, e);
        }

        public event EventHandler<PlayerKickHostEventArgs> PlayerKickHost;
        public void RaiseOnPlayerKickHost(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(PlayerKickHost, e);
        }

        public event EventHandler<PlayerUpdateEventArgs> PlayerUpdate;
        public void RaiseOnPlayerUpdate(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(PlayerUpdate, e);
        }

        public event EventHandler<PlayerDanceEventArgs> PlayerDance;
        public void RaiseOnPlayerDance(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(PlayerDance, e);
        }

        public event EventHandler<PlayerGestureEventArgs> PlayerGesture;
        public void RaiseOnPlayerGesture(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(PlayerGesture, e);
        }
        #endregion
        #region Outgoing Game Event Handlers
        public event EventHandler<HostBanPlayerEventArgs> HostBanPlayer;
        public void RaiseOnHostBanPlayer(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostBanPlayer, e);
        }

        public event EventHandler<HostUpdateClothesEventArgs> HostUpdateClothes;
        public void RaiseOnHostUpdateClothes(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostUpdateClothes, e);
        }

        public event EventHandler<HostUpdateMottoEventArgs> HostUpdateMotto;
        public void RaiseOnHostUpdateMotto(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostUpdateMotto, e);
        }

        public event EventHandler<HostUpdateStanceEventArgs> HostUpdateStance;
        public void RaiseOnHostUpdateStance(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostUpdateStance, e);
        }

        public event EventHandler<HostClickPlayerEventArgs> HostClickPlayer;
        public void RaiseOnHostClickPlayer(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostClickPlayer, e);
        }

        public event EventHandler<HostDanceEventArgs> HostDance;
        public void RaiseOnHostDance(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostDance, e);
        }

        public event EventHandler<HostGestureEventArgs> HostGesture;
        public void RaiseOnHostGesture(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostGesture, e);
        }

        public event EventHandler<HostKickPlayerEventArgs> HostKickPlayer;
        public void RaiseOnHostKickPlayer(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostKickPlayer, e);
        }

        public event EventHandler<HostMoveFurnitureEventArgs> HostMoveFurniture;
        public void RaiseOnHostMoveFurniture(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostMoveFurniture, e);
        }

        public event EventHandler<HostMutePlayerEventArgs> HostMutePlayer;
        public void RaiseOnHostMutePlayer(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostMutePlayer, e);
        }

        public event EventHandler<HostRaiseSignEventArgs> HostRaiseSign;
        public void RaiseOnHostRaiseSign(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostRaiseSign, e);
        }

        public event EventHandler<HostExitRoomEventArgs> HostExitRoom;
        public void RaiseOnHostExitRoom(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostExitRoom, e);
        }

        public event EventHandler<HostNavigateRoomEventArgs> HostNavigateRoom;
        public void RaiseOnHostNavigateRoom(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostNavigateRoom, e);
        }

        public event EventHandler<HostSayEventArgs> HostSay;
        public void RaiseOnHostSay(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostSay, e);
        }

        public event EventHandler<HostShoutEventArgs> HostShout;
        public void RaiseOnHostShout(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostShout, e);
        }

        public event EventHandler<HostTradeEventArgs> HostTradePlayer;
        public void RaiseOnHostTradePlayer(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostTradePlayer, e);
        }

        public event EventHandler<HostWalkEventArgs> HostWalk;
        public void RaiseOnHostWalk(DataInterceptedEventArgs e)
        {
            RaiseOnGameEvent(HostWalk, e);
        }
        #endregion

        private readonly Stack<HMessage> _outPrevious, _inPrevious;
        private readonly List<Func<HMessage, HMessage, bool>> _outHandlers, _inHandlers;
        private readonly Dictionary<ushort, Action<DataInterceptedEventArgs>> _inAttaches, _outAttaches;

        protected Dictionary<ushort, Action<DataInterceptedEventArgs>> InDetected { get; }
        protected Dictionary<ushort, Action<DataInterceptedEventArgs>> OutDetected { get; }

        /// <summary>
        /// Gets or sets a value that determines whether to begin identifying outgoing events.
        /// </summary>
        public bool DetectOutgoing { get; set; }
        /// <summary>
        /// Gets or sets a value that determines whether to begin identifying incoming events.
        /// </summary>
        public bool DetectIncoming { get; set; }
        public bool IsDisposed { get; private set; }

        public Incoming InHeaders { get; }
        public Outgoing OutHeaders { get; }

        public HTriggers()
            : this(false)
        { }
        public HTriggers(bool isDetecting)
        {
            DetectOutgoing =
                DetectIncoming = isDetecting;

            InHeaders = new Incoming();
            OutHeaders = new Outgoing();

            InDetected = new Dictionary<ushort, Action<DataInterceptedEventArgs>>();
            OutDetected = new Dictionary<ushort, Action<DataInterceptedEventArgs>>();

            _inPrevious = new Stack<HMessage>();
            _outPrevious = new Stack<HMessage>();
            _inAttaches = new Dictionary<ushort, Action<DataInterceptedEventArgs>>();
            _outAttaches = new Dictionary<ushort, Action<DataInterceptedEventArgs>>();

            _inHandlers = new List<Func<HMessage, HMessage, bool>>();
            _inHandlers.Add(TryHandlePlayerKickHost);
            _inHandlers.Add(TryHandleEntityAction);
            _inHandlers.Add(TryHandleFurnitureMove);

            _outHandlers = new List<Func<HMessage, HMessage, bool>>();
            _outHandlers.Add(TryHandleAvatarMenuClick);
            _outHandlers.Add(TryHandleHostExitRoom);
            _outHandlers.Add(TryHandleHostRaiseSign);
            _outHandlers.Add(TryHandleHostNavigateRoom);
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
        public void OutAttach(ushort header, Action<DataInterceptedEventArgs> callback)
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
        public void InAttach(ushort header, Action<DataInterceptedEventArgs> callback)
        {
            _inAttaches[header] = callback;
        }

        public void HandleOutgoing(DataInterceptedEventArgs e)
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
                    HMessage previous = (_outPrevious.Count > 0 ?
                        _outPrevious.Pop() : e.Packet);

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
                        else if (!previousDetected &&
                            OutDetected.ContainsKey(previous.Header))
                        {
                            var args = new DataInterceptedEventArgs(previous, e.Step, null);
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
            return HandleMessages(current, previous, _outHandlers, OutDetected);
        }

        public void HandleIncoming(DataInterceptedEventArgs e)
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
                        else if (!previousDetected &&
                            InDetected.ContainsKey(previous.Header))
                        {
                            var args = new DataInterceptedEventArgs(previous, e.Step, null);
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
            return HandleMessages(current, previous, _inHandlers, InDetected);
        }

        private bool TryHandleHostExitRoom(HMessage current, HMessage previous)
        {
            if (previous.Length != 2 ||
                current.ReadInteger(0) != -1)
            {
                return false;
            }

            OutHeaders.AvatarEffectSelected = previous.Header;
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
                InHeaders.UserUpdate = current.Header;
                InDetected[current.Header] = RaiseOnEntityAction;
                return true;
            }
            else return false;
        }
        private bool TryHandleHostRaiseSign(HMessage current, HMessage previous)
        {
            if (previous.Length != 6) return false;
            if (previous.ReadInteger(0) > 18) return false;

            if (!current.CanReadString(22)) return false;
            if (current.ReadString(22) != "sign") return false;

            OutHeaders.ApplySign = previous.Header;
            OutDetected[previous.Header] = RaiseOnHostRaiseSign;
            return true;
        }
        private bool TryHandleFurnitureMove(HMessage current, HMessage previous)
        {
            return false;
        }
        private bool TryHandlePlayerKickHost(HMessage current, HMessage previous)
        {
            bool isPlayerKickHost = (current.ReadInteger(0) == 4008);
            if (isPlayerKickHost)
            {
                InHeaders.GenericError = current.Header;
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
                    OutHeaders.Sit = current.Header;
                    OutDetected[current.Header] = RaiseOnHostUpdateStance;
                    return true;
                }

                case "dance_stop":
                case "dance_start":
                {
                    OutHeaders.Dance = current.Header;
                    OutDetected[current.Header] = RaiseOnHostDance;
                    return true;
                }

                case "wave":
                case "idle":
                case "laugh":
                case "blow_kiss":
                {
                    OutHeaders.Action = current.Header;
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
                    OutHeaders.OpenFlatConnection = previous.Header;
                    OutDetected[previous.Header] = RaiseOnHostNavigateRoom;
                    return true;
                }
            }
            return false;
        }

        protected virtual void RaiseOnGameEvent<T>(EventHandler<T> handler, DataInterceptedEventArgs e) where T : DataInterceptedEventArgs
        {
            if (handler != null)
            {
                var args = (T)Activator.CreateInstance(typeof(T),
                    e._continuation, e.Step, e.Packet);

                OnGameEvent(handler, args, e);
            }
        }
        protected virtual void OnGameEvent<T>(EventHandler<T> handler, T arguments, DataInterceptedEventArgs e) where T : DataInterceptedEventArgs
        {
            try { handler?.Invoke(this, arguments); }
            catch { /* Swallow all exceptions. */ }
            finally
            {
                e.Packet = arguments.Packet;
                e.IsBlocked = arguments.IsBlocked;
                e.Continuations = arguments.Continuations;
            }
        }

        protected virtual bool HandleMessages(
            HMessage current, HMessage previous,
            List<Func<HMessage, HMessage, bool>> handlers,
            Dictionary<ushort, Action<DataInterceptedEventArgs>> detected)
        {
            Func<HMessage, HMessage, bool>[] handlersArray = handlers.ToArray();
            foreach (Func<HMessage, HMessage, bool> handler in handlersArray)
            {
                if (handler(current, previous))
                {
                    handlers.Remove(handler);
                    break;
                }
            }

            return detected.ContainsKey(current.Header) ||
                detected.ContainsKey(previous.Header);
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