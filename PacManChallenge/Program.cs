using System;
using System.Linq;
using System.Collections.Generic;


/**
 * Grab the pellets as fast as you can!
 **/

public class Map {
    private List<Position> floorSpaces;
    private List<Position> nodes;

    private Dictionary<Position, int> getIndex;
    
    private Dictionary<Position, bool> isNode;
    private Dictionary<Position, bool> isDeadEnd;
    
    private Dictionary<Position, List<Position>> adjacentSpaces;
    private Dictionary<Position, List<Position>> adjacentNodes;
    
    private Dictionary<Position, List<Position>> closestSpaces;
    private Dictionary<Position, List<Position>> closestNodes;
    
    private Dictionary< Tuple<Position, Position>, List<Position>> shortestPaths;
    private Dictionary<Position, bool> isInDeadEndPath;
    
    private int width;
    private int height;
    private static int NUMBER_OF_CLOSEST_SPACES = 10;

    public Map(int[,] map)
    {
        floorSpaces = new List<Position>();
        nodes = new List<Position>();
        getIndex = new Dictionary<Position, int>();
        isNode = new Dictionary<Position, bool>();
        isDeadEnd = new Dictionary<Position, bool>();
        adjacentSpaces = new Dictionary<Position, List<Position>>();
        adjacentNodes = new Dictionary<Position, List<Position>>();
        closestSpaces = new Dictionary<Position, List<Position>>();
        closestNodes = new Dictionary<Position, List<Position>>();
        shortestPaths = new Dictionary<Tuple<Position, Position>, List<Position>>();
        isInDeadEndPath = new Dictionary<Position, bool>();
        
        width = map.GetLength(1);
        height = map.GetLength(0);
        
        PrepForCompute(map); //Build lists, bools, space adjacency
        RunFloydWarshall(); //Build shortest paths
        ComputeAdjacentNodes(); //Build node adjacency, dead end info
        
        //Populate the adjacentIntersections
        //Populate the closestNodes lists
        //Populate the shortestPaths
    }


    public bool InDeadEndPath(Position position)
    {
        return isInDeadEndPath[position];
    }
    
    public List<Position> getShortestPath(Position point1, Position point2)
    {
        return shortestPaths[new Tuple<Position, Position>(point1, point2)];
    }
    
