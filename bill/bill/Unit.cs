﻿using System;
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

namespace bill
{
    class Unit : RtsObject
    {
        public static List<Unit> units = new List<Unit>();
        public static List<Unit> unitsSorted = new List<Unit>();
        public static List<Unit> DeadUnits = new List<Unit>();
        //static PotentialCollisionSweeper potentialCollisionSweeper = new PotentialCollisionSweeper(5);
        public static UnitCollisionSweeper UnitCollisionSweeper = new UnitCollisionSweeper();
        //static UnitWallCollisionSweeper wallCollisionSweeper = new UnitWallCollisionSweeper();
        static Map map;
        public static PathFinder PathFinder;

        public static Texture2D BulletTexture;
        public static Texture2D[] Explosion1Textures;

        //public List<MapTile> NearbyWalls = new List<MapTile>();

        int type;
        int attackDamage = 1;
        int attackRange = 50, attackDelay = 750, timeSinceLastAttack = 500;

        //public List<Vector2> WayPoints = new List<Vector2>();
        public List<UnitCommand> Commands = new List<UnitCommand>();
        Vector2 lastWayPoint, lastMoveDestination;
        //BaseObject followTarget;
        //BaseObject attackTarget;
        public PathNode CurrentPathNode;

        //bool isBlocked;
        //bool isIdle = true, isMoving, isFollowing, isAttacking, isAttackMoving;
        bool isWithinRangeOfTarget, avoidingUnits;

        public Unit(int type, Vector2 position, int size)
            : base(position, size)
        {
            this.type = type;
            AddUnit(this);
            initializeCurrentPathNode();
        }
        public Unit(int type, Vector2 position, int size, float speed)
            : this(type, position, size)
        {
            this.speed = new Vector2(speed, speed);
        }

        void initializeCurrentPathNode()
        {
            foreach (PathNode node in PathFinder.PathNodes)
            {
                if (node != null && node.Tile.Rectangle.Contains(Rectangle.Center))
                {
                    CurrentPathNode = node;
                    return;
                }
            }
            throw new Exception("unable to find path node containing unit");
        }

        void updateCurrentPathNode()
        {
            CurrentPathNode.UnitsContained.Remove(this);

            int y = (int)MathHelper.Clamp(CenterPoint.Y / Map.TileSize, 0, Map.Height - 1);
            int x = (int)MathHelper.Clamp(CenterPoint.X / Map.TileSize, 0, Map.Width - 1);

            CurrentPathNode = PathFinder.PathNodes[y, x];

            if (!CurrentPathNode.Tile.Walkable)
                CurrentPathNode = PathFinder.FindNearestPathNode(y, x);

            CurrentPathNode.UnitsContained.Add(this);

            // prevent getting stuck in walls
            if (!CurrentPathNode.Tile.Rectangle.Intersects(Rectangle))
                CenterPoint = CurrentPathNode.Tile.CenterPoint;

        }

        //static int frameCount = 0;
        public static void UpdateUnits(GameTime gameTime)
        {
            //if (++frameCount % 10 == 0)
                //Util.UpdatePotentialCollisions(UnitsSorted);
            //if (++frameCount % 6 == 0)
            //    wallCollisionSweeper.UpdateNearbyWalls(UnitsSorted, Map.Walls);

            //potentialCollisionSweeper.UpdatePotentialCollisions(UnitsSorted);

            PathFinder.FulfillDonePathFindRequests(PathFinder);

            foreach (Unit unit in Units)
            {
                if (!unit.IsDead)
                    unit.Update(gameTime);
            }

            for (int i = 0; i < Units.Count; i++)
            {
                Unit unit = Units[i];
                if (unit.IsDead)
                {
                    RemoveUnit(unit);
                    DeadUnits.Add(unit);
                    i--;
                }
            }
        }

