﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SocialPlatforms;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

[System.Serializable]
public struct WallPrefab {
	public GameObject Prefab;
	public int Weight;
}

public enum RecursionMethod {
	Random,
	Newest,
	NewAndOld,
	NewAndRandom,
	OldAndRandom
}

[RequireComponent(typeof(BoxCollider))]
public class MazeManager : MonoBehaviour {
	public bool isActive = true;  // To Know if the Controller is On or Off
	public bool looping = true;		// Active Maze Lopping Generation
	public float loopTime;				// Time Between Loop

	public RecursionMethod method;      // Method of neighbor selection
	public int[] mazeSize = new int[2]; // Size of Maze
	public int[] startPos = new int[2]; // Initial position of maze path
	public int[] exitPos = new int[2];  // Where is placed the exit

	public Vector3 wallSize = Vector3.one;                        // Size of the walls
	public List<WallPrefab> wallPrefabs = new List<WallPrefab>(); // List of wallPrefabs
	public GameObject exitPrefab;                                 // Exit Prefab

	readonly int[,] mazeData;                                                        // Maze Array
	readonly List<MazeWall> wallList = new List<MazeWall>();                         // List of walls in maze
	readonly Dictionary<string, int> wallByPosition = new Dictionary<string, int>(); // Dictionary for List of walls based in position

	List<int[]> visitedWalls = new List<int[]>();                   // List of walls visited
	List<int[]> posibleNeighbors = new List<int[]>();               // List of posible moves
	// List<GameObject> debugInstatiate = new List<GameObject>();		// List of walls for Debug

	public float smoothVelocity;  // Walls velocity
	public float stepTime;        // Time between neighbor choose

	void Start() {
		/* Make a list of walls to control better the data */
		int indx = 0;
		for (int i = 0; i < mazeSize[0]; i++) {
			for (int j = 0; j < mazeSize[1]; j++) {
				var wall = new MazeWall {
					// Index in the list
					Index = indx,
					// GameObject of the wall
					Prefab = Instantiate(GetNewRandomWall(new int[] { i, j }.SequenceEqual(exitPos))),
					// Coordinates in the Maze
					MazePosition = new int[] { i, j },
					// Default position in wolrd
					WorldPosition = transform.position + new Vector3(wallSize.x * j, 0, wallSize.z * i)
				};
				wallByPosition.Add(i + ", " + j, indx); // Add coordinates of the wall and the list index in a Dictinary for easy access
				indx++;
				wall.Prefab.GetComponent<WallListener>().controller = this; // Assign the controller to the GameObject
				wall.Prefab.GetComponent<WallListener>().index = wall;      // Assign the MazeWall struct for easy access of data
				wall.Prefab.transform.parent = gameObject.transform;
				wallList.Add(wall); // Add most important: add wall to teh list
			}
		}
	}

	/* Reactive / Active the maze when "Player" is inside */
	void OnTriggerEnter(Collider other) {
		if (other.tag == "Player") {
			isActive = true;
			StartCoroutine(MazeInitializer()); // Call the Maze Initializer
		}
	}

	/* Desactive the maze when "Player" exits the maze */
	void OnTriggerExit(Collider other) {
		if (other.tag == "Player") {
			isActive = false;
			StopAllCoroutines();
		}
	}

	/* Initializer of the maze */
	IEnumerator MazeInitializer() {
		while (isActive) {
			visitedWalls.Clear();       // Reset the visited walls
			posibleNeighbors.Clear();   // Reset the neighbors
			foreach (MazeWall wall in wallList) {
				wall.Prefab.GetComponent<WallListener>().isActive = true;
			}
			yield return StartCoroutine(PrimsAlgorithm(startPos[0], startPos[1])); // Start the Algorithm
			if (!looping) break;
			Debug.LogWarning("Starting Over Maze: " + gameObject.GetInstanceID() + ". \n At: " + Time.realtimeSinceStartup);
			yield return new WaitForSeconds(loopTime);
		}
		yield break;
	}

	IEnumerator PrimsAlgorithm(int row, int col) {
		int[] movement = ChoosePath(row, col);
		visitedWalls.Add(movement);
		int[] nextMove = ChooseNextNeighbor();
		GetWallByPos(movement).Prefab.GetComponent<WallListener>().isActive = false;
		while (posibleNeighbors.Any()) {
			yield return new WaitForSeconds(stepTime);
			movement = ChoosePath(nextMove[0], nextMove[1]);
			if (movement.Length == 0) break; // No more posible moves
			visitedWalls.Add(movement);
			nextMove = ChooseNextNeighbor();
			GetWallByPos(movement).Prefab.GetComponent<WallListener>().isActive = false;
		}
		yield break;
	}

