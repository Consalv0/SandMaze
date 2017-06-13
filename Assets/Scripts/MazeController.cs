﻿﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using UnityEngine.SocialPlatforms;
#endif

[System.Serializable]
public struct WallPrefs {
	public GameObject Prefab;
	public float Weight;
}

public enum RecursionMethod {
	Random,
	Newest,
	Oldest,
	NewOld,
	NewRandom,
	OldRandom
}

public class MazeController : MonoBehaviour {
	public RecursionMethod method;
	public int[] mazeSize = new int[2];
	public int[] startPos = new int[2];
	public int[] exitPos = new int[2];

	public List<WallPrefs> wallPrefabs = new List<WallPrefs>();

	int[,] mazeData;
	List<int[]> visitedWalls = new List<int[]>();
	List<int[]> posibleNeighbors = new List<int[]>();
	public List<GameObject> walls;


	List<GameObject> instantiaed = new List<GameObject>();
	public float stepTime;

	void Start() {
		StartCoroutine(BeginMaze());
		var mazeBorders = MakeBorders();
		for (int i = 0; i < mazeBorders.GetLength(0); i++) {
			for (int j = 0; j < mazeBorders.GetLength(1); j++) {
				if (mazeBorders[i, j] == 1)
					Instantiate(wallPrefabs.Last().Prefab, new Vector3(transform.position.x + j, transform.position.y, transform.position.z + i), Quaternion.identity);
			}
		}
	}

	IEnumerator BeginMaze() {
		int[,] _maze = new int[mazeSize[0] + 1, mazeSize[1] + 1];
		yield return StartCoroutine(TreeMazeAlgorithm(_maze));
		yield return new WaitForSeconds(5);
	}

	public int[,] MakeBorders() {
		int[,] _maze = new int[mazeSize[0] + 1, mazeSize[1] + 1];
		for (int col = 0; col < mazeSize[1] + 1; col++) {
			_maze[0, col] = 1;
			_maze[_maze.GetLength(0) - 1, col] = 1;
		}
		for (int row = 0; row < mazeSize[0] + 1; row++) {
			_maze[row, 0] = 1;
			_maze[row, _maze.GetLength(1) - 1] = 1;
		}
		return _maze;
	}

	IEnumerator TreeMazeAlgorithm(int[,] _maze) {
		_maze[startPos[0], startPos[1]] = 1;
		_maze[exitPos[0], exitPos[1]] = 1;

		Instantiate(wallPrefabs.Last().Prefab, new Vector3(transform.position.x + startPos[1], transform.position.y, transform.position.z + startPos[0]), Quaternion.identity);
		visitedWalls.Add(startPos);
		yield return StartCoroutine(RecursivePath(startPos[0], startPos[1]));
	}

