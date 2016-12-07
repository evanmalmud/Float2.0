using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Linq;
using System.Text;
using Priority_Queue;

public class Astar: MonoBehaviour {

	public GameObject Enemy;
	public GameObject EnemyBullet;
	//The class to be enqueued.
	public class Info
	{
		public string action_name { get; private set;}
		public State info_state { get; private set;}
		public Info(string name, State state)
		{
			action_name = name;
			info_state = state;
		}
	}
	public AIBehavior AIBehavior;
	//public State State;
	//public State newState;
	//Queue
	SimplePriorityQueue<Info> frontier;

	public List<string> path;
	List<string> path_maker;
	public Dictionary<State, State> came_from;
	public Dictionary<State, string> came_from_name; //string name of action that brought it to that state
	public Dictionary<State, float> cost_so_far; //distance
	public Dictionary<State, int> steps_so_far;

	public List<string> good_moves;
	string action_name = null;
	float distance = 0.0f;
	float new_cost = 0.0f;
	float priority = 0.0f;
	Vector2 current_pos;
	string state_name = null;
	State current_state;
	State initial_state;
	public enemySpawn eneScript;
	public List<GameObject> eneList;

	Collider2D[] colliders;
	Collider2D testcol;
	Vector2 testpos;
	Vector2 colliderpos;

	GameObject test_ene;
	Vector2 vel;
	Vector2 pos;
	Vector2 est_pos;

	float STEP_TIME = 0.02f;
	float COLLISION_DIST = 0.8f;
	float GREEDY_HIT = 1000000000f;

	//List<Collider2D> colliders;
	int i = 0;
	int count;
	// Use this for initialization
	void Start () {

		frontier = new SimplePriorityQueue<Info>();

		came_from = new Dictionary<State, State> ();
		came_from_name = new Dictionary<State, string> ();
		cost_so_far = new Dictionary<State, float> ();
		steps_so_far = new Dictionary<State, int> ();
		path = new List<string> ();
		path_maker = new List<string> ();

		//colliders = new Collider2D[300]; //or some large allocation for space
		//colliders = new List<Collider2D> ();
		good_moves = new List<string>();
		good_moves.Add ("moveUpLeft");
		good_moves.Add ("moveUpRight");
		good_moves.Add ("moveDownLeft");
		good_moves.Add ("moveDownRight");
		good_moves.Add ("moveUp");
		good_moves.Add ("moveDown");
		good_moves.Add ("moveLeft");
		good_moves.Add ("moveRight");
		good_moves.Add ("doNothing");

	}


	// Update is called once per frame
	void Update () {

	}

	void FixedUpdate() {
		eneList = eneScript.listOfEnemies;

		//left wall
		if (this.transform.position.x <= -5.0f) {
			good_moves.Remove ("moveUpLeft");
			good_moves.Remove ("moveLeft");
			good_moves.Remove ("moveDownLeft");
		}
		//right wall
		else if (this.transform.position.x >= 5.0f) {
			good_moves.Remove ("moveUpRight");
			good_moves.Remove ("moveRight");
			good_moves.Remove ("moveDownRight");
		}
		//top wall
		else if (this.transform.position.y >= 4f) {
			good_moves.Remove ("moveUpLeft");
			good_moves.Remove ("moveUp");
			good_moves.Remove ("moveUpRight");
		}
		//bottom wall
		else if (this.transform.position.y <= -4.5f) {
			good_moves.Remove ("moveDownLeft");
			good_moves.Remove ("moveDown");
			good_moves.Remove ("moveDownRight");
		} else {
			good_moves.Clear ();
			good_moves.Add ("moveUpLeft");
			good_moves.Add ("moveUpRight");
			good_moves.Add ("moveDownLeft");
			good_moves.Add ("moveDownRight");
			good_moves.Add ("moveUp");
			good_moves.Add ("moveDown");
			good_moves.Add ("moveLeft");
			good_moves.Add ("moveRight");
			good_moves.Add ("doNothing");
		}

	}

