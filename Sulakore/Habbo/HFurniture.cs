/*
    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    This file is part of the Sulakore library.
    Copyright (C) 2015 ArachisH
    
    This code is licensed under the GNU General Public License.
    See License.txt in the project root for license information.
*/

using System.Collections.Generic;

using Sulakore.Protocol;

namespace Sulakore.Habbo
{
    public class HFurniture
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }

        public HPoint Tile { get; set; }
        public HDirection Direction { get; set; }

        public HFurniture(int id, int typeId, int ownerId,
            string ownerName, HPoint tile, HDirection direction)
        {
            Id = id;
            TypeId = typeId;
            OwnerId = ownerId;
            OwnerName = ownerName;
            Tile = tile;
            Direction = direction;
        }

        public static IReadOnlyList<HFurniture> Parse(HMessage packet)
        {
            int ownersCount = packet.ReadInteger();
            var owners = new Dictionary<int, string>(ownersCount);

            for (int i = 0; i < ownersCount; i++)
            {
                int ownerId = packet.ReadInteger();
                string ownerName = packet.ReadString();

                owners.Add(ownerId, ownerName);
            }

            int furnitureCount = packet.ReadInteger();
            var furnitureList = new List<HFurniture>(furnitureCount);

            for (int i = 0; i < furnitureList.Capacity; i++)
            {
                int id = packet.ReadInteger();
                int typeId = packet.ReadInteger();

                int x = packet.ReadInteger();
                int y = packet.ReadInteger();
                var direction = (HDirection)packet.ReadInteger();
                double z = double.Parse(packet.ReadString());

                packet.ReadString();
                packet.ReadInteger();

                int categoryId = (packet.ReadInteger() & 0xFF);
                #region Switch Statement: categoryId
                switch (categoryId)
                {
                    case 0:
                    {
                        packet.ReadString();
                        break;
                    }
                    case 1:
                    {
                        int count = packet.ReadInteger();
                        for (int j = 0; j < count; j++)
                        {
                            packet.ReadString();
                            packet.ReadString();
                        }
                        break;
                    }
                    case 2:
                    {
                        int count = packet.ReadInteger();
                        for (int j = 0; j < count; j++)
                        {
                            packet.ReadString();
                        }
                        break;
                    }
                    case 3:
                    {
                        packet.ReadString();
                        packet.ReadInteger();
                        break;
                    }
                    case 5:
                    {
                        int count = packet.ReadInteger();
                        for (int j = 0; j < count; j++)
                        {
                            packet.ReadInteger();
                        }
                        break;
                    }
                    case 6:
                    {
                        packet.ReadString();
                        packet.ReadInteger();
                        packet.ReadInteger();

                        int count = packet.ReadInteger();
                        for (int j = 0; j < count; j++)
                        {
                            int subCount = packet.ReadInteger();
                            for (int k = 0; k < subCount; k++)
                            {
                                packet.ReadString();
                            }
                        }
                        break;
                    }
                    case 7:
                    {
                        packet.ReadString();
                        packet.ReadInteger();
                        packet.ReadInteger();
                        break;
                    }
                }
                #endregion

                packet.ReadInteger();
                packet.ReadInteger();

                int ownerId = packet.ReadInteger();
                if (typeId < 0) packet.ReadString();

                var furniture = new HFurniture(id, typeId, ownerId,
                    owners[ownerId], new HPoint(x, y, z), direction);

                furnitureList.Add(furniture);
            }

            return furnitureList;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(TypeId)}: {TypeId}, " +
                $"{nameof(OwnerId)}: {OwnerId}, {nameof(OwnerName)}: {OwnerName}";
        }
    }
}