    //Can speed up a tiny bit because some info is computed redundantly
    private void PrepForCompute(int[,] map)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (map[j, i] == 0)
                {
                    //Add to list of floor spaces
                    Position location = new Position(i,j);
                    floorSpaces.Add(location);
                    getIndex[location] = floorSpaces.Count - 1;
                    
                    if (i == 0 || i == width - 1)
                    {
                        //Compute adjacent spaces
                        List<Position > adjacencies = new List<Position>();
                        adjacencies.Add(new Position((i + 1) % width, j));
                        adjacencies.Add(new Position((i + width - 1) % width, j));
                        adjacentSpaces[location] = adjacencies;
                        
                        //Node or dead end
                        isNode[location] = false;
                        isDeadEnd[location] = false;

                    }
                    else
                    {
                        //Compute adjacent spaces
                        List<Position> adjacencies = new List<Position>();
                        if (map[j,i+1] == 0) 
                        {
                            adjacencies.Add(new Position(i+1,j));
                        }
                        if (map[j,i-1] == 0) 
                        {
                            adjacencies.Add(new Position(i-1,j));
                        }
                        if (map[j+1,i] == 0) 
                        {
                            adjacencies.Add(new Position(i,j+1));
                        }
                        if (map[j-1,i] == 0) 
                        {
                            adjacencies.Add(new Position(i,j-1));
                        }

                        adjacentSpaces[location] = adjacencies;
                        
                        //Node or dead end
                        int adjacentWallCount = map[j,i + 1] + map[j,i - 1] + map[ j + 1,i] + map[j - 1,i];
                        isNode[location] = (adjacentWallCount <= 1) || (adjacentWallCount >= 3);
                        isDeadEnd[location] = adjacentWallCount >= 3;

                        if (isNode[location])
                        {
                            nodes.Add(location);
                        }
                        
                    }
                    
                }
            }
        }
    }

    //Can speed this up some in the path building step by exploiting symmetry
    private void RunFloydWarshall()
    {
        //Initialize arrays
        int[,] dist = new int[floorSpaces.Count, floorSpaces.Count];
        int[,] next = new int[floorSpaces.Count, floorSpaces.Count];
        for (int i = 0; i < floorSpaces.Count; i++)
        {
            for (int j = 0; j < floorSpaces.Count; j++)
            {
                dist[j,i] = 100;
                next[j,i] = -1;
            }
        }
        
        //Adjacencies
        for (int i = 0; i < floorSpaces.Count; i++)
        {
            Position location1 = floorSpaces[i];
            List<Position> adjacencies = adjacentSpaces[location1];
            for (int j = 0; j < adjacencies.Count; j++)
            {
                Position location2 = adjacencies[j];
                dist[getIndex[location2], i] = 1;
                next[getIndex[location2], i] = getIndex[location2];
            }
        }
        
        //Reflexivity
        for (int i = 0; i < floorSpaces.Count; i++)
        {
            dist[i, i] = 0;
            next[i, i] = i;
        }
        
        //Floyd-Warshall
        for (int k = 0; k < floorSpaces.Count; k++)
        {
            for (int i = 0; i < floorSpaces.Count; i++)
            {
                for (int j = 0; j < floorSpaces.Count; j++)
                {
                    if (dist[j,i] > dist[k,i] + dist[j,k])
                    {
                        dist[j,i] = dist[k,i] + dist[j,k];
                        next[j,i] = next[k,i];
                    }
                }
            }
        }
        
        //Fill paths
        for (int i = 0; i < floorSpaces.Count; i++)
        {
            for (int j = 0; j < floorSpaces.Count; j++)
            {
                List<Position> path = new List<Position>();
                Position location1 = floorSpaces[i];
                Position location2 = floorSpaces[j];
                if (next[j,i] != -1)
                {
                    int k = i;
                    path.Add(location1);
                    while (k != j)
                    {
                        k = next[j,k];
                        path.Add(floorSpaces[k]);
                    }
                }
                shortestPaths[new Tuple<Position, Position>(location1,location2)] = path;
            }
        }
    }

    private void ComputeAdjacentNodes()
    {
        foreach (var space in floorSpaces)
        {
            adjacentNodes[space] = new List<Position>();
        }
        
        //For each space, for each direction, explore in that direction using adjacency info
        //when you reach a new node, call that the adjacent node in that direction
        foreach (var space in floorSpaces)
        {
            List<Position> directions = new List<Position>();
            Position up = new Position(space.x, space.y - 1);
            Position down = new Position(space.x, space.y + 1);
            Position left = new Position(space.x - 1, space.y);
            Position right = new Position(space.x + 1, space.y);

            directions.Add(adjacentSpaces[space].Contains(up) ? up : null);
            directions.Add(adjacentSpaces[space].Contains(down) ? down : null);
            directions.Add(adjacentSpaces[space].Contains(left) ? left : null);
            directions.Add(adjacentSpaces[space].Contains(right) ? right : null);

            foreach (var direction in directions)
            {
                if (direction != null)
                {
                    Position previousSpace = space;
                    Position currentSpace = direction;
                    while (isNode[currentSpace] == false)
                    {
                        Position temp = currentSpace;
                        currentSpace = GetNextSpace(currentSpace, previousSpace);
                        previousSpace = temp;
                    }

                    adjacentNodes[space].Add(currentSpace);
                }
                else
                {
                    adjacentNodes[space].Add(null);
                }
            }
        }
        
        //Initialize isInDeadEndPath to false
        //Could not do it this way and just catch the exception in the public function...
        //But that's gross
        foreach (var space in floorSpaces)
        {
            isInDeadEndPath.Add(space,false);
        }
        
        //For each dead end, find its unique adjacent node, find the path between them
        //Mark every space in that path except for the adjacent node as "in a dead end"
        foreach (var deadEnd in nodes)
        {
            if (isDeadEnd[deadEnd])
            {
                Position adjacentNode = null;
                foreach (var position in adjacentNodes[deadEnd])
                {
                    if (position != null)
                    {
                        adjacentNode = position;
                    }
                }

                List<Position> path = getShortestPath(deadEnd, adjacentNode);
                path.RemoveAt(path.Count-1);
                foreach (var position in path)
                {
                    isInDeadEndPath[position] = true;
                }
            }
        }
    }

    //Only works if currentSpace is not a node!
    //Very specific function, do not touch.
    private Position GetNextSpace(Position currentSpace, Position previousSpace)
    {
        List<Position> adj = adjacentSpaces[currentSpace];
        if (adjacentSpaces[currentSpace][0] == previousSpace)
        {
            return adjacentSpaces[currentSpace][1];
        }
        else
        {
            return adjacentSpaces[currentSpace][0];
        }
    }

    private enum Direction {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3
    }
}

