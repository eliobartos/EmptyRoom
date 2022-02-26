using System;
using System.Collections.Generic;
using System.Linq;

class IntCoordinates {
    public int x;
    public int y;

    public IntCoordinates(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public List<IntCoordinates> neighbors(int width, int height) {
        var neighbors = new List<IntCoordinates>();
        foreach (int delta in new List<int>() {-1, 1}) {
            neighbors.Add(new IntCoordinates(x + delta, y));
            neighbors.Add(new IntCoordinates(x, y + delta));
        }
        return neighbors.Where(coord => (coord.x >= 0) && (coord.x < width) && (coord.y >= 0) && (coord.y < height)).ToList();
    }

    public int distance_to_other(IntCoordinates other) {
        return Math.Abs(x - other.x) + Math.Abs(y - other.y);
    }
}


/// <summary> Class for generating a single game world.
/// To generate a world, first set parameters in the constructor and then call <generate function cref="generate_world">.
/// You can then acces <stages of the world cref="stages"> (0 - no cells removed, 1 - some cells removed etc.) and the <list of reward positions cref="rewards">.
/// Other stuff should not be relevant for now, documentation will follow
///</summary>
class GameWorld {
    public  const int CELL_MARK_PASSABLE = 0;
    public const int CELL_MARK_OCCUPIED = 1;
    public const int CELL_MARK_PLACEHOLDER = -1;

    public const int CELL_MARK_REWARD = 2;
    public const int CELL_MARK_UNUSED_AFTER_THIS = 5;
    public int width;
    public int height;
    public int nr_rewards;
    public List<double> percolation_stages_probs;
    public double prefered_rewards_distance;
    public double[,] score_matrix;
    public List<int[,]> stages;

    public List<IntCoordinates> rewards;
    private void _init_block(List<double> percolation_stages_probs, int width, int height, int nr_rewards, double prefered_rewards_distance) {
        this.width = width;
        this.height = height;
        this.nr_rewards = nr_rewards;
        this.percolation_stages_probs = percolation_stages_probs;
        this.prefered_rewards_distance = prefered_rewards_distance;
    }
    public GameWorld(List<double> percolation_stages_probs, int width=25, int height=25, int nr_rewards=9, double prefered_rewards_distance=10) {
        _init_block(percolation_stages_probs, width, height, nr_rewards, prefered_rewards_distance);
    }

    public GameWorld(int width=25, int height=25, int nr_rewards=9, double starting_percolation_prob=1.0, double final_percolation_prob=0.64, int percolation_steps=5, double prefered_rewards_distance=10) {
        var stages_probs = new List<double>();
        double threshold_step = (final_percolation_prob - starting_percolation_prob) / percolation_steps;
        for (int step_nr = 0; step_nr <= percolation_steps; step_nr++) {
            stages_probs.Add(starting_percolation_prob + step_nr * threshold_step);
        }
        _init_block(stages_probs, width, height, nr_rewards, prefered_rewards_distance);
    }

    private void _generate_matrix(Random rand_gen) {
        score_matrix = new double[width, height];
        for(var x=0; x<width; x++) {
            for(var y=0; y < height; y++) {
                score_matrix[x, y] = rand_gen.NextDouble();
            }
        }
    }