	public int[] ChooseNextNeighbor() {
		int[] nextNeighbor;
		int i;
		switch (method) {
			case RecursionMethod.Random:
				i = Random.Range(0, posibleNeighbors.Count() - 1);
				nextNeighbor = posibleNeighbors[i];
				break;

			case RecursionMethod.Newest:
				nextNeighbor = posibleNeighbors.Last();
				break;

			case RecursionMethod.NewAndOld:
				if (Random.Range(0f, 1f) > 0.5f) {
					nextNeighbor = posibleNeighbors.Last();
				} else {
					nextNeighbor = posibleNeighbors.First();
				}
				break;

			case RecursionMethod.NewAndRandom:
				if (Random.Range(0f, 1f) > 0.75f) {
					nextNeighbor = posibleNeighbors.Last();
				} else {
					i = Random.Range(0, posibleNeighbors.Count() - 1);
					nextNeighbor = posibleNeighbors[i];
				}
				break;

			case RecursionMethod.OldAndRandom:
				if (Random.Range(0f, 1f) > 0.75f) {
					nextNeighbor = posibleNeighbors.Last();
				} else {
					i = Random.Range(0, posibleNeighbors.Count() - 1);
					nextNeighbor = posibleNeighbors[i];
				}
				break;

			default:
				i = Random.Range(0, posibleNeighbors.Count() - 1);
				nextNeighbor = posibleNeighbors[i];
				break;
		}
		return nextNeighbor;
	}

	int[] AddNeighbor(ref List<int[]> neighbors, int wRow, int wColumn, int[] dir) {
		dir = new int[] { dir[0] * 2, dir[1] * 2 };
		if (dir[0] == 0 || wRow + dir[0] > 0 && wRow + dir[0] < mazeSize[0]) {
			if (dir[1] == 0 || wColumn + dir[1] > 0 && wColumn + dir[1] < mazeSize[1]) {
				if (IsInList(visitedWalls, new int[] { wRow + dir[0], wColumn + dir[1] })) {
					neighbors.Add(new int[] { wRow + dir[0], wColumn + dir[1] });
					return new int[] { wRow + (dir[0] / 2), wColumn + (dir[1] / 2) };
				}
			}
		}
		return new int[0];
	}

	public int[] ChoosePath(int row, int col) {
		List<int[]> neighbors = new List<int[]>();
		List<int[]> upWalls = new List<int[]> {
			AddNeighbor(ref neighbors, row, col, new int[] { 1, 0 }), // up
			AddNeighbor(ref neighbors, row, col, new int[] { 0, 1 }), // right
			AddNeighbor(ref neighbors, row, col, new int[] { -1, 0 }), // down
			AddNeighbor(ref neighbors, row, col, new int[] { 0, -1 }) // left
		};

		if (neighbors.Any()) {
			int[] move = neighbors[Random.Range(0, neighbors.Count)];
			posibleNeighbors.Add(move);

			foreach (int[] upwall in upWalls) {
				if (upwall.Any())
					GetWallByPos(upwall).Prefab.GetComponent<WallListener>().isActive = true;
			}

			int[] moveDir = { (row - move[0]) / 2, (col - move[1]) / 2 };
			GetWallByPos(moveDir[0] + move[0], moveDir[1] + move[1]).Prefab.GetComponent<WallListener>().isActive = false;
			return move;
		}
		int j = posibleNeighbors.FindIndex(l => l.SequenceEqual(new int[] { row, col }));
		posibleNeighbors.RemoveAt(j);
		if (posibleNeighbors.Any()) {
			return ChooseNextNeighbor();
		}
		return new int[0];
	}

	public static bool IsInList(List<int[]> list, int[] arr) {
		return list.FindIndex(l => l.SequenceEqual(arr)) == -1;
	}

	GameObject GetNewRandomWall(bool exit) {
		if (!exit) {
			int totalSum = 0;
			foreach (WallPrefab obj in wallPrefabs) {
				totalSum += obj.Weight;
			}

			int guess = Random.Range(0, totalSum - 1);
			totalSum = 0;
			foreach (WallPrefab obj in wallPrefabs) {
				totalSum += obj.Weight;
				if (guess < totalSum)
					return obj.Prefab;
			}
		}
		return exitPrefab;
	}

	MazeWall GetWallByPos(int[] pos) {
		return GetWallByPos(pos[0], pos[1]);
	}

	MazeWall GetWallByPos(int r, int c) {
		string pos = r + ", " + c;
		int indx = wallByPosition[pos];
		return wallList[indx];
		// return walls.Find(l => l.MazePosition.SequenceEqual(new int[] { r, c }));
	}

}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(MazeManager))]
public class PrimsMazeEditor : Editor {
	MazeManager script;
	ReorderableList prefabList;

