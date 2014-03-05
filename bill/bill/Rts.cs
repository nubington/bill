using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Windows.Forms;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace bill
{
    class Rts : GameState
    {
        static bool contentLoaded = false;
        protected TimeSpan fpsElapsedTime;
        protected int frameCounter;
        protected static bool paused, allowPause;
        Random rand = new Random();
        string fpsMessage = "";
        static SpriteFont pauseFont, fpsFont;

        Form winForm;
        static Cursor normalCursor, attackCursor;

        double timeForPathFindingProfiling, pathFindingPercentage;

        MouseState mouseState;
        KeyboardState keyboardState;

        Camera camera;
        float cameraScrollSpeed = 1000, cameraZoomSpeed = .05f, cameraRotationSpeed = .1f, cameraRotationTarget, cameraRotationIncrement = MathHelper.PiOver2;//MathHelper.PiOver4 / 2;

        static Song rtsMusic;

        BaseObject button1, button2, button3;
        static Texture2D buttonTexture;

        static Texture2D brownGuyTexture, brownGuySelectingTexture, brownGuySelectedTexture;
        static Texture2D moveCommandTexture, normalCursorTexture, attackCommandCursorTexture;
        static Texture2D redCircleTexture, transparentTexture;

        //Unit isaac;
        static readonly int ISAACSIZE = 20;
        static readonly float ISAACSPEED = 100;

        static List<Unit> SelectingUnits = new List<Unit>();
        static List<Unit> SelectedUnits = new List<Unit>();
        static List<Unit>[] HotkeyGroups = new List<Unit>[10];

        bool usingAttackCommand, queueingAttackCommand;
        int normalCursorSize = 28, attackCommandCursorSize = 23;

        Map map;
        static Texture2D boulder1Texture, tree1Texture;
        int actualMapWidth, actualMapHeight;

        Texture2D minimapTexture;
        Rectangle minimap;
        int minimapSize = 125, minimapBorderSize = 5;
        int minimapPosX, minimapPosY;
        float minimapToMapRatioX, minimapToMapRatioY;
        float minimapToScreenRatioX, minimapToScreenRatioY;
        BaseObject minimapScreenIndicatorBox;
        PrimitiveLine minimapScreenIndicatorBoxLine;

        Viewport worldViewport, uiViewport;

        public Rts(EventHandler callback)
            : base(callback)
        {
            Game1.Game.DebugMonitor.Position = Direction.NorthEast;
            //Game1.Game.IsMouseVisible = false;
            //Game1.Game.Graphics.SynchronizeWithVerticalRetrace = false;
            //Game1.Game.IsFixedTimeStep = false;
            //Game1.Game.Graphics.ApplyChanges();

            map = new Map(@"Content/map1.txt");
            Unit.Map = map;
            actualMapWidth = map.Width * map.TileSize;
            actualMapHeight = map.Height * map.TileSize;

            Unit.UnitCollisionSweeper.Thread.Suspend();
            Unit.UnitCollisionSweeper.Thread.Resume();
            Unit.PathFinder.ResumeThread();

            uiViewport = GraphicsDevice.Viewport;
            worldViewport = GraphicsDevice.Viewport;
            worldViewport.Height -= (minimapSize + minimapBorderSize * 2);
            GraphicsDevice.Viewport = worldViewport;

            camera = new Camera();
            camera.Pos = new Vector2(worldViewport.Width / 2, worldViewport.Height / 2);

            button1 = new BaseObject(new Rectangle(10, 25, 25, 25));
            button2 = new BaseObject(new Rectangle(10, 52, 25, 25));
            button3 = new BaseObject(new Rectangle(10, 79, 25, 25));

            if (!contentLoaded)
            {
                pauseFont = Content.Load<SpriteFont>("Battle1BillStatusFont");
                fpsFont = Content.Load<SpriteFont>("TitleFont1");
                brownGuyTexture = Content.Load<Texture2D>("browncircleguy");
                brownGuySelectingTexture = Content.Load<Texture2D>("browncircleguyselected2");
                brownGuySelectedTexture = Content.Load<Texture2D>("browncircleguyselecting2");
                buttonTexture = Content.Load<Texture2D>("titlebutton1");
                moveCommandTexture = Content.Load<Texture2D>("greencircle2");
                //normalCursorTexture = Content.Load<Texture2D>("greencursor2");
                //attackCommandCursorTexture = Content.Load<Texture2D>("crosshair");
                normalCursor = Util.LoadCustomCursor(@"Content/SC2-cursor.cur");
                attackCursor = Util.LoadCustomCursor(@"Content/SC2-target-none.cur");
                boulder1Texture = Content.Load<Texture2D>("boulder1");
                tree1Texture = Content.Load<Texture2D>("tree2");
                redCircleTexture = Content.Load<Texture2D>("redcircle");
                transparentTexture = Content.Load<Texture2D>("transparent");
                rtsMusic = Content.Load<Song>("rtsmusic");
                Unit.BulletTexture = Content.Load<Texture2D>("bullet");
                Unit.Explosion1Textures = Util.SplitTexture(Content.Load<Texture2D>("explosionsheet1"), 45, 45);
                contentLoaded = true;
            }

            winForm = (Form)Form.FromHandle(Game1.Game.Window.Handle);
            winForm.Cursor = normalCursor;

            initializeMinimap();

            SelectBox.InitializeSelectBoxLine(GraphicsDevice, Color.Green);
            InitializeSelectionRingLine(GraphicsDevice, Color.Yellow);

            minimapScreenIndicatorBoxLine = new PrimitiveLine(GraphicsDevice, 1);
            minimapScreenIndicatorBoxLine.Colour = Color.White;

            for (int i = 0; i < HotkeyGroups.Length; i++)
                HotkeyGroups[i] = new List<Unit>();

            MediaPlayer.Play(rtsMusic);
            MediaPlayer.Volume = .25f;
            MediaPlayer.IsRepeating = true;
        }

        public override void Update(GameTime gameTime)
        {
            // check for exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Game1.Game.Exit();
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
            {
                //Graphics.ToggleFullScreen();
                cleanup();
                returnControl("title");
                return;
            }

            // mute check
            checkForMute();

            // pause check
            if (Keyboard.GetState(PlayerIndex.One).IsKeyUp(Keys.P))
                allowPause = true;
            if (allowPause && Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.P))
            {
                paused ^= true;
                allowPause = false;
                if (paused)
                    MediaPlayer.Volume /= 4;
                else
                    MediaPlayer.Volume *= 4;
            }

            // update mouse and keyboard state
            mouseState = Mouse.GetState();
            keyboardState = Keyboard.GetState();

            checkForCameraZoom();
            checkForCameraRotate();

            // do nothing else if paused
            if (paused)
                return;

            //update fps
            fpsElapsedTime += gameTime.ElapsedGameTime;
            if (fpsElapsedTime > TimeSpan.FromSeconds(1))
            {
                //Game1.Game.Window.Title = "FPS: " + (frameCounter > 2 ? frameCounter.ToString() : "COOL");
                fpsMessage = "FPS: " + (frameCounter > 2 ? frameCounter.ToString() : "COOL");
                fpsMessage += " - Unit count: " + Unit.Units.Count;
                fpsElapsedTime -= TimeSpan.FromSeconds(1);
                frameCounter = 0;
            }

            if (button1.Rectangle.Contains(mouseState.X, mouseState.Y) && mouseState.LeftButton == ButtonState.Pressed)
            {
                Unit brownGuy;
                brownGuy = new Unit(1, new Vector2(worldViewport.Width * .25f, worldViewport.Height / 2), ISAACSIZE, ISAACSPEED);
                brownGuy.Texture = brownGuyTexture;
                //brownGuy.AddWayPoint(new Vector2(Graphics.GraphicsDevice.Viewport.Width * .75f, Graphics.GraphicsDevice.Viewport.Height / 2));
                brownGuy.GiveCommand(new MoveCommand(new Vector2(worldViewport.Width * .75f, worldViewport.Height / 2), 1));
            }
            else if (button2.Rectangle.Contains(mouseState.X, mouseState.Y) && mouseState.LeftButton == ButtonState.Pressed)
            {
                for (int i = 0; i < 10; i++)
                {
                    Unit brownGuy;
                    brownGuy = new Unit(1, new Vector2(worldViewport.Width * .25f, worldViewport.Height / 2), ISAACSIZE, ISAACSPEED);
                    brownGuy.Texture = brownGuyTexture;
                    //brownGuy.AddWayPoint(new Vector2(Graphics.GraphicsDevice.Viewport.Width * .75f, Graphics.GraphicsDevice.Viewport.Height / 2));
                    brownGuy.GiveCommand(new MoveCommand(new Vector2(worldViewport.Width * .75f, worldViewport.Height / 2), 1));
                }
            }
            else if (button3.Rectangle.Contains(mouseState.X, mouseState.Y) && mouseState.LeftButton == ButtonState.Pressed)
            {
                Graphics.ToggleFullScreen();
                uiViewport = GraphicsDevice.Viewport;
                worldViewport = GraphicsDevice.Viewport;
                worldViewport.Height -= (minimapSize + minimapBorderSize * 2);
                GraphicsDevice.Viewport = worldViewport;
                initializeMinimap();
            }

            if (SelectedUnits.Count == 1)
            {
                Game1.Game.Window.Title = "idle: " + SelectedUnits[0].IsIdle + " hp: " + SelectedUnits[0].Hp + "/" +  SelectedUnits[0].MaxHp + ". position " + SelectedUnits[0].CenterPoint;
            }

            timeForPathFindingProfiling += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timeForPathFindingProfiling >= 1000)
            {
                double pathFindingTime;
                lock (Unit.PathFinder.TimeSpentPathFindingLock)
                {
                    pathFindingTime = Unit.PathFinder.TimeSpentPathFinding.TotalMilliseconds;
                    Unit.PathFinder.TimeSpentPathFinding = TimeSpan.Zero;
                }
                pathFindingPercentage = pathFindingTime / timeForPathFindingProfiling * 100;
                timeForPathFindingProfiling = 0;
            }

            checkForAttackCommand();

            Shrinker.UpdateShrinkers(gameTime);

            checkHotKeyGroups(gameTime);

            SelectBox.Update(worldViewport, camera);

            checkForLeftClick(gameTime);

            checkForRightClick();

            checkForStop();

            RtsBullet.UpdateAll(gameTime);

            Unit.UpdateUnits(gameTime);
            UnitAnimation.UpdateAll();

            removeDeadUnitsFromSelections();

            checkForMouseCameraScroll(gameTime);
            if (keyboardState.IsKeyDown(Keys.Space))
                centerCameraOnSelectedUnits();
            clampCameraToMap();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Game1.Game.DebugMonitor.AddLine("camera position: " + camera.Pos);
            Game1.Game.DebugMonitor.AddLine("camera zoom: " + camera.Zoom);
            Game1.Game.DebugMonitor.AddLine("camera rotation: " + camera.Rotation);
            Game1.Game.DebugMonitor.AddLine("pathfinding usage: " + pathFindingPercentage.ToString("F1") + "%");

            GraphicsDevice.Clear(Color.Gray);

            spriteBatch.End();
            GraphicsDevice.Viewport = worldViewport;
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.get_transformation(worldViewport));

            drawMap(spriteBatch);

            // units
            foreach (BaseObject unit in Unit.Units)
                spriteBatch.Draw(unit.Texture, new Rectangle((int)unit.CenterPoint.X, (int)unit.CenterPoint.Y, unit.Width, unit.Height), null, Color.White, unit.Rotation, unit.TextureCenterOrigin, SpriteEffects.None, 0f);

            // selection rings
            foreach (Unit unit in SelectingUnits)
                //drawSelectionRing(unit, spriteBatch, Color.Green);
                drawSelectingRing(unit, spriteBatch);
            foreach (Unit unit in SelectedUnits)
                //drawSelectionRing(unit, spriteBatch, Color.Khaki);
                drawSelectedRing(unit, spriteBatch);

            // unit animations
            foreach (UnitAnimation a in UnitAnimation.UnitAnimations)
                spriteBatch.Draw(a, new Rectangle(a.Rectangle.Center.X, a.Rectangle.Center.Y, a.Rectangle.Width, a.Rectangle.Height), null, Color.White, a.Rotation, new Vector2(((Texture2D)a).Width / 2, ((Texture2D)a).Height / 2), SpriteEffects.None, 0f);

            foreach (Unit unit in Unit.Units)
            {
                if (unit.IsMoving)
                {
                    selectionRingLine.ClearVectors();
                    selectionRingLine.AddVector(unit.CenterPoint);
                    //foreach (Vector2 v in unit.WayPoints)
                    //    selectionRingLine.AddVector(v);
                    foreach (MoveCommand command in unit.Commands)
                    {
                        if (command is AttackCommand)
                            selectionRingLine.Colour = Color.Red;
                        else
                            selectionRingLine.Colour = Color.Green;

                        foreach (Vector2 v in command.WayPoints)
                            selectionRingLine.AddVector(v);

                        selectionRingLine.Render(spriteBatch);
                        selectionRingLine.ClearVectors();
                        selectionRingLine.AddVector(command.Destination);
                    }
                }
            }

            if (SelectedUnits.Count == 1)
            {
                PrimitiveLine line = new PrimitiveLine(GraphicsDevice, 1);
                line.Colour = Color.Black;
                lock (SelectedUnits[0].PotentialCollisionsLock)
                {
                    foreach (Unit unit in SelectedUnits[0].PotentialCollisions)
                    {
                        line.ClearVectors();
                        line.AddVector(SelectedUnits[0].CenterPoint);
                        line.AddVector(unit.CenterPoint);
                        line.Render(spriteBatch);
                    }
                }
                line.Colour = Color.Red;
                line.ClearVectors();
                line.AddVector(new Vector2(SelectedUnits[0].CurrentPathNode.Tile.X, SelectedUnits[0].CurrentPathNode.Tile.Y));
                line.AddVector(new Vector2(SelectedUnits[0].CurrentPathNode.Tile.X + SelectedUnits[0].CurrentPathNode.Tile.Width, SelectedUnits[0].CurrentPathNode.Tile.Y));
                line.AddVector(new Vector2(SelectedUnits[0].CurrentPathNode.Tile.X + SelectedUnits[0].CurrentPathNode.Tile.Width, SelectedUnits[0].CurrentPathNode.Tile.Y + SelectedUnits[0].CurrentPathNode.Tile.Height));
                line.AddVector(new Vector2(SelectedUnits[0].CurrentPathNode.Tile.X, SelectedUnits[0].CurrentPathNode.Tile.Y + SelectedUnits[0].CurrentPathNode.Tile.Height));
                line.AddVector(new Vector2(SelectedUnits[0].CurrentPathNode.Tile.X, SelectedUnits[0].CurrentPathNode.Tile.Y));
                line.Render(spriteBatch);
                line.Colour = Color.Black;
                foreach (MapTile tile in SelectedUnits[0].CurrentPathNode.Tile.Neighbors)
                {
                    line.ClearVectors();
                    line.AddVector(new Vector2(tile.X, tile.Y));
                    line.AddVector(new Vector2(tile.X + tile.Width, tile.Y));
                    line.AddVector(new Vector2(tile.X + tile.Width, tile.Y + tile.Height));
                    line.AddVector(new Vector2(tile.X, tile.Y + tile.Height));
                    line.AddVector(new Vector2(tile.X, tile.Y));
                    line.Render(spriteBatch);
                }
            }

            // bullets
            foreach (RtsBullet b in RtsBullet.RtsBullets)
                spriteBatch.Draw(b.Texture, b, Color.White);

            // move command shrinker things
            foreach (Shrinker shrinker in Shrinker.Shrinkers)
                spriteBatch.Draw(shrinker.Texture, shrinker, Color.White);
                //spriteBatch.Draw(shrinker.Texture, new Rectangle((int)shrinker.CenterPoint.X, (int)shrinker.CenterPoint.Y, shrinker.Width, shrinker.Height), null, Color.White, shrinker.Rotation, shrinker.TextureCenterOrigin, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin();

            GraphicsDevice.Viewport = uiViewport;

            SelectBox.Draw(spriteBatch, camera);

            drawMinimap(spriteBatch);

            //pause and fps count
            Vector2 pauseStringSize = pauseFont.MeasureString("PAUSED");
            if (paused)
                spriteBatch.DrawString(pauseFont, "PAUSED", new Vector2(uiViewport.Width / 2 - pauseStringSize.X / 2, uiViewport.Height / 2 - pauseStringSize.Y / 2), Color.White);
            else
                frameCounter++;

            // fps message
            spriteBatch.DrawString(fpsFont, fpsMessage, new Vector2(8, 5), Color.Black);

            spriteBatch.Draw(buttonTexture, button1, Color.White);
            spriteBatch.DrawString(fpsFont, "1", new Vector2((int)(button1.X + button1.Width / 2 - fpsFont.MeasureString("1").X / 2), (int)(button1.Y + button1.Height / 2 - fpsFont.MeasureString("1").Y / 2)), Color.White);
            spriteBatch.Draw(buttonTexture, button2, Color.White);
            spriteBatch.DrawString(fpsFont, "10", new Vector2((int)(button2.X + button2.Width / 2 - fpsFont.MeasureString("10").X / 2), (int)(button2.Y + button2.Height / 2 - fpsFont.MeasureString("10").Y / 2)), Color.White);
            spriteBatch.Draw(buttonTexture, button3, Color.White);
            spriteBatch.DrawString(fpsFont, "FS", new Vector2((int)(button3.X + button3.Width / 2 - fpsFont.MeasureString("FS").X / 2), (int)(button3.Y + button3.Height / 2 - fpsFont.MeasureString("FS").Y / 2)), Color.White);

            // cursor
            /*if (usingAttackCommand)
                spriteBatch.Draw(attackCommandCursorTexture, new Rectangle(mouseState.X - attackCommandCursorSize / 2, mouseState.Y - attackCommandCursorSize / 2, attackCommandCursorSize, attackCommandCursorSize), Color.White);
            else
                spriteBatch.Draw(normalCursorTexture, new Rectangle(mouseState.X, mouseState.Y, attackCommandCursorSize, attackCommandCursorSize), Color.White);*/
        }

        void cleanup()
        {
            Unit.UnitCollisionSweeper.Thread.Suspend();
            Unit.Units.Clear();
            Unit.UnitsSorted.Clear();
            lock (PathFindRequest.PathFindRequestsLock)
            {
                PathFindRequest.PathFindRequests.Clear();
            }
            lock (PathFindRequest.DonePathFindRequestsLock)
            {
                PathFindRequest.DonePathFindRequests.Clear();
            }
            Unit.PathFinder.SuspendThread();
            Game1.Game.DebugMonitor.Position = Direction.SouthEast;
            Game1.Game.IsMouseVisible = true;
            GraphicsDevice.Viewport = uiViewport;
        }

        static PrimitiveLine selectionRingLine;
        public static void InitializeSelectionRingLine(GraphicsDevice graphicsDevice, Color color)
        {
            selectionRingLine = new PrimitiveLine(graphicsDevice, 1);
            selectionRingLine.Colour = color;
        }

        void drawSelectionRing(Unit unit, SpriteBatch spriteBatch, Color color)
        {
            selectionRingLine.Colour = color;

            selectionRingLine.Position = unit.CenterPoint;
            selectionRingLine.CreateCircle(unit.Radius, (int)Math.Round(unit.Radius * 2));
            selectionRingLine.Render(spriteBatch);
        }
        void drawSelectingRing(Unit unit, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(brownGuySelectingTexture, new Rectangle((int)unit.CenterPoint.X, (int)unit.CenterPoint.Y, unit.Width, unit.Height), null, Color.White, unit.Rotation, unit.TextureCenterOrigin, SpriteEffects.None, 0f);
        }
        void drawSelectedRing(Unit unit, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(brownGuySelectedTexture, new Rectangle((int)unit.CenterPoint.X, (int)unit.CenterPoint.Y, unit.Width, unit.Height), null, Color.White, unit.Rotation, unit.TextureCenterOrigin, SpriteEffects.None, 0f);
        }

        bool allowAKey;
        void checkForAttackCommand()
        {
            if (keyboardState.IsKeyUp(Keys.A))
                allowAKey = true;
            else if (allowAKey && keyboardState.IsKeyDown(Keys.A) &&
                SelectedUnits.Count > 0)
            {
                usingAttackCommand = true;
                winForm.Cursor = attackCursor;
                allowAKey = false;
            }

            if (queueingAttackCommand && keyboardState.IsKeyUp(Keys.LeftShift))
            {
                usingAttackCommand = queueingAttackCommand = false;
                winForm.Cursor = normalCursor;
            }
        }

        bool allowSelect = true, allowAttackCommand = true;
        void checkForLeftClick(GameTime gameTime)
        {
            if (mouseState.Y > worldViewport.Height)
            {
                if (!selecting)
                {
                    SelectBox.Enabled = false;
                    //SelectBox.Clear();
                    //SelectingUnits.Clear();
                }
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    usingAttackCommand = false;
                    winForm.Cursor = normalCursor;

                    if (minimap.Contains(mouseState.X, mouseState.Y))
                    {
                        camera.Pos = new Vector2((mouseState.X - minimapPosX) / minimapToMapRatioX, (mouseState.Y - minimapPosY) / minimapToMapRatioY);
                    }
                }
            }
            else
            {
                if (mouseState.LeftButton == ButtonState.Released)
                    SelectBox.Enabled = true;
            }

            if (usingAttackCommand)
            {
                if (mouseState.LeftButton == ButtonState.Released)
                    allowAttackCommand = true;
                else if (allowAttackCommand && mouseState.LeftButton == ButtonState.Pressed)
                {
                    allowAttackCommand = false;

                    if (keyboardState.IsKeyDown(Keys.LeftShift))
                        queueingAttackCommand = true;
                    else
                    {
                        usingAttackCommand = false;
                        winForm.Cursor = normalCursor;
                    }
                    allowSelect = false;
                    SelectBox.Enabled = false;

                    Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
                    if (minimap.Contains(mouseState.X, mouseState.Y))
                        mousePosition = new Vector2((mousePosition.X - minimapPosX) / minimapToMapRatioX, (mousePosition.Y - minimapPosY) / minimapToMapRatioY);
                    else
                        mousePosition = Vector2.Transform(mousePosition, Matrix.Invert(camera.get_transformation(worldViewport)));

                    giveAttackCommand(mousePosition);
                }
            }
            else
            {
                if (mouseState.LeftButton == ButtonState.Released)
                {
                    allowSelect = true;
                    SelectBox.Enabled = true;
                }

                if (allowSelect)
                    SelectUnits(gameTime);
            }
        }

        void giveAttackCommand(Vector2 mousePosition)
        {
            foreach (Unit unit in Unit.Units)
            {
                if (unit.Contains(mousePosition))
                {
                    UnitAnimation a = new UnitAnimation(unit, unit.Width, .75f, 8, false, redCircleTexture, transparentTexture);
                    a.Start();

                    foreach (Unit u in SelectedUnits)
                    {
                        if (u != unit)
                        {
                            if (keyboardState.IsKeyUp(Keys.LeftShift))
                            {
                                AttackCommand command = new AttackCommand(unit);
                                u.GiveCommand(command);
                                Unit.PathFinder.AddPathFindRequest(u, command, u.CurrentPathNode, false);
                                //u.GiveCommand(new AttackCommand(unit));
                            }
                            else
                                u.QueueCommand(new AttackCommand(unit));
                        }
                    }
                    return;
                }
            }

            giveMoveCommand(mousePosition);
        }

        bool selecting, unitsSelected;
        int doubleClickDelay = 225, timeSinceLastSimpleClick = 225, simpleClickSize = 4;
        Unit lastUnitClicked = null;
        void SelectUnits(GameTime gameTime)
        {
            //SelectBox.Box.CalculateCorners();

            int selectingUnitsCount = SelectingUnits.Count;
            SelectingUnits.Clear();

            timeSinceLastSimpleClick += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            bool simpleClick = (SelectBox.Box.GreaterOfWidthAndHeight <= simpleClickSize);


            if (SelectBox.IsSelecting)
            {
                selecting = true;
                unitsSelected = false;
                foreach (Unit unit in Unit.Units)
                {
                    if ((simpleClick && unit.Contains(Vector2.Transform(new Vector2(mouseState.X, mouseState.Y), Matrix.Invert(camera.get_transformation(worldViewport))))) ||
                            (!simpleClick && SelectBox.Box.Rectangle.Intersects(unit.Rectangle)))
                        SelectingUnits.Add(unit);
                }
            }
            else if (unitsSelected == false)
            {
                selecting = false;
                unitsSelected = true;

                // holding shift
                if (keyboardState.IsKeyDown(Keys.LeftShift))
                {
                    foreach (Unit unit in Unit.Units)
                    {
                        if ((simpleClick && unit.Contains(Vector2.Transform(new Vector2(mouseState.X, mouseState.Y), Matrix.Invert(camera.get_transformation(worldViewport))))) ||
                            (!simpleClick && SelectBox.Box.Rectangle.Intersects(unit.Rectangle)))
                        {
                            // holding ctrl or double click
                            if ((simpleClick && lastUnitClicked == unit && timeSinceLastSimpleClick <= doubleClickDelay) ||
                                (simpleClick && keyboardState.IsKeyDown(Keys.LeftControl)))
                            {
                                timeSinceLastSimpleClick = 0;

                                foreach (Unit u in Unit.Units)
                                    if (u.Type == unit.Type && !u.IsOffScreen(worldViewport, camera))
                                        SelectedUnits.Add(u);
                            }
                            // not holding ctrl or double click
                            else
                            {
                                if (!SelectedUnits.Contains(unit))
                                    SelectedUnits.Add(unit);
                                else if (simpleClick)
                                    SelectedUnits.Remove(unit);
                            }
                            lastUnitClicked = unit;
                        }
                    }
                }
                // not holding shift
                else
                {
                    SelectedUnits.Clear();

                    foreach (Unit unit in Unit.Units)
                    {
                        if ((simpleClick && unit.Contains(Vector2.Transform(new Vector2(mouseState.X, mouseState.Y), Matrix.Invert(camera.get_transformation(worldViewport))))) ||
                            (!simpleClick && SelectBox.Box.Rectangle.Intersects(unit.Rectangle)))
                        {
                            // holding ctrl or double click
                            if ((simpleClick && lastUnitClicked == unit && timeSinceLastSimpleClick <= doubleClickDelay) ||
                                (simpleClick && keyboardState.IsKeyDown(Keys.LeftControl)))
                            {
                                timeSinceLastSimpleClick = 0;

                                foreach (Unit u in Unit.Units)
                                    if (u.Type == unit.Type && !u.IsOffScreen(worldViewport, camera))
                                        SelectedUnits.Add(u);
                            }
                            // not holding ctrl or double click
                            else
                                SelectedUnits.Add(unit);

                            lastUnitClicked = unit;
                        }
                    }
                }
                if (simpleClick)
                    timeSinceLastSimpleClick = 0;
            }
        }

        bool allowRightClick = true;
        void checkForRightClick()
        {
            if (mouseState.RightButton == ButtonState.Released)
                allowRightClick = true;
            else if (allowRightClick && mouseState.RightButton == ButtonState.Pressed)
            {
                allowRightClick = false;

                if (usingAttackCommand)
                {
                    usingAttackCommand = false;
                    winForm.Cursor = normalCursor;
                }
                else
                    rightClick();
            }
        }

        int magicBoxMaxSize = 500;
        int moveCommandShrinkerSize = 18;
        void rightClick()
        {
            if (SelectedUnits.Count == 0)
                return;

            //magicBoxMaxSize = SelectedUnits.Count * 5;

            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
            if (minimap.Contains(mouseState.X, mouseState.Y))
                mousePosition =  new Vector2((mousePosition.X - minimapPosX) / minimapToMapRatioX, (mousePosition.Y - minimapPosY) / minimapToMapRatioY);
            else
                mousePosition = Vector2.Transform(mousePosition, Matrix.Invert(camera.get_transformation(worldViewport)));

            // follow a unit
            /*foreach (Unit unit in Unit.Units)
            {
                if (unit.Contains(mousePosition))
                {
                    foreach (Unit u in SelectedUnits)
                    {
                        if (u != unit)
                            u.FollowTarget = unit;
                        else if (SelectedUnits.Count == 1)
                            u.MoveTarget = mousePosition;
                    }
                    return;
                }
            }*/

            // move to a point
            giveMoveCommand(mousePosition);
        }

        void giveMoveCommand(Vector2 mousePosition)
        {
            // create move command shrinker thing
            Shrinker moveCommandThing;
            if (Unit.PathFinder.IsPointWalkable(mousePosition))
                moveCommandThing = new Shrinker(mousePosition - new Vector2(moveCommandShrinkerSize / 2f, moveCommandShrinkerSize / 2f), moveCommandShrinkerSize, 10);
            else
                moveCommandThing = new Shrinker(map.FindNearestWalkableTile(mousePosition) - new Vector2(moveCommandShrinkerSize / 2f, moveCommandShrinkerSize / 2f), moveCommandShrinkerSize, 10);
            moveCommandThing.Texture = moveCommandTexture;

            // create magic box
            Rectangle magicBox = SelectedUnits[0];
            foreach (Unit unit in SelectedUnits)
                magicBox = Rectangle.Union(magicBox, unit.Rectangle);

            // box is too big or clicked inside magic box
            if (magicBox.Width > magicBoxMaxSize || magicBox.Height > magicBoxMaxSize ||
                magicBox.Contains((int)mousePosition.X, (int)mousePosition.Y))
            {
                bool isPointWalkable = Unit.PathFinder.IsPointWalkable(mousePosition);
                // assign move targets to mouse position
                foreach (Unit unit in SelectedUnits)
                {
                    // if mouse position is not in a walkable tile, find nearest walkable tile
                    Vector2 destinationPoint;
                    if (isPointWalkable)
                        destinationPoint = mousePosition;
                    else
                        destinationPoint = map.FindNearestWalkableTile(mousePosition);

                    // not holding shift
                    if (keyboardState.IsKeyUp(Keys.LeftShift))
                    {
                        //float distanceToDestination = Vector2.Distance(unit.CurrentPathNode.Tile.CenterPoint, destinationPoint);
                        //if (distanceToDestination <= unit.Diameter)
                        //unit.GiveCommand(new MoveCommand(destinationPoint, 1));
                        //else
                        MoveCommand command = new MoveCommand(destinationPoint, 1);
                        unit.GiveCommand(command);
                        Unit.PathFinder.AddPathFindRequest(unit, command, unit.CurrentPathNode, false);
                    }
                    // holding shift
                    else
                    {
                        //float distanceBetweenCurrentAndNewMoveTarget = Vector2.Distance(unit.FinalMoveDestination, destinationPoint);

                        //if (distanceBetweenCurrentAndNewMoveTarget <= unit.Diameter)
                        //    unit.QueueCommand(new MoveCommand(destinationPoint, 1));
                        //else
                        MoveCommand command = new MoveCommand(destinationPoint, 1);
                        unit.QueueCommand(command);
                        //Unit.PathFinder.AddPathFindRequest(unit, command, unit.CurrentPathNode);
                    }
                }
            }
            // clicked outside magic box
            else
            {
                // make destination box and keep in screen
                Rectangle destBox = magicBox;
                destBox.X = (int)mousePosition.X - destBox.Width / 2;
                destBox.Y = (int)mousePosition.Y - destBox.Height / 2;

                // calculate angle from magic box to destination box
                float angle = (float)Math.Atan2(destBox.Center.Y - magicBox.Center.Y, destBox.Center.X - magicBox.Center.X);
                float angleX = (float)Math.Cos(angle);
                float angleY = (float)Math.Sin(angle);
                float distance = Vector2.Distance(new Vector2(magicBox.Center.X, magicBox.Center.Y), new Vector2(destBox.Center.X, destBox.Center.Y));

                // assign move targets based on angle
                foreach (Unit unit in SelectedUnits)
                {
                    // if mouse position is not in a walkable tile, find nearest walkable tile
                    Vector2 destinationPoint = unit.CenterPoint + new Vector2(distance * angleX, distance * angleY);
                    if (!Unit.PathFinder.IsPointWalkable(destinationPoint))
                        destinationPoint = map.FindNearestWalkableTile(destinationPoint);

                    // not holding shift
                    if (keyboardState.IsKeyUp(Keys.LeftShift))
                    {
                        //float distanceToDestination = Vector2.Distance(unit.CurrentPathNode.Tile.CenterPoint, destinationPoint);
                        //if (distanceToDestination <= unit.Diameter)
                        //    unit.GiveCommand(new MoveCommand(destinationPoint, 1));
                        //else

                        MoveCommand command = new MoveCommand(destinationPoint, 1);
                        unit.GiveCommand(command);
                        Unit.PathFinder.AddPathFindRequest(unit, command, unit.CurrentPathNode, false);
                    }
                    // holding shift
                    else
                    {
                        //float distanceBetweenCurrentAndNewMoveTarget = Vector2.Distance(unit.FinalMoveDestination, destinationPoint);

                        //if (distanceBetweenCurrentAndNewMoveTarget <= unit.Diameter)
                        //    unit.QueueCommand(new MoveCommand(destinationPoint, 1));
                        //else
                        unit.QueueCommand(new MoveCommand(destinationPoint, 1));
                    }
                }
            }
        }

        void checkForStop()
        {
            if (keyboardState.IsKeyDown(Keys.S))
            {
                foreach (Unit unit in SelectedUnits)
                    unit.Stop();

                usingAttackCommand = false;
                winForm.Cursor = normalCursor;
            }
        }

        bool allowHotkeyGroupSelect;
        int lastHotkeyGroupSelected, doubleHotkeySelectDelay = 250, timeSinceLastHotkeyGroupSelect = 250;
        void checkHotKeyGroups(GameTime gameTime)
        {
            timeSinceLastHotkeyGroupSelect += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (keyboardState.IsKeyUp(Keys.D0) && keyboardState.IsKeyUp(Keys.D1) &&
                keyboardState.IsKeyUp(Keys.D2) && keyboardState.IsKeyUp(Keys.D3) &&
                keyboardState.IsKeyUp(Keys.D4) && keyboardState.IsKeyUp(Keys.D5) &&
                keyboardState.IsKeyUp(Keys.D6) && keyboardState.IsKeyUp(Keys.D7) &&
                keyboardState.IsKeyUp(Keys.D8) && keyboardState.IsKeyUp(Keys.D9))
                allowHotkeyGroupSelect = true;
            else if (allowHotkeyGroupSelect &&
                (keyboardState.IsKeyDown(Keys.D0) || keyboardState.IsKeyDown(Keys.D1) ||
                keyboardState.IsKeyDown(Keys.D2) || keyboardState.IsKeyDown(Keys.D3) ||
                keyboardState.IsKeyDown(Keys.D4) || keyboardState.IsKeyDown(Keys.D5) ||
                keyboardState.IsKeyDown(Keys.D6) || keyboardState.IsKeyDown(Keys.D7) ||
                keyboardState.IsKeyDown(Keys.D8) || keyboardState.IsKeyDown(Keys.D9)))
            {
                allowHotkeyGroupSelect = false;
                usingAttackCommand = false;
                winForm.Cursor = normalCursor;

                if (keyboardState.IsKeyDown(Keys.LeftControl))
                {
                    if (keyboardState.IsKeyDown(Keys.D0))
                        HotkeyGroups[0] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else if (keyboardState.IsKeyDown(Keys.D1))
                        HotkeyGroups[1] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else if (keyboardState.IsKeyDown(Keys.D2))
                        HotkeyGroups[2] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else if (keyboardState.IsKeyDown(Keys.D3))
                        HotkeyGroups[3] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else if (keyboardState.IsKeyDown(Keys.D4))
                        HotkeyGroups[4] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else if (keyboardState.IsKeyDown(Keys.D5))
                        HotkeyGroups[5] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else if (keyboardState.IsKeyDown(Keys.D6))
                        HotkeyGroups[6] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else if (keyboardState.IsKeyDown(Keys.D7))
                        HotkeyGroups[7] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else if (keyboardState.IsKeyDown(Keys.D8))
                        HotkeyGroups[8] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                    else if (keyboardState.IsKeyDown(Keys.D9))
                        HotkeyGroups[9] = new List<Unit>(SelectedUnits.ToArray<Unit>());
                }
                else
                {
                    if (keyboardState.IsKeyDown(Keys.D0) && HotkeyGroups[0].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[0].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 0 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 0;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D1) && HotkeyGroups[1].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[1].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 1 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 1;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D2) && HotkeyGroups[2].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[2].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 2 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 2;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D3) && HotkeyGroups[3].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[3].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 3 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 3;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D4) && HotkeyGroups[4].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[4].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 4 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 4;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D5) && HotkeyGroups[5].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[5].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 5 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 5;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D6) && HotkeyGroups[6].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[6].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 6 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 6;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D7) && HotkeyGroups[7].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[7].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 7 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 7;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D8) && HotkeyGroups[8].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[8].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 8 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 8;
                    }
                    else if (keyboardState.IsKeyDown(Keys.D9) && HotkeyGroups[9].Count > 0)
                    {
                        SelectedUnits = new List<Unit>(HotkeyGroups[9].ToArray<Unit>());
                        if (lastHotkeyGroupSelected == 9 && timeSinceLastHotkeyGroupSelect <= doubleHotkeySelectDelay)
                            centerCameraOnSelectedUnits();
                        timeSinceLastHotkeyGroupSelect = 0;
                        lastHotkeyGroupSelected = 9;
                    }
                }
            }
        }

        void checkForMouseCameraScroll(GameTime gameTime)
        {
            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);

            Vector2 movement = Vector2.Zero;

            /*if (mousePosition.X <= 0)
                movement += new Vector2(-cameraScrollSpeed / camera.Zoom, 0);
            else if (mousePosition.X >= GraphicsDevice.Viewport.Width - 1)
                movement += new Vector2(cameraScrollSpeed / camera.Zoom, 0);

            if (mousePosition.Y <= 0)
                movement += new Vector2(0, -cameraScrollSpeed / camera.Zoom);
            else if (mousePosition.Y >= GraphicsDevice.Viewport.Height - 1)
                movement += new Vector2(0, cameraScrollSpeed / camera.Zoom);*/

            float adjustedScrollSpeed = cameraScrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds / camera.Zoom;

            if (mousePosition.X <= 0 || keyboardState.IsKeyDown(Keys.Left))
            {
                float angle = MathHelper.WrapAngle(-camera.Rotation + (float)Math.PI);
                movement += new Vector2(adjustedScrollSpeed * (float)Math.Cos(angle), adjustedScrollSpeed * (float)Math.Sin(angle));
            }
            else if (mousePosition.X >= uiViewport.Width - 1 || keyboardState.IsKeyDown(Keys.Right))
            {
                float angle = MathHelper.WrapAngle(-camera.Rotation);
                movement += new Vector2(adjustedScrollSpeed * (float)Math.Cos(angle), adjustedScrollSpeed * (float)Math.Sin(angle));
            }

            if (mousePosition.Y <= 0 || keyboardState.IsKeyDown(Keys.Up))
            {
                float angle = MathHelper.WrapAngle(-camera.Rotation - (float)Math.PI / 2);
                movement += new Vector2(adjustedScrollSpeed * (float)Math.Cos(angle), adjustedScrollSpeed * (float)Math.Sin(angle));
            }
            else if (mousePosition.Y >= uiViewport.Height - 1 || keyboardState.IsKeyDown(Keys.Down))
            {
                float angle = MathHelper.WrapAngle(-camera.Rotation + (float)Math.PI / 2);
                movement += new Vector2(adjustedScrollSpeed * (float)Math.Cos(angle), adjustedScrollSpeed * (float)Math.Sin(angle));
            }

            if (movement != Vector2.Zero)
                camera.Move(movement);
        }

        void checkForCameraZoom()
        {
            if (keyboardState.IsKeyDown(Keys.OemMinus))
                //camera.Zoom -= cameraZoomSpeed;
                camera.Zoom = MathHelper.Max(camera.Zoom - camera.Zoom * cameraZoomSpeed, .5f);

            if (keyboardState.IsKeyDown(Keys.OemPlus))
                //camera.Zoom += cameraZoomSpeed;
                camera.Zoom = MathHelper.Min(camera.Zoom + camera.Zoom * cameraZoomSpeed, 2f);
        }

        bool allowCameraRotate;
        void checkForCameraRotate()
        {
            // check for changes to rotation target
            if (keyboardState.IsKeyUp(Keys.PageDown) && keyboardState.IsKeyUp(Keys.PageUp))
                allowCameraRotate = true;
            else if (allowCameraRotate)
            {
                if (keyboardState.IsKeyDown(Keys.PageDown))
                {
                    cameraRotationTarget += cameraRotationIncrement;
                    allowCameraRotate = false;
                }

                if (keyboardState.IsKeyDown(Keys.PageUp))
                {
                    cameraRotationTarget -= cameraRotationIncrement;
                    allowCameraRotate = false;
                }
            }

            // rotate camera to target rotation
            if (Util.AngleDifference(camera.Rotation, cameraRotationTarget) < cameraRotationSpeed)
                camera.Rotation = cameraRotationTarget;
            else if (camera.Rotation < cameraRotationTarget)
                camera.Rotation += cameraRotationSpeed;
            else
                camera.Rotation -= cameraRotationSpeed;
        }

        void centerCameraOnSelectedUnits()
        {
            if (SelectedUnits.Count == 0)
                return;

            Rectangle rectangle = SelectedUnits[0];
            foreach (Unit unit in SelectedUnits)
                rectangle = Rectangle.Union(rectangle, unit.Rectangle);

            camera.Pos = new Vector2(rectangle.Center.X, rectangle.Center.Y);
        }

        // only draws tiles that are on the screen
        void drawMap(SpriteBatch spriteBatch)
        {
            // finds indices to start and stop drawing at based on the camera transform, viewport size, and tile size
            /*Vector2 minIndices = Vector2.Transform(Vector2.Zero, Matrix.Invert(camera.get_transformation(GraphicsDevice))) / Map.TILESIZE;
            Vector2 maxIndices = Vector2.Transform(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Matrix.Invert(camera.get_transformation(GraphicsDevice))) / Map.TILESIZE;

            // keeps min indices >= 0
            minIndices.Y = MathHelper.Max(minIndices.Y, 0);
            minIndices.X = MathHelper.Max(minIndices.X, 0);

            // keeps max indices within map size
            maxIndices.Y = (float)Math.Ceiling(MathHelper.Min(maxIndices.Y, map.Height));
            maxIndices.X = (float)Math.Ceiling(MathHelper.Min(maxIndices.X, map.Width));

            for (int y = (int)minIndices.Y; y < (int)maxIndices.Y; y++)
            {
                for (int x = (int)minIndices.X; x < (int)maxIndices.X; x++)
                {
                    MapTile tile = map.Tiles[y, x];

                    if (tile.Type == 0)
                        spriteBatch.Draw(ColorTexture.Gray, tile.Rectangle, Color.White);
                    else if (tile.Type == 1)
                        spriteBatch.Draw(boulder1Texture, tile.Rectangle, Color.White);
                    else if (tile.Type == 2)
                        spriteBatch.Draw(tree1Texture, tile.Rectangle, Color.White);
                }
            }*/
            spriteBatch.Draw(minimapTexture, new Rectangle(0, 0, map.Width * map.TileSize, map.Height * map.TileSize), Color.White);
        }

        void initializeMinimap()
        {
            // set minimap fields and create rectangle object
            minimapPosX = minimapBorderSize;
            minimapPosY = uiViewport.Height - minimapSize - minimapBorderSize;
            minimapToMapRatioX = (float)minimapSize / (map.Width * map.TileSize);
            minimapToMapRatioY = (float)minimapSize / (map.Height * map.TileSize);
            minimapToScreenRatioX = (float)minimapSize / worldViewport.Width;
            minimapToScreenRatioY = (float)minimapSize / worldViewport.Height;
            minimap = new Rectangle(minimapPosX, minimapPosY, minimapSize, minimapSize);
            minimapScreenIndicatorBox = new BaseObject(new Rectangle(0, 0, (int)(worldViewport.Width * minimapToMapRatioX), (int)(worldViewport.Height * minimapToMapRatioY)));
            //minimapTexture = new Texture2D(GraphicsDevice, map.Width * Map.TILESIZE, map.Height * Map.TILESIZE);
            //minimapTexture = new RenderTarget2D(GraphicsDevice, map.Width * Map.TILESIZE, map.Height * Map.TILESIZE);

            // create minimap texture from map tiles
            RenderTarget2D renderTarget = new RenderTarget2D(GraphicsDevice, map.Width * map.TileSize, map.Height * map.TileSize);
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Gray);
            SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch.Begin();
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    MapTile tile = map.Tiles[y, x];

                    if (tile.Type == 0)
                        spriteBatch.Draw(ColorTexture.Gray, tile.Rectangle, Color.White);
                    else if (tile.Type == 1)
                        spriteBatch.Draw(boulder1Texture, tile.Rectangle, Color.White);
                    else if (tile.Type == 2)
                        spriteBatch.Draw(tree1Texture, tile.Rectangle, Color.White);
                }
            }
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            //minimapTexture = (Texture2D)renderTarget;
            minimapTexture = new Texture2D(GraphicsDevice, map.Width * map.TileSize, map.Height * map.TileSize);
            Color[] textureData = new Color[(map.Width * map.TileSize) * (map.Height * map.TileSize)];
            renderTarget.GetData<Color>(textureData);
            minimapTexture.SetData<Color>(textureData);

            renderTarget = null;
            GC.Collect();
        }

        // draw minimap with units
        void drawMinimap(SpriteBatch spriteBatch)
        {
            //float aspectRatio = (float)map.Width / map.Height;

            // draw minimap border then minimap
            //spriteBatch.Draw(ColorTexture.Black, new Rectangle(minimapPosX - minimapBorderSize, minimapPosY - minimapBorderSize, minimapSize + minimapBorderSize * 2, minimapSize + minimapBorderSize * 2), Color.White);
            spriteBatch.Draw(ColorTexture.Black, new Rectangle(minimapPosX - minimapBorderSize, minimapPosY - minimapBorderSize, uiViewport.Width, minimapSize + minimapBorderSize * 2), Color.White);
            spriteBatch.Draw(minimapTexture, minimap, Color.White);

            // draw units on minimap
            Rectangle rectangle = new Rectangle(0, 0, 2, 2);
            foreach (Unit unit in Unit.Units)
            {
                rectangle.X = (int)(unit.X * minimapToMapRatioX + minimapPosX);
                rectangle.Y = (int)(unit.Y * minimapToMapRatioY + minimapPosY);
                spriteBatch.Draw(ColorTexture.Green, rectangle, Color.White);
            }

            // update size of screen indicator box
            minimapScreenIndicatorBox.Width = (int)(worldViewport.Width * minimapToMapRatioX / camera.Zoom);
            minimapScreenIndicatorBox.Height = (int)(worldViewport.Height * minimapToMapRatioY / camera.Zoom);

            // calculate position of screen indicator box
            minimapScreenIndicatorBox.CenterPoint = new Vector2(camera.Pos.X * minimapToMapRatioX + minimapPosX, camera.Pos.Y * minimapToMapRatioY + minimapPosY);
            minimapScreenIndicatorBox.Rotation = -camera.Rotation;
            minimapScreenIndicatorBox.CalculateCorners();

            // draw screen indicator box on minimap
            minimapScreenIndicatorBoxLine.ClearVectors();
            //minimapScreenIndicatorBoxLine.AddVector(new Vector2(minimapScreenIndicatorBox.X, minimapScreenIndicatorBox.Y));
            //minimapScreenIndicatorBoxLine.AddVector(new Vector2(minimapScreenIndicatorBox.X + minimapScreenIndicatorBox.Width, minimapScreenIndicatorBox.Y));
            //minimapScreenIndicatorBoxLine.AddVector(new Vector2(minimapScreenIndicatorBox.X + minimapScreenIndicatorBox.Width, minimapScreenIndicatorBox.Y + minimapScreenIndicatorBox.Height));
            //minimapScreenIndicatorBoxLine.AddVector(new Vector2(minimapScreenIndicatorBox.X, minimapScreenIndicatorBox.Y + minimapScreenIndicatorBox.Height));
            //minimapScreenIndicatorBoxLine.AddVector(new Vector2(minimapScreenIndicatorBox.X, minimapScreenIndicatorBox.Y));
            minimapScreenIndicatorBoxLine.AddVector(minimapScreenIndicatorBox.UpperLeftCorner);
            minimapScreenIndicatorBoxLine.AddVector(minimapScreenIndicatorBox.UpperRightCorner);
            minimapScreenIndicatorBoxLine.AddVector(minimapScreenIndicatorBox.LowerRightCorner);
            minimapScreenIndicatorBoxLine.AddVector(minimapScreenIndicatorBox.LowerLeftCorner);
            minimapScreenIndicatorBoxLine.AddVector(minimapScreenIndicatorBox.UpperLeftCorner);
            minimapScreenIndicatorBoxLine.Render(spriteBatch);
        }

        BaseObject cameraView = new BaseObject(new Rectangle());
        void clampCameraToMap()
        {
            cameraView.Width = (int)(worldViewport.Width / camera.Zoom);
            cameraView.Height = (int)(worldViewport.Height / camera.Zoom);
            cameraView.CenterPoint = camera.Pos;
            cameraView.Rotation = -camera.Rotation;
            cameraView.CalculateCorners();

            // upper left corner
            if (cameraView.UpperLeftCorner.X < 0)
            {
                cameraView.X += (int)Math.Round(-cameraView.UpperLeftCorner.X * (float)Math.Cos(0));
                cameraView.CalculateCorners();
            }
            if (cameraView.UpperLeftCorner.Y < 0)
            {
                cameraView.Y += (int)Math.Round(-cameraView.UpperLeftCorner.Y * (float)Math.Sin(MathHelper.PiOver2));
                cameraView.CalculateCorners();
            }
            if (cameraView.UpperLeftCorner.X > actualMapWidth)
            {
                cameraView.X += (int)Math.Round((cameraView.UpperLeftCorner.X - actualMapWidth) * (float)Math.Cos(Math.PI));
                cameraView.CalculateCorners();
            }
            if (cameraView.UpperLeftCorner.Y > actualMapHeight)
            {
                cameraView.Y += (int)Math.Round((cameraView.UpperLeftCorner.Y - actualMapHeight) * (float)Math.Sin(-MathHelper.PiOver2));
                cameraView.CalculateCorners();
            }

            // lower left corner
            if (cameraView.LowerLeftCorner.X < 0)
            {
                cameraView.X += (int)Math.Round(-cameraView.LowerLeftCorner.X * (float)Math.Cos(0));
                cameraView.CalculateCorners();
            }
            if (cameraView.LowerLeftCorner.Y < 0)
            {
                cameraView.Y += (int)Math.Round(-cameraView.LowerLeftCorner.Y * (float)Math.Sin(MathHelper.PiOver2));
                cameraView.CalculateCorners();
            }
            if (cameraView.LowerLeftCorner.X > actualMapWidth)
            {
                cameraView.X += (int)Math.Round((cameraView.LowerLeftCorner.X - actualMapWidth) * (float)Math.Cos(Math.PI));
                cameraView.CalculateCorners();
            }
            if (cameraView.LowerLeftCorner.Y > actualMapHeight)
            {
                cameraView.Y += (int)Math.Round((cameraView.LowerLeftCorner.Y - actualMapHeight) * (float)Math.Sin(-MathHelper.PiOver2));
                cameraView.CalculateCorners();
            }

            // upper right corner
            if (cameraView.UpperRightCorner.X < 0)
            {
                cameraView.X += (int)Math.Round(-cameraView.UpperRightCorner.X * (float)Math.Cos(0));
                cameraView.CalculateCorners();
            }
            if (cameraView.UpperRightCorner.Y < 0)
            {
                cameraView.Y += (int)Math.Round(-cameraView.UpperRightCorner.Y * (float)Math.Sin(MathHelper.PiOver2));
                cameraView.CalculateCorners();
            }
            if (cameraView.UpperRightCorner.X > actualMapWidth)
            {
                cameraView.X += (int)Math.Round((cameraView.UpperRightCorner.X - actualMapWidth) * (float)Math.Cos(Math.PI));
                cameraView.CalculateCorners();
            }
            if (cameraView.UpperRightCorner.Y > actualMapHeight)
            {
                cameraView.Y += (int)Math.Round((cameraView.UpperRightCorner.Y - actualMapHeight) * (float)Math.Sin(-MathHelper.PiOver2));
                cameraView.CalculateCorners();
            }

            // lower right corner
            if (cameraView.LowerRightCorner.X < 0)
            {
                cameraView.X += (int)Math.Round(-cameraView.LowerRightCorner.X * (float)Math.Cos(0));
                cameraView.CalculateCorners();
            }
            if (cameraView.LowerRightCorner.Y < 0)
            {
                cameraView.Y += (int)Math.Round(-cameraView.LowerRightCorner.Y * (float)Math.Sin(MathHelper.PiOver2));
                cameraView.CalculateCorners();
            }
            if (cameraView.LowerRightCorner.X > actualMapWidth)
            {
                cameraView.X += (int)Math.Round((cameraView.LowerRightCorner.X - actualMapWidth) * (float)Math.Cos(Math.PI));
                cameraView.CalculateCorners();
            }
            if (cameraView.LowerRightCorner.Y > actualMapHeight)
            {
                cameraView.Y += (int)Math.Round((cameraView.LowerRightCorner.Y - actualMapHeight) * (float)Math.Sin(-MathHelper.PiOver2));
                cameraView.CalculateCorners();
            }

            camera.Pos = cameraView.CenterPoint;

            /*float cameraLeftBound = camera.Pos.X + (GraphicsDevice.Viewport.Width / 2 * (float)Math.Cos((float)Math.PI + camera.Rotation));
            float cameraRightBound = camera.Pos.X + (GraphicsDevice.Viewport.Width / 2 * (float)Math.Cos(camera.Rotation));
            float cameraTopBound = camera.Pos.Y + (GraphicsDevice.Viewport.Height / 2 * (float)Math.Sin(-MathHelper.PiOver2 + camera.Rotation));
            float cameraBottomBound = camera.Pos.Y + (GraphicsDevice.Viewport.Height / 2 * (float)Math.Sin(MathHelper.PiOver2 + camera.Rotation));*/

            /*if (camera.Pos.X < GraphicsDevice.Viewport.Width / camera.Zoom / 2)
                camera.Pos = new Vector2(GraphicsDevice.Viewport.Width / camera.Zoom / 2, camera.Pos.Y);
            if (camera.Pos.X > map.Width * Map.TILESIZE - GraphicsDevice.Viewport.Width / camera.Zoom / 2)
                camera.Pos = new Vector2(map.Width * Map.TILESIZE - GraphicsDevice.Viewport.Width / camera.Zoom / 2, camera.Pos.Y);
            if (camera.Pos.Y < GraphicsDevice.Viewport.Height / camera.Zoom / 2)
                camera.Pos = new Vector2(camera.Pos.X, GraphicsDevice.Viewport.Height / camera.Zoom / 2);
            if (camera.Pos.Y > map.Height * Map.TILESIZE - GraphicsDevice.Viewport.Height / camera.Zoom / 2)
                camera.Pos = new Vector2(camera.Pos.X, map.Height * Map.TILESIZE - GraphicsDevice.Viewport.Height / camera.Zoom / 2);*/
        }

        void removeDeadUnitsFromSelections()
        {
            for (int i = 0; i < Unit.DeadUnits.Count; )
            {
                Unit unit = Unit.DeadUnits[i];

                SelectingUnits.Remove(unit);
                SelectedUnits.Remove(unit);

                foreach (List<Unit> group in HotkeyGroups)
                    group.Remove(unit);

                Unit.DeadUnits.Remove(unit);
            }
        }
    }
}