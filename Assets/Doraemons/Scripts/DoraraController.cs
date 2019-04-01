using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public enum InputForward
{
    None,
    Up,
    Down,
    Left,
    Right,
}

public class DoraraController : MonoBehaviour
{
    private SpriteRenderer sprite;
    private Animator anim;
    private Transform myTransform;

    [Header("Status")]
    public float realX, realY;
    public bool isBoundary;
    private int indexWP = 100;
    private bool isStart = true;

    [Header("Leader")]
    public bool isLeader;
    public InputForward inputForward;
    public int outputVertical, outputHorizontal, realVertical, realHorizontal;
    public DoraraController myFollower;
    [SerializeField]
    private float gap;
    private bool isMoving = false;
    private Queue<Vector3> cornerPos = new Queue<Vector3>();
    //private Queue<int> cornerIndex = new Queue<int>();

    private InputForward lastForward;
    private int softLayer;

    [Header("Follower")]
    public bool inOrigin;
    public bool follow =false;
    public Vector3 waypoint;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        myTransform = transform;
        cornerPos.Enqueue(myTransform.position);
        waypoint = myTransform.position;
        realX = (int)(myTransform.position.x * 10);
        realY = (int)(myTransform.position.y * 10);

    }


    private void Update()
    {
        if (!isLeader) return;

        #region Input
        if (Input.GetKeyUp(KeyCode.W))
        {
            realVertical = Input.GetKey(KeyCode.S) ? -1 : 0;

            if (inputForward == InputForward.Up)
            {
                if (realHorizontal == 1)
                    inputForward = InputForward.Right;
                else if (realHorizontal == -1)
                    inputForward = InputForward.Left;
                else if (realVertical == -1)
                    inputForward = InputForward.Down;
                else
                    isMoving = false;
            }
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            realVertical = Input.GetKey(KeyCode.W) ? 1 : 0;

            if (inputForward == InputForward.Down)
            {
                if (realHorizontal == 1)
                    inputForward = InputForward.Right;
                else if (realHorizontal == -1)
                    inputForward = InputForward.Left;
                else if (realVertical == 1)
                    inputForward = InputForward.Up;
                else
                    isMoving = false;
            }
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            realHorizontal = Input.GetKey(KeyCode.A) ? -1 : 0;

            if (inputForward == InputForward.Right)
            {
                if (realVertical == 1)
                    inputForward = InputForward.Up;
                else if (realVertical == -1)
                    inputForward = InputForward.Down;
                else if (realHorizontal == -1)
                    inputForward = InputForward.Left;
                else
                    isMoving = false;
            }
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            realHorizontal = Input.GetKey(KeyCode.D) ? 1 : 0;

            if (inputForward == InputForward.Left)
            {
                if (realVertical == 1)
                    inputForward = InputForward.Up;
                else if (realVertical == -1)
                    inputForward = InputForward.Down;
                else if (realHorizontal == 1)
                    inputForward = InputForward.Right;
                else
                    isMoving = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            inputForward = InputForward.Up;
            realVertical = 1;
            isMoving = true;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            inputForward = InputForward.Down;
            realVertical = -1;
            isMoving = true;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            inputForward = InputForward.Right;
            realHorizontal = 1;
            isMoving = true;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            inputForward = InputForward.Left;
            realHorizontal = -1;
            isMoving = true;
        }
        #endregion
    }

 
    private void FixedUpdate()
    {
        if (!isLeader) return;
        if (isBoundary) isMoving = false;
        if (!isMoving)
        {
            outputVertical = 0;
            outputHorizontal = 0;
        }
        else
        {
            switch (inputForward)
            {
                case InputForward.Up: outputVertical = 1; outputHorizontal = 0; break;
                case InputForward.Down: outputVertical = -1; outputHorizontal = 0; break;
                case InputForward.Right: outputHorizontal = 1; outputVertical = 0; break;
                case InputForward.Left: outputHorizontal = -1; outputVertical = 0; break;
            }
            
            if (inputForward != lastForward)
            {
                if (isStart) isStart = !isStart;
                else
                {
                    // 轉角位置佇列
                    //indexWP++;
                    cornerPos.Enqueue(myTransform.position);
                    //cornerIndex.Enqueue(indexWP);
                    //Debug.LogWarning("SetWP/" + indexWP + "/Frame: " + Time.frameCount + "/Pos: " + myTransform.position);
                }
            }
            lastForward = inputForward;
        }

        Move();
    }
    private int countdown=0;
    public void Follow(bool move, DoraraController followee, int index)
    {
        if (isStart)
        {
            isStart = !isStart;
            softLayer = ++index;
            sprite.sortingOrder = (int)(myTransform.position.y * -100) + 1000000 - softLayer;

            //if (softLayer == 6 || softLayer == 7)
            //    Debug.LogWarning("WP/" + softLayer + "/Pos: " + waypoint.ToString("F2") + "/" + name);

            waypoint = followee.cornerPos.Dequeue(); // 讀取followee作為導航點

            // 判斷是否與前一個角色位於同一位置
            if (waypoint == myTransform.position)
            {
                inOrigin = true;
                // 相同位置，不保存（因為countdown結束會再保存）
            }
            else
            {
                cornerPos.Enqueue(waypoint); // 保存為follower的導航點
                if (softLayer == 6 || softLayer == 7)
                    Debug.LogWarning("SP/" + softLayer + "/Pos: " + myTransform.position.ToString("F2") + "/" + name);
            }
                
        }

        isMoving = move;
        if (!isMoving)
        {
            outputVertical = 0;
            outputHorizontal = 0;
        }
        else
        {
            if (inOrigin)
            {
                countdown++;
                if (countdown <= 20)
                    return;
                else
                    inOrigin = false;
            }

            float dis = Vector3.Distance(waypoint, myTransform.position);

            follow = dis == 0 ? false : true;
            //if (dis == 0)
            //{
            //    follow = false;
            //    //cornerPos.Enqueue(waypoint); // 存入給後面
            //}
            //else
            //    follow = true;

            if (!follow)
            {
                follow = true;

                if (followee.cornerPos.Count != 0)
                {
                    waypoint = followee.cornerPos.Dequeue(); // 讀取followee作為導航點
                    cornerPos.Enqueue(waypoint); // 保存為follower的導航點

                    //if (softLayer == 6 || softLayer == 7)
                    //{
                    //    Debug.LogWarning("WP/" + softLayer + "/Pos: " + waypoint.ToString("F2") + "/" + name);
                    //    Debug.LogWarning("SP/" + softLayer + "/Pos: " + myTransform.position.ToString("F2") + "/" + name);
                    //}

                    if (waypoint == myTransform.position)
                    {
                        //Debug.LogWarning(softLayer + "/ SDS");
                        waypoint = followee.myTransform.position;
                    }
                }
                else
                {
                    waypoint = followee.myTransform.position;
                    //if (softLayer == 6 || softLayer == 7)
                    //    Debug.Log("XP/" + softLayer + "/Pos: " + waypoint.ToString("F2") + "/" + name);
                }
            }
            outputVertical = (int)Vector3.Dot(new Vector3(0, 1, 0), Vector3.Normalize(waypoint - myTransform.position));
            outputHorizontal = (int)Vector3.Dot(new Vector3(1, 0, 0), Vector3.Normalize(waypoint - myTransform.position));
            //if (softLayer == 2)
            //    Debug.Log(index + "/   " + countdown + "/   " + myTransform.position.ToString("F2") + "/" + followee.myTransform.position.ToString("F2") + "/" + waypoint.ToString("F2"));
        }

        Move();
        //if (isMoving)
        //    Debug.Log(index + "/   " + countdown + "/   " + myTransform.position.ToString("F2") + "/" + followee.myTransform.position.ToString("F2") + "/" + waypoint.ToString("F2"));
    }

    public void Move()
    {
        //Debug.Log("Follower/Frame: " + Time.frameCount + "/Gap: " + gap + "/Move: " + isMoving + "/Pos: " + myTransform.position);

        // 移動
        realX += outputHorizontal *0.5f;
        realY += outputVertical*0.5f;
        myTransform.position = new Vector3(realX * 0.1f, realY * 0.1f, 0);

        // 動畫
        anim.SetFloat("V", outputVertical);
        anim.SetFloat("H", outputHorizontal);
        sprite.sortingOrder = (int)(myTransform.position.y * -100) + 1000000 - softLayer;

        // 跟隨
        if (myFollower)
        {
            gap = Vector3.Distance(myTransform.position, myFollower.myTransform.position);
            myFollower.Follow(isMoving, this, softLayer);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isLeader) return;
        //cornerPos.Enqueue(myTransform.position);
        //isMoving = false;
        isBoundary = true;
        touch = inputForward;

        int touchVertical=0, touchHorizontal=0;

        switch (inputForward)
        {
            case InputForward.Up: touchVertical = -1; touchHorizontal = 0; inputForward = InputForward.Down; break;
            case InputForward.Down: touchVertical = 1; touchHorizontal = 0; inputForward = InputForward.Up; break;
            case InputForward.Right: touchHorizontal = -1; touchVertical = 0; inputForward = InputForward.Left; break;
            case InputForward.Left: touchHorizontal = 1; touchVertical = 0; inputForward = InputForward.Right; break;
        }
        lastForward = inputForward;

        //indexWP++;
        cornerPos.Enqueue(myTransform.position);
        //cornerIndex.Enqueue(indexWP);
        //Debug.LogWarning("SetWP/" + indexWP + "/Frame: " + Time.frameCount + "/Pos: " + myTransform.position);

        //gap = Vector3.Distance(myTransform.position, myFollower.myTransform.position);
        //Debug.LogWarning("Leader/Frame: " + Time.frameCount + "/Gap: " + gap + "/Move: " + isMoving + "/Pos: " + myTransform.position);

        // 移動
        realX += touchHorizontal*0.5f;
        realY += touchVertical*0.5f;
        myTransform.position = new Vector3(realX * 0.1f, realY * 0.1f, 0);

        // 動畫
        anim.SetFloat("V", touchVertical);
        anim.SetFloat("H", touchHorizontal);
        sprite.sortingOrder = (int)(myTransform.position.y * -100) + 1000000;

        // 跟隨
        if (myFollower)
            myFollower.Follow(isMoving, this, 0);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isLeader) return;
        //Debug.LogWarning("OUT1/Frame: " + Time.frameCount + "/Pos: " + myTransform.position);
        isBoundary = false;
    }
    public InputForward touch = InputForward.None;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isLeader)
        {
            isBoundary = true;
            cornerPos.Enqueue(myTransform.position);
            Debug.Log(myTransform.position);
            Debug.Log(collision.contacts[0].point);

            if (Mathf.Abs(collision.contacts[0].point.x - myTransform.position.x) < 0.5f)
                touch = collision.contacts[0].point.x < myTransform.position.x ? InputForward.Left : InputForward.Right;
            else if (Mathf.Abs(collision.contacts[0].point.y - myTransform.position.y) < 0.5f)
                touch = collision.contacts[0].point.y < myTransform.position.y ? InputForward.Down : InputForward.Up;


        }


        //Debug.Log(myTransform.position);
    }
}