	public List<string> Aalgorithm(State state) {
		path.Clear ();
		came_from.Clear ();
		cost_so_far.Clear ();
		steps_so_far.Clear ();
		came_from_name.Clear ();

		initial_state = new State(state.position);
		came_from[initial_state] = null;
		came_from_name[initial_state] = "";
		cost_so_far[initial_state] = 0.0f;
		steps_so_far[initial_state] = 0;
		path.Add ("");
		Info initial = new Info ("", initial_state);
		frontier.Enqueue(initial, 0);
		count = 0;

		while (frontier.Count != 0) {
			++count;
			Info current_info = frontier.Dequeue ();
			state_name = current_info.action_name;
			current_state = current_info.info_state;
			current_pos = current_state.position;

			GameObject[] bulletArray = GameObject.FindGameObjectsWithTag ("EnemyBullet");

			List<GameObject> nearbyThreats = keepThreatsIntoList (current_pos, bulletArray);

			//find nearest enemy within X location amongst bulletArray
			GameObject[] enemyArray = GameObject.FindGameObjectsWithTag ("Enemy");
			if (enemyArray.Length == 0 && bulletArray.Length == 0) {
				path.Add ("doNothing");
				return path;
			}
			float nearestEnemyX = findNearestEnemyX(enemyArray, current_pos);
			// 2 exit conditions; one checking for an enemy, another for no enemy in which case, remain in place dodging
			Vector2 dodge_pos = transform.position;

			if (steps_so_far[current_state] == 2) {
				path = creatingPath (current_state, state_name);
				return path;
			}
				
			for (int i = 0; i < good_moves.Count; ++i) {
				//NEXT = Name, State effected by action, and Time cost
				action_name = good_moves[i];

				State newState = new State(current_state.position);
				AIBehavior.apply_move(action_name, newState);
				// Simulate state here
				new_cost = cost_so_far[current_state] + 0.1f;

				float testValue;
				if ((cost_so_far.TryGetValue(newState, out testValue) == false) || new_cost < cost_so_far [newState]) {
					cost_so_far[newState] = new_cost;
					priority = new_cost + heuristic(newState, nearestEnemyX, nearbyThreats, steps_so_far[current_state], action_name);
					Info newer_info = new Info(action_name, newState);
					steps_so_far[newState] = steps_so_far[current_state]+1;
					came_from[newState] = current_state;
					came_from_name[newState] = action_name;
					frontier.Enqueue(newer_info, priority);
				}
			}
		}
		frontier.Clear ();
		return path;
	}

	//Almost like P5
	private float heuristic(State newState, float en_x, List<GameObject> nearby_bullets, int steps, string action) {
		float output = 0.0f;
		int hitCounter = 0;

		float ai_x = newState.position.x;

		output = Mathf.Abs(ai_x - en_x);
		
		// Check if hit
		for (int i = 0; i < nearby_bullets.Count; ++i) {
			test_ene = nearby_bullets[i];
			vel = test_ene.GetComponent<Rigidbody2D>().velocity;
			pos = test_ene.transform.position;
			est_pos = pos + (vel * STEP_TIME * steps);

			float dist = Vector2.Distance(newState.position, est_pos);

			if (dist <= COLLISION_DIST) {
				return output + GREEDY_HIT;
			}
		}
		return output;
	}


	private List<string> creatingPath(State current, string name) {
		path_maker.Clear ();
		string F_name = name;
		State F_state = current;

		while (came_from[F_state] != null) {
			path_maker.Add(F_name);
			F_state = came_from[F_state];
			F_name = came_from_name[F_state];
		}
		path_maker.Reverse();
		frontier.Clear ();
		return path_maker;
	}

	public List<GameObject> keepThreatsIntoList(Vector2 pos, GameObject[] array){
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject bullet in array) {
			float dist = Vector2.Distance (pos, bullet.transform.position);
			if (dist <= 4.0f) {
				list.Add(bullet);
			}
		}
		return list; 
	}

	private float findNearestEnemyX(GameObject[] ba, Vector2 AI_pos){
		GameObject return_this;
		if (ba.Length == 0) {
			return AI_pos.x;
		}
		return_this = ba [0];
		Vector3 v3pos = new Vector3 (AI_pos.x, AI_pos.y, 0);
		foreach(GameObject enemy in ba){
			float lowest_distance = Vector3.Distance (return_this.transform.position, v3pos);
			float new_distance = Vector3.Distance (enemy.transform.position, v3pos);
			if (new_distance < lowest_distance) {
				return_this = enemy;
			}
		}
		return return_this.transform.position.x;
	}
}