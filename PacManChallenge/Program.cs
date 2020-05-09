﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Net;

/**
 * Grab the pellets as fast as you can!
 **/

 class Map 
 {
     // TODO: this isn't used yet, but might be a good idea to
     // throw all the map function into here.
     public int width;
     public int height;
 
     public Map (int w, int h)
     {
         width = w;
         height = h;
     }
 
 }

public enum MapItem
{
    Wall,
    Empty,
    Pellet,
    Super,
    Unknown
}

public class Distance
{
    // Considering putting the other distance calculating functions in here.
    public static float Manhattan(Position a, Position b)
    {
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
    }
}

public class Position
{
    public int x;
    public int y;
    public float dist;
    public Position(int posX, int posY) 
    {
        x = posX;
        y = posY;
        dist = 0;
    }
    
    public Position(int posX, int posY, float dist) 
    {
        x = posX;
        y = posY;
        this.dist = dist;
    }

    public static bool operator ==(Position a, Position b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Position a, Position b)
    {
        return !(a == b);
    }

    public override string ToString()
    {
        return $"{x}, {y}";
    }
}

public class Pellet
{
    public Position Position { get; set; }
    public int value;
    // public bool chosen;

    public Pellet(Position position, int value)
    {
        Position = position;
        this.value = value;
    }
}

public enum PacType
{
    ROCK,
    PAPER,
    SCISSORS
}

public class PacMan
{
    public int id;
    public bool isMine;
    public Position Position { get; set; }
    public PacType type;
    public int speedTurns;
    public int cooldown;

    public void SetInfo(int id, bool mine, Position position, string typeId, int speedTurns, int cooldown)
    {
        this.id = id;
        isMine = mine;
        Position = position;
        type = (PacType) Enum.Parse(typeof(PacType), typeId);
        this.speedTurns = speedTurns;
        this.cooldown = cooldown;
    }

}
public class Enemy : PacMan
{
    // Trying to make it so we don't double up on an enemy
    public bool beingPursed;
    public int pursedById;

    public Enemy(bool beingPursed, int pursedById)
    {
        this.beingPursed = beingPursed;
        this.pursedById = pursedById;
    }
}

public class Friendly : PacMan
{
    public bool isAvoiding;
    public int avoidingCooldown;
    public bool isPursuing;
    public int pursuingId;
    public int avoidingPacId;
    public bool hasGoal;
    public Position goal;

    public Friendly()
    {
        isAvoiding = false;
        avoidingCooldown = 0;
        isPursuing = false;
        pursuingId = -1;
        avoidingPacId = -1;
        hasGoal = false;
        goal = new Position(0,0);
    }
    public Friendly(bool isAvoiding, int avoidingCooldown, int avoidingPacId, bool isPursuing, int pursuingId, bool hasGoal, Position goal)
    {
        this.isAvoiding = isAvoiding;
        this.avoidingCooldown = avoidingCooldown;
        this.avoidingPacId = avoidingPacId;
        this.isPursuing = isPursuing;
        this.pursuingId = pursuingId;
        this.hasGoal = hasGoal;
        this.goal = goal;
    }

