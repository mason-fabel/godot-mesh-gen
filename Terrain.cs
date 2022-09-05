using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public class Terrain : Spatial
{
    private const int ROW_COUNT = 256;
    private const int COL_COUNT = 256;
    private const float QUAD_DIMENSION = 0.5f;
    private const float MAX_HEIGHT = 30.0f;


    private Node ActiveMesh { get; set; } = null;
    private OpenSimplexNoise Noise { get; }
    private RandomNumberGenerator Rng { get; }
    private SurfaceTool Surface { get; }


    public ulong? ElapsedMs  { get; private set; } = null;

    public Terrain() {

        Noise = new OpenSimplexNoise();
        Rng = new RandomNumberGenerator();
        Surface = new SurfaceTool();

        Noise.Octaves = 9;
    }

    public override void _Ready()
    {
        GenerateTerrain();
    }

    public void GenerateTerrain(int rows = ROW_COUNT, int cols = COL_COUNT, float quadSize = QUAD_DIMENSION, float maxHeight = MAX_HEIGHT) {
        ElapsedMs = null;
        var startTime = OS.GetTicksMsec();

        if (ActiveMesh != null) {
            RemoveChild(ActiveMesh);
            ActiveMesh = null;
        }

        Rng.Randomize();
        Noise.Seed = unchecked((int) Rng.Randi());

        var meshInstance = GenerateMesh(rows + 1, cols + 1, quadSize, maxHeight);

        ActiveMesh = meshInstance;
        AddChild(meshInstance);
    
        var endTime = OS.GetTicksMsec();
        ElapsedMs = endTime - startTime;
    }

    private MeshInstance GenerateMesh(int rows, int cols, float quadSize, float maxHeight) { 

        int GetIndex(int row, int col) {
            return col + (row * cols);
        }

        var xOffset = -1 * ((cols * quadSize) / 2) + (quadSize / 2);
        var yOffset = maxHeight / 2.0f;
        var zOffset = -1 * ((rows * quadSize) / 2) - (quadSize / 2);

        var verticies = new List<Vector3>();
        for (var row = rows; row > 0; row--) {
            for (var col = 0; col < cols; col++) {
                var vertex = new Vector3();

                vertex.x = col * quadSize + xOffset;
                vertex.z = row * quadSize + zOffset;

                vertex.y = Noise.GetNoise2d(vertex.x, vertex.z) * yOffset;

                verticies.Add(vertex);
            }
        }

        var indices = new List<int>();
        for (var row = 0; row < rows; row++) {
            for (var col = 0; col < cols; col++) {
                if (col != cols - 1 && row != rows - 1) {
                    indices.Add(GetIndex(row, col));
                    indices.Add(GetIndex(row + 1, col));
                    indices.Add(GetIndex(row, col + 1));
                }

                if (col != 0 && row != 0) {
                    indices.Add(GetIndex(row, col));
                    indices.Add(GetIndex(row - 1, col));
                    indices.Add(GetIndex(row, col - 1));
                }
            }
        }

        Vector3 Normal(Vector3 a, Vector3 b, Vector3 c) {
            return (b - a).Cross(c - a).Normalized();
        }

        var normals =  new List<Vector3>();
        for (var row = 0; row < rows; row++) {
            for (var col = 0; col < cols; col++) {
                if (col != cols - 1 && row != rows - 1) {
                    normals.Add(Normal(
                        verticies[GetIndex(row, col)],
                        verticies[GetIndex(row, col + 1)],
                        verticies[GetIndex(row + 1, col)]
                    ));
                } else if (row != rows - 1) {
                    normals.Add(Normal(
                        verticies[GetIndex(row, col)],
                        verticies[GetIndex(row, col - 1)],
                        verticies[GetIndex(row + 1, col)]
                    ));
                } else {
                    normals.Add(Normal(
                        verticies[GetIndex(row, col)],
                        verticies[GetIndex(row, col - 1)],
                        verticies[GetIndex(row - 1, col)]
                    ));
                }
            }
        }

        var arrayMesh = new ArrayMesh();

        var arrays = new Godot.Collections.Array();
        arrays.Resize((int) Mesh.ArrayType.Max);
        arrays[(int) Mesh.ArrayType.Vertex] = verticies.ToArray();
        arrays[(int) Mesh.ArrayType.Normal] = normals.ToArray();
        arrays[(int) Mesh.ArrayType.Index] = indices.ToArray();

        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        var meshInstance = new MeshInstance();
        meshInstance.Mesh = arrayMesh;

        return meshInstance;
    }
}