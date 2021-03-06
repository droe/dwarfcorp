// DigTool.cs
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
using Microsoft.Xna.Framework.Graphics;

namespace DwarfCorp
{
    public class CancelTasksTool : PlayerTool
    {
        public Gui.Widgets.CancelToolOptions Options;

        public override void OnBegin()
        {
            Player.World.Tutorial("cancel-tasks");
        }

        public override void OnEnd()
        {
            Player.VoxSelector.Clear();
        }

        public override void OnVoxelsSelected(List<VoxelHandle> refs, InputManager.MouseButton button)
        {
            if (Options.Voxels.CheckState)
                foreach (var r in refs)
                {
                    if (r.IsValid)
                    {
                        var designations = Player.Faction.Designations.EnumerateDesignations(r).ToList();
                        foreach (var des in designations)
                            if (des.Task != null)
                                Player.TaskManager.CancelTask(des.Task);
                    }
                }
        }

        public override void OnMouseOver(IEnumerable<Body> bodies)
        {
        }

        public override void Update(DwarfGame game, DwarfTime time)
        {
            if (Player.IsCameraRotationModeActive())
            {
                Player.VoxSelector.Enabled = false;
                Player.BodySelector.Enabled = false;
                Player.World.SetMouse(null);
                return;
            }

            Player.VoxSelector.Enabled = Options.Voxels.CheckState;

            if (Player.World.IsMouseOverGui)
                Player.World.SetMouse(Player.World.MousePointer);
            else
                Player.World.SetMouse(new Gui.MousePointer("mouse", 0, 0));
        
            Player.BodySelector.Enabled = Options.Entities.CheckState;
            Player.VoxSelector.SelectionType = VoxelSelectionType.SelectFilled;
        }

        public override void Render(DwarfGame game, DwarfTime time)
        {
        }

        public override void OnBodiesSelected(List<Body> bodies, InputManager.MouseButton button)
        {
            if (Options.Entities.CheckState)
                foreach (var body in bodies)
                {
                    foreach (var des in Player.Faction.Designations.EnumerateEntityDesignations(body).ToList())
                        if (des.Task != null)
                            Player.TaskManager.CancelTask(des.Task);
                }
        }

        public override void OnVoxelsDragged(List<VoxelHandle> voxels, InputManager.MouseButton button)
        {

        }
    }
}