    // return a command
    public string RoShamBo(Enemy enemy)
    {
        if (enemy.beingPursed)
        {
            return "ignore";
        }
        // Check for all the different combinations
        switch (type)
        {
            case PacType.ROCK:
                switch (enemy.type)
                {
                   case PacType.ROCK:
                       if (cooldown == 0)
                       {
                           isPursuing = true;
                           pursuingId = enemy.id;
                           enemy.beingPursed = true;
                           enemy.pursedById = id;
                           return "SWITCH " + id + " PAPER";
                       }
                       return "ignore";
                   case PacType.PAPER:
                       if (cooldown == 0)
                       {
                           isPursuing = true;
                           pursuingId = enemy.id;
                           enemy.beingPursed = true;
                           enemy.pursedById = id;
                           return "SWITCH " + id + " SCISSORS";
                       }
                       // TODO: calculate run away with walls.
                       return AvoidEnemy(enemy);
                   case PacType.SCISSORS:
                       isPursuing = true;
                       pursuingId = enemy.id;
                       enemy.beingPursed = true;
                       enemy.pursedById = id;
                       return "MOVE " + id + " " + enemy.Position.x + " " + enemy.Position.y + " Pursuing";
                }
                break;
            case PacType.PAPER:
                switch (enemy.type)
                {
                    case PacType.ROCK:
                        isPursuing = true;
                        pursuingId = enemy.id;
                        enemy.beingPursed = true;
                        enemy.pursedById = id;
                        return "MOVE " + id + " " + enemy.Position.x + " " + enemy.Position.y + " Pursuing";
                    case PacType.PAPER:
                        if (cooldown == 0)
                        {
                            isPursuing = true;
                            pursuingId = enemy.id;
                            enemy.beingPursed = true;
                            enemy.pursedById = id;
                            return "SWITCH " + id + " SCISSORS";
                        }
                        return "ignore";
                    case PacType.SCISSORS:
                        if (cooldown == 0)
                        {
                            isPursuing = true;
                            pursuingId = enemy.id;
                            enemy.beingPursed = true;
                            enemy.pursedById = id;
                            return "SWITCH " + id + " ROCK";
                        }
                        return AvoidEnemy(enemy);
                }
                break;
            case PacType.SCISSORS:
                switch (enemy.type)
                {
                    case PacType.ROCK:
                        if (cooldown == 0)
                        {
                            isPursuing = true;
                            pursuingId = enemy.id;
                            enemy.beingPursed = true;
                            enemy.pursedById = id;
                            return "SWITCH " + id + " PAPER";
                        }
                        return AvoidEnemy(enemy);

                    case PacType.PAPER:
                        isPursuing = true;
                        pursuingId = enemy.id;
                        enemy.beingPursed = true;
                        enemy.pursedById = id;
                        return "MOVE " + id + " " + enemy.Position.x + " " + enemy.Position.y + " Pursuing";
                    case PacType.SCISSORS:
                        if (cooldown == 0)
                        {
                            isPursuing = true;
                            pursuingId = enemy.id;
                            enemy.beingPursed = true;
                            enemy.pursedById = id;
                            return "SWITCH " + id + " ROCK";
                        }
                        return "ignore";
                }
                break;
        }
        return "ignore";
    }

    public string AvoidEnemy(Enemy enemy)
    {
        // This isn't complete, This will occasionally error with out of bounds
        if (Position.x > enemy.Position.x)
        {
            return "MOVE " + id + " " + (Position.x + 2) + " " + (Position.y) + " Avoiding";
        }
        if (Position.y < enemy.Position.y)
        {
            return "MOVE " + id + " " + (Position.x) + " " + (Position.y - 2) + " Avoiding";
        }        
        if (Position.x < enemy.Position.x)
        {
            return "MOVE " + id + " " + (Position.x - 2) + " " + (Position.y) + " Avoiding";
        }        
        if (Position.y > enemy.Position.y)
        {
            return "MOVE " + id + " " + (Position.x) + " " + Position.y + 2 + " Avoiding";
        }
        // we probably dead idk
        return "ignore";
    }
}

public class GameState
{
    // This is where I keep track of where the game is at.
    // I plan on making this map the Class Map instead.
    public MapItem[,] Map;
    public int MapWidth;
    public int MapHeight;
    public List<Friendly> FriendlyPacs;
    public List<Enemy> EnemyPacs;
    public bool[,] PacTrail;
    public int turn;

    public GameState()
    {
        FriendlyPacs = new List<Friendly>();
        EnemyPacs = new List<Enemy>();
        turn = 1;
    }

    public void InitMap(int width, int height)
    {
        Map = new MapItem[width,height];
        MapWidth = width;
        MapHeight = height;
        PacTrail = new bool[width,height];
    }
}



class Player
{
    public static void Run(string arg, string[] inputs)
    {
        if (arg == "test")
        {
            Test();
        }
        else
        {
            Challenge(inputs);
        }
    }

    public static void Test()
    {
        Position start = new Position(4, 7);
        Position end = new Position(8, 5);
        Console.Error.WriteLine($"Starting at {start.x}, {start.y}");
        while (start != end)
        {
            var paths = FindShortestPath(start, end);
            // Console.Error.WriteLine($"Next in path {paths[1]}");
            start = paths[1];
        }
    }
    public static GameState gameState = new GameState();