public enum MapItem
{
    Wall = -1,
    Empty = 0,
    Unknown = 1,
    Pellet = 2,
    Super = 3
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

    public Position(int posX, int posY)
    {
        x = posX;
        y = posY;
    }

    public static bool operator ==(Position a, Position b)
    {
        if (ReferenceEquals(a, null))
        {
            if (ReferenceEquals(b, null))
            {
                return true;
            }
            return false;
        }

        if (ReferenceEquals(b,null))
        {
            return false;
        }
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
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Position) obj);
    }

    protected bool Equals(Position other)
    {
        return x == other.x && y == other.y;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (x * 397) ^ y;
        }
    }
}

public class Pellet
{
    public Position Position { get; set; }
    public int value;

    public int lastSeen;
    // public bool chosen;

    public Pellet(Position position, int value)
    {
        Position = position;
        this.value = value;
        lastSeen = 0;
    }
}

public enum PacType
{
    ROCK,
    PAPER,
    SCISSORS,
    DEAD
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
    public Goal goal;
    public bool isWaiting;


    public Friendly()
    {
        isAvoiding = false;
        avoidingCooldown = 0;
        isPursuing = false;
        pursuingId = -1;
        avoidingPacId = -1;
        hasGoal = false;
        goal = null;
        isWaiting = false;
    }
    public Friendly(bool isAvoiding, int avoidingCooldown, int avoidingPacId, bool isPursuing, int pursuingId, bool hasGoal, Goal goal, bool waiting)
    {
        this.isAvoiding = isAvoiding;
        this.avoidingCooldown = avoidingCooldown;
        this.avoidingPacId = avoidingPacId;
        this.isPursuing = isPursuing;
        this.pursuingId = pursuingId;
        this.hasGoal = hasGoal;
        this.goal = goal;
        isWaiting = waiting;
    }

    // return a command
    public string RoShamBo(Enemy enemy, List<Friendly> friendlyPacs)
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
                       if (cooldown == 0 && enemy.cooldown > 0)
                       {
                           return "SWITCH " + id + " PAPER";
                       }
                       if (cooldown > 0 && enemy.cooldown == 0)
                       {
                           return AvoidEnemy(enemy, friendlyPacs);
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
                       return AvoidEnemy(enemy, friendlyPacs);
                   case PacType.SCISSORS:
                       if (enemy.cooldown > 0)
                       {
                           isPursuing = true;
                           pursuingId = enemy.id;
                           enemy.beingPursed = true;
                           enemy.pursedById = id;
                           return "MOVE " + id + " " + enemy.Position.x + " " + enemy.Position.y + " Pursuing";
                       }

                       return "wait";
                }
                break;
            case PacType.PAPER:
                switch (enemy.type)
                {
                    case PacType.ROCK:
                        Console.Error.WriteLine(enemy.cooldown);
                        if (enemy.cooldown > 0)
                        {
                            isPursuing = true;
                            pursuingId = enemy.id;
                            enemy.beingPursed = true;
                            enemy.pursedById = id;
                            return "MOVE " + id + " " + enemy.Position.x + " " + enemy.Position.y + " Pursuing";
                        }
                        return "wait";
                    case PacType.PAPER:
                        if (cooldown == 0 && enemy.cooldown > 0)
                        {
                            isPursuing = true;
                            pursuingId = enemy.id;
                            enemy.beingPursed = true;
                            enemy.pursedById = id;
                            return "SWITCH " + id + " SCISSORS";
                        }

                        if (cooldown > 0 && enemy.cooldown == 0)
                        {
                            return AvoidEnemy(enemy, friendlyPacs);
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
                        return AvoidEnemy(enemy, friendlyPacs);
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
                        return AvoidEnemy(enemy, friendlyPacs);

                    case PacType.PAPER:
                        if (enemy.cooldown > 0)
                        {
                            isPursuing = true;
                            pursuingId = enemy.id;
                            enemy.beingPursed = true;
                            enemy.pursedById = id;
                            return "MOVE " + id + " " + enemy.Position.x + " " + enemy.Position.y + " Pursuing";
                        }
                        return "wait";
                    case PacType.SCISSORS:
                        if (cooldown == 0 && enemy.cooldown > 0)
                        {
                            isPursuing = true;
                            pursuingId = enemy.id;
                            enemy.beingPursed = true;
                            enemy.pursedById = id;
                            return "SWITCH " + id + " ROCK";
                        }
                        if (cooldown > 0 && enemy.cooldown == 0)
                        {
                            return AvoidEnemy(enemy, friendlyPacs);
                        }
                        return "ignore";
                }
                break;
        }
        return "ignore";
    }

    public string AvoidEnemy(Enemy enemy, List<Friendly> friendlyPacs)
    {
        goal = Player.FindAvoidingPath(this, enemy.Position, friendlyPacs);
        hasGoal = true;
        return $"Move {id} {goal.position.x} {goal.position.y} A{goal.position}";
    }
}

