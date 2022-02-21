using System;
using System.Collections.Generic;
using System.Linq;

class Coordinates {
    public int x;
    public int y;

    public Coordinates(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public List<Coordinates> neighbors(int width, int height) {
        var neighbors = new List<Coordinates>();
        foreach (int delta in new List<int>() {-1, 1}) {
            neighbors.Add(new Coordinates(x + delta, y));
            neighbors.Add(new Coordinates(x, y + delta));
        }
        return neighbors.Where(coord => (coord.x >= 0) & (coord.x < width) & (coord.y >= 0) & (coord.y < height)).ToList();
    }

    public int distance_to_other(Coordinates other) {
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

    public List<Coordinates> rewards;
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

    private int _mark_cluster_and_return_size(int[,] matrix, Coordinates start, int mark) {
        if (matrix[start.x, start.y] == CELL_MARK_OCCUPIED) {
            return 0;
        }
        var cluster_size = 1;
        var to_visit = new Queue<Coordinates>();
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

    private Coordinates _find_one(int[,] matrix, int mark) {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (matrix[x, y] == mark) {
                    return new Coordinates(x, y);
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
            Coordinates start = _find_one(matrix, CELL_MARK_PASSABLE);
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
        double threshold_step = (final_percolation_prob - starting_percolation_prob) / (percolation_steps - 1);
        for (double threshold = starting_percolation_prob; threshold > final_percolation_prob; threshold += threshold_step) {
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

    public double compute_separation(List<Coordinates> rewards){
        double total = 0;
        for (int first = 0; first < rewards.Count; first++) {
            for (int second = first + 1; second < rewards.Count; second++) {
                total += rewards[first].distance_to_other(rewards[second]);
            }
        }
        return total / rewards.Count;
    }

    public void _generate_rewards(int nr_rewards, Random rand_generator, int nr_retries=10) {
        var free_cells = new List<Coordinates>();
        var final_stage = stages.Last();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (final_stage[x, y] == CELL_MARK_PASSABLE) {
                    free_cells.Add(new Coordinates(x, y));
                }
            }
        }
        double best_separation = 0;
        List<Coordinates> best_rewards = null;
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

    public void generate_world(Int32 seed=0) {
        var rand_gen = new Random(seed);
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