	void OnEnable() {
		script = (MazeManager)target;

		prefabList = new ReorderableList(serializedObject, serializedObject.FindProperty("wallPrefabs"), true, true, true, true) {
			drawHeaderCallback = (Rect rect) => {
				EditorGUI.LabelField(rect, "Wall Prefabs");
			}
		};
		prefabList.drawElementCallback =
		(Rect rect, int index, bool isActive, bool isFocused) => {
			var element = prefabList.serializedProperty.GetArrayElementAtIndex(index);
			rect.y += 2;
			EditorGUI.LabelField(
					new Rect(rect.width * 0.1f + 15, rect.y, rect.width * 0.14f, EditorGUIUtility.singleLineHeight), "Prefab");
			EditorGUI.PropertyField(
				new Rect(rect.width * 0.24f + 15, rect.y, rect.width * 0.6f, EditorGUIUtility.singleLineHeight),
					element.FindPropertyRelative("Prefab"), GUIContent.none);
			EditorGUI.LabelField(
					new Rect(rect.width * 0.88f + 15, rect.y, rect.width * 0.05f, EditorGUIUtility.singleLineHeight), "%");
			EditorGUI.PropertyField(
					new Rect(rect.width * 0.93f + 15, rect.y, rect.width * 0.1f, EditorGUIUtility.singleLineHeight),
					element.FindPropertyRelative("Weight"), GUIContent.none);
		};

		if (script.mazeSize.Length == 0)
			script.mazeSize = new int[2];
		if (script.startPos.Length == 0)
			script.startPos = new int[2];
		if (script.exitPos.Length == 0)
			script.exitPos = new int[2];
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();
		script.isActive = EditorGUILayout.Toggle("Is Active", script.isActive);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Loop Step Time");
		script.looping = EditorGUILayout.Toggle(script.looping, GUILayout.MaxWidth(15.0f));
		script.loopTime = script.looping && script.loopTime > 0 ? script.loopTime : script.looping ? 0.1f : script.loopTime;
		GUI.enabled = script.looping;
		script.loopTime = EditorGUILayout.FloatField(script.loopTime);
		GUI.enabled = true;
		script.looping = script.loopTime >= 0 && script.looping;
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(5);
		script.method = (RecursionMethod)EditorGUILayout.EnumPopup("Method", script.method);
		GUILayout.Space(5);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Maze Size");
		EditorGUIUtility.labelWidth = 15;
		script.mazeSize = new int[] { EditorGUILayout.IntField("R", script.mazeSize[0]),
																 	EditorGUILayout.IntField("C", script.mazeSize[1]) };
		script.mazeSize[0] = script.mazeSize[0] < 4 ? 4 : script.mazeSize[0] > 500 ? 500 : script.mazeSize[0];
		script.mazeSize[1] = script.mazeSize[1] < 4 ? 4 : script.mazeSize[1] > 500 ? 500 : script.mazeSize[1];
		EditorGUIUtility.labelWidth = 0;
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(5);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Starting Position");
		EditorGUIUtility.labelWidth = 15;
		script.startPos = new int[] { EditorGUILayout.IntField("R", script.startPos[0]),
																	EditorGUILayout.IntField("C", script.startPos[1]) };
		script.startPos[0] = script.startPos[0] < 0 ? 0 : script.startPos[0];
		script.startPos[1] = script.startPos[1] < 0 ? 0 : script.startPos[1];
		script.startPos[0] = script.startPos[0] > script.mazeSize[0] ? script.mazeSize[0] : script.startPos[0];
		script.startPos[1] = script.startPos[1] > script.mazeSize[1] ? script.mazeSize[1] : script.startPos[1];
		EditorGUIUtility.labelWidth = 0;
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Exit Position");
		EditorGUIUtility.labelWidth = 15;
		script.exitPos = new int[] { EditorGUILayout.IntField("R", script.exitPos[0]),
																 EditorGUILayout.IntField("C", script.exitPos[1]) };
		script.exitPos[0] = script.exitPos[0] < 0 ? 0 : script.exitPos[0];
		script.exitPos[1] = script.exitPos[1] < 0 ? 0 : script.exitPos[1];
		script.exitPos[0] = script.exitPos[0] > script.mazeSize[0] ? script.mazeSize[0] : script.exitPos[0];
		script.exitPos[1] = script.exitPos[1] > script.mazeSize[1] ? script.mazeSize[1] : script.exitPos[1];
		EditorGUIUtility.labelWidth = 0;
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(10);

		script.wallSize = EditorGUILayout.Vector3Field("Wall Size", script.wallSize);
		script.smoothVelocity = EditorGUILayout.Slider("Movement Factor", script.smoothVelocity, 0f, 0.7f);
		script.exitPrefab = EditorGUILayout.ObjectField("Exit Prefab", script.exitPrefab, typeof(GameObject), false) as GameObject;
		prefabList.DoLayoutList();
		GUILayout.Space(10);

		script.stepTime = EditorGUILayout.Slider("Step Time", script.stepTime, 0, 5);
		serializedObject.ApplyModifiedProperties();

		if (script.GetComponent<BoxCollider>() != null) {
			script.GetComponent<BoxCollider>().size = new Vector3(script.wallSize.x * script.mazeSize[1], script.wallSize.y * 2, script.wallSize.z * script.mazeSize[0]);
			script.GetComponent<BoxCollider>().center = new Vector3(script.wallSize.x * script.mazeSize[1] * 0.5f - script.wallSize.x * 0.5f, 0,
																															script.wallSize.z * script.mazeSize[0] * 0.5f - script.wallSize.z * 0.5f);
		}
	}
}
#endif