public class Goal
{
    public int value;
    public Position position;
    public MapItem type;
    public int distance;
    public bool targeted;

    public Goal(Position position,  MapItem type)
    {
        this.position = position;
        this.type = type;
        targeted = false;
    }

    public void Update(Position pacPosition)
    {
        // distance = Player.FindShortestPath(pacPosition, position).Length;
        Console.Error.WriteLine(distance);
    }
}
public class GameState
{
    // This is where I keep track of where the game is at.
    // I plan on making this map the Class Map instead.
    // public MapItem[,] Map;
    public int MapWidth;
    public int MapHeight;
    public MapItem[,] ItemMap;
    public Map Map;
    public List<Friendly> FriendlyPacs;
    public List<Enemy> EnemyPacs;
    public bool[,] PacTrail;
    public int turn;
    public List<Goal> goals;
    public bool bigPelletsGone;

    public GameState()
    {
        FriendlyPacs = new List<Friendly>();
        EnemyPacs = new List<Enemy>();
        turn = 1;
        goals = new List<Goal>();
        bigPelletsGone = false;
    }
    
    public void InitItemMap(int width, int height)
    {
        ItemMap = new MapItem[width,height];
        MapWidth = width;
        MapHeight = height;
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
        Position start = new Position(1, 10);
        Position end = new Position(32, 11);
        // Console.Error.WriteLine($"Starting at {start.x}, {start.y}");
        while (start != end)
        {
            var paths = gameState.Map.getShortestPath(start, end);
            Console.Error.WriteLine($"Next in path {paths[1]}");
            start = paths[1];
        }
    }
    public static GameState gameState = new GameState();

    public static bool IsCloserToOthers(Friendly pac, List<Friendly> friendlyPacs, Position p)
    {
        var closer = false;
            
        foreach (var friend in friendlyPacs)
        {
            if (gameState.Map.getShortestPath(p, friend.Position).Count > gameState.Map.getShortestPath(p, pac.Position).Count)
            {
                closer = true;
            }
        }

        return closer;
    }
    
    public static bool IsCloserTo(Friendly pac, Position otherPacPos, Position p)
    {
        return gameState.Map.getShortestPath(p, otherPacPos).Count >= gameState.Map.getShortestPath(p, pac.Position).Count;
    }
//    public static Position FindClosestPellet(Friendly pac, List<Pellet> pellets, List<Friendly> friendlyPacs)
//    {
//        var closerPellets = pellets.Where(p =>
//        {
//            var closer = false;
//            var lessThan10 = Distance.Manhattan(p.Position, pac.Position) < 10;
//            
//            foreach (var friend in friendlyPacs)
//            {
//                if (Distance.Manhattan(p.Position, friend.Position) > Distance.Manhattan(p.Position, pac.Position))
//                {
//                    closer = true;
//                }
//            }
//
//            return closer && lessThan10;
//        });
//        // default to make anything closest
//        int closestLength = int.MaxValue;
//        // The coordinates of the closest pellet
//        var cX = 0;
//        var cY = 0;
//        foreach ( Pellet p in closerPellets )
//        {
//            // var distance = Distance.Manhattan(pac.Position, p.Position);
//            var path = FindShortestPath(pac.Position, p.Position);
//            // Console.Error.WriteLine($"pellet at {p.Position.x}, {p.Position.y} for {pac.id} is {path.Length} moves away");
//            if( path.Length < closestLength)
//            {
//                closestLength = path.Length;
//                // Console.Error.WriteLine($"Pellet at {p.Position.x}, {p.Position.y} is shortest for ${pac.id}");
//                cX = p.Position.x;
//                cY = p.Position.y;
//            }
//        }
//        // Console.Error.WriteLine($"{pac.id} going for Pellet at {cX}, {cY}");
//        // return FindShortestPath(pac.Position, new Position(cX, cY))[1];
//        return new Position(cX, cY);
//    }
    