    public static Position FindClosestPellet(Friendly pac, List<Pellet> pellets) {
        // default to make anything closest
        int closestLength = int.MaxValue;
        // The coordinates of the closest pellet
        var cX = 0;
        var cY = 0;
        foreach ( Pellet p in pellets )
        {
            // var distance = Distance.Manhattan(pac.Position, p.Position);
            var path = FindShortestPath(pac.Position, p.Position);
            // Console.Error.WriteLine($"pellet at {p.Position.x}, {p.Position.y} for {pac.id} is {path.Length} moves away");
            if( path.Length < closestLength)
            {
                closestLength = path.Length;
                Console.Error.WriteLine($"Pellet at {p.Position.x}, {p.Position.y} is shortest for ${pac.id}");
                cX = p.Position.x;
                cY = p.Position.y;
            }
        }
        // Console.Error.WriteLine($"{pac.id} going for Pellet at {cX}, {cY}");
        // return FindShortestPath(pac.Position, new Position(cX, cY))[1];
        return new Position(cX, cY);
    }
    
    public static int Clamp( int value, int min, int max )
    {
        return value < min ? min : value > max ? max : value;
    }

    public static Position FindNewSpot(Friendly pac, Friendly nearestPac)
    {
        // check angles
        if (pac.Position.x > nearestPac.Position.x && pac.Position.y > nearestPac.Position.y)
        {
            // go to quadrant 3
            return PickAreaInQuadrant(pac,3);
        }
        if (pac.Position.x > nearestPac.Position.x && pac.Position.y < nearestPac.Position.y)
        {
            // go to quadrant 2
            return PickAreaInQuadrant(pac,2);
        }        
        if (pac.Position.x < nearestPac.Position.x && pac.Position.y > nearestPac.Position.y)
        {
            // 4
            Console.Error.WriteLine("going to quadrant 4");
            return PickAreaInQuadrant(pac,4); 
        }

        if (pac.Position.x < nearestPac.Position.x && pac.Position.y < nearestPac.Position.y)
        {
            // 1
            return PickAreaInQuadrant(pac,1);
        }
        // direct sides
        if (pac.Position.x > nearestPac.Position.x)
        {
            // go right
            return new Position(Clamp(pac.Position.x + 3, 0, gameState.MapWidth - 1), pac.Position.y);
        }        
        if (pac.Position.x < nearestPac.Position.x)
        {
            // go left
            return new Position(Clamp(pac.Position.x - 3, 0, gameState.MapWidth - 1), pac.Position.y);
        }        
        if (pac.Position.y > nearestPac.Position.y)
        {
            // go up
            return new Position(pac.Position.x, Clamp(pac.Position.y + 3, 0, gameState.MapHeight - 1));
        }        
        // go down
        return new Position(pac.Position.x, Clamp(pac.Position.y - 3,0, gameState.MapHeight - 1));
        
    }

    public static Position PickAreaInQuadrant(Friendly pac, int quadrant)
    {
        int x;
        int y;
        Position pos;
        switch (quadrant)
        {
            case 1:
                x = (int) Math.Round(gameState.MapWidth * 0.75f);
                y = (int) Math.Round(gameState.MapHeight * 0.25f);
                pos = CheckForWalls(new Position(x, y));
                return FindShortestPath(pac.Position, pos)[1];
            case 2:
                x = (int) Math.Round(gameState.MapWidth * 0.25f);
                y = (int) Math.Round(gameState.MapHeight * 0.25f);
                pos = CheckForWalls(new Position(x, y));
                return FindShortestPath(pac.Position, pos)[1];
            case 3:
                x = (int) Math.Round(gameState.MapWidth * 0.25f);
                y = (int) Math.Round(gameState.MapHeight * 0.75f);
                pos = CheckForWalls(new Position(x, y));
                return FindShortestPath(pac.Position, pos)[1];
            case 4:
                x = (int) Math.Round(gameState.MapWidth * 0.75f);
                y = (int) Math.Round(gameState.MapHeight * 0.75f);
                pos = CheckForWalls(new Position(x, y));
                return FindShortestPath(pac.Position, pos)[1];
        }
        throw new SyntaxErrorException("Quadrant must be a number 1-4");
    }

    public static Position CheckForWalls(Position pos)
    {
        if (gameState.Map[pos.x, pos.y] == MapItem.Wall)
        {
            // check the angles around the wall
            if (gameState.Map[pos.x + 1, pos.y + 1] != MapItem.Wall)
            {
                return new Position(pos.x + 1, pos.y + 1);
            }           
            if (gameState.Map[pos.x - 1, pos.y + 1] != MapItem.Wall)
            {
                return new Position(pos.x - 1, pos.y + 1);
            }            
            if (gameState.Map[pos.x - 1, pos.y - 1] != MapItem.Wall)
            {
                return new Position(pos.x - 1, pos.y - 1);
            }            
            // Default, I don't think there is a case where this can't happen.
            return new Position(pos.x + 1, pos.y - 1);
        }
        return pos;
        
    }