        int instanceFrameCount = 0, reCalculatePathFrameDelay = 20;
        public void Update(GameTime gameTime)
        {
            instanceFrameCount++;
            updateCurrentPathNode();
            timeSinceLastAttack += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (Commands.Count == 0)
                return;

            UnitCommand command = Commands[0];

            if (command is AttackCommand)
            {
                AttackCommand attackCommand = (AttackCommand)command;

                if (attackCommand.Target.IsDead)
                    nextCommand();
                else
                {
                    Attack(attackCommand, gameTime);
                    performAttackIfStarted(attackCommand);
                }
            }
            else if (command is MoveCommand)
            {
                MoveCommand moveCommand = (MoveCommand)command;
                if (instanceFrameCount % reCalculatePathFrameDelay == 0)
                {
                    //moveCommand.WayPoints = PathFinder.FindPath(CurrentPathNode, moveCommand.Destination, false);
                    //PathFinder.SmoothPath(moveCommand.WayPoints, this);
                    PathFinder.AddPathFindRequest(this, moveCommand, CurrentPathNode, false);
                }
                //if (instanceFrameCount % reCalculatePathFrameDelay == 1)
                //    PathFinder.SmoothPath(moveCommand.WayPoints, this);
                Move(moveCommand, gameTime);
            }

            // update attack command destinations
            for (int i = 0; i < Commands.Count; i++)
            {
                AttackCommand c = Commands[i] as AttackCommand;
                if (c != null)
                {
                    if (c.Target.IsDead)
                    {
                        Commands.Remove(c);
                        i--;
                    }
                    else
                        c.Destination = c.Target.CenterPoint;
                }
            }

            // update queued command starting points
            for (int i = 1; i < Commands.Count; i++)
            {
                MoveCommand c = Commands[i] as MoveCommand;
                MoveCommand previous = Commands[i - 1] as MoveCommand;
                if (c != null && previous != null)
                    c.WayPoints[0] = previous.Destination;
            }

            // recalculate queued paths
            /*if (command is MoveCommand && instanceFrameCount % (reCalculatePathFrameDelay) == 0)
            {
                MoveCommand moveCommand = (MoveCommand)command;
                for (int i = 1; i < Commands.Count; i++)
                {
                    MoveCommand c = Commands[i] as MoveCommand;
                    MoveCommand previousCommand = Commands[i - 1] as MoveCommand;
                    if (c != null && previousCommand != null)
                    {
                        int y = (int)MathHelper.Clamp(previousCommand.Destination.Y / Map.TileSize, 0, Map.Height - 1);
                        int x = (int)MathHelper.Clamp(previousCommand.Destination.X / Map.TileSize, 0, Map.Width - 1);

                        PathNode node = PathFinder.PathNodes[y, x];
                        if (!node.Tile.Walkable)
                            node = PathFinder.FindNearestPathNode(y, x);

                        c.WayPoints = PathFinder.FindPath(node, c.Destination, false);
                    }
                }
            }*/
        }

        void nextCommand()
        {
            UnitCommand lastCommand = Commands[0];
            Commands.RemoveAt(0);

            MoveCommand lastMoveCommand = lastCommand as MoveCommand;
            if (lastMoveCommand != null)
            {
                lastMoveDestination = lastWayPoint;

                if (Commands.Count > 0 && Commands[0] is MoveCommand)
                {
                    MoveCommand newMoveCommand = (MoveCommand)Commands[0];
                    PathFinder.AddPathFindRequest(this, newMoveCommand, CurrentPathNode, false);
                    //newMoveCommand.WayPoints = PathFinder.FindPath(CurrentPathNode, newMoveCommand.Destination, false);
                }
            }

            attackStarted = false;
        }

        public void GiveCommand(UnitCommand command)
        {
            Commands.Clear();
            Commands.Add(command);
        }
        public void GiveCommand(MoveCommand command)
        {
            Commands.Clear();
            Commands.Add(command);
            lastWayPoint = centerPoint;
            lastMoveDestination = command.Destination;
            //command.WayPoints = PathFinder.FindPath(CurrentPathNode, command.Destination, false);
        }

        public void QueueCommand(UnitCommand command)
        {
            Commands.Add(command);
        }
        public void QueueCommand(MoveCommand command)
        {
            if (Commands.Count == 0)
                GiveCommand(command);
            else
            {
                MoveCommand previousMoveCommand = Commands[Commands.Count - 1] as MoveCommand;
                if (previousMoveCommand != null)
                    command.WayPoints.Insert(0, previousMoveCommand.Destination);
                Commands.Add(command);
            }
        }

        int pushCount;
        static void clearPushStatus()
        {
            foreach (Unit unit in Units)
                unit.pushCount = 0;
        }
        int hitWall;
        static void clearHitWallStatus()
        {
            foreach (Unit unit in Units)
                unit.hitWall = 0;
        }