    public static int wrapIndex(int i, int i_max) {
        return ((i % i_max) + i_max) % i_max;
    }

    public static Goal FindAvoidingPath(Friendly pac, Position nearestPacPos, List<Friendly> friendlyPacs)
    {
        var potentialGoals = gameState.goals.Where(g => g.type != MapItem.Empty && g.type != MapItem.Wall);
        // We can order all the goals in time now with our incredible map.
        var goals = potentialGoals
            .OrderByDescending(g => IsCloserTo(pac, nearestPacPos, g.position))
            .ThenByDescending(g => (int) g.type)
            .ThenBy(g => gameState.Map.InDeadEndPath(g.position) && g.type != MapItem.Super)
            .ThenBy(g => gameState.Map.getShortestPath(pac.Position, g.position).Count);

        var tempList = goals.ToList();
        var attempts = 0;
        var path = gameState.Map.getShortestPath(pac.Position, tempList.First().position);
        while (path.Exists(p => p == nearestPacPos))
        {
            Console.Error.WriteLine($"goal at {tempList.First().position} is in the enemy path, removing");
            // remove it from the list of goals
            tempList = tempList.Where(g => g.position != tempList.First().position).ToList();
            path = gameState.Map.getShortestPath(pac.Position, tempList.First().position);
            attempts++;
            if (attempts >= 20)
            {
                // assume we are trapped...assume defeat
                return new Goal(pac.Position, MapItem.Empty);
            }
        }
        
        // Update the state goal list so that other pacmen know it's targeted.
        tempList.First().targeted = true;
        var targetedGoal = tempList.First();
        gameState.goals[gameState.goals.FindIndex(x => x.position == targetedGoal.position)] = targetedGoal;
        
        return tempList.First();
    }

