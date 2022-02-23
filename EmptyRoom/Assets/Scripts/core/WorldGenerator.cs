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
    public double[,] score_matrix;
    public List<int[,]> stages;

    public List<IntCoordinates> rewards;
    private void _init_block(List<double> percolation_stages_probs, int width, int height, int nr_rewards) {
        this.width = width;
        this.height = height;
        this.nr_rewards = nr_rewards;
        this.percolation_stages_probs = percolation_stages_probs;
    }
    public GameWorld(List<double> percolation_stages_probs, int width=25, int height=25, int nr_rewards=9) {
        _init_block(percolation_stages_probs, width, height, nr_rewards);
    }

    public GameWorld(int width=25, int height=25, int nr_rewards=9, double starting_percolation_prob=1.0, double final_percolation_prob=0.64, int percolation_steps=5) {
        var stages_probs = new List<double>();
        double threshold_step = (final_percolation_prob - starting_percolation_prob) / percolation_steps;
        for (int step_nr = 0; step_nr <= percolation_steps; step_nr++) {
            stages_probs.Add(starting_percolation_prob + step_nr * threshold_step);
        }
        _init_block(stages_probs, width, height, nr_rewards);
    }

    private void _generate_matrix(Random rand_gen) {
        score_matrix = new double[width, height];
        for(var x=0; x<width; x++) {
            for(var y=0; y < height; y++) {
                score_matrix[x, y] = rand_gen.NextDouble();
            }
        }
    }

    private int _mark_cluster_and_return_size(int[,] matrix, IntCoordinates start, int mark) {
        if (matrix[start.x, start.y] == CELL_MARK_OCCUPIED) {
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
                if (matrix[n.x, n.y] == CELL_MARK_PASSABLE){
                    matrix[n.x, n.y] = mark;
                    to_visit.Enqueue(n);
                    cluster_size += 1;
                }
            }
        }
        return cluster_size;
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

    private IntCoordinates _find_one(int[,] matrix, int mark) {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (matrix[x, y] == mark) {
                    return new IntCoordinates(x, y);
                }
            }
        }
        return null;
    }
    private void _polish_matrix(int[,] matrix) {
        int cluster_mark = CELL_MARK_UNUSED_AFTER_THIS + 1;
        int total_cluster_size = 0;
        int largest_cluster_size = 0;
        int largest_cluster_mark = CELL_MARK_PLACEHOLDER;
        int nr_passable_cells = _count_marks(matrix, CELL_MARK_PASSABLE);
        while (largest_cluster_size  < nr_passable_cells - total_cluster_size) {
            IntCoordinates start = _find_one(matrix, CELL_MARK_PASSABLE);
            var cluster_size = _mark_cluster_and_return_size(matrix, start, cluster_mark);
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

    public double compute_separation(List<IntCoordinates> rewards){
        double total = 0;
        for (int first = 0; first < rewards.Count; first++) {
            for (int second = first + 1; second < rewards.Count; second++) {
                total += rewards[first].distance_to_other(rewards[second]);
            }
        }
        return total / rewards.Count;
    }

    public void _generate_rewards(int nr_rewards, Random rand_generator, int nr_retries=10) {
        var free_cells = GameWorldUtils.get_passable_cells_as_list(stages.Last());

        double best_separation = 0;
        List<IntCoordinates> best_rewards = null;
        for (int retry = 0; retry < nr_retries; retry++) {
            var _random_rewards = free_cells.OrderBy(cell => rand_generator.Next()).Take(nr_rewards).ToList();
            var separation = compute_separation(_random_rewards);
            if (separation > best_separation) {
                best_rewards = _random_rewards;
                best_separation = separation;
            }
        }
        rewards = best_rewards;
    }

    public void generate_world(Int32? seed=null) {
        var rand_gen = (seed.HasValue) ? new Random(seed.Value) : new Random();
        _generate_matrix(rand_gen);
        _generate_stages();
        _generate_rewards(nr_rewards, rand_gen);
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

    private class PlacementRequirement {
        public readonly Func<IntCoordinates, bool> requirement;
        PlacementRequirement(Func<IntCoordinates, bool> requirement) {
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
    }

    private static IntCoordinates place_an_object_with_requirements(int[,] world_stage, List<PlacementRequirement> placement_requirements, Int32? seed = null) {
        Random rand_gen;
        if (seed.HasValue) {
            rand_gen = new Random(seed.Value);
        } else {
            rand_gen = new Random();
        }

        var passable_cells = get_passable_cells_as_list(world_stage);

        foreach (var placement_requirement in placement_requirements) {
            var filtered = passable_cells.Where(placement_requirement.requirement).ToList();
            if (filtered.Count == 0) {
                break;
            }
            passable_cells = filtered;
        }
        return passable_cells[rand_gen.Next() % passable_cells.Count];
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
        var origin = place_an_object_with_requirements(world_stage, placement_requirements);

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
}