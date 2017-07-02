// PlayState.cs
// 
//  Modified MIT License (MIT)
//  
//  Copyright (c) 2015 Completely Fair Games Ltd.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// The following content pieces are considered PROPRIETARY and may not be used
// in any derivative works, commercial or non commercial, without explicit 
// written permission from Completely Fair Games:
// 
// * Images (sprites, textures, etc.)
// * 3D Models
// * Sound Effects
// * Music
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using BloomPostprocess;
using DwarfCorp.Gui.Widgets;
using DwarfCorp.Tutorial;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using DwarfCorp.GameStates;

namespace DwarfCorp
{
    /// <summary>
    /// This is the main game state for actually playing the game.
    /// </summary>
    public partial class WorldManager
    {
       
        /// <summary>
        /// Generates a random set of dwarves in the given chunk.
        /// </summary>
        /// <param name="c">The chunk the dwarves belong to</param>
        public void CreateInitialDwarves(VoxelChunk c)
        {
            if (InitialEmbark == null)
            {
                InitialEmbark = Embarkment.DefaultEmbarkment;
            }
            Vector3 g = c.WorldToGrid(Camera.Position);
            // Find the height of the world at the camera
            float h = c.GetFilledVoxelGridHeightAt((int)g.X, ChunkHeight - 1, (int)g.Z);

            // This is done just to make sure the camera is in the correct place.
            Camera.UpdateBasisVectors();
            Camera.UpdateProjectionMatrix();
            Camera.UpdateViewMatrix();

            foreach (string ent in InitialEmbark.Party)
            {
                Vector3 dorfPos = new Vector3(Camera.Position.X + (float)MathFunctions.Random.NextDouble(), h + 10,
                    Camera.Position.Z + (float)MathFunctions.Random.NextDouble());
                Physics creat = (Physics)EntityFactory.CreateEntity<Physics>(ent, dorfPos);
                creat.Velocity = new Vector3(1, 0, 0);
            }

            Camera.Target = new Vector3(Camera.Position.X, h, Camera.Position.Z + 10);
            Camera.Position = new Vector3(Camera.Target.X, Camera.Target.Y + 20, Camera.Position.Z - 10);
        }

        /// <summary>
        /// Creates the balloon, the dwarves, and the initial balloon port.
        /// </summary>
        public void CreateInitialEmbarkment()
        {
            // If no file exists, we have to create the balloon and balloon port.
            if (!string.IsNullOrEmpty(ExistingFile)) return;

            VoxelChunk c = ChunkManager.ChunkData.GetVoxelChunkAtWorldLocation(Camera.Position);
            BalloonPort port = GenerateInitialBalloonPort(Master.Faction.RoomBuilder, ChunkManager,
                Camera.Position.X, Camera.Position.Z, 3);
            CreateInitialDwarves(c);
            PlayerFaction.Economy.CurrentMoney = InitialEmbark.Money;

            foreach (var res in InitialEmbark.Resources)
            {
                PlayerFaction.AddResources(new ResourceAmount(res.Key, res.Value));
            }
            var portBox = port.GetBoundingBox();
            EntityFactory.CreateBalloon(
                portBox.Center() + new Vector3(0, 100, 0),
                portBox.Center() + new Vector3(0, 10, 0), ComponentManager, Content,
                GraphicsDevice, new ShipmentOrder(0, null), Master.Faction);

            Camera.Target = portBox.Center();
            Camera.Position = Camera.Target + new Vector3(0, 15, -15);

            GenerateInitialObjects();
        }

        private void GenerateInitialObjects()
        {
            foreach (var chunk in ChunkManager.ChunkData.ChunkMap)
            {
                ChunkManager.ChunkGen.GenerateVegetation(chunk.Value, ComponentManager, Content, GraphicsDevice);
                ChunkManager.ChunkGen.GenerateFauna(chunk.Value, ComponentManager, Content, GraphicsDevice,
                    Factions);
            }
        }

