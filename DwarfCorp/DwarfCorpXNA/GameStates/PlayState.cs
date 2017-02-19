using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DwarfCorp.GameStates
{
    public class PlayState : GameState
    {
        private bool IsShuttingDown { get; set; }
        private bool QuitOnNextUpdate { get; set; }
        public bool ShouldReset { get; set; }

        public WorldManager World { get; set; }

        public GameMaster Master
        {
            get { return World.Master; }
            set { World.Master = value; }
        }        public bool Paused
        {
            get { return World.Paused; }
            set { World.Paused = value; }
        }

        // ------GUI--------
        // Draws and manages the user interface 
        public static DwarfGUI GUI = null;

        private Gum.Widget MoneyLabel;
        private Gum.Widget StockLabel;
        private Gum.Widget LevelLabel;
        private NewGui.ToolTray.Tray BottomRightTray;
        private Gum.Widget TimeLabel;
        private Gum.Widget PausePanel;
        private NewGui.MinimapFrame MinimapFrame;
        private NewGui.MinimapRenderer MinimapRenderer;
        private NewGui.GameSpeedControls GameSpeedControls;
        private Gum.Widget ResourcePanel;
        private NewGui.InfoTray InfoTray;

        private class ToolbarItem
        {
            public NewGui.FramedIcon Icon;
            public Func<bool> Available;

            public ToolbarItem(Gum.Widget Icon, Func<bool> Available)
            {
                System.Diagnostics.Debug.Assert(Icon is NewGui.FramedIcon);
                this.Icon = Icon as NewGui.FramedIcon;
                this.Available = Available;
            }
        }

        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private Dictionary<GameMaster.ToolMode, NewGui.FramedIcon> ToolHiliteItems = new Dictionary<GameMaster.ToolMode, NewGui.FramedIcon>();

        private void AddToolSelectIcon(GameMaster.ToolMode Mode, Gum.Widget Icon)
        {
            if (!ToolHiliteItems.ContainsKey(Mode))
                ToolHiliteItems.Add(Mode, Icon as NewGui.FramedIcon);
        }

        private void ChangeTool(GameMaster.ToolMode Mode)
        {
            Master.ChangeTool(Mode);
            foreach (var icon in ToolHiliteItems)
                icon.Value.Hilite = icon.Key == Mode;
        }

        // Provides event-based keyboard and mouse input.
        public static InputManager Input;// = new InputManager();

        private Gum.Root GuiRoot;

        /// <summary>
        /// Creates a new play state
        /// </summary>
        /// <param name="game">The program currently running</param>
        /// <param name="stateManager">The game state manager this state will belong to</param>
        public PlayState(DwarfGame game, GameStateManager stateManager, WorldManager world) :
            base(game, "PlayState", stateManager)
        {
            World = world;
            IsShuttingDown = false;
            QuitOnNextUpdate = false;
            ShouldReset = true;
            Paused = false;
            RenderUnderneath = true;
            EnableScreensaver = false;
            IsInitialized = false;
        }

        private void World_OnLoseEvent()
        {
            //Paused = true;
            //StateManager.PushState("LoseState");
        }

        /// <summary>
        /// Called when the PlayState is entered from the state manager.
        /// </summary>
        public override void OnEnter()
        {
            // Just toss out any pending input.
            DwarfGame.GumInputMapper.GetInputQueue();

            if (!IsInitialized)
            {
                // Setup new gui. Double rendering the mouse?
                GuiRoot = new Gum.Root(new Point(640, 480), DwarfGame.GumSkin);
                GuiRoot.MousePointer = new Gum.MousePointer("mouse", 4, 0);
                World.NewGui = GuiRoot;

                // Setup input event handlers. All of the actions should already be established - just 
                // need handlers.
                DwarfGame.GumInput.ClearAllHandlers();

                World.ShowInfo += (text) =>
                    {
                        InfoTray.AddMessage(text);
                    };

                World.ShowTooltip += (text) =>
                    {
                        GuiRoot.ShowTooltip(GuiRoot.MousePosition, text);
                    };

                World.SetMouse += (mouse) =>
                    {
                        GuiRoot.MousePointer = mouse;
                    };

                World.ShowToolPopup += text => GuiRoot.ShowTooltip(new Point(GuiRoot.MousePosition.X, GuiRoot.MousePosition.Y - 30), text, 3.0f);

                World.gameState = this;
                World.OnLoseEvent += World_OnLoseEvent;
                CreateGUIComponents();
                IsInitialized = true;
            }

            World.Unpause();
            base.OnEnter();
        }

        /// <summary>
        /// Called when the PlayState is exited and another state (such as the main menu) is loaded.
        /// </summary>
        public override void OnExit()
        {
            World.Pause();
            base.OnExit();
        }

        /// <summary>
        /// Called every frame
        /// </summary>
        /// <param name="gameTime">The current time</param>
        public override void Update(DwarfTime gameTime)
        {
            //WorldManager.GUI.IsMouseVisible = false;

            if (GuiRoot.ResolutionChanged())
            {
                GuiRoot.ResizeVirtualScreen(new Point(640, 480));
                GuiRoot.ResetGui();
                CreateGUIComponents();

                if (PausePanel != null)
                {
                    PausePanel = null;
                    OpenPauseMenu();
                }
            }

            // If this playstate is not supposed to be running,
            // just exit.
            if (!Game.IsActive || !IsActiveState || IsShuttingDown)
            {
                return;
            }

            if (QuitOnNextUpdate)
            {
                QuitGame();
                IsShuttingDown = true;
                QuitOnNextUpdate = false;
                return;
            }

            // Needs to run before old input so tools work
            // Update new input system.
            DwarfGame.GumInput.FireActions(GuiRoot, (@event, args) =>
            {
                // Let old input handle mouse interaction for now. Will eventually need to be replaced.

                if (@event == Gum.InputEvents.MouseDown) // Mouse down but not handled by GUI? Collapse menu.
                    BottomRightTray.CollapseTrays();
            });

            World.Update(gameTime);
            GUI.Update(gameTime);
            Input.Update();

            #region Update time label
            TimeLabel.Text = String.Format("{0} {1}",
                World.Time.CurrentDate.ToShortDateString(),
                World.Time.CurrentDate.ToShortTimeString());
            TimeLabel.Invalidate();
            #endregion

            #region Update top left panel
            MoneyLabel.Text = Master.Faction.Economy.CurrentMoney.ToString();
            MoneyLabel.Invalidate();

            StockLabel.Text = Master.Faction.Economy.Company.StockPrice.ToString();
            StockLabel.Invalidate();

            LevelLabel.Text = String.Format("{0}/{1}",
                World.ChunkManager.ChunkData.MaxViewingLevel,
                World.ChunkHeight);
            LevelLabel.Invalidate();
            #endregion

            #region Update toolbar tray
            if (Master.SelectedMinions.Count == 0)
            {
                if (Master.CurrentToolMode != GameMaster.ToolMode.God)
                    Master.CurrentToolMode = GameMaster.ToolMode.SelectUnits;
            }

            foreach (var tool in ToolbarItems)
                tool.Icon.Enabled = tool.Available();
            
            #endregion

            #region Update resource panel
            ResourcePanel.Clear();
            foreach (var resource in Master.Faction.ListResources().Where(p => p.Value.NumResources > 0))
            {
                var resourceTemplate = ResourceLibrary.GetResourceByName(resource.Key);

                var row = ResourcePanel.AddChild(new Gum.Widget
                    {
                        MinimumSize = new Point(0, 32),
                        AutoLayout = global::Gum.AutoLayout.DockTop
                    });

                row.AddChild(new Gum.Widget
                    {
                        Background = new Gum.TileReference("resources", resourceTemplate.NewGuiSprite),
                        MinimumSize = new Point(32, 32),
                        AutoLayout = global::Gum.AutoLayout.DockLeft,
                        Tooltip = string.Format("{0} - {1}",
                            resourceTemplate.ResourceName,
                            resourceTemplate.Description)
                    });

                row.AddChild(new Gum.Widget
                {
                    Text = resource.Value.NumResources.ToString(),
                    MinimumSize = new Point(32, 32),
                    AutoLayout = global::Gum.AutoLayout.DockLeft,
                    Tooltip = string.Format("{0} - {1}",
                        resourceTemplate.ResourceName,
                        resourceTemplate.Description),
                    Font = "outline-font",
                    TextVerticalAlign = global::Gum.VerticalAlign.Center,
                    TextColor = new Vector4(1,1,1,1)
                });
            }
            ResourcePanel.Layout();
            #endregion

            GameSpeedControls.CurrentSpeed = (int)DwarfTime.LastTime.Speed;

            // Really just handles mouse pointer animation.
            GuiRoot.Update(gameTime.ToGameTime());
        }

        /// <summary>
        /// Called when a frame is to be drawn to the screen
        /// </summary>
        /// <param name="gameTime">The current time</param>
        public override void Render(DwarfTime gameTime)
        {
            EnableScreensaver = !World.ShowingWorld;

            MinimapRenderer.PreRender(gameTime, DwarfGame.SpriteBatch);

            if (World.ShowingWorld)
            {
                GUI.PreRender(gameTime, DwarfGame.SpriteBatch);
                World.Render(gameTime);

                if (!MinimapFrame.Hidden)
                    MinimapRenderer.Render(new Rectangle(0, GuiRoot.VirtualScreen.Bottom - 192, 192, 192), GuiRoot);
                GuiRoot.Draw();

                // SpriteBatch Begin and End must be called again. Hopefully we can factor this out with the new gui
                RasterizerState rasterizerState = new RasterizerState()
                {
                    ScissorTestEnable = true
                };
                DwarfGame.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp,
                    null, rasterizerState);
                GUI.Render(gameTime, DwarfGame.SpriteBatch, Vector2.Zero);
                GUI.PostRender(gameTime);
                DwarfGame.SpriteBatch.End();
                //WorldManager.SelectionBuffer.DebugDraw(0, 0);
            }        

            base.Render(gameTime);
        }

        /// <summary>
        /// If the game is not loaded yet, just draws a loading message centered
        /// </summary>
        /// <param name="gameTime">The current time</param>
        public override void RenderUnitialized(DwarfTime gameTime)
        {
            EnableScreensaver = true;
            World.Render(gameTime);
            base.RenderUnitialized(gameTime);
        }

        /// <summary>
        /// Creates all of the sub-components of the GUI in for the PlayState (buttons, etc.)
        /// </summary>
        public void CreateGUIComponents()
        {
            GUI.RootComponent.ClearChildren();

            GUI.RootComponent.AddChild(Master.Debugger.MainPanel);

            #region Setup company information section
            GuiRoot.RootItem.AddChild(new NewGui.CompanyLogo
                {
                    Rect = new Rectangle(8,8,32,32),
                    MinimumSize = new Point(32, 32),
                    MaximumSize = new Point(32, 32),
                    AutoLayout = Gum.AutoLayout.None,
                    CompanyInformation = World.PlayerCompany.Information
                });

            GuiRoot.RootItem.AddChild(new Gum.Widget
                {
                    Rect = new Rectangle(48,8,256,20),
                    Text = World.PlayerCompany.Information.Name,
                    AutoLayout = Gum.AutoLayout.None,
                    Font = "outline-font",
                    TextColor = new Vector4(1,1,1,1)
                });

            var infoPanel = GuiRoot.RootItem.AddChild(new Gum.Widget
            {
                Rect = new Rectangle(0, 40, 128, 102),
                AutoLayout = global::Gum.AutoLayout.None
            });

            var moneyRow = infoPanel.AddChild(new Gum.Widget
            {
                MinimumSize = new Point(0, 34),
                AutoLayout = global::Gum.AutoLayout.DockTop
            });

            moneyRow.AddChild(new Gum.Widget
            {
                Background = new Gum.TileReference("resources", 40),
                MinimumSize = new Point(32, 32),
                MaximumSize = new Point(32, 32),
                AutoLayout = global::Gum.AutoLayout.DockLeft
            });

            MoneyLabel = moneyRow.AddChild(new Gum.Widget
                {
                    Rect = new Rectangle(48, 32, 128, 20),
                    AutoLayout = global::Gum.AutoLayout.DockFill,
                    Font = "outline-font",
                    TextVerticalAlign = global::Gum.VerticalAlign.Center,
                    TextColor = new Vector4(1,1,1,1)
                });

            var stockRow = infoPanel.AddChild(new Gum.Widget
            {
                MinimumSize = new Point(0, 34),
                AutoLayout = global::Gum.AutoLayout.DockTop
            });

            stockRow.AddChild(new Gum.Widget
            {
                Background = new Gum.TileReference("resources", 41),
                MinimumSize = new Point(32, 32),
                MaximumSize = new Point(32, 32),
                AutoLayout = global::Gum.AutoLayout.DockLeft
            });

            StockLabel = stockRow.AddChild(new Gum.Widget
            {
                Rect = new Rectangle(48, 32, 128, 20),
                AutoLayout = global::Gum.AutoLayout.DockFill,
                Font = "outline-font",
                TextVerticalAlign = global::Gum.VerticalAlign.Center,
                TextColor = new Vector4(1, 1, 1, 1)
            });

            var levelRow = infoPanel.AddChild(new Gum.Widget
            {
                MinimumSize = new Point(0, 34),
                AutoLayout = global::Gum.AutoLayout.DockTop
            });

            levelRow.AddChild(new Gum.Widget
            {
                Background = new Gum.TileReference("resources", 42),
                MinimumSize = new Point(32, 32),
                MaximumSize = new Point(32, 32),
                AutoLayout = global::Gum.AutoLayout.DockLeft
            });

            levelRow.AddChild(new Gum.Widget
            {
                Background = new Gum.TileReference("round-buttons", 7),
                MinimumSize = new Point(16,16),
                MaximumSize = new Point(16,16),
                AutoLayout = global::Gum.AutoLayout.FloatLeft,
                OnLayout = (sender) => sender.Rect.X += 18,
                OnClick = (sender, args) =>
                {
                    World.ChunkManager.ChunkData.SetMaxViewingLevel(
                        World.ChunkManager.ChunkData.MaxViewingLevel - 1,
                        ChunkManager.SliceMode.Y);
                }
            });

            levelRow.AddChild(new Gum.Widget
            {
                Background = new Gum.TileReference("round-buttons", 3),
                MinimumSize = new Point(16, 16),
                MaximumSize = new Point(16, 16),
                AutoLayout = global::Gum.AutoLayout.FloatLeft,
                OnClick = (sender, args) =>
                {
                    World.ChunkManager.ChunkData.SetMaxViewingLevel(
                        World.ChunkManager.ChunkData.MaxViewingLevel + 1,
                        ChunkManager.SliceMode.Y);
                }
            });

            LevelLabel = levelRow.AddChild(new Gum.Widget
            {
                Rect = new Rectangle(48, 32, 128, 20),
                AutoLayout = global::Gum.AutoLayout.DockFill,
                Font = "outline-font",
                OnLayout = (sender) => sender.Rect.X += 36,
                TextVerticalAlign = global::Gum.VerticalAlign.Center,
                TextColor = new Vector4(1, 1, 1, 1)
            });
            #endregion

            ResourcePanel = GuiRoot.RootItem.AddChild(new Gum.Widget
                {
                    Transparent = true,
                    Rect = new Rectangle(0, 104, 128, 128),
                    AutoLayout = global::Gum.AutoLayout.None,
                    OnLayout = sender => sender.Rect.Y = infoPanel.Rect.Bottom + 4
                });

            #region Setup time display
            TimeLabel = GuiRoot.RootItem.AddChild(new Gum.Widget
                {
                    AutoLayout = global::Gum.AutoLayout.FloatTop,
                    TextHorizontalAlign = global::Gum.HorizontalAlign.Center,
                    MinimumSize = new Point(128, 20),
                    Font = "outline-font",
                    TextColor = new Vector4(1,1,1,1)
                });
            #endregion

            #region Minimap

            // Little hack here - Normally this button his hidden by the minimap. Hide the minimap and it 
            // becomes visible! 
            // Todo: Doh, doesn't actually work.
            GuiRoot.RootItem.AddChild(new Gum.Widget
                {
                    AutoLayout = global::Gum.AutoLayout.FloatBottomLeft,
                    Background = new Gum.TileReference("round-buttons", 3),
                    MinimumSize = new Point(16,16),
                    MaximumSize = new Point(16,16),
                    OnClick = (sender, args) =>
                        {
                            MinimapFrame.Hidden = false;
                            MinimapFrame.Invalidate();
                        }
                });

            MinimapRenderer = new NewGui.MinimapRenderer(192, 192, World,
                TextureManager.GetTexture(ContentPaths.Terrain.terrain_colormap));

            MinimapFrame = GuiRoot.RootItem.AddChild(new NewGui.MinimapFrame
            {
                AutoLayout = global::Gum.AutoLayout.FloatBottomLeft,
                Renderer = MinimapRenderer
            }) as NewGui.MinimapFrame;
            #endregion

            #region Setup top right tray
            var topRightTray = GuiRoot.RootItem.AddChild(new NewGui.IconTray
                {
                    Corners = global::Gum.Scale9Corners.Left | global::Gum.Scale9Corners.Bottom,
                    AutoLayout = global::Gum.AutoLayout.FloatTopRight,
                    SizeToGrid = new Point(2,1),
                    ItemSource = new Gum.Widget[] 
                        { 
                            new NewGui.FramedIcon
                            {
                                Icon = new Gum.TileReference("tool-icons", 10),
                                OnClick = (sender, args) => StateManager.PushState(new EconomyState(Game, StateManager, World))
                        },
                        new NewGui.FramedIcon
                        {
                            Icon = new Gum.TileReference("tool-icons", 12),
                            OnClick = (sender, args) => { OpenPauseMenu(); }
                        }
                        }
                });
            #endregion

            #region Setup game speed controls
            GameSpeedControls = GuiRoot.RootItem.AddChild(new NewGui.GameSpeedControls
            {
                AutoLayout = Gum.AutoLayout.FloatBottomRight,
                OnLayout = (sender) =>
                {
                    sender.Rect.X -= 8;
                    sender.Rect.Y -= 8;
                },
                OnSpeedChanged = (sender, speed) =>
                {
                    DwarfTime.LastTime.Speed = (float)speed;
                    Paused = speed == 0;
                }
            }) as NewGui.GameSpeedControls;

            #endregion

            InputManager.KeyReleasedCallback += InputManager_KeyReleasedCallback;

            World.OnAnnouncement = (title, message, clickAction) =>
                {
                    var announcer = GuiRoot.RootItem.AddChild(new NewGui.AnnouncementPopup
                    {
                        Text = title,
                        Message = message,
                        OnClick = (sender, args) => { if (clickAction != null) clickAction(); },
                        Rect = new Rectangle(
                            BottomRightTray.Rect.X,
                            GameSpeedControls.Rect.Y - 128,
                            BottomRightTray.Rect.Width,
                            128)
                    });

                    // Make the announcer stay behind other controls.
                    GuiRoot.RootItem.SendToBack(announcer);
                };

            InfoTray = GuiRoot.RootItem.AddChild(new NewGui.InfoTray
            {
                OnLayout = (sender) =>
                {
                    sender.Rect = new Rectangle(MinimapFrame.Rect.Right,
                            MinimapFrame.Rect.Top, 256, MinimapFrame.Rect.Height);
                },
                Transparent = true
            }) as NewGui.InfoTray;

            #region Setup bottom right tray

            var roomIcons = GuiRoot.GetTileSheet("rooms") as Gum.TileSheet;
            var craftIcons = GuiRoot.GetTileSheet("crafts") as Gum.TileSheet;

            BottomRightTray = GuiRoot.RootItem.AddChild(new NewGui.ToolTray.Tray
            {
                AutoLayout = global::Gum.AutoLayout.FloatBottom,
                IsRootTray = true,
                ItemSource = new Gum.Widget[]
                {
                    #region Select Tool
                    new NewGui.ToolTray.Icon
                    {
                        Icon = new Gum.TileReference("tool-icons", 5),
                        OnClick = (sender, args) => ChangeTool(GameMaster.ToolMode.SelectUnits),
                        OnConstruct = (sender) =>
                        {
                            ToolbarItems.Add(new ToolbarItem(sender, () => true));
                            AddToolSelectIcon(GameMaster.ToolMode.SelectUnits, sender);
                        }
                    },
                    #endregion

                    #region Build tools
                    new NewGui.ToolTray.Icon
                    {
                        Icon = new Gum.TileReference("tool-icons", 2),
                        KeepChildVisible = true,
                        OnConstruct = (sender) =>
                        {
                            ToolbarItems.Add(new ToolbarItem(sender, () =>
                            Master.Faction.SelectedMinions.Any(minion =>
                                minion.Stats.CurrentClass.HasAction(GameMaster.ToolMode.Build))));
                            AddToolSelectIcon(GameMaster.ToolMode.Build, sender);
                        },
                        ExpansionChild = new NewGui.ToolTray.Tray
                        {
                            ItemSource = new NewGui.ToolTray.Icon[]
                            {

                    #region Build room tool
                    new NewGui.ToolTray.Icon
                    {
                        Icon = new Gum.TileReference("tool-icons", 2),
                        KeepChildVisible = true,
                        ExpansionChild = new NewGui.ToolTray.Tray
                        {
                            ItemSource = RoomLibrary.GetRoomTypes().Select(name => RoomLibrary.GetData(name))
                                .Select(data => new NewGui.ToolTray.Icon
                                {
                                    Icon = data.NewIcon,
                                    ExpansionChild = new NewGui.BuildRoomInfo
                                    {
                                        Data = data,
                                        Rect = new Rectangle(0,0,256,128)
                                    },
                                    OnClick = (sender, args) =>
                                    {
                                        Master.Faction.RoomBuilder.CurrentRoomData = data;
                                        Master.VoxSelector.SelectionType = VoxelSelectionType.SelectFilled;
                                        Master.Faction.WallBuilder.CurrentVoxelType = null;
                                        Master.Faction.CraftBuilder.IsEnabled = false;
                                        ChangeTool(GameMaster.ToolMode.Build);
                                        World.ShowToolPopup("Click and drag to build " + data.Name);
                                    }
                                    //Todo: Add to toolbar item list & disable if not enough resources?
                                })
                        }
                    },
                    #endregion

                    #region Build wall tool
                    new NewGui.ToolTray.Icon
                    {
                        Icon = new Gum.TileReference("tool-icons", 2),
                        KeepChildVisible = true,
                        ExpansionChild = new NewGui.ToolTray.Tray
                        {
                            ItemSource = VoxelLibrary.GetTypes().Where(voxel => voxel.IsBuildable)
                                .Select(data => new NewGui.ToolTray.Icon
                                {
                                    // Todo: Need icons for wall types.
                                    Icon = new Gum.TileReference("rooms", 0),
                                    ExpansionChild = new NewGui.BuildWallInfo
                                    {
                                        Data = data,
                                        Rect = new Rectangle(0,0,256,128)
                                    },
                                    OnClick = (sender, args) =>
                                    {
                                        Master.Faction.RoomBuilder.CurrentRoomData = null;
                                        Master.VoxSelector.SelectionType = VoxelSelectionType.SelectEmpty;
                                        Master.Faction.WallBuilder.CurrentVoxelType = data;
                                        Master.Faction.CraftBuilder.IsEnabled = false;
                                        ChangeTool(GameMaster.ToolMode.Build);
                                        World.ShowToolPopup("Click and drag to build " + data.Name + " wall.");
                                    }
                                    //Todo: Add to toolbar item list & disable if not enough resources?
                                })
                        }
                    },
                    #endregion

                    #region Build craft
                    new NewGui.ToolTray.Icon
                    {
                        Icon = new Gum.TileReference("tool-icons", 2),
                        KeepChildVisible = true,
                        ExpansionChild = new NewGui.ToolTray.Tray
                        {
                            ItemSource = CraftLibrary.CraftItems.Values.Where(item => item.Type == CraftItem.CraftType.Object)
                                .Select(data => new NewGui.ToolTray.Icon
                                {
                                    // Todo: Need to get all the icons into one sheet.
                                    Icon = data.Icon,
                                    KeepChildVisible = true, // So the player can interact with the popup.
                                    ExpansionChild = new NewGui.BuildCraftInfo
                                    {
                                        Data = data,
                                        Rect = new Rectangle(0,0,256,128),
                                        Master = Master,
                                        World = World
                                    },
                                    OnClick = (sender, args) =>
                                    {
                                        var buildInfo = (sender as NewGui.ToolTray.Icon).ExpansionChild as NewGui.BuildCraftInfo;
                                        data.SelectedResources = buildInfo.GetSelectedResources();

                                        Master.Faction.RoomBuilder.CurrentRoomData = null;
                                        Master.VoxSelector.SelectionType = VoxelSelectionType.SelectEmpty;
                                        Master.Faction.WallBuilder.CurrentVoxelType = null;
                                        Master.Faction.CraftBuilder.IsEnabled = true;
                                        Master.Faction.CraftBuilder.CurrentCraftType = data;
                                        ChangeTool(GameMaster.ToolMode.Build);
                                        World.ShowToolPopup("Click and drag to build " + data.Name);
                                    }
                                    //Todo: Add to toolbar item list & disable if not enough resources?
                                })
                        }
                    },
                    #endregion

                    #region Build Resource
                    new NewGui.ToolTray.Icon
                    {
                        Icon = new Gum.TileReference("tool-icons", 2),
                        KeepChildVisible = true,
                        ExpansionChild = new NewGui.ToolTray.Tray
                        {
                            ItemSource = CraftLibrary.CraftItems.Values.Where(item => item.Type == CraftItem.CraftType.Resource && ResourceLibrary.Resources.ContainsKey(item.ResourceCreated) && !ResourceLibrary.Resources[item.ResourceCreated].Tags.Contains(Resource.ResourceTags.Edible))
                                .Select(data => new NewGui.ToolTray.Icon
                                {
                                    // Todo: Need to get all the icons into one sheet.
                                    Icon = data.Icon,
                                    KeepChildVisible = true, // So the player can interact with the popup.
                                    ExpansionChild = new NewGui.BuildCraftInfo
                                    {
                                        Data = data,
                                        Rect = new Rectangle(0,0,256,128),
                                        Master = Master,
                                        World = World
                                    },
                                    OnClick = (sender, args) =>
                                    {
                                        var buildInfo = (sender as NewGui.ToolTray.Icon).ExpansionChild as NewGui.BuildCraftInfo;
                                        data.SelectedResources = buildInfo.GetSelectedResources();

                                        Master.Faction.RoomBuilder.CurrentRoomData = null;
                                        Master.VoxSelector.SelectionType = VoxelSelectionType.SelectEmpty;
                                        Master.Faction.WallBuilder.CurrentVoxelType = null;
                                        Master.Faction.CraftBuilder.IsEnabled = true;
                                        Master.Faction.CraftBuilder.CurrentCraftType = data;
                                        ChangeTool(GameMaster.ToolMode.Build);
                                        World.ShowToolPopup("Click and drag to build " + data.Name);
                                    }
                                    //Todo: Add to toolbar item list & disable if not enough resources?
                                })
                        }
                    },
                                #endregion

                            }
                        }
                    },
                    #endregion

                    #region Cook
                    new NewGui.ToolTray.Icon
                    {
                        Icon = new Gum.TileReference("tool-icons", 27),
                        KeepChildVisible = true,
                        OnConstruct = (sender) =>
                        {
                            ToolbarItems.Add(new ToolbarItem(sender, () =>
                            Master.Faction.SelectedMinions.Any(minion =>
                                minion.Stats.CurrentClass.HasAction(GameMaster.ToolMode.Build))));
                            AddToolSelectIcon(GameMaster.ToolMode.Build, sender);
                        },
                        ExpansionChild = new NewGui.ToolTray.Tray
                        {
                            ItemSource = CraftLibrary.CraftItems.Values.Where(item => item.Type == CraftItem.CraftType.Resource && ResourceLibrary.Resources.ContainsKey(item.ResourceCreated) && ResourceLibrary.Resources[item.ResourceCreated].Tags.Contains(Resource.ResourceTags.Edible))
                                .Select(data => new NewGui.ToolTray.Icon
                                {
                                    Icon = data.Icon,
                                    KeepChildVisible = true, // So the player can interact with the popup.
                                    ExpansionChild = new NewGui.BuildCraftInfo
                                    {
                                        Data = data,
                                        Rect = new Rectangle(0,0,256,128),
                                        Master = Master,
                                        World = World
                                    },
                                    OnClick = (sender, args) =>
                                    {
                                        var buildInfo = (sender as NewGui.ToolTray.Icon).ExpansionChild as NewGui.BuildCraftInfo;
                                        data.SelectedResources = buildInfo.GetSelectedResources();

                                        Master.Faction.RoomBuilder.CurrentRoomData = null;
                                        Master.VoxSelector.SelectionType = VoxelSelectionType.SelectEmpty;
                                        Master.Faction.WallBuilder.CurrentVoxelType = null;
                                        Master.Faction.CraftBuilder.IsEnabled = true;
                                        Master.Faction.CraftBuilder.CurrentCraftType = data;
                                        ChangeTool(GameMaster.ToolMode.Build);
                                        World.ShowToolPopup("Click and drag to build " + data.Name);
                                    }
                                    //Todo: Add to toolbar item list & disable if not enough resources?
                                })
                        }
                    },
                    #endregion

                    #region Dig tool
                    new NewGui.ToolTray.Icon
                    {
                        Icon = new Gum.TileReference("tool-icons", 0),
                        OnClick = (sender, args) => ChangeTool(GameMaster.ToolMode.Dig),
                        OnConstruct = (sender) =>
                        {
                            ToolbarItems.Add(new ToolbarItem(sender, () =>
                            Master.Faction.SelectedMinions.Any(minion =>
                                minion.Stats.CurrentClass.HasAction(GameMaster.ToolMode.Dig))));
                            AddToolSelectIcon(GameMaster.ToolMode.Dig, sender);
                        }
                    },
                    #endregion

                    #region Gather tool
                    new NewGui.ToolTray.Icon
                    {
                        Icon = new Gum.TileReference("tool-icons", 6),
                        OnClick = (sender, args) => ChangeTool(GameMaster.ToolMode.Gather),
                        OnConstruct = (sender) =>
                        {
                            ToolbarItems.Add(new ToolbarItem(sender, () =>
                            Master.Faction.SelectedMinions.Any(minion =>
                                minion.Stats.CurrentClass.HasAction(GameMaster.ToolMode.Gather))));
                            AddToolSelectIcon(GameMaster.ToolMode.Gather, sender);
                        }
                    },
                    #endregion

                    #region Chop tool
                    new NewGui.ToolTray.Icon
                    {
                        Icon = new Gum.TileReference("tool-icons", 1),
                        OnClick = (sender, args) => ChangeTool(GameMaster.ToolMode.Chop),
                        OnConstruct = (sender) =>
                        {
                            ToolbarItems.Add(new ToolbarItem(sender, () =>
                            Master.Faction.SelectedMinions.Any(minion =>
                                minion.Stats.CurrentClass.HasAction(GameMaster.ToolMode.Chop))));
                            AddToolSelectIcon(GameMaster.ToolMode.Chop, sender);
                        }
                    },
                    #endregion

                    #region Guard tool
                    new NewGui.ToolTray.Icon
                    {
                        Icon = new Gum.TileReference("tool-icons", 4),
                        OnClick = (sender, args) => ChangeTool(GameMaster.ToolMode.Guard),
                        OnConstruct = (sender) =>
                        {
                            ToolbarItems.Add(new ToolbarItem(sender, () =>
                            Master.Faction.SelectedMinions.Any(minion =>
                                minion.Stats.CurrentClass.HasAction(GameMaster.ToolMode.Guard))));
                            AddToolSelectIcon(GameMaster.ToolMode.Guard, sender);
                        }
                    },
                    #endregion

                    #region Attack tool
                    new NewGui.ToolTray.Icon
                    {
                        Icon = new Gum.TileReference("tool-icons", 3),
                        OnClick = (sender, args) => ChangeTool(GameMaster.ToolMode.Attack),
                        OnConstruct = (sender) =>
                        {
                            ToolbarItems.Add(new ToolbarItem(sender, () =>
                            Master.Faction.SelectedMinions.Any(minion =>
                                minion.Stats.CurrentClass.HasAction(GameMaster.ToolMode.Attack))));
                            AddToolSelectIcon(GameMaster.ToolMode.Attack, sender);
                        }
                    },
                    #endregion
                                        
                    #region Farm tool
                    new NewGui.ToolTray.Icon
                    {
                        Icon = new Gum.TileReference("tool-icons", 13),
                        OnClick = (sender, args) => ChangeTool(GameMaster.ToolMode.Farm),
                        OnConstruct = (sender) =>
                        {
                            ToolbarItems.Add(new ToolbarItem(sender, () =>
                            Master.Faction.SelectedMinions.Any(minion =>
                                minion.Stats.CurrentClass.HasAction(GameMaster.ToolMode.Farm))));
                            AddToolSelectIcon(GameMaster.ToolMode.Farm, sender);
                        }
                    },
                    #endregion

                    #region Magic tool
                    new NewGui.ToolTray.Icon
                    {
                        Icon = new Gum.TileReference("tool-icons", 14),
                        OnClick = (sender, args) => ChangeTool(GameMaster.ToolMode.Magic),
                        OnConstruct = (sender) =>
                        {
                            ToolbarItems.Add(new ToolbarItem(sender, () =>
                            Master.Faction.SelectedMinions.Any(minion =>
                                minion.Stats.CurrentClass.HasAction(GameMaster.ToolMode.Magic))));
                            AddToolSelectIcon(GameMaster.ToolMode.Magic, sender);
                        }
                    },
                    #endregion

                }
            }) as NewGui.ToolTray.Tray;

            BottomRightTray.Hidden = false;
            ChangeTool(GameMaster.ToolMode.SelectUnits);

            #endregion

            GuiRoot.RootItem.Layout();
        }

        private NewGui.FramedIcon CreateIcon(int Tile, GameMaster.ToolMode Mode)
        {
            return new NewGui.FramedIcon
            {
                Icon = new Gum.TileReference("tool-icons", Tile),
                OnClick = (sender, args) => Master.ChangeTool(Mode)
            };
        }
                
        /// <summary>
        /// Called when the user releases a key
        /// </summary>
        /// <param name="key">The keyboard key released</param>
        private void InputManager_KeyReleasedCallback(Keys key)
        {
            if (key == ControlSettings.Mappings.Map)
            {
                World.DrawMap = !World.DrawMap;
                MinimapFrame.Hidden = true;
                MinimapFrame.Invalidate();
            }

            if (key == Keys.Escape)
            {
                if (Master.CurrentToolMode != GameMaster.ToolMode.SelectUnits)
                {
                    Master.ChangeTool(GameMaster.ToolMode.SelectUnits);
                }
                else if (PausePanel != null)
                {
                    PausePanel.Close();
                    Paused = false;
                }
                else
                {
                    OpenPauseMenu();
                }
            }

            // Special case: number keys reserved for changing tool mode
            else if (InputManager.IsNumKey(key))
            {
                int index = InputManager.GetNum(key) - 1;


                if (index < 0)
                {
                    index = 9;
                }

                // In this special case, all dwarves are selected
                if (index == 0 && Master.SelectedMinions.Count == 0)
                {
                    Master.SelectedMinions.AddRange(Master.Faction.Minions);
                }
                int i = 0;
                if (index == 0 || Master.SelectedMinions.Count > 0)
                {
                    //foreach (var pair in ToolbarItems)
                    //{
                    //    if (i == index)
                    //    {
                    //        List<CreatureAI> minions = Faction.FilterMinionsWithCapability(Master.SelectedMinions,
                    //            pair.Key);

                    //        if ((index == 0 || minions.Count > 0))
                    //        {
                    //            pair.Value.OnClick(null,null);
                    //            break;
                    //        }
                    //    }
                    //    i++;
                    //}

                    //Master.ToolBar.CurrentMode = modes[index];
                }
            }
            else if (key == ControlSettings.Mappings.Pause)
            {
                Paused = !Paused;
                if (Paused) GameSpeedControls.CurrentSpeed = 0;
                else GameSpeedControls.CurrentSpeed = GameSpeedControls.PlaySpeed;
            }
            else if (key == ControlSettings.Mappings.TimeForward)
            {
                GameSpeedControls.CurrentSpeed += 1;
            }
            else if (key == ControlSettings.Mappings.TimeBackward)
            {
                GameSpeedControls.CurrentSpeed -= 1;
            }
            else if (key == ControlSettings.Mappings.ToggleGUI)
            {
                // Todo: Reimplement.
                GUI.RootComponent.IsVisible = !GUI.RootComponent.IsVisible;
            }
        }

        private void MakeMenuItem(Gum.Widget Menu, string Name, string Tooltip, Action<Gum.Widget, Gum.InputEventArgs> OnClick)
        {
            Menu.AddChild(new Gum.Widget
            {
                AutoLayout = global::Gum.AutoLayout.DockTop,
                Border = "border-thin",
                Text = Name,
                OnClick = OnClick,
                Tooltip = Tooltip,
                TextHorizontalAlign = global::Gum.HorizontalAlign.Center,
                TextVerticalAlign = global::Gum.VerticalAlign.Center,
                TextSize = 2
            });
        }

        public void OpenPauseMenu()
        {
            if (PausePanel != null) return;
            GameSpeedControls.Pause();

            PausePanel = new Gum.Widget
            {
                Rect = new Rectangle(GuiRoot.VirtualScreen.Center.X - 128,
                    GuiRoot.VirtualScreen.Center.Y - 100, 256, 200),
                Border = "border-fancy",
                TextHorizontalAlign = global::Gum.HorizontalAlign.Center,
                Text = "- Paused -",
                InteriorMargin = new Gum.Margin(12, 0, 0, 0),
                Padding = new Gum.Margin(2, 2, 2, 2),
                OnClose = (sender) => PausePanel = null
            };

            GuiRoot.ConstructWidget(PausePanel);

            MakeMenuItem(PausePanel, "Continue", "", (sender, args) =>
                {
                    PausePanel.Close();
                    GameSpeedControls.Resume();
                    PausePanel = null;
                });

            MakeMenuItem(PausePanel, "Options", "", (sender, args) => StateManager.PushState(new OptionsState(Game, StateManager)));

            MakeMenuItem(PausePanel, "New Options", "", (sender, args) =>
                {
                    var state = new NewOptionsState(Game, StateManager)
                    {
                        OnClosed = () =>
                        {
                            OpenPauseMenu();
                        }
                    };

                    StateManager.PushState(state);
                });

            MakeMenuItem(PausePanel, "Save", "",
                (sender, args) =>
                {
                    GuiRoot.ShowPopup(new NewGui.Confirm
                    {
                        Text = "Warning: Saving is still an unstable feature. Are you sure you want to continue?",
                        OnClose = (s) =>
                        {
                            if ((s as NewGui.Confirm).DialogResult == DwarfCorp.NewGui.Confirm.Result.OKAY)
                                World.Save(
                                    String.Format("{0}_{1}", Overworld.Name, World.GameID),
                                    (success, exception) =>
                                    {
                                        GuiRoot.ShowPopup(new NewGui.Popup
                                        {
                                            Text = success ? "File saved." : "Save failed - " + exception.Message,
                                            OnClose = (s2) => OpenPauseMenu()
                                        });
                                    });
                        }
                    });
                });
            
            MakeMenuItem(PausePanel, "Quit", "", (sender, args) => QuitOnNextUpdate = true);

            PausePanel.Layout();

            GuiRoot.ShowPopup(PausePanel);
        }
      
        public void Destroy()
        {
            InputManager.KeyReleasedCallback -= InputManager_KeyReleasedCallback;
            Input.Destroy();
        }

        public void QuitGame()
        {
            World.Quit();
            StateManager.ClearState();
            Destroy();

            // This line needs to stay in so the GC can properly collect all the items the PlayState keeps active.
            // If you want to remove this line you better be prepared to fully clean up the PlayState instance
            // using another method.
            //StateManager.States["PlayState"] = new PlayState(Game, StateManager);
            
            StateManager.PushState(new MainMenuState(Game, StateManager));
        }
    }
}   