    private int _count_marks(int[,] matrix, int mark) {
        int count = 0;
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (matrix[x, y] == mark) {
                    count += 1;
                }
            }
        }
        return count;
    }

    private void _polish_matrix(int[,] matrix) {
        int cluster_mark = CELL_MARK_UNUSED_AFTER_THIS + 1;
        int total_cluster_size = 0;
        int largest_cluster_size = 0;
        int largest_cluster_mark = CELL_MARK_PLACEHOLDER;
        int nr_passable_cells = _count_marks(matrix, CELL_MARK_PASSABLE);
        while (largest_cluster_size  < nr_passable_cells - total_cluster_size) {
            IntCoordinates start = GameWorldUtils.find_one_with_mark(matrix, CELL_MARK_PASSABLE);
            var cluster_size = GameWorldUtils.mark_cluster_and_return_size(matrix, start, cluster_mark);
            total_cluster_size += cluster_size;
            if (cluster_size > largest_cluster_size) {
                largest_cluster_size = cluster_size;
                largest_cluster_mark = cluster_mark;
            }
            cluster_mark += 1;
        }
        for (var x = 0; x < width; x++) {
            for (var y = 0; y < height; y++) {
                matrix[x, y] = matrix[x, y] == largest_cluster_mark ? CELL_MARK_PASSABLE : CELL_MARK_OCCUPIED;
            }
        }
    }

    private void _generate_stages() {
        stages = new List<int[,]>();
        foreach (var percolation_prob in percolation_stages_probs) {
            var stage = new int[width, height];
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    stage[x, y] = (score_matrix[x, y] > percolation_prob) ? CELL_MARK_OCCUPIED: CELL_MARK_PASSABLE;
                }
            }
            _polish_matrix(stage);
            stages.Add(stage);
        }
    }

    public void _generate_rewards(int nr_rewards, Random rand_generator, double prefered_distance) {
        rewards = GameWorldUtils.find_space_for_n_objects(stages.Last(), nr_rewards, prefered_distance);
    }

    public void generate_world(Int32? seed=null) {
        var rand_gen = (seed.HasValue) ? new Random(seed.Value) : new Random();
        _generate_matrix(rand_gen);
        _generate_stages();
        _generate_rewards(nr_rewards, rand_gen, prefered_rewards_distance);
    }

    public void _place_rewards_on_stages() {
        foreach (var reward in rewards) {
            foreach (var stage in stages) {
                if (stage[reward.x, reward.y] != CELL_MARK_PASSABLE) {
                    throw new Exception("Reward placed on occupied cell!");
                }
                stage[reward.x, reward.y] =  CELL_MARK_REWARD;
            }
        }
    }

    public static void print_world_matrix_to_console(int[, ] world){
        var width = world.GetLength(0);
        var height = world.GetLength(1);
        for (int x=0; x<width + 2; x++) {
            Console.Write("-");
        }
        Console.WriteLine();
        for (int y=height - 1; y >= 0; y--) {
            Console.Write("|");
            for (int x=0; x < width; x++) {
                if (world[x, y] == CELL_MARK_PASSABLE){
                    Console.Write(" ");
                } else if (world[x, y] == CELL_MARK_OCCUPIED) {
                    Console.Write("\x2588");
                } else if (world[x, y] == CELL_MARK_REWARD) {
                    Console.Write("\xB7");
                } else {
                    Console.Write(world[x, y]);
                }
            }
            Console.Write("|");
            Console.WriteLine();
        }
        for (int x=0; x<width + 2; x++) {
            Console.Write("-");
        }
    }

    public static void _print(int[, ] world_matrix, List<IntCoordinates> objects, int mark = CELL_MARK_REWARD) {
        var matrix = GameWorldUtils.copy_as_matrix_of_occupied_or_passable(world_matrix);
        foreach (var o in objects) {
            matrix[o.x, o.y] = mark;
        }
        print_world_matrix_to_console(matrix);
    }
    public static void _cluster(int[, ] world_matrix, List<IntCoordinates> objects) {
        _print(world_matrix, objects, mark: CELL_MARK_OCCUPIED);
    }
}

class Vector2Substitute{
    public float x;
    public float y;

    public Vector2Substitute(float x, float y){
        this.x = x;
        this.y = y;
    }

    public Vector2Substitute(IntCoordinates coodrinates) : this((float) coodrinates.x, (float) coodrinates.y) {}

    public float magnitude() {
        return (float) Math.Sqrt(x*x + y*y);
    }

    public void normalize() {
        var m = magnitude();
        x /= m;
        y /= m;
    }
}

class ArrowData {
    public IntCoordinates origin;
    public Vector2Substitute direction;

    public ArrowData(IntCoordinates origin, Vector2Substitute direction) {
        this.origin = origin;
        this.direction = direction;
    }
}

class GameWorldUtils {

    private const double PRECISION_TOLERANCE = 0.001;

