﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [CreateAssetMenu(fileName = "Maza Manager", menuName = "Maze", order = 1)]
public class MazeManager : MonoBehaviour {
	public GameObject pieceTrap;
	public GameObject pieceWall;
	public GameObject initialWall;
	[Range(10, 100)]
	public int width = 10;
	[Range(10, 100)]
	public int height = 10;
	public int[] initialPos = new int[2];
	[Range(5, 100)]
	public float timer;

	[Range(0, 1)]
	public float trapComplexity = 0.75f;
	[Range(0, 1)]
	public float trapDensity = 0.75f;

	// public bool CreateMaze;
	[HideInInspector]
	public List<GameObject> traps;
	[HideInInspector]
	public List<GameObject> walls;

	int[,] maze;

	//void Update() {
	//	foreach(GameObject wall in walls) {
	//		if (wall.GetComponentInChildren<Detector>().pos == initialPos) {
	//			wall.GetComponentInChildren<Renderer>().material = Resources.Load("Materials/Green") as Material;
	//		} else {
	//			wall.GetComponentInChildren<Renderer>().material = Resources.Load("Materials/White") as Material;
	//		}
	//	}
	//}

	void Start() {
		if (pieceWall != null) {
			BuildMaze(MazeCreator());
			StartCoroutine(MazeTimer());
		}
		initialWall.transform.position = new Vector3(initialPos[0], 1, initialPos[1]);
	}

	IEnumerator MazeTimer() {
		while (true) {
			yield return new WaitForSeconds(timer);
			ModifyMaze(MazeCreator());
		}
	}

	public int[,] MazeCreator() {
		maze = new int[height, width];
		// Initialize
		for (int i = 0; i < height; i++)
			for (int j = 0; j < width; j++)
				maze[i, j] = 1;

		System.Random rand = new System.Random();
		// r for row、c for column
		maze[initialPos[0], initialPos[1]] = 0;

		// Allocate the maze with recursive method
		RecursivePath(initialPos[0], initialPos[1]);

		return maze;
	}

	public void RecursivePath(int r, int c) {
		// 4 random directions
		int[] randDirs = generateRandomDirections();
		// Examine each direction
		for (int i = 0; i < randDirs.GetLength(0); i++) {

			switch (randDirs[i]) {
				case 1: // Up
								// Whether 2 cells up is out or not
					if (r - 2 <= 0)
						continue;
					if (maze[r - 2, c] != 0) {
						maze[r - 2, c] = 0;
						maze[r - 1, c] = 0;
						RecursivePath(r - 2, c);
					}
					break;
				case 2: // Right
								// Whether 2 cells to the right is out or not
					if (c + 2 >= width - 1)
						continue;
					if (maze[r, c + 2] != 0) {
						maze[r, c + 2] = 0;
						maze[r, c + 1] = 0;
						RecursivePath(r, c + 2);
					}
					break;
				case 3: // Down
								// Whether 2 cells down is out or not
					if (r + 2 >= height - 1)
						continue;
					if (maze[r + 2, c] != 0) {
						maze[r + 2, c] = 0;
						maze[r + 1, c] = 0;
						RecursivePath(r + 2, c);
					}
					break;
				case 4: // Left
								// Whether 2 cells to the left is out or not
					if (c - 2 <= 0)
						continue;
					if (maze[r, c - 2] != 0) {
						maze[r, c - 2] = 0;
						maze[r, c - 1] = 0;
						RecursivePath(r, c - 2);
					}
					break;
			}
		}
	}

	/**
	* Generate an array with random directions 1-4
	* @return Array containing 4 directions in random order
	*/
	public int[] generateRandomDirections() {
		List<int> randoms = new List<int>();
		for (int i = 0; i < 4; i++)
			randoms.Add(i + 1); int n = randoms.Count;
		while (n > 1) {
			n--;
			int k = Random.Range(0, n + 1);
			int value = randoms[k];
			randoms[k] = randoms[n];
			randoms[n] = value;
		}
		return randoms.ToArray();
	}

	public bool[,] TrapPositioning() {
		// Only odd shapes
		int[] shape = { (height / 2) * 2 + 1, (width / 2) * 2 + 1 };
		// Adjust complexity and density relative to maze size
		var complexity = trapComplexity * (5 * (shape[0] + shape[1]));
		var density = trapDensity * (((shape[0 / 2])) * ((shape[1 / 2])));
		// Build actual maze
		bool[,] Z = new bool[shape[0], shape[1]];
		// Fill borders
		for (int col = 0; col < shape[1]; col++) {
			Z[0, col] = true;
			Z[Z.GetLength(0) -1, col] = true;
		}

		for (int row = 0; row < shape[0]; row++) {
			Z[row, 0] = true;
			Z[row, Z.GetLength(1) -1] = true;
		}
		// Make aisles
		for (int i = 0; i < density; i++) {
			int x = Random.Range(0, shape[1] / 2) * 2;
			int y = Random.Range(0, shape[0] / 2) * 2;
			Z[y, x] = true;

			for (int j = 0; j < density; j++) {
				List<int[]> neighbours = new List<int[]>();

				if (x > 1) {
					int[] arrayInt = { y, x - 2 };
					neighbours.Add(arrayInt);
				}
				if (x < shape[1] - 2) {
					int[] arrayInt = { y, x + 2 };
					neighbours.Add(arrayInt);
				}
				if (y > 1) {
					int[] arrayInt = { y - 2, x };
					neighbours.Add(arrayInt);
				}
				if (y < shape[0] - 2) {
					int[] arrayInt = { y - 2, x };
					neighbours.Add(arrayInt);
				}
				if (neighbours.Count == 0) {
					int[] randomNeighbor = neighbours[Random.Range(0, neighbours.Count - 1)];
					int y_ = randomNeighbor[1];
					int x_ = randomNeighbor[0];

					if (!Z[y_, x_]) {
						Z[y_, x_] = true;
						Z[y_ + (y - y_) / 2, x_ + (x - x_) / 2] = true;
						x = x_;
						y = y_;
					}
				}
			}
		}
		return Z;
	}

	public void BuildMaze(int[,] maze) {
		for (int i = 0; i < maze.GetLength(0); i++) {
			for (int j = 0; j < maze.GetLength(1); j++) {
				walls.Add(Instantiate(pieceWall, transform.position + new Vector3(i * 1f, 0, j * 1f), Quaternion.identity));
				var wall = walls[walls.Count - 1].GetComponentInChildren<Detector>();
				wall.manager = this;
				wall.pos = new int[] { i, j };
				wall.isActive = maze[i, j] != 0; 
			}
		}
	}

	public void ModifyMaze(int[,] maze) {
		for (int i = 0; i < maze.GetLength(0); i++) {
			for (int j = 0; j < maze.GetLength(1); j++) {
				var wall = walls[i * maze.GetLength(1) + j].GetComponentInChildren<Detector>();
				wall.isActive = maze[i, j] != 0;
			}
		}
	}
}