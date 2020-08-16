using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Instanciator : MonoBehaviour
{

    public GameObject myPrefab;
    public GameObject enemy;
    public float enemy_speed;

    public Vector3[] wp = new Vector3[] {
        new Vector3(3,0,0),
        new Vector3(3,0,14),
        new Vector3(15,0,14)
    };

    private MovingActor moving_actor;

    struct MovingActor
    {
        public GameObject actor;
        public List<Vector3> waypoints;
        public delegate void LastWaypointReached(MovingActor actor);

        public LastWaypointReached onGoal;
        private int waypoint_n;
        
        private static void DefaultOnGoal(MovingActor actor)
        {
            Debug.Log("Goal reached !");
        }

        public MovingActor(GameObject prefab, Vector3 at, Vector3[] wp)
        {
            actor = Instantiate(prefab, at, Quaternion.identity);
            waypoints = new List<Vector3>(wp);
            waypoint_n = -1;
            onGoal = DefaultOnGoal;

            NextWaypoint();
        }

        private Vector3 CurrentWaypoint()
        {
            return waypoints[waypoint_n];
        }

        private void NextWaypoint()
        {
            waypoint_n += 1;
            if (ReachedLastWaypoint()) onGoal(this);
            else actor.transform.LookAt(CurrentWaypoint());
        }

        private bool ReachedLastWaypoint()
        {
            return waypoint_n >= waypoints.Count;
        }
        public void Move(float speed)
        {
            if (ReachedLastWaypoint()) return;

            Vector3 current_destination = waypoints[waypoint_n];
            Vector3 current_pos = actor.transform.position;

            if (current_pos != current_destination)
            {

                /* We're currently LookingAt(CurrentWaypoint()) so, we only need
                 * to compute the distance and move forwards the destination.
                 * We make sure to advance at the right speed, in order to avoid
                 * going too far.
                 */
                float distance = Vector3.Distance(current_pos, current_destination);
                float current_speed = distance >= speed ? speed : distance;

                Vector3 step = actor.transform.forward * current_speed;
                actor.transform.position += step;

            }

            /* IDEA : Abuse the Animation State Machine to move the character ? */
            if (current_pos == current_destination) NextWaypoint();
            
        }
    }
    private List<MovingActor> moving_actors;

    // Start is called before the first frame update
    void Start()
    {
        for (var x = 0; x < 16; x++)
            for (var z = 0; z < 16; z++)
                Instantiate(myPrefab, new Vector3(x, 0, z), Quaternion.identity);
        Mesh cube_mesh = myPrefab.GetComponentsInChildren<MeshFilter>()[0].sharedMesh;
        Mesh enemy_mesh = enemy.GetComponentsInChildren<MeshFilter>()[0].sharedMesh;
        float height = (float)((cube_mesh.bounds.size.y / 2.0) + (enemy_mesh.bounds.size.y / 2.0));
        Debug.LogErrorFormat("Bounds size : Cube - {0}, enemy_mesh : {1} - Cube + Half enemy : {2}", cube_mesh, enemy_mesh, height);
        //enemy_obj = Instantiate(enemy, new Vector3(0, height, 0), Quaternion.identity);

        
        moving_actor = new MovingActor(enemy, new Vector3(0, height, 0), wp);

    }

    // Update is called once per frame
    void Update()
    {

        //var transform = enemy_obj.transform.position;
        //transform.x += (enemy_speed * Time.deltaTime);
        //enemy_obj.transform.position = transform;

        moving_actor.Move(enemy_speed * Time.deltaTime);
    }
}