    internal static IntCoordinates find_one_with_mark(int[,] matrix, int mark) {
        int width = matrix.GetLength(0);
        int height = matrix.GetLength(1);
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (matrix[x, y] == mark) {
                    return new IntCoordinates(x, y);
                }
            }
        }
        return null;
    }

    internal static int mark_cluster_and_return_size(int[,] matrix, IntCoordinates start, int mark) {
        var width = matrix.GetLength(0);
        var height = matrix.GetLength(1);
        if (matrix[start.x, start.y] == GameWorld.CELL_MARK_OCCUPIED) {
            return 0;
        }
        var cluster_size = 1;
        var to_visit = new Queue<IntCoordinates>();
        to_visit.Enqueue(start);
        matrix[start.x, start.y] = mark;
        while (to_visit.Count > 0) {
            var current = to_visit.Dequeue();
            var neighbors = current.neighbors(width, height);
            foreach (var n in neighbors) {
                if (matrix[n.x, n.y] == GameWorld.CELL_MARK_PASSABLE){
                    matrix[n.x, n.y] = mark;
                    to_visit.Enqueue(n);
                    cluster_size += 1;
                }
            }
        }
        return cluster_size;
    }

    public static List<IntCoordinates> get_passable_cells_as_list(int[,] world_stage) {
        var width = world_stage.GetLength(0);
        var height = world_stage.GetLength(1);

        var free_cells = new List<IntCoordinates>();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (world_stage[x, y] == GameWorld.CELL_MARK_PASSABLE) {
                    free_cells.Add(new IntCoordinates(x, y));
                }
            }
        }
        return free_cells;
    }

    public static int[, ] copy_as_matrix_of_occupied_or_passable(int[, ] game_world) {
        var width = game_world.GetLength(0);
        var height = game_world.GetLength(1);
        var matrix = new int[width, height];
        for (var x = 0; x < width; x++) {
            for (var y = 0; y < height; y++) {
                if (game_world[x, y] == GameWorld.CELL_MARK_PASSABLE) {
                    matrix[x, y] = GameWorld.CELL_MARK_PASSABLE;
                } else {
                    matrix[x, y] = GameWorld.CELL_MARK_OCCUPIED;
                }
            }
        }
        return matrix;
    }

    public static bool is_tile_separates_cluster(int[, ] game_world, IntCoordinates tile, int? cluster_size=null) {
        if (game_world[tile.x, tile.y] != GameWorld.CELL_MARK_PASSABLE) {
            return false;
        }
        var width = game_world.GetLength(0);
        var height = game_world.GetLength(1);
        var passable_neighbors = tile.neighbors(width, height).Where(n => game_world[n.x, n.y] == GameWorld.CELL_MARK_PASSABLE).ToList();
        if (passable_neighbors.Count == 0) {
            return false;
        }

        var _cluster_mark = GameWorld.CELL_MARK_UNUSED_AFTER_THIS + 142;
        cluster_size = cluster_size ?? mark_cluster_and_return_size(copy_as_matrix_of_occupied_or_passable(game_world), tile, _cluster_mark);

        var matrix = copy_as_matrix_of_occupied_or_passable(game_world);
        matrix[tile.x, tile.y] = GameWorld.CELL_MARK_OCCUPIED;
        var new_size = GameWorldUtils.mark_cluster_and_return_size(matrix, passable_neighbors.First(), _cluster_mark);

        return (new_size != cluster_size - 1);
    }


    internal class PlacementRequirement {
        public readonly Func<IntCoordinates, bool> requirement;
        public PlacementRequirement(Func<IntCoordinates, bool> requirement) {
            this.requirement = requirement;
        }

        public static PlacementRequirement from_obtacles(List<IntCoordinates> obstacles, double? min_distance=null, double? max_distance=null){
            Func<IntCoordinates, bool> requirement = coord => {
                foreach (var obstacle in obstacles) {
                    var distance = coord.distance_to_other(obstacle);
                    if (min_distance.HasValue && (distance < min_distance.Value)) {
                        return false;
                    }
                    if (max_distance.HasValue && (distance > max_distance.Value)) {
                        return false;
                    }
                }
                return true;
            };
            return new PlacementRequirement(requirement);
        }

        public static PlacementRequirement keep_world_connected(int[, ] game_world, List<IntCoordinates> obstacles) {
            Func<IntCoordinates, bool> requirement = coord => {
                var matrix = copy_as_matrix_of_occupied_or_passable(game_world);
                foreach (var obstacle in obstacles) {
                    matrix[obstacle.x, obstacle.y] = GameWorld.CELL_MARK_OCCUPIED;
                }
                return (!GameWorldUtils.is_tile_separates_cluster(matrix, coord));
            };
            return new PlacementRequirement(requirement);
        }
    }

    internal static IntCoordinates place_while_fulfilling_requirements(List<IntCoordinates> available_cells, List<PlacementRequirement> optional_requirements,
                                                                      List<PlacementRequirement> necessary_requirements = null, Int32? seed = null) {
        Random rand_gen;
        if (seed.HasValue) {
            rand_gen = new Random(seed.Value);
        } else {
            rand_gen = new Random();
        }
        if (necessary_requirements != null) {
            foreach (var placement_requirement in necessary_requirements) {
                available_cells = available_cells.Where(placement_requirement.requirement).ToList();
            }
            if (available_cells.Count == 0) {
                return null;
            }
        }
        foreach (var placement_requirement in optional_requirements) {
            var filtered = available_cells.Where(placement_requirement.requirement).ToList();
            if (filtered.Count == 0) {
                break;
            }
            available_cells = filtered;
        }
        return available_cells[rand_gen.Next() % available_cells.Count];
    }

    public static ArrowData generate_arrow(int[, ] world_stage, IntCoordinates player_position, List<IntCoordinates> targets, List<IntCoordinates> objects_to_avoid = null,
                                           double min_distance_to_player=3, double max_distance_to_player=7, double min_distance_to_targets=1,
                                           Int32? seed = null, int nr_retries=20){
        objects_to_avoid = objects_to_avoid ?? new List<IntCoordinates>();
        var player_position_list = new List<IntCoordinates>() {player_position};
        var placement_requirements = new List<PlacementRequirement>() {
            PlacementRequirement.from_obtacles(player_position_list, min_distance: PRECISION_TOLERANCE),
            PlacementRequirement.from_obtacles(objects_to_avoid, min_distance: PRECISION_TOLERANCE),
            PlacementRequirement.from_obtacles(targets, min_distance: PRECISION_TOLERANCE),
            PlacementRequirement.from_obtacles(player_position_list, min_distance: min_distance_to_player),
            PlacementRequirement.from_obtacles(targets, min_distance: min_distance_to_targets),
            PlacementRequirement.from_obtacles(player_position_list, max_distance: max_distance_to_player),
        };
        var origin = place_while_fulfilling_requirements(GameWorldUtils.get_passable_cells_as_list(world_stage), placement_requirements);

        var min_target = targets.First();
        var min_distance = origin.distance_to_other(min_target);
        for (int index = 1; index < targets.Count; index++) {
            var current_distance = origin.distance_to_other(targets[index]);
            if (current_distance < min_distance) {
                min_target = targets[index];
                min_distance = current_distance;
            }
        }

        var direction = new Vector2Substitute(min_target.x - origin.x, min_target.y - origin.y);
        direction.normalize();
        return new ArrowData(origin, direction);
    }

    public static List<IntCoordinates> find_space_for_n_objects(int[,] world_stage, int nr_objects, double min_distance_between_objects = PRECISION_TOLERANCE,
                                                                List<IntCoordinates> obstacles=null, double min_distance_to_obstacles = PRECISION_TOLERANCE,
                                                                IntCoordinates player_position=null, double min_distance_to_player=PRECISION_TOLERANCE,
                                                                bool keep_world_connected=false, Int32? seed=null) {
        obstacles = obstacles ?? new List<IntCoordinates>();
        var player_position_list = new List<IntCoordinates>();
        if (player_position != null) {
            player_position_list.Add(player_position);
        }

        var coordinates_to_return = new List<IntCoordinates>();
        var cluser_blocking_coordinates = new List<IntCoordinates>();

        var necessary_requirements = new List<PlacementRequirement>() {
            PlacementRequirement.from_obtacles(coordinates_to_return, min_distance: PRECISION_TOLERANCE),
            PlacementRequirement.from_obtacles(player_position_list, min_distance: PRECISION_TOLERANCE)
        };
        if (keep_world_connected) {
            necessary_requirements.Add(
                PlacementRequirement.keep_world_connected(world_stage, cluser_blocking_coordinates));
        }

        var optional_requirements = new List<PlacementRequirement>() {
            PlacementRequirement.from_obtacles(player_position_list, min_distance: min_distance_to_player),
            PlacementRequirement.from_obtacles(obstacles, min_distance: min_distance_to_obstacles),
            PlacementRequirement.from_obtacles(coordinates_to_return, min_distance: min_distance_between_objects)
        };

        var available_cells = GameWorldUtils.get_passable_cells_as_list(world_stage);
        for (int object_index = 0; object_index < nr_objects; object_index++) {
            var coord = place_while_fulfilling_requirements(available_cells, optional_requirements, necessary_requirements: necessary_requirements, seed: seed);
            if (coord != null) {
                coordinates_to_return.Add(coord);
                cluser_blocking_coordinates.Add(coord);
            }
        }
        return coordinates_to_return;
    }

    public class TileRectangle {
        public IntCoordinates bottom_left_corner;
        public int width;
        public int height;

        public TileRectangle(IntCoordinates bottom_left_corner, int width, int height) {
            this.bottom_left_corner = bottom_left_corner;
            this.width = width;
            this.height = height;
        }
        public List<IntCoordinates> get_occupied_coordinates() {
            List<IntCoordinates> occupied = new List<IntCoordinates>();
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    occupied.Add(new IntCoordinates(bottom_left_corner.x + x, bottom_left_corner.y + y));
                }
            }
            return occupied;
        }

        public bool is_contain_tile(IntCoordinates tile) {
            if ((tile.x < bottom_left_corner.x) || (tile.x >= bottom_left_corner.x + width)) {
                return false;
            }
            if ((tile.y < bottom_left_corner.y) || (tile.y >= bottom_left_corner.y + height)) {
                return false;
            }
            return true;
        }

        private List<IntCoordinates> other_corners(){
            return new List<IntCoordinates>() {
                new IntCoordinates(bottom_left_corner.x + width, bottom_left_corner.y),
                new IntCoordinates(bottom_left_corner.x + width, bottom_left_corner.y + height),
                new IntCoordinates(bottom_left_corner.x, bottom_left_corner.y + height)
            };
        }

        public double distance_to_tile(IntCoordinates tile) {
            if (is_contain_tile(tile)) {
                return .0;
            }
            if ((tile.x >= bottom_left_corner.x) && (tile.x < bottom_left_corner.x + width)) {
                return Math.Min(Math.Abs(tile.y - bottom_left_corner.y), Math.Abs(tile.y - bottom_left_corner.y - height));
            }
            if ((tile.y >= bottom_left_corner.y) && (tile.y < bottom_left_corner.y + height)) {
                return Math.Min(Math.Abs(tile.x - bottom_left_corner.x), Math.Abs(tile.y - bottom_left_corner.x - width));
            }
            var final_distance = bottom_left_corner.distance_to_other(tile);
            foreach (var corner in other_corners()) {
                var dist = corner.distance_to_other(tile);
                if (dist > final_distance) {
                    final_distance = dist;
                }
            }
            return final_distance;
        }
    }

    public static List<TileRectangle> find_n_free_tile_rectangles(int[, ] game_world, int rectangel_width, int rectangle_height, int nr_rectangles_to_find,
                                                                List<IntCoordinates> blocked_squares = null,
                                                                IntCoordinates player_coordinates = null, double min_distance_to_player = PRECISION_TOLERANCE, Int32? seed=null) {
        var world_width = game_world.GetLength(0);
        var world_height = game_world.GetLength(1);
        var player_coordinates_list = new List<IntCoordinates> ();
        if (player_coordinates == null) {
            player_coordinates_list.Add(player_coordinates);
        }
        blocked_squares = (blocked_squares == null) ? new List<IntCoordinates>() : new List<IntCoordinates> (blocked_squares);

        Func<IntCoordinates, bool> space_requirement = coord => {
            var rectangle = new TileRectangle(coord, rectangel_width, rectangle_height);
            for (int x = coord.x; x < coord.x + rectangel_width; x++) {
                for (int y = coord.y; y < coord.y + rectangle_height; y++) {
                    if ((x >= world_width) || (y >= world_height)) {
                        return false;
                    }
                    if (game_world[x, y] != GameWorld.CELL_MARK_PASSABLE) {
                        return false;
                    }
                }
            }
            foreach (var blocked in blocked_squares.Concat(player_coordinates_list).ToList()) {
                if (rectangle.is_contain_tile(blocked)) {
                    return false;
                }
            }
            return true;
        };


        Func<IntCoordinates, bool> player_requirement = coord => {
            var rectange = new TileRectangle(coord, rectangel_width, rectangle_height);
            if (player_coordinates == null) {
                return true;
            }
            var dist = rectange.distance_to_tile(player_coordinates);
            return (dist >= min_distance_to_player);
        };

        var necessary_requirements = new List<PlacementRequirement>() {
            new PlacementRequirement(space_requirement),
            new PlacementRequirement(player_requirement),
        };

        var optional_requirements = new List<PlacementRequirement>() {
        };

        var rectangles = new List<TileRectangle>();
        for (var index = 0; index < nr_rectangles_to_find; index++) {
            var bottom_left = place_while_fulfilling_requirements(GameWorldUtils.get_passable_cells_as_list(game_world), optional_requirements, necessary_requirements, seed);
            if (bottom_left != null) {
                var rect = new TileRectangle(bottom_left, rectangel_width, rectangle_height);
                rectangles.Add(rect);
                blocked_squares.AddRange(rect.get_occupied_coordinates());
            }
        }
        return rectangles;
    }
}