    public static List<Position> CheckForEatenPellets(Position pacPosition, List<Pellet> visablePellets)
    {
        var eatenPellets = new List<Position>();

        // first search up
        var check = new Position(pacPosition.x, wrapIndex(pacPosition.y - 1, gameState.MapHeight));
        while (gameState.ItemMap[check.x, check.y] != MapItem.Wall)
        {
            if (gameState.ItemMap[check.x, check.y] == MapItem.Pellet && !visablePellets.Exists(p => p.Position == check))
            {
                Console.Error.WriteLine($"notice pellet eaten at {check}");
                eatenPellets.Add(check);
            }
            if (gameState.ItemMap[check.x, check.y] == MapItem.Unknown && !visablePellets.Exists(p => p.Position == check))
            {
                Console.Error.WriteLine($"notice pellet eaten at {check}");
                eatenPellets.Add(check);
            }
            check = new Position(check.x, wrapIndex(check.y - 1, gameState.MapHeight));
        }        
        // search down
        check = new Position(pacPosition.x, wrapIndex(pacPosition.y + 1, gameState.MapHeight));
        while (gameState.ItemMap[check.x, check.y] != MapItem.Wall)
        {
            if (gameState.ItemMap[check.x, check.y] == MapItem.Pellet && !visablePellets.Exists(p => p.Position == check))
            {
                Console.Error.WriteLine($"notice pellet eaten at {check}");
                eatenPellets.Add(check);
            }
            if (gameState.ItemMap[check.x, check.y] == MapItem.Unknown && !visablePellets.Exists(p => p.Position == check))
            {
                Console.Error.WriteLine($"notice pellet eaten at {check}");
                eatenPellets.Add(check);
            }
            check = new Position(check.x, wrapIndex(check.y + 1, gameState.MapHeight));
        }        
        // search left
        check = new Position(wrapIndex(pacPosition.x - 1, gameState.MapWidth), pacPosition.y);
        Console.Error.WriteLine(check);
        while (gameState.ItemMap[check.x, check.y] != MapItem.Wall)
        {
            if (gameState.ItemMap[check.x, check.y] == MapItem.Pellet && !visablePellets.Exists(p => p.Position == check))
            {
                Console.Error.WriteLine($"notice pellet eaten at {check}");
                eatenPellets.Add(check);
            }
            if (gameState.ItemMap[check.x, check.y] == MapItem.Unknown && !visablePellets.Exists(p => p.Position == check))
            {
                Console.Error.WriteLine($"notice pellet eaten at {check}");
                eatenPellets.Add(check);
            }
            check = new Position(wrapIndex(check.x - 1, gameState.MapWidth), check.y);
            Console.Error.WriteLine(check);
        }     
        // search right
        check = new Position(wrapIndex(pacPosition.x + 1, gameState.MapWidth), pacPosition.y);
        while (gameState.ItemMap[check.x, check.y] != MapItem.Wall)
        {
            if (gameState.ItemMap[check.x, check.y] == MapItem.Pellet && !visablePellets.Exists(p => p.Position == check))
            {
                Console.Error.WriteLine($"notice pellet eaten at {check}");
                eatenPellets.Add(check);
            }
            if (gameState.ItemMap[check.x, check.y] == MapItem.Unknown && !visablePellets.Exists(p => p.Position == check))
            {
                Console.Error.WriteLine($"notice pellet eaten at {check}");
                eatenPellets.Add(check);
            }
            check = new Position(wrapIndex(check.x + 1, gameState.MapWidth), check.y);
        }
        

        return eatenPellets;
    }

//    public static Position[] FindShortestPath(Position start, Position end) 
//    {
//        
//        // List containing of a Tuple that tracks where we should, and where we've been.
//        List<Tuple<Position, Position[]>> searchQueue = new List<Tuple<Position, Position[]>>();
//        // We need to know if we've already been somewhere
//        bool[,] breadCrumbs = new bool[gameState.MapWidth, gameState.MapHeight];
//        // This will be what we return
//        Position[] path = {};
//        while (true)
//        {
//            // The current position, allow for wrapping around the map.
//            start = new Position((start.x + gameState.MapWidth) % gameState.MapWidth, (start.y + gameState.MapHeight) % gameState.MapHeight, start.dist);
//            // Util.PrintMap(gameState, start.x, start.y, end.x, end.y);
//            // Mark that we've been here
//            breadCrumbs[start.x, start.y] = true;
//            if (start == end)
//            {
//                // we got it! return the path
//                // Console.Error.WriteLine($"Number of Moves = {start.dist}");
//                return path.Length > 1 ? path : new[] {start, end};
//            }
//
//            if (gameState.Map[start.x, start.y] == MapItem.Wall)
//            {
//                start = searchQueue[0].Item1;
//                path = searchQueue[0].Item2;
//                searchQueue.RemoveAt(0);
//                continue;
//            }
//
//            // Check if we've been there, then add to queue if not
//            if (!breadCrumbs[(start.x - 1 + gameState.MapWidth) % gameState.MapWidth, start.y])
//            {
//                // check left
//                searchQueue.Add(new Tuple<Position, Position[]>(new Position(start.x - 1, start.y, start.dist + 1), new List<Position>(path).Concat(new[] {start}).ToArray()));
//            }            
//            if (!breadCrumbs[(start.x + 1 + gameState.MapWidth) % gameState.MapWidth, start.y])
//            {
//                // check right
//                searchQueue.Add(new Tuple<Position, Position[]>(new Position(start.x + 1, start.y, start.dist + 1), new List<Position>(path).Concat(new[] {start}).ToArray()));
//            }            
//            if (!breadCrumbs[start.x, (start.y - 1 + gameState.MapHeight) % gameState.MapHeight])
//            {
//                // check up
//                searchQueue.Add(new Tuple<Position, Position[]>(new Position(start.x, start.y - 1, start.dist + 1), new List<Position>(path).Concat(new[] {start}).ToArray()));
//            }           
//            if (!breadCrumbs[start.x, (start.y + 1 + gameState.MapHeight) % gameState.MapHeight])
//            {
//                // check down
//                searchQueue.Add(new Tuple<Position, Position[]>(new Position(start.x, start.y + 1, start.dist + 1), new List<Position>(path).Concat(new[] {start}).ToArray()));
//            }
//            
//            // Hold up, this lambda is kinda nice c#. reminds me of home (javascript)
//            searchQueue.Sort((a, b) =>
//            {
//                // make sure the first item in queue would be the next best path to take.
//                float goalA = a.Item2.Length + Distance.Manhattan(end, a.Item1);
//                float goalB = b.Item2.Length + Distance.Manhattan(end, b.Item1);
//                return goalA < goalB ? -1 : 1;
//            });
//
//            // go to the next in queue
//            start = searchQueue[0].Item1;
//            path = searchQueue[0].Item2;
//            // pop
//            searchQueue.RemoveAt(0);
//        }
//    }

