// Mushroom.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DwarfCorp;
using DwarfCorp.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace DwarfCorp
{
    public class Mushroom : Plant
    {
        public Mushroom()
        {

        }

        public Mushroom(ComponentManager Manager,
                        Vector3 position, 
                        string asset, 
                        ResourceType resource, 
                        int numRelease, bool selfIlluminate) :
            base(Manager, "Mushroom", position, MathFunctions.Rand(-0.1f, 0.1f), new Vector3(1.0f, 1.0f, 1.0f), 
                System.IO.Path.GetFileName(asset), 1.0f)
        {

            Inventory inventory = AddChild(new Inventory(Manager, "Inventory", BoundingBox.Extents(), LocalBoundingBoxOffset)) as Inventory;

            for (int i = 0; i < numRelease; i++)
            {
                inventory.Resources.Add(new Inventory.InventoryItem()
                {
                    MarkedForRestock = false,
                    MarkedForUse = false,
                    Resource = resource
                });
            }

            var particles = AddChild(new ParticleTrigger("Leaves", Manager, "LeafEmitter",
    Matrix.Identity, LocalBoundingBoxOffset, GetBoundingBox().Extents())
            {
                SoundToPlay = ContentPaths.Audio.Oscar.sfx_env_bush_harvest_1
            }) as ParticleTrigger;

            AddChild(new Health(Manager.World.ComponentManager, "HP", 30, 0.0f, 30));
            AddChild(new Flammable(Manager.World.ComponentManager, "Flames"));

            Tags.Add("Mushroom");
            Tags.Add("Vegetation");
            CollisionType = CollisionType.Static;
            PropogateTransforms();
        }
    }
}