        /*void Move(Vector2 target, GameTime gameTime)
        {
            clearPushStatus();

            float moveX = Util.ScaleWithGameTime(speed.X, gameTime);
            float moveY = Util.ScaleWithGameTime(speed.Y, gameTime);

            Vector2 difference = target - centerPoint;
            if (Math.Abs(difference.X) < moveX && Math.Abs(difference.Y) < moveY)
            {
                this.CenterPoint = target;

                nextWayPoint();
                return;
            }

            float angle = (float)Math.Atan2((double)(target.Y - CenterPoint.Y), (double)(target.X - CenterPoint.X));

            moveX *= (float)Math.Cos(angle);
            moveY *= (float)Math.Sin(angle);

            lastMove.X = moveX;
            lastMove.Y = moveY;

            PrecisePosition += lastMove;

            checkForWallHit();

            //foreach (Unit unit in Units)
            foreach (Unit unit in PotentialCollisions)
            {
                if (unit != this && Intersects(unit))
                {
                    if (isMoving)
                    {
                        if (unit.isIdle && unit.lastWayPoint == target)
                        {
                            nextWayPoint();
                            //Stop();
                        }
                        else if (Contains(MoveTarget))// || unit.Contains(moveTarget))
                        {
                            nextWayPoint();
                            //Stop();
                        }
                    }

                    angle = (float)Math.Atan2(unit.centerPoint.Y - centerPoint.Y, unit.centerPoint.X - centerPoint.X);

                    float distance = Radius + unit.Radius;
                    float force = distance - Vector2.Distance(unit.centerPoint, centerPoint);


                    if (unit.isFollowing && unit.followTarget == this)
                    {
                        unit.Push(this, angle, force);
                    }
                    else if (isFollowing && unit == followTarget)
                    {
                        //Push(angle + (float)Math.PI, force);
                        PushSimple(angle + (float)Math.PI, force);
                    }
                    else
                    {
                        isPushing = true;
                        unit.Push(this, angle, force * .1f);
                        PushSimple(angle + (float)Math.PI, force * .9f);

                        //Push(angle + (float)Math.PI, force * .5f);
                        //unit.Push(angle, force);
                    }
                }
            }

            //if (instanceFrameCount % 6 == 0)
            //    checkIfCurrentPathNodeChanged();
            checkIfCloserToNextWayPointThanCurrentWayPoint();
            //if (instanceFrameCount % 120 == 0)
            //    checkIfCloserToGoalThanCurrentWayPoint();

            turnTowards(target, 100 / radius, gameTime);
        }*/

        void Move(MoveCommand command, GameTime gameTime)
        {
            clearPushStatus();
            clearHitWallStatus();

            Vector2 wayPoint = command.WayPoints[0];

            float moveX = Util.ScaleWithGameTime(speed.X, gameTime);
            float moveY = Util.ScaleWithGameTime(speed.Y, gameTime);

            Vector2 difference = wayPoint - centerPoint;
            if (Math.Abs(difference.X) < moveX && Math.Abs(difference.Y) < moveY)
            {
                this.CenterPoint = wayPoint;

                lastWayPoint = wayPoint;
                command.NextWayPoint(this, PathFinder);
                if (command.WayPoints.Count == 0)
                    nextCommand();
                return;
            }

            float angle = (float)Math.Atan2((double)(wayPoint.Y - CenterPoint.Y), (double)(wayPoint.X - CenterPoint.X));
            moveX *= (float)Math.Cos(angle);
            moveY *= (float)Math.Sin(angle);

            lastMove.X = moveX;
            lastMove.Y = moveY;
            PrecisePosition += lastMove;

            checkForWallHit();
            if (checkForPush(command))
                return;

            turnTowards(wayPoint, 120 / Radius, gameTime);
        }