    public static void UpdateMap(List<Pellet> visablePellets, List<Friendly> myPacs, List<Enemy> visableEnemies)
    {
        // This will keep track of all the things we know about the map
        // We need to update what we can see first
        foreach (var pellet in visablePellets)
        {
            if (pellet.value == 10)
            {
                gameState.ItemMap[pellet.Position.x, pellet.Position.y] = MapItem.Super;
                
                var updatedGoal = new Goal(pellet.Position, MapItem.Super);
                gameState.goals[gameState.goals.FindIndex(x => x.position == pellet.Position)] = updatedGoal;
            }
            else
            {
                gameState.ItemMap[pellet.Position.x, pellet.Position.y] = MapItem.Pellet;
                var updatedGoal = new Goal(pellet.Position, MapItem.Pellet);
                gameState.goals[gameState.goals.FindIndex(x => x.position == pellet.Position)] = updatedGoal;
            }
        }
        // Then which enemies we can see
        foreach (var pac in visableEnemies)
        {
            gameState.ItemMap[pac.Position.x, pac.Position.y] = MapItem.Empty;
            var updatedGoal = new Goal(pac.Position, MapItem.Empty);
            gameState.goals[gameState.goals.FindIndex(x => x.position == pac.Position)] = updatedGoal;
        }   
        // Then where we are.
        foreach (var pac in myPacs)
        {
            gameState.ItemMap[pac.Position.x, pac.Position.y] = MapItem.Empty;
            var updatedGoal = new Goal(pac.Position, MapItem.Empty);
            gameState.goals[gameState.goals.FindIndex(x => x.position == pac.Position)] = updatedGoal;
            // now update what we CANT see
            List<Position> eatenPelletsPositions = CheckForEatenPellets(pac.Position, visablePellets);
            foreach (var p in eatenPelletsPositions)
            {
                updatedGoal = new Goal(p, MapItem.Empty);
                gameState.goals[gameState.goals.FindIndex(x => x.position == p)] = updatedGoal;
            }
        }
       

        if (!visablePellets.Exists(p => p.value == 10) && !gameState.bigPelletsGone)
        {
            // all the big pellets have been eaten, this needs to be set in case enemies ate the last one.
            gameState.bigPelletsGone = true;
            List<Goal> goalsToBeChanged = gameState.goals.Where(g => g.type == MapItem.Super).ToList();

            foreach (var goal in goalsToBeChanged)
            {
                var updatedGoal = new Goal(goal.position, MapItem.Empty);
                gameState.goals[gameState.goals.FindIndex(x => x.position == goal.position)] = updatedGoal;
            }
        }
    }
    
    public static bool isFriendlyPacTooClose(Friendly pac, List<Friendly> friendlies)
    {
        float threashhold = 3;
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
            var distance = gameState.Map.getShortestPath(pac.Position, p.Position).Count - 1;
            Console.Error.WriteLine(distance);
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
        pac.goal = FindAvoidingPath(pac, friendlyPacs.Find(p => p.id == pac.avoidingPacId).Position, friendlyPacs);
        pac.hasGoal = true;
        return $"Move {pac.id} {pac.goal.position.x} {pac.goal.position.y} A{pac.goal.position}";
    }