    public static Position[] FindShortestPath(Position start, Position end) 
    {
        
        // List containing of a Tuple that tracks where we should go, and where we've been.
        List<Tuple<Position, Position[]>> searchQueue = new List<Tuple<Position, Position[]>>();
        // We need to know if we've already been somewhere
        bool[,] breadCrumbs = new bool[gameState.MapWidth, gameState.MapHeight];
        // This will be what we return
        Position[] path = {};
        while (true)
        {
            // The current position, allow for wrapping around the map.
            start = new Position((start.x + gameState.MapWidth) % gameState.MapWidth, (start.y + gameState.MapHeight) % gameState.MapHeight, start.dist);
            // Util.PrintMap(gameState, start.x, start.y, end.x, end.y);
            // Mark that we've been here
            breadCrumbs[start.x, start.y] = true;
            if (start == end)
            {
                // we got it! return the path
                Console.Error.WriteLine($"Number of Moves = {start.dist}");
                return path.Length > 1 ? path : new[] {start, end};
            }

            if (gameState.Map[start.x, start.y] == MapItem.Wall)
            {
                start = searchQueue[0].Item1;
                path = searchQueue[0].Item2;
                searchQueue.RemoveAt(0);
                continue;
            }

            // Check if we've been there, then add to queue if not
            if (!breadCrumbs[(start.x - 1 + gameState.MapWidth) % gameState.MapWidth, start.y])
            {
                // check left
                searchQueue.Add(new Tuple<Position, Position[]>(new Position(start.x - 1, start.y, start.dist + 1), new List<Position>(path).Concat(new[] {start}).ToArray()));
            }            
            if (!breadCrumbs[(start.x + 1 + gameState.MapWidth) % gameState.MapWidth, start.y])
            {
                // check right
                searchQueue.Add(new Tuple<Position, Position[]>(new Position(start.x + 1, start.y, start.dist + 1), new List<Position>(path).Concat(new[] {start}).ToArray()));
            }            
            if (!breadCrumbs[start.x, (start.y - 1 + gameState.MapHeight) % gameState.MapHeight])
            {
                // check up
                searchQueue.Add(new Tuple<Position, Position[]>(new Position(start.x, start.y - 1, start.dist + 1), new List<Position>(path).Concat(new[] {start}).ToArray()));
            }           
            if (!breadCrumbs[start.x, (start.y + 1 + gameState.MapHeight) % gameState.MapHeight])
            {
                // check down
                searchQueue.Add(new Tuple<Position, Position[]>(new Position(start.x, start.y + 1, start.dist + 1), new List<Position>(path).Concat(new[] {start}).ToArray()));
            }
            
            // Hold up, this lambda is kinda nice c#. reminds me of home (javascript)
            searchQueue.Sort((a, b) =>
            {
                // make sure the first item in queue would be the next best path to take.
                float goalA = a.Item2.Length + Distance.Manhattan(end, a.Item1);
                float goalB = b.Item2.Length + Distance.Manhattan(end, b.Item1);
                return goalA < goalB ? -1 : 1;
            });

            // go to the next in queue
            start = searchQueue[0].Item1;
            path = searchQueue[0].Item2;
            // pop
            searchQueue.RemoveAt(0);
        }
    }

    public static void UpdateMap(List<Pellet> visablePellets, List<Friendly> myPacs, List<Enemy> visableEnemies)
    {
        // This will keep track of all the things we know about the map
        foreach (var pac in visableEnemies)
        {
            gameState.Map[pac.Position.x, pac.Position.y] = MapItem.Empty;
        }        
        foreach (var pac in myPacs)
        {
            gameState.Map[pac.Position.x, pac.Position.y] = MapItem.Empty;
        }
        foreach (var pellet in visablePellets)
        {
            if (pellet.value == 10)
            {
                gameState.Map[pellet.Position.x, pellet.Position.y] = MapItem.Super;
            }
            else
            {
                gameState.Map[pellet.Position.x, pellet.Position.y] = MapItem.Pellet;
            }
        }
    }
    