        bool attackStarted;
        int initialAttackDelay = 150;
        void Attack(AttackCommand command, GameTime gameTime)
        {
            avoidingUnits = false;
            isWithinRangeOfTarget = false;

            if (command.Target.IsDead)
            {
                nextCommand();
                return;
            }

            clearPushStatus();
            clearHitWallStatus();

            //float angle = (float)Math.Atan2(command.Target.CenterPoint.Y - centerPoint.Y, command.Target.CenterPoint.X - centerPoint.X);

            float distanceToTarget = Vector2.Distance(centerPoint, command.Target.CenterPoint) - (Radius + command.Target.Radius);

            if (distanceToTarget <= attackRange)
            {
                isWithinRangeOfTarget = true;

                // begin attack animation
                if (timeSinceLastAttack >= attackDelay)
                {
                    timeSinceLastAttack = 0;
                    attackStarted = true;
                }

                turnTowards(command.Target.CenterPoint, 120 / Radius, gameTime);
            }
            else if (!attackStarted)
            {
                float moveX = Util.ScaleWithGameTime(speed.X, gameTime);
                float moveY = Util.ScaleWithGameTime(speed.Y, gameTime);

                Vector2 wayPoint = command.WayPoints[0];

                Vector2 difference = wayPoint - centerPoint;
                if (Math.Abs(difference.X) < moveX && Math.Abs(difference.Y) < moveY)
                {
                    this.CenterPoint = wayPoint;

                    lastWayPoint = wayPoint;
                    if (command.WayPoints.Count > 1)
                        command.NextWayPoint(this, PathFinder);
                    return;
                }

                float angle = (float)Math.Atan2(wayPoint.Y - centerPoint.Y, wayPoint.X - centerPoint.X);
                moveX *= (float)Math.Cos(angle);
                moveY *= (float)Math.Sin(angle);

                lastMove.X = moveX;
                lastMove.Y = moveY;
                PrecisePosition += lastMove;

                checkForWallHit();
                // checkForPush sets avoidingUnits
                checkForPush(command);
                
                command.Destination = command.Target.CenterPoint;

                turnTowards(wayPoint, 120 / Radius, gameTime);
            }

            if (avoidingUnits)
            {
                if (instanceFrameCount % (reCalculatePathFrameDelay * 2) == 0)
                    //command.WayPoints = PathFinder.FindPath(CurrentPathNode, command.Target.CenterPoint, true);
                    PathFinder.AddPathFindRequest(this, command, CurrentPathNode, true);
            }
            else if (instanceFrameCount % reCalculatePathFrameDelay == 0)
                //command.WayPoints = PathFinder.FindPath(CurrentPathNode, command.Target.CenterPoint, false);
                PathFinder.AddPathFindRequest(this, command, CurrentPathNode, false);
        }

        void performAttackIfStarted(AttackCommand command)
        {
            if (attackStarted)
            {
                if (timeSinceLastAttack >= initialAttackDelay)
                {
                    attackStarted = false;

                    RtsBullet b = new RtsBullet(this, command.Target, centerPoint, 5, 150);
                    b.Texture = BulletTexture;
                }
            }
        }

        // for move command. returns true if command ended
        bool checkForPush(MoveCommand command)
        {
            lock (PotentialCollisionsLock)
            {
                foreach (Unit unit in PotentialCollisions)
                {
                    if (Intersects(unit))
                    {
                        float angle = (float)Math.Atan2(unit.centerPoint.Y - centerPoint.Y, unit.centerPoint.X - centerPoint.X);

                        float distance = Radius + unit.Radius;
                        float force = distance - Vector2.Distance(unit.centerPoint, centerPoint);

                        if (unit.IsIdle && (unit.lastMoveDestination == command.Destination || unit.Contains(command.Destination)))
                        {
                            PushSimple(angle + (float)Math.PI, force);
                            lastWayPoint = command.WayPoints[0];
                            command.NextWayPoint(this, PathFinder);
                            if (command.WayPoints.Count == 0)
                            {
                                nextCommand();
                                return true;
                            }
                        }
                        else if (Contains(command.Destination))
                        {
                            PushSimple(angle + (float)Math.PI, force);
                            lastWayPoint = command.WayPoints[0];
                            command.NextWayPoint(this, PathFinder);
                            if (command.WayPoints.Count == 0)
                            {
                                nextCommand();
                                return true;
                            }
                        }

                        /*if (unit.isFollowing && unit.followTarget == this)
                        {
                            unit.Push(this, angle, force);
                        }
                        else if (isFollowing && unit == followTarget)
                        {
                            PushSimple(angle + (float)Math.PI, force);
                        }
                        else*/
                        {
                            //pushCount++;
                            unit.Push(this, angle, force * .1f);
                            PushSimple(angle + (float)Math.PI, force * .9f);
                        }
                    }
                }
            }

            return false;
        }
        // when attacking
        void checkForPush(AttackCommand command)
        {
            lock (PotentialCollisionsLock)
            {
                foreach (Unit unit in PotentialCollisions)
                {
                    if (Intersects(unit))
                    {
                        float angle = (float)Math.Atan2(unit.centerPoint.Y - centerPoint.Y, unit.centerPoint.X - centerPoint.X);

                        float distance = Radius + unit.Radius;
                        float force = distance - Vector2.Distance(unit.centerPoint, centerPoint);

                        if (unit == command.Target)
                        {
                            PushSimple(angle + (float)Math.PI, force);
                        }
                        //else if (unit.IsAttacking && ((AttackCommand)unit.Commands[0]).Target == command.Target)
                        //{
                        //    PushSimple(angle + (float)Math.PI, force);
                        //}
                        else if (unit.IsAttacking && unit.isWithinRangeOfTarget)
                        {
                            PushSimple(angle + (float)Math.PI, force);

                            avoidingUnits = true;
                            isWithinRangeOfTarget = true;
                        }
                        else
                        {
                            //pushCount++;
                            unit.Push(this, angle, force * .1f);
                            PushSimple(angle + (float)Math.PI, force * .9f);
                        }
                    }
                }
            }
        }