        /// <summary>
        /// Creates a flat, wooden balloon port for the balloon to land on, and Dwarves to sit on.
        /// </summary>
        /// <param name="roomDes">The player's BuildRoom designator (so that we can create a balloon port)</param>
        /// <param name="chunkManager">The terrain handler</param>
        /// <param name="x">The position of the center of the balloon port</param>
        /// <param name="z">The position of the center of the balloon port</param>
        /// <param name="size">The size of the (square) balloon port in voxels on a side</param>
        public BalloonPort GenerateInitialBalloonPort(RoomBuilder roomDes, ChunkManager chunkManager, float x, float z,
            int size)
        {
            Vector3 pos = new Vector3(x, ChunkHeight - 1, z);

            // First, compute the maximum height of the terrain in a square window.
            int averageHeight = 0;
            int count = 0;
            for (int dx = -size; dx <= size; dx++)
            {
                for (int dz = -size; dz <= size; dz++)
                {
                    Vector3 worldPos = new Vector3(pos.X + dx, pos.Y, pos.Z + dz);
                    VoxelChunk chunk = chunkManager.ChunkData.GetVoxelChunkAtWorldLocation(worldPos);

                    if (chunk == null)
                    {
                        continue;
                    }

                    Vector3 gridPos = chunk.WorldToGrid(worldPos);
                    int h = chunk.GetFilledHeightOrWaterAt((int)gridPos.X + dx, (int)gridPos.Y, (int)gridPos.Z + dz);

                    if (h > 0)
                    {
                        averageHeight += h;
                        count++;
                    }
                }
            }

            averageHeight = (int)Math.Round(((float)averageHeight / (float)count));


            // Next, create the balloon port by deciding which voxels to fill.
            List<Voxel> balloonPortDesignations = new List<Voxel>();
            List<Voxel> treasuryDesignations = new List<Voxel>();
            for (int dx = -size; dx <= size; dx++)
            {
                for (int dz = -size; dz <= size; dz++)
                {
                    Vector3 worldPos = new Vector3(pos.X + dx, pos.Y, pos.Z + dz);
                    VoxelChunk chunk = chunkManager.ChunkData.GetVoxelChunkAtWorldLocation(worldPos);

                    if (chunk == null)
                    {
                        continue;
                    }

                    Vector3 gridPos = chunk.WorldToGrid(worldPos);
                    int h = chunk.GetFilledVoxelGridHeightAt((int)gridPos.X, (int)gridPos.Y, (int)gridPos.Z);

                    if (h == -1)
                    {
                        continue;
                    }

                    for (int y = averageHeight; y < h; y++)
                    {
                        Voxel v = chunk.MakeVoxel((int)gridPos.X, y, (int)gridPos.Z);
                        v.Type = VoxelLibrary.GetVoxelType(0);
                        chunk.Manager.ChunkData.Reveal(v);
                        chunk.Data.Water[v.Index].WaterLevel = 0;
                    }

                    if (averageHeight < h)
                    {
                        h = averageHeight;
                    }

                    bool isPosX = (dx == size && dz == 0);
                    bool isPosZ = (dz == size & dx == 0);
                    bool isNegX = (dx == -size && dz == 0);
                    bool isNegZ = (dz == -size && dz == 0);
                    bool isSide = (isPosX || isNegX || isPosZ || isNegZ);

                    Vector3 offset = Vector3.Zero;

                    if (isSide)
                    {
                        if (isPosX)
                        {
                            offset = Vector3.UnitX;
                        }
                        else if (isPosZ)
                        {
                            offset = Vector3.UnitZ;
                        }
                        else if (isNegX)
                        {
                            offset = -Vector3.UnitX;
                        }
                        else if (isNegZ)
                        {
                            offset = -Vector3.UnitZ;
                        }
                    }

                    // Fill from the top height down to the bottom.
                    for (int y = h - 1; y < averageHeight; y++)
                    {
                        Voxel v = chunk.MakeVoxel((int)gridPos.X, y, (int)gridPos.Z);
                        v.Type = VoxelLibrary.GetVoxelType("Scaffold");
                        chunk.Data.Water[v.Index].WaterLevel = 0;
                        v.Chunk = chunk;
                        v.Chunk.NotifyTotalRebuild(!v.IsInterior);

                        if (y == averageHeight - 1)
                        {
                            if (dz >= 0)
                            {
                                balloonPortDesignations.Add(v);
                            }
                            else
                            {
                                treasuryDesignations.Add(v);
                            }
                        }

                        if (isSide)
                        {
                            Voxel ladderVox = new Voxel();

                            Vector3 center = new Vector3(worldPos.X, y, worldPos.Z) + offset + Vector3.One * .5f;
                            if (chunk.Manager.ChunkData.GetVoxel(center, ref ladderVox) && ladderVox.IsEmpty)
                            {
                                EntityFactory.CreateEntity<Ladder>("Wooden Ladder", center);
                            }
                        }
                    }
                }
            }

            // Actually create the BuildRoom.
            BalloonPort toBuild = new BalloonPort(PlayerFaction, balloonPortDesignations, this);
            BuildRoomOrder buildDes = new BuildRoomOrder(toBuild, roomDes.Faction, this);
            buildDes.Build(true);
            roomDes.DesignatedRooms.Add(toBuild);

            // Also add a treasury
            Treasury treasury = new Treasury(PlayerFaction, treasuryDesignations, this);
            treasury.OnBuilt();
            roomDes.DesignatedRooms.Add(treasury);
            return toBuild;
        }

    }
}