    public static bool isFriendlyPacTooClose(Friendly pac, List<Friendly> friendlies)
    {
        float threashhold = 4;
        foreach ( Friendly p in friendlies )
        {
            var distance = Distance.Manhattan(pac.Position, p.Position);
            if( distance < threashhold && !p.isAvoiding)
            {
                pac.avoidingPacId = p.id;
                return true;
            }
        }

        return false;
    }

    public static int isEnemyPacNearby(Friendly pac, List<Enemy> enemies)
    {
        float threashhold = 3;
        foreach ( Enemy p in enemies )
        {
            var distance = Distance.Manhattan(pac.Position, p.Position);
            if( distance < threashhold)
            {
                return p.id;
            }
        }

        return -1;
    }

    public static string ChangeToAvoiding(Friendly pac, List<Friendly> friendlyPacs)
    {
        pac.isAvoiding = true;
        pac.avoidingCooldown = 3;
        Position newQuadPos = FindNewSpot(pac, friendlyPacs.Find(p => p.id == pac.avoidingPacId));
        pac.hasGoal = true;
        pac.goal = newQuadPos;
        var nextStep = FindShortestPath(pac.Position, newQuadPos)[1];
        return $"Move {pac.id} {nextStep.x} {nextStep.y} A{nextStep.x}, {nextStep.y}";
    }

    public static string MoveToPellets(Friendly pac, List<Pellet> littlePelletList, List<Pellet> bigPelletList, List<Friendly> friendlyPacs)
    {
        if (isFriendlyPacTooClose(pac, friendlyPacs.FindAll(p => p.id != pac.id)))
        {
            // We don't want to double up on the same pellets if possible
            return ChangeToAvoiding(pac, friendlyPacs);
        }

//        if (pac.cooldown == 0)
//        {
//            return "Speed " + pac.id;
//        }

        Position closestPelletPos;
        // Prioritize the big pellets, may change this later to prioritize only if they are close.
        if (bigPelletList.Any())
        {
            // Console.Error.WriteLine("going to big pellet");
            closestPelletPos = FindClosestPellet(pac, bigPelletList);
            // All the big pellets are taken. find little ones instead.
            if (closestPelletPos.y == 0)
            {
                closestPelletPos = FindClosestPellet(pac, littlePelletList);
            }
        }
        else
        {
            closestPelletPos = FindClosestPellet(pac, littlePelletList);
            // TODO: if returned 0.0 pick a new place on the map.
        }
        // Console.Error.WriteLine(pac.id + "Moving to x: " + closestPelletPos.x + " y: " + closestPelletPos.y);
        return "Move " + pac.id + " " + closestPelletPos.x + " " + closestPelletPos.y + $" {closestPelletPos}";
    }

