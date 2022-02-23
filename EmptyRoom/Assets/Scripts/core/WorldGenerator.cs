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
        return neighbors.Where(coord => (coord.x >= 0) & (coord.x < width) & (coord.y >= 0) & (coord.y < height)).ToList();
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
    public readonly int width;
    public readonly int height;
    public readonly int nr_rewards;

    public readonly double starting_percolation_prob;

    public readonly double final_percolation_prob;

    public readonly int percolation_steps;
    public double[,] score_matrix;
    public List<int[,]> stages;

    public List<IntCoordinates> rewards;
    public GameWorld(int width=25, int height=25, int nr_rewards=9, double starting_percolation_prob=1.0, double final_precolation_prob=0.64, int percolation_steps=5) {
        this.width = width;
        this.height = height;
        this.nr_rewards = nr_rewards;
        this.starting_percolation_prob = starting_percolation_prob;
        this.final_percolation_prob =final_precolation_prob;
        this.percolation_steps = percolation_steps;
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
        double threshold_step = (final_percolation_prob - starting_percolation_prob) / percolation_steps;
        for (int step_nr = 0; step_nr <= percolation_steps; step_nr++) {
            double threshold = starting_percolation_prob + step_nr * threshold_step;
            var stage = new int[width, height];
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    stage[x, y] = (score_matrix[x, y] > threshold) ? CELL_MARK_OCCUPIED: CELL_MARK_PASSABLE;
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

    public static ArrowData generate_arrow(int[, ] world_stage, IntCoordinates player_position, List<IntCoordinates> targets, float min_distance_to_player=1,float max_distance_to_player=7, Int32? seed = null, int nr_retries=20){
        Random rand_gen;
        if (seed.HasValue) {
            rand_gen = new Random(seed.Value);
        } else {
            rand_gen = new Random();
        }

        var random_target = new Vector2Substitute(targets[rand_gen.Next() % targets.Count]);

        var passable_cells = get_passable_cells_as_list(world_stage);
        for (int retry = 0; retry < nr_retries; retry++) {
            var origin = passable_cells[rand_gen.Next() % passable_cells.Count];
            var distance_to_player = (new Vector2Substitute(player_position.x - origin.x, player_position.y - origin.y)).magnitude();
            var is_good_choice = ((distance_to_player > min_distance_to_player) & (distance_to_player < max_distance_to_player));
            if  (is_good_choice || retry == nr_retries - 1) {
                var direction = new Vector2Substitute(random_target.x - origin.x, random_target.y - origin.y);
                direction.normalize();
                return new ArrowData(origin, direction);
            }
        }
        return null;
    }
}