        public void Push(Unit pusher, float angle, float force)
        {
            pushCount++;
            float angleX = (float)Math.Cos(angle);
            float angleY = (float)Math.Sin(angle);

            CenterPoint += new Vector2(force * angleX, force * angleY);

            checkForWallHit();
            
            //if (isFollowing)
            //    return;

            lock (PotentialCollisionsLock)
            {
                foreach (Unit unit in PotentialCollisions)
                {
                    if (unit.pushCount < 1 && Intersects(unit))
                    {
                        angle = (float)Math.Atan2(unit.centerPoint.Y - centerPoint.Y, unit.centerPoint.X - centerPoint.X);

                        unit.Push(this, angle, force);
                        //unit.Push(this, angle, force * .1f);
                        //Push(unit, angle + (float)Math.PI, force * .9f);
                    }
                }
            }
        }

        public void PushSimple(float angle, float force)
        {
            float angleX = (float)Math.Cos(angle);
            float angleY = (float)Math.Sin(angle);

            CenterPoint += new Vector2(force * angleX, force * angleY);

            checkForWallHit();
        }

        void checkForWallHit()
        {
            if (hitWall >= 2)
                return;

            foreach (MapTile tile in CurrentPathNode.Tile.Neighbors)
            {
                if (!tile.Walkable && tile.IntersectsUnit(this))
                {
                    float angle = (float)Math.Atan2(centerPoint.Y - tile.CenterPoint.Y, centerPoint.X - tile.CenterPoint.X);

                    float distance = Radius + tile.CollisionRadius;
                    float force = distance - Vector2.Distance(tile.CenterPoint, centerPoint);

                    hitWall++;
                    //isBlocked = true;
                    PushSimple(angle, force);
                    //Push(null, angle, force);
                }
            }
        }

        public void Stop()
        {
            //moveTarget = null;
            //attackTarget = null;
            //if (WayPoints.Count > 0)
            //    lastWayPoint = WayPoints[WayPoints.Count - 1];
            //WayPoints.Clear();
            Commands.Clear();
            //IsIdle = true;
        }

        public bool Intersects(Unit u)
        {
            //if (radius == u.radius)
            //    return Vector2.DistanceSquared(centerPoint, u.centerPoint) < (radiusTimesTwoSquared);
            //else
                return Vector2.Distance(centerPoint, u.centerPoint) < (this.Radius + u.Radius);
        }
        public override bool Intersects(BaseObject o)
        {
            float angle = (float)Math.Atan2(o.CenterPoint.Y - centerPoint.Y, o.CenterPoint.X - centerPoint.X);
            Vector2 point = centerPoint + new Vector2(Radius * (float)Math.Cos(angle), Radius * (float)Math.Sin(angle));
            //return o.Touches(point);

            return o.Touches(point);// || 
                //((Math.Abs(centerPoint.X - o.X) < radius || Math.Abs(centerPoint.X - (o.X  + o.Width)) < radius) &&
                //(Math.Abs(centerPoint.Y - o.Y) < radius || Math.Abs(centerPoint.Y - (o.Y + o.Height)) < radius));
        }

