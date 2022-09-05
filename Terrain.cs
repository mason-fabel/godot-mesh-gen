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
        // Noise.Period = 20;
        // Noise.Persistence = 0.8f;
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

        // var meshInstance = GenerateTerrainImpl(rows, cols, quadSize, maxHeight);
        var meshInstance = GenerateTerrainImplV2(rows, cols, quadSize, maxHeight);
        // var meshInstance = GenerateTerrainImplV3(rows, cols, quadSize, maxHeight);

        ActiveMesh = meshInstance;
        AddChild(meshInstance);
    
        var endTime = OS.GetTicksMsec();
        ElapsedMs = endTime - startTime;
    }

    private MeshInstance GenerateTerrainImplV3(int rows, int cols, float quadSize, float maxHeight) { 
        SeedRng();

        var xOffset = -1 * ((cols * quadSize) / 2);
        var yOffset = maxHeight / 2.0f;
        var zOffset = -1 * ((rows * quadSize) / 2);

        var verticies = new List<Vector3>();
        for (var row = rows; row >= 0; row--) {
            for (var col = 0; col <= cols; col++) {
                var vertex = new Vector3();

                vertex.x = col * quadSize + xOffset;
                vertex.z = row * quadSize + zOffset;

                // vertexPosition.y = 0.0f;
                vertex.y = Noise.GetNoise2d(vertex.x, vertex.z) * yOffset + yOffset;

                verticies.Add(vertex);
            }
        }

        var indices = new List<int>();
        for (var row = 0; row < rows; row++) {
            for (var col = 0; col < cols; col++) {
                // lower index is the index of the X in the following picture:
                // +
                // |\
                // X-+
                var lowerIndex = (row * cols) + row + col;
                indices.Add(lowerIndex);
                indices.Add(lowerIndex + cols + 1);
                indices.Add(lowerIndex + 1);

                // upper index is the index of the X in the following picture:
                // +-X
                //  \|
                //   +
                var upperIndex = ((row + 1) * cols) + (row + 1) + (col + 1);
                indices.Add(upperIndex);
                indices.Add(upperIndex - (cols + 1));
                indices.Add(upperIndex - 1);
            }
        }

        var arrayMesh = new ArrayMesh();

        var arrays = new Godot.Collections.Array();
        arrays.Resize((int) Mesh.ArrayType.Max);
        arrays[(int) Mesh.ArrayType.Vertex] = verticies.ToArray();
        arrays[(int) Mesh.ArrayType.Index] = indices.ToArray();

        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        var meshInstance = new MeshInstance();
        meshInstance.Mesh = arrayMesh;

        return meshInstance;
    }

    private MeshInstance GenerateTerrainImplV2(int rows, int cols, float quadSize, float maxHeight) {
        SeedRng();

        Surface.Begin(Mesh.PrimitiveType.Triangles);

        var xOffset = -1 * ((cols * quadSize) / 2);
        var yOffset = maxHeight / 2.0f;
        var zOffset = -1 * ((rows * quadSize) / 2);
        var vertexPosition = new Vector3();

        for (var row = rows; row >= 0; row--) {
            for (var col = 0; col <= cols; col++) {
                vertexPosition.x = col * quadSize + xOffset;
                vertexPosition.z = row * quadSize + zOffset;

                // vertexPosition.y = 0.0f;
                vertexPosition.y = Noise.GetNoise2d(vertexPosition.x, vertexPosition.z) * yOffset + yOffset;

                Surface.AddVertex(vertexPosition);
            }
        }

        for (var row = 0; row < rows; row++) {
            for (var col = 0; col < cols; col++) {
                // lower index is the index of the X in the following picture:
                // +
                // |\
                // X-+
                var lowerIndex = (row * cols) + row + col;
                Surface.AddIndex(lowerIndex);
                Surface.AddIndex(lowerIndex + cols + 1);
                Surface.AddIndex(lowerIndex + 1);

                // upper index is the index of the X in the following picture:
                // +-X
                //  \|
                //   +
                var upperIndex = ((row + 1) * cols) + (row + 1) + (col + 1);
                Surface.AddIndex(upperIndex);
                Surface.AddIndex(upperIndex - (cols + 1));
                Surface.AddIndex(upperIndex - 1);
            }
        }

        Surface.GenerateNormals();

        var meshInstance = new MeshInstance();
        meshInstance.Mesh = Surface.Commit();

        return meshInstance;
    }

    private MeshInstance GenerateTerrainImpl(int rows, int cols, float quadSize, float maxHeight) {
        SeedRng();

        Surface.Begin(Mesh.PrimitiveType.Triangles);
        //Surface.AddSmoothGroup(true);

        var baseIndex = 0;
        for (var row = 0; row < rows; row++) {
            for (var column = 0; column < cols; column++) {
                baseIndex = GenerateQuad(row, column, baseIndex, rows, cols, quadSize, maxHeight);
            }
        }

        Surface.GenerateNormals();

        var meshInstance = new MeshInstance();
        meshInstance.Mesh = Surface.Commit();

        return meshInstance;
    }

    private void SeedRng() {
        Rng.Randomize();
        Noise.Seed = unchecked((int) Rng.Randi());
    }

    private int GenerateQuad(int row, int col, int baseIndex, int rows, int cols, float quadSize, float maxHeight) {
        AddVertex(row, col, rows, cols, quadSize, maxHeight);
        AddVertex(row, col + 1, rows, cols, quadSize, maxHeight);
        AddVertex(row + 1, col + 1, rows, cols, quadSize, maxHeight);
        AddVertex(row + 1, col, rows, cols, quadSize, maxHeight);

        Surface.AddIndex(baseIndex + 0);
        Surface.AddIndex(baseIndex + 1);
        Surface.AddIndex(baseIndex + 2);

        Surface.AddIndex(baseIndex + 0);
        Surface.AddIndex(baseIndex + 2);
        Surface.AddIndex(baseIndex + 3);

        return baseIndex + 4;
    }

    private void AddVertex(int row, int col, int rows, int cols, float quadSize, float maxHeight) {
        var position = RowColToVector3(row, col, rows, cols, quadSize);
        var noiseScale = MAX_HEIGHT / 2.0f;

        position.y = Noise.GetNoise2d(position.x, position.z) * noiseScale + noiseScale;

        Surface.AddVertex(position);
    }

    private Vector3 RowColToVector3(int row, int col, int rows, int cols, float quadSize) {
        var columnOffset = -1 * ((cols * quadSize) / 2);
        var rowOffset = (-1 * ((rows * quadSize) / 2));

        return new Vector3(col * quadSize + columnOffset, 0, row * quadSize + rowOffset);
    }
}