    static void Challenge(string[] inputs)
    {
              while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            // I need to use these eventually...
            int myScore = int.Parse(inputs[0]);
            int opponentScore = int.Parse(inputs[1]);
            int visiblePacCount = int.Parse(Console.ReadLine()); // all your pacs and enemy pacs in sight
            var friendlyPacs = new List<Friendly>();
            var enemyPacs = new List<Enemy>();
            for (int i = 0; i < visiblePacCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int pacId = int.Parse(inputs[0]); // pac number (unique within a team)
                bool mine = inputs[1] != "0"; // true if this pac is yours
                int x = int.Parse(inputs[2]); // position in the grid
                int y = int.Parse(inputs[3]); // position in the grid
                string typeId = inputs[4];
                int speedTurnsLeft = int.Parse(inputs[5]);
                int abilityCooldown = int.Parse(inputs[6]);
                if (mine) {
                    if (gameState.turn == 1)
                    {
                        var initFriendly = new Friendly();
                        initFriendly.SetInfo(pacId, mine, new Position(x, y), typeId, speedTurnsLeft, abilityCooldown);
                        friendlyPacs.Add(initFriendly);
                    }
                    else
                    {
                        var statePac = gameState.FriendlyPacs.Find(p => p.id == pacId);
                        var myPac = new Friendly(statePac.isAvoiding, statePac.avoidingCooldown, statePac.avoidingPacId, statePac.isPursuing, statePac.pursuingId, statePac.hasGoal, statePac.goal);
                        myPac.SetInfo(pacId, mine, new Position(x, y), typeId, speedTurnsLeft, abilityCooldown);
                        friendlyPacs.Add(myPac);
                    }
                }
                else 
                {
                    if (gameState.turn == 1)
                    {
                        var enemyPacInit = new Enemy(false, -1);
                        enemyPacInit.SetInfo(pacId, mine, new Position(x, y), typeId, speedTurnsLeft, abilityCooldown);
                        enemyPacs.Add(enemyPacInit);
                    }
                    else
                    {
                        if (gameState.EnemyPacs.Exists(p => p.id == pacId))
                        {
                            var statePac = gameState.EnemyPacs.Find(p => p.id == pacId);
                            var enemyPac = new Enemy(statePac.beingPursed, statePac.pursedById);
                            enemyPac.SetInfo(pacId, mine, new Position(x, y), typeId, speedTurnsLeft, abilityCooldown);
                            enemyPacs.Add(enemyPac);
                        }
                        else
                        {
                            // enemy found for the first time after turn 1
                            var enemyPacInit = new Enemy(false, -1);
                            enemyPacInit.SetInfo(pacId, mine, new Position(x, y), typeId, speedTurnsLeft, abilityCooldown);
                            enemyPacs.Add(enemyPacInit);
                        }

                    }
                    
                }
            }
            int visiblePelletCount = int.Parse(Console.ReadLine()); // all pellets in sight
            var bigPelletList = new List<Pellet>();
            var littlePelletList = new List<Pellet>();
            for (int i = 0; i < visiblePelletCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]);
                int value = int.Parse(inputs[2]); // amount of points this pellet is worth
                if (value == 10) {
                    bigPelletList.Add(new Pellet(new Position(x, y), value));
                } else 
                {
                    littlePelletList.Add(new Pellet(new Position(x, y), value));
                }
                
            }
               
            UpdateMap(littlePelletList.Concat(bigPelletList).ToList(), friendlyPacs, enemyPacs);
            // Write an action using Console.WriteLine()
            

            var commands = new List<string>();
            foreach (var pac in friendlyPacs)
            {
                if (pac.isPursuing)
                {
                    // This is unnecessarily aggresive, but it is fun to watch.
                    if (pac.cooldown == 0)
                    {
                        // gain on them!
                        commands.Add("Speed " + pac.id + " Pursuing");
                        continue;
                    }

                    // can we see it? target them.
                    if (enemyPacs.Exists(p => p.id == pac.pursuingId))
                    {
                        var target = enemyPacs.Find(p => p.id == pac.pursuingId);
                        if (pac.speedTurns > 0)
                        {
                            // aim ahead of them in case we are speeding
                            if (gameState.EnemyPacs.Exists(p => p.id == pac.pursuingId))
                            {
                                var targetLastTurn = gameState.EnemyPacs.Find(p => p.id == pac.pursuingId);
                                // I'm trying to predict where it will move next. Need will keep track of walls eventually.
                                var differenceX = target.Position.x - targetLastTurn.Position.x;
                                var differenceY = target.Position.y - targetLastTurn.Position.y;
                            
                                commands.Add("MOVE " + pac.id + " " + ((target.Position.x + differenceX) % gameState.MapWidth) + " " + ((target.Position.y + differenceY) % gameState.MapHeight) + " Pursuing");
                                continue;
                            }
                            // umm, not sure why we get here yet. Just going to default to look for pellet 
                            commands.Add(MoveToPellets(pac, littlePelletList, bigPelletList, friendlyPacs));
                            continue;

                        }

                        commands.Add("MOVE " + pac.id + " " + target.Position.x + " " + target.Position.y + " Pursuing");
                        continue;
                    }

                    // We can't see it, Move to last known position.
                    if (gameState.EnemyPacs.Exists(p => p.id == pac.pursuingId))
                    {
                        var target = gameState.EnemyPacs.Find(p => p.id == pac.pursuingId);
                        if (target.Position.x == pac.Position.x && target.Position.y == pac.Position.y)
                        {
                            // we lost it. go back to finding pellets
                            pac.isPursuing = false;
                            pac.pursuingId = -1;
                            target.beingPursed = false;
                            target.pursedById = -1;
                            commands.Add(MoveToPellets(pac, littlePelletList, bigPelletList, friendlyPacs));
                            continue;
                        }
                        commands.Add("MOVE " + pac.id + " " + target.Position.x + " " + target.Position.y + " Pursuing");
                        continue;
                    }

                    // Target nuetralized... Or we super lost it, I think?
                    pac.isPursuing = false;
                    pac.pursuingId = -1;
                    commands.Add(MoveToPellets(pac, littlePelletList, bigPelletList, friendlyPacs));
                    continue;
                }

                var nearbyEnemyId = isEnemyPacNearby(pac, enemyPacs);
                if (nearbyEnemyId != -1)
                {
                    var command = pac.RoShamBo(enemyPacs.Find(p => p.id == nearbyEnemyId));
                    if (command == "ignore")
                    {
                        commands.Add(MoveToPellets(pac, littlePelletList, bigPelletList, friendlyPacs));
                        continue;
                    }

                    commands.Add(command);
                    continue;
                }

                // This is what I need to work on, setting goals and moving towards them. this is incomplete atm
                if (pac.hasGoal)
                {
                    if (pac.Position == pac.goal)
                    {
                        // we made it, pick a new goal.
                        // find nearest unchecked spot and nearest known pellet.
                    }
                    
                    if (isFriendlyPacTooClose(pac, friendlyPacs.FindAll(p => p.id != pac.id)))
                    {
                        commands.Add(ChangeToAvoiding(pac, friendlyPacs));
                        continue;
                    }
                    var nextStep = FindShortestPath(pac.Position, pac.goal)[1];
                    commands.Add("MOVE " + pac.id + " " + nextStep.x + " " + nextStep.y + " Go");
                    continue;
                }
                
                // This is what I doing before I started tracking goals, this will probably be removed.
                if (!pac.isAvoiding)
                {
                    if (gameState.turn == 2 && pac.cooldown == 0)
                    {
                        commands.Add("Speed " + pac.id);
                        continue;
                    }
                    commands.Add(MoveToPellets(pac, littlePelletList, bigPelletList, friendlyPacs));
                    continue;
                }

                // default: avoid your friends and avoid collision. only sorta works.
                pac.avoidingCooldown--;
                if (pac.avoidingCooldown == 0)
                {
                    pac.isAvoiding = false;
                }
                Console.Error.WriteLine("going to fathest pellet");
                if (friendlyPacs.Exists(p => p.id == pac.avoidingPacId))
                {
                    Position newQuadPos = FindNewSpot(pac, friendlyPacs.Find(p => p.id == pac.avoidingPacId));
                    commands.Add("Move " + pac.id + " " + newQuadPos.x + " " + newQuadPos.y + " Avoid");
                    continue;
                }
                // I guess our friend died :'( time to continue on with a normal life.
                commands.Add(MoveToPellets(pac, littlePelletList, bigPelletList, friendlyPacs));
                
            }
            
            Console.WriteLine(String.Join("|", commands));
            gameState.FriendlyPacs = new List<Friendly>(friendlyPacs);
            gameState.EnemyPacs = new List<Enemy>(enemyPacs);
            gameState.turn++;
        }
    }
    
    static void Main(string[] args)
    {
        
        // The lines commented out in this function are used for local debugging.
//        string[] map =
//        {
//            "#################################",
//            "#   # #     #       #     # #   #",
//            "# # # # ### # ##### # ### # # # #",
//            "# #   #   #           #   #   # #",
//            "# ### # # # # # # # # # # # ### #",
//            "# #       # #       # #       # #",
//            "# # ##### # ######### # ##### # #",
//            "#     #                   #     #",
//            "##### # ### ### # ### ### # #####",
//            "#         # ### # ### #         #",
//            "# # ##### # ### # ### # ##### # #",
//            "# #     # #           # #     # #",
//            "##### # # # # ##### # # # # #####",
//            "###   #     #       #     #   ###",
//            "#################################"
//        };
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        
        int width = int.Parse(inputs[0]); // size of the grid
        int height = int.Parse(inputs[1]); // top left corner is (x=0, y=0)
//        int width = map[0].Length;
//        int height = map.Length;
        
        Console.Error.WriteLine($"Width: {width}, Height {height}");
        gameState.InitMap(width, height);
        // var map = new Map(width, height);
        for (int i = 0; i < height; i++)
        {
            var row = Console.ReadLine(); // one line of the grid: space " " is floor, pound "#" is wall
            // var row = map[i];
            Console.Error.WriteLine(row);
            // Build the map
            for (int j = 0; j < width; j++)
            {
                switch (row[j])
                {
                    case '#':
                        gameState.Map[j, i] = MapItem.Wall;
                        break;
                    case ' ':
                        gameState.Map[j, i] = MapItem.Unknown;
                        break;
                }
            }
        }
        // game loop
        Run("main", inputs);
        // Run("test", null);
        
    }
}