    public static Goal FindNewGoal(Friendly pac, List<Friendly> friendlyPacs)
    {
        
        var potentialGoals = gameState.goals.Where(g => g.type != MapItem.Empty && g.type != MapItem.Wall);
        // We can order all the goals in time now with our incredible map.
        var goals = potentialGoals
            .OrderByDescending(g => IsCloserToOthers(pac, friendlyPacs, g.position))
            .ThenByDescending(g => (int) g.type)
            .ThenBy(g => gameState.Map.InDeadEndPath(g.position) && g.type != MapItem.Super)
            .ThenBy(g => gameState.Map.getShortestPath(pac.Position, g.position).Count);
        
        
        var tempList = goals.ToList();
        // We don't want pacmen to pick the same goal.
        while (gameState.goals[gameState.goals.FindIndex(x => x.position == tempList.First().position)].targeted)
        {
            Console.Error.WriteLine($"goal at {goals.First().position} is taken, removing");
            // remove it from the list of goals
            tempList = tempList.Where(g => g.position != tempList.First().position).ToList();
        }

        // Update the state goal list so that other pacmen know it's targeted.
        tempList.First().targeted = true;
        var targetedGoal = tempList.First();
        gameState.goals[gameState.goals.FindIndex(x => x.position == targetedGoal.position)] = targetedGoal;
        
        return tempList.First();
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
                        // initFriendly.goals = gameState.goals;
                        friendlyPacs.Add(initFriendly);
                    }
                    else
                    {
                        var statePac = gameState.FriendlyPacs.Find(p => p.id == pacId);
                        var myPac = new Friendly(false, 0, -1, statePac.isPursuing, statePac.pursuingId, statePac.hasGoal, statePac.goal, statePac.isWaiting);
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

            var commands = new List<string>();
            foreach (var pac in friendlyPacs)
            {
                var thisPacFriendlies = friendlyPacs.FindAll(p => p.id != pac.id);
                if (gameState.turn == 1)
                {
                    commands.Add("Speed " + pac.id);
                    continue;
                }     
                var nearbyEnemyId = isEnemyPacNearby(pac, enemyPacs);
                if (nearbyEnemyId != -1)
                {
                    var command = pac.RoShamBo(enemyPacs.Find(p => p.id == nearbyEnemyId), thisPacFriendlies);
                    if (command == "ignore")
                    {
                        pac.goal = FindNewGoal(pac, thisPacFriendlies);
                        // Console.Error.WriteLine($"{pac.id} is going to {pac.goal.position} for {pac.goal.type}");
                        commands.Add("MOVE " + pac.id + " " + pac.goal.position.x + " " + pac.goal.position.y + $" G{pac.goal.position}");
                        continue;
                    }

                    if (command == "wait")
                    {
                        commands.Add("MOVE " + pac.id + " " + pac.Position.x + " " + pac.Position.y + $" W{pac.Position}");
                        continue;
                    }

                    commands.Add(command);
                    continue;
                }
                var pastPac = gameState.FriendlyPacs.Find(p => p.id == pac.id);
                if (pastPac.Position == pac.Position && pac.cooldown < 9 && !pac.isWaiting)
                {
                    // are we stuck by a friend?
                    if (isFriendlyPacTooClose(pac, thisPacFriendlies))
                    {
                        commands.Add(ChangeToAvoiding(pac, thisPacFriendlies));
                        continue;
                    }
                    // stuck by an enemy
                    commands.Add("MOVE " + pac.id + " " + pac.Position.x + " " + pac.Position.y + $" W{pac.Position}");
                    pac.isWaiting = true;
                    continue;
                }

                pac.isWaiting = false;
                if (pac.cooldown == 0 && enemyPacs.Count == 0)
                {
                    commands.Add("Speed " + pac.id);
                    continue;
                }
                

                pac.goal = FindNewGoal(pac, thisPacFriendlies);
                Console.Error.WriteLine($"{pac.id} is going to {pac.goal.position} for {pac.goal.type}");
                commands.Add("MOVE " + pac.id + " " + pac.goal.position.x + " " + pac.goal.position.y + $" G{pac.goal.position}");
            }
            
            Console.WriteLine(String.Join("|", commands));
            gameState.FriendlyPacs = new List<Friendly>(friendlyPacs);
            gameState.EnemyPacs = new List<Enemy>(enemyPacs);
            gameState.turn++;
            // reset the gamestate goals after the commands have been issued
            foreach (var pac in friendlyPacs)
            {
                if (pac.goal != null)
                {
                    var updatedGoal = pac.goal;
                    updatedGoal.targeted = false;
                    gameState.goals[gameState.goals.FindIndex(x => x.position == updatedGoal.position)] = updatedGoal;
                }
            }
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
//            "  #     # #           # #     #  ",
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
        
        // Console.Error.WriteLine($"Width: {width}, Height {height}");
        int[,] basicMap = new int[height,width];
        gameState.InitItemMap(width, height);
        // var map = new Map(width, height);
        for (int i = 0; i < height; i++)
        {
            var row = Console.ReadLine(); // one line of the grid: space " " is floor, pound "#" is wall
            // var row = map[i];
            // Console.Error.WriteLine(row);
            // Build the map
            for (int j = 0; j < width; j++)
            {
                switch (row[j])
                {
                    case '#':
                        basicMap[i, j] = 1;
                        gameState.ItemMap[j, i] = MapItem.Wall;
                        break;
                    case ' ':
                        basicMap[i, j] = 0;
                        gameState.ItemMap[j, i] = MapItem.Unknown;
                        gameState.goals.Add(new Goal(new Position(j,i), MapItem.Unknown));
                        break;
                }
            }
        }
        gameState.Map = new Map(basicMap);
        // game loop
        Run("main", inputs);
        // Run("test", null);
        
    }
}