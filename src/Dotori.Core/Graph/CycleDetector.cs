namespace Dotori.Core.Graph;

/// <summary>
/// Detects cycles in a directed graph using iterative DFS (grey/black coloring).
/// </summary>
internal static class CycleDetector
{
    private enum Color { White, Grey, Black }

    /// <summary>
    /// Checks if the graph rooted at each node in <paramref name="adjacency"/> has a cycle.
    /// </summary>
    /// <param name="adjacency">Map from node to its direct successors.</param>
    /// <returns>The cycle path if found, or null if the graph is acyclic.</returns>
    internal static IReadOnlyList<string>? FindCycle(
        IReadOnlyDictionary<string, IReadOnlyList<string>> adjacency)
    {
        var color  = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);
        var parent = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var node in adjacency.Keys)
            color[node] = Color.White;

        foreach (var node in adjacency.Keys)
        {
            if (color[node] == Color.White)
            {
                var cycle = Dfs(node, adjacency, color, parent);
                if (cycle is not null) return cycle;
            }
        }

        return null;
    }

    private static IReadOnlyList<string>? Dfs(
        string start,
        IReadOnlyDictionary<string, IReadOnlyList<string>> adjacency,
        Dictionary<string, Color> color,
        Dictionary<string, string?> parent)
    {
        // Iterative DFS with explicit stack to avoid stack overflow on deep graphs
        var stack = new Stack<(string Node, bool Entering)>();
        stack.Push((start, true));
        parent[start] = null;

        while (stack.Count > 0)
        {
            var (node, entering) = stack.Pop();

            if (entering)
            {
                if (color.GetValueOrDefault(node) == Color.Black)
                    continue;

                color[node] = Color.Grey;
                stack.Push((node, false));  // push exit marker

                if (adjacency.TryGetValue(node, out var neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        if (!color.ContainsKey(neighbor))
                        {
                            // neighbor not in adjacency — no outgoing edges, treat as black
                            color[neighbor] = Color.Black;
                            continue;
                        }

                        if (color[neighbor] == Color.Grey)
                        {
                            // Found a back edge → cycle detected
                            parent[neighbor] = node;
                            return BuildCyclePath(neighbor, parent);
                        }

                        if (color[neighbor] == Color.White)
                        {
                            parent[neighbor] = node;
                            stack.Push((neighbor, true));
                        }
                    }
                }
            }
            else
            {
                // Exiting the node
                color[node] = Color.Black;
            }
        }

        return null;
    }

    private static IReadOnlyList<string> BuildCyclePath(
        string cycleStart,
        Dictionary<string, string?> parent)
    {
        var path = new List<string> { cycleStart };
        var current = parent[cycleStart];
        while (current is not null && current != cycleStart)
        {
            path.Add(current);
            current = parent.GetValueOrDefault(current);
        }
        path.Add(cycleStart);  // close the cycle
        path.Reverse();
        return path;
    }
}