        public bool Contains(Vector2 point)
        {
            return Vector2.Distance(centerPoint, point) < (this.Radius);
        }

        public int Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }
        public int AttackDamage
        {
            get
            {
                return attackDamage;
            }
            set
            {
                attackDamage = (int)MathHelper.Max(0, value);
            }
        }

        public Vector2 CurrentMoveDestination
        {
            get
            {
                for (int i = 0; i < Commands.Count; i++)
                    if (Commands[i] is MoveCommand)
                        return ((MoveCommand)Commands[i]).Destination;
                return centerPoint;
            }
        }
        public Vector2 FinalMoveDestination
        {
            get
            {
                for (int i = Commands.Count - 1; i >= 0; i--)
                    if (Commands[i] is MoveCommand)
                        return ((MoveCommand)Commands[i]).Destination;
                return centerPoint;
            }
        }
        /*public BaseObject FollowTarget
        {
            get
            {
                return followTarget;
            }
            set
            {
                followTarget = value;
                IsFollowing = true;
            }
        }
        public BaseObject AttackTarget
        {
            get
            {
                return attackTarget;
            }
            set
            {
                attackTarget = value;
                IsAttacking = true;
            }
        }*/
        public float Speed
        {
            get
            {
                return speed.X;
            }
            set
            {
                speed.X = value;
                speed.Y = value;
            }
        }

        public bool IsIdle
        {
            get
            {
                //return isIdle;
                return (Commands.Count == 0);
            }
            //set
            //{
                //AllFlags = false;
                //isIdle = value;
            //}
        }
        new public bool IsMoving
        {
            get
            {
                //return isMoving;
                return (Commands.Count > 0 && Commands[0] is MoveCommand);
            }
            //set
            //{
            //    AllFlags = false;
            //    isMoving = value;
            //}
        }
        public bool IsAttacking
        {
            get
            {
                return (Commands.Count > 0 && Commands[0] is AttackCommand);
            }
        }
        /*public bool IsFollowing
        {
            get
            {
                return isFollowing;
            }
            set
            {
                AllFlags = false;
                isFollowing = true;
            }
        }
        public bool IsAttackMoving
        {
            get
            {
                return isAttackMoving;
            }
            set
            {
                AllFlags = false;
                isAttackMoving = value;
            }
        }*/
        //public bool AllFlags
        //{
        //    set
        //    {
        //        isIdle = isMoving = isFollowing = isAttacking = isAttackMoving = value;
        //    }
        //}

        public override float LeftBound
        {
            get
            {
                return centerPoint.X - Radius * 1.5f;
            }
        }
        public override float RightBound
        {
            get
            {
                return centerPoint.X + Radius * 1.5f;
            }
        }
        public override float TopBound
        {
            get
            {
                return centerPoint.Y - Radius * 1.5f;
            }
        }
        public override float BottomBound
        {
            get
            {
                return centerPoint.Y + Radius * 1.5f;
            }
        }

        public static Map Map
        {
            get
            {
                return map;
            }
            set
            {
                map = value;
                PathFinder = new PathFinder(map);
            }
        }

