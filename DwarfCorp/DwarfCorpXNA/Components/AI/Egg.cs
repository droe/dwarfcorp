// Creature.cs
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
using System.Runtime.Serialization;
using DwarfCorp.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Linq;

namespace DwarfCorp
{
    [JsonObject(IsReference = true)]
    public class Egg : GameComponent
    {
        public string Adult { get; set; }
        public DateTime Birthday { get; set; }
        public Body ParentBody { get; set; }
        public BoundingBox PositionConstrain { get; set; }
        public Egg()
        {
            
        }

        public Egg(string adult, ComponentManager manager, Vector3 position, BoundingBox positionConstraint) :
            base(manager)
        {
            PositionConstrain = positionConstraint;
            Adult = adult;
            Birthday = Manager.World.Time.CurrentDate + new TimeSpan(0, 12, 0, 0);

            if (ResourceLibrary.GetResourceByName(adult + " Egg") == null 
                || !EntityFactory.EnumerateEntityTypes().Contains(adult + " Egg Resource"))
            {
                Resource newEggResource =
                    new Resource(ResourceLibrary.GetResourceByName(ResourceType.Egg));
                newEggResource.Name = adult + " Egg";
                ResourceLibrary.Add(newEggResource);
            }

            ParentBody = EntityFactory.CreateEntity<Body>(adult + " Egg Resource", position);
            ParentBody.AddChild(this);
        }

        override public void Update(DwarfTime gameTime, ChunkManager chunks, Camera camera)
        {
            if (Manager.World.Time.CurrentDate > Birthday)
            {
                Hatch();
            }
        }

        public void Hatch()
        {
            var adult = EntityFactory.CreateEntity<Body>(Adult, ParentBody.Position);
            if (adult != null)
            {
                adult.GetRoot().GetComponent<CreatureAI>().PositionConstraint = PositionConstrain;
            }
            GetRoot().Die();
        }
    }
}