	IEnumerator RecursivePath(int row, int col) {
		int[] movement = ChoosePath(row, col);
		visitedWalls.Add(movement);
		int[] nextMove = ChooseNextNeighbor();
		Instantiate(wallPrefabs.Last().Prefab, new Vector3(transform.position.x + movement[1], transform.position.y, transform.position.z + movement[0]), Quaternion.identity);
		while (posibleNeighbors.Count() != 0) {
			yield return new WaitForSeconds(stepTime);
			Debug.Log(nextMove[0] + ", " + nextMove[1]);
			movement = ChoosePath(nextMove[0], nextMove[1]);
			if (movement.Length == 0) break;
			visitedWalls.Add(movement);
			nextMove = ChooseNextNeighbor();
			Instantiate(wallPrefabs.Last().Prefab, new Vector3(transform.position.x + movement[1], transform.position.y, transform.position.z + movement[0]), Quaternion.identity);

			foreach (GameObject obj in instantiaed) {
				Destroy(obj.gameObject);
			}
			instantiaed.Clear();
			foreach(int[] nei in posibleNeighbors) {
				GameObject neighbor = Instantiate(wallPrefabs.First().Prefab, new Vector3(transform.position.x + nei[1], transform.position.y + 0.5f, transform.position.z + nei[0]), Quaternion.identity);
				instantiaed.Add(neighbor);
			}
		}
		foreach (GameObject obj in instantiaed) {
			Destroy(obj.gameObject);
		}

		Debug.Log(visitedWalls.Count);
		yield return new WaitForSeconds(stepTime);
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

			case RecursionMethod.Oldest:
				nextNeighbor = posibleNeighbors.First();
				break;

			case RecursionMethod.NewOld:
				if (Random.Range(0f, 1f) > 0.5f) {
					nextNeighbor = posibleNeighbors.Last();
				} else {
					nextNeighbor = posibleNeighbors.First();
				}
				break;

			case RecursionMethod.NewRandom:
				if (Random.Range(0f, 1f) > 0.75f) {
					nextNeighbor = posibleNeighbors.Last();
				} else {
					i = Random.Range(0, posibleNeighbors.Count() - 1);
					nextNeighbor = posibleNeighbors[i];
				}
				break;

			case RecursionMethod.OldRandom:
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

	public int[] ChoosePath(int row, int col) {
		List<int[]> neighbors = new List<int[]>();

		if (row + 2 > 0 && row + 2 < mazeSize[0]) // up
			if (IsInList(visitedWalls, new int[] { row + 2, col }))
				neighbors.Add(new int[] { row + 2, col });
		if (row - 2 > 0 && row - 2 < mazeSize[0]) // down
			if (IsInList(visitedWalls, new int[] { row - 2, col }))
				neighbors.Add(new int[] { row - 2, col });
		if (col + 2 > 0 && col + 2 < mazeSize[1]) // right
			if (IsInList(visitedWalls, new int[] { row, col + 2 }))
				neighbors.Add(new int[] { row, col + 2 });
		if (col - 2 > 0 && col - 2 < mazeSize[1]) // left
			if (IsInList(visitedWalls, new int[] { row, col - 2 }))
				neighbors.Add(new int[] { row, col - 2 });

		if (neighbors.Count != 0) {
			int[] move = neighbors[Random.Range(0, neighbors.Count)];
			posibleNeighbors.Add(move);

			int[] moveDir = { row - move[0], col - move[1] };
			Debug.Log("Dir: " + moveDir[0] + ", " + moveDir[1]);
 			Instantiate(wallPrefabs.Last().Prefab, new Vector3(transform.position.x - moveDir[1] * 0.5f + col, transform.position.y,
																												 transform.position.z - moveDir[0] * 0.5f + row), Quaternion.identity);
			return move;
		}
		int j = posibleNeighbors.FindIndex(l => l.SequenceEqual(new int[]{row, col}));
		posibleNeighbors.RemoveAt(j);
		if (posibleNeighbors.Count() != 0) {
			return ChooseNextNeighbor();
		}
		return new int[0];
	}

	static bool IsInList(List<int[]> list, int[] arr) {
		return list.FindIndex(l => l.SequenceEqual(arr)) == -1;
	}
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(MazeController))]
public class PrimsMazeEditor : Editor {
	MazeController script;
	ReorderableList prefabList;

	void OnEnable() {
		script = (MazeController)target;

		prefabList = new ReorderableList(serializedObject,
						serializedObject.FindProperty("wallPrefabs"),
						true, true, true, true);
		prefabList.drawHeaderCallback = (Rect rect) => {
			EditorGUI.LabelField(rect, "Wall Prefabs");
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
		script.method = (RecursionMethod)EditorGUILayout.EnumPopup("Method", script.method);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Maze Size");
		EditorGUIUtility.labelWidth = 15;
		script.mazeSize = new int[] { EditorGUILayout.IntField("R", script.mazeSize[0]),
																 	EditorGUILayout.IntField("C", script.mazeSize[1]) };
		script.mazeSize[0] = script.mazeSize[0] < 4 ? 4 : script.mazeSize[0] > 500 ? 500 : script.mazeSize[0];
		script.mazeSize[1] = script.mazeSize[1] < 4 ? 4 : script.mazeSize[1] > 500 ? 500 : script.mazeSize[1];
		EditorGUIUtility.labelWidth = 0;
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(10);

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

		prefabList.DoLayoutList();
		GUILayout.Space(10);

		script.stepTime = EditorGUILayout.Slider("Step Time", script.stepTime, 0, 100);
		serializedObject.ApplyModifiedProperties();
	}
}
#endif