        public static Object UnitsLock = new Object();
        public static List<Unit> Units
        {
            get
            {
                //lock (UnitsLock)
                //{
                    return units;
                //}
            }
        }
        public static Object UnitsSortedLock = new Object();
        public static List<Unit> UnitsSorted
        {
            get
            {
                //lock (UnitsSortedLock)
                //{
                    return unitsSorted;
                //}
            }
        }
        public static void AddUnit(Unit u)
        {
            lock (UnitsLock)
            {
                units.Add(u);
            }
            lock (UnitsSortedLock)
            {
                unitsSorted.Add(u);
            }
        }
        public static void RemoveUnit(Unit u)
        {
            lock (UnitsLock)
            {
                units.Remove(u);
            }
            lock (UnitsSortedLock)
            {
                unitsSorted.Remove(u);
            }
        }
    }

    /*class UnitWallCollisionSweeper
    {
        // assumes walls are already sorted by x
        public void UpdateNearbyWalls(List<Unit> units, List<MapTile> walls)
        {
            if (units.Count == 0 || walls.Count == 0)
                return;

            Util.SortByX(units);

            List<int[]> pairs = new List<int[]>();

            for (int i = 0; i < units.Count; i++)
            {
                Unit object1 = units[i];
                object1.NearbyWalls.Clear();

                for (int s = 0; s < walls.Count; s++)
                {
                    MapTile object2 = walls[s];

                    if (object2.RightBound < object1.LeftBound)
                        continue;

                    if (object2.LeftBound > object1.RightBound)
                        break;

                    if (object2.TopBound <= object1.BottomBound &&
                        object2.BottomBound >= object1.TopBound)
                        pairs.Add(new int[2] { i, s });
                }
            }

            foreach (int[] pair in pairs)
            {
                units[pair[0]].NearbyWalls.Add(walls[pair[1]]);
            }
        }
    }*/

    class UnitCommand
    {
    }

    class MoveCommand : UnitCommand
    {
        private Vector2 destination;
        public List<Vector2> WayPoints;
        public int Priority;

        /*public MoveCommand(List<Vector2> wayPoints, int priority)
        {
            WayPoints = wayPoints;
            Priority = priority;
        }
        public MoveCommand(Vector2 wayPoint, int priority)
        {
            WayPoints = new List<Vector2>();
            WayPoints.Add(wayPoint);
            Priority = priority;
        }*/
        public MoveCommand(Vector2 destination, int priority)
        {
            this.destination = destination;
            WayPoints = new List<Vector2>();
            WayPoints.Add(destination);
            Priority = priority;
        }

        public void NextWayPoint(Unit unit, PathFinder pathFinder)
        {
            WayPoints.RemoveAt(0);
            pathFinder.SmoothPath(WayPoints, unit);
        }

        public Vector2 Destination
        {
            get
            {
                return destination;
            }
            set
            {
                destination = value;
                WayPoints[WayPoints.Count - 1] = value;
            }
        }
    }

    class AttackCommand : MoveCommand
    {
        RtsObject target;

        /*public AttackCommand(List<Vector2> wayPoints, RtsObject target) 
            : base(wayPoints, 1)
        {
            this.target = target;
        }*/
        public AttackCommand(RtsObject target)
            : base(target.CenterPoint, 1)
        {
            this.target = target;
        }

        public RtsObject Target
        {
            get
            {
                return target;
            }
        }
    }

    class RtsObject : BaseObject
    {
        float radius, radiusSquared, radiusTimesTwoSquared, diameter;
        int hp = 50, maxHp = 50;
        bool isDead;

        public RtsObject(Vector2 position, int size)
            : base(new Rectangle(0, 0, size, size))
        {
            PrecisePosition = position;
            radius = size / 2f;
            radiusSquared = (float)Math.Pow(radius, 2);
            radiusTimesTwoSquared = (float)Math.Pow(radius * 2, 2);
            diameter = radius * 2;
        }

        public void TakeDamage(int damage)
        {
            Hp -= damage;
            if (Hp == 0)
                Die();
        }

        public void Die()
        {
            IsDead = true;
            if (this is Unit)
            {
                UnitAnimation a = new UnitAnimation((Unit)this, Width, .5f, true, Unit.Explosion1Textures);
                a.Start();
            }
        }

        public float Radius
        {
            get
            {
                return radius;
            }
        }
        public float Diameter
        {
            get
            {
                return diameter;
            }
        }
        public int Hp
        {
            get
            {
                return hp;
            }
            set
            {
                hp = (int)MathHelper.Clamp(value, 0, maxHp);
            }
        }
        public int MaxHp
        {
            get
            {
                return maxHp;
            }
            set
            {
                maxHp = (int)MathHelper.Max(0, value);
            }
        }
        public bool IsDead
        {
            get
            {
                return isDead;
            }
            set
            {
                isDead = value;
            }
        }
    }

    class PathFindRequest
    {
        public static Queue<PathFindRequest> PathFindRequests = new Queue<PathFindRequest>();
        public static Object PathFindRequestsLock = new Object();

        public static Queue<PathFindRequest> DonePathFindRequests = new Queue<PathFindRequest>();
        public static Object DonePathFindRequestsLock = new Object();

        public Unit Unit;
        public MoveCommand Command;
        public PathNode StartNode;
        public bool AvoidUnits;

        public List<Vector2> WayPoints;
        //public Object WayPointsLock = new Object();

        public PathFindRequest(Unit unit, MoveCommand command, PathNode startNode, bool avoidUnits)
        {
            Unit = unit;
            Command = command;
            StartNode = startNode;
            AvoidUnits = avoidUnits;
        }
    }
}