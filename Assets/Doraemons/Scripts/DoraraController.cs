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

    [Header("Leader")]
    public bool isLeader;
    public InputForward inputForward;
    public float outputVertical, outputHorizontal, realVertical, realHorizontal;
    public DoraraController myFollower;
    private bool isMoving = false;
    private Queue<Vector3> cornerPos = new Queue<Vector3>();
    private InputForward lastForward;
    [SerializeField]
    private int order;

    [Header("Follower")]
    public bool follow =false;
    public Vector3 waypoint;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        myTransform = transform;
        //cornerPos.Enqueue(myTransform.position);
        waypoint = myTransform.position;
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
            if (touch != InputForward.Up)
                isColli = false;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            inputForward = InputForward.Down;
            realVertical = -1;
            isMoving = true;
            if (touch != InputForward.Down)
                isColli = false;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            inputForward = InputForward.Right;
            realHorizontal = 1;
            isMoving = true;
            if (touch != InputForward.Right)
                isColli = false;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            inputForward = InputForward.Left;
            realHorizontal = -1;
            isMoving = true;
            if (touch != InputForward.Left)
                isColli = false;
        }
        #endregion

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
        }

    }

    public float dddd;
    private void FixedUpdate()
    {
        if (!isLeader) return;
        // 轉角位置佇列
        if (inputForward != lastForward)
            cornerPos.Enqueue(myTransform.position);
        lastForward = inputForward;

        dddd = Vector3.Distance(myTransform.position, myFollower.myTransform.position);

        Vector3 Displacement = new Vector3(outputHorizontal, outputVertical, 0).normalized * Time.deltaTime * 3;
        //Debug.Log(Displacement);
        //Debug.Log(Displacement.magnitude);
        myTransform.Translate(Displacement, Space.Self);
        anim.SetFloat("V", outputVertical);
        anim.SetFloat("H", outputHorizontal);
        sprite.sortingOrder = (int)(myTransform.position.y * -100) + 1000000;
        if (myFollower)
            myFollower.Follow(isMoving, this,0);
        Debug.Log(Time.frameCount + "/" + dddd);
    }

    public void Follow(bool isMoving, DoraraController followee, int index)
    {
        if (order == 0) order= ++index;
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

            float dis = Vector3.Distance(waypoint, myTransform.position);
            if (dis == 0)
            {
                follow = false;
            }

            if (!follow)
            {
                if (followee.cornerPos.Count != 0)
                {
                    waypoint = followee.cornerPos.Dequeue(); // 讀取前面
                    cornerPos.Enqueue(waypoint); // 存入給後面

                    follow = true;
                }
                else
                {
                    waypoint = followee.myTransform.position;
                }
            }


            outputVertical = Vector3.Dot(new Vector3(0, 1, 0), Vector3.Normalize(waypoint - myTransform.position));
            outputHorizontal = Vector3.Dot(new Vector3(1, 0, 0), Vector3.Normalize(waypoint - myTransform.position));

            if (outputVertical > 0) outputVertical = 1;
            else if (outputVertical < 0) outputVertical = -1;
            else outputVertical = 0;

            if (outputHorizontal > 0) outputHorizontal = 1;
            else if (outputHorizontal < 0) outputHorizontal = -1;
            else outputHorizontal = 0;

            myTransform.position = Vector3.MoveTowards(myTransform.position, waypoint, Time.deltaTime * 3);
        }
        anim.SetFloat("V", outputVertical);
        anim.SetFloat("H", outputHorizontal);
        sprite.sortingOrder = (int)(myTransform.position.y * -100)+1000000- order;
        if (myFollower)
            myFollower.Follow(isMoving, this, order);
    }
    public bool isColli;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isLeader) return;
        //cornerPos.Enqueue(myTransform.position);
        isMoving = false;
        isColli = true;
        touch = inputForward;
        Debug.LogWarning(Time.frameCount+"/"+ dddd);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isLeader) return;
        //cornerPos.Enqueue(myTransform.position);

        isColli = false;
        touch = InputForward.None;
        //Debug.LogWarning(myTransform.position);
    }
    public InputForward touch = InputForward.None;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isLeader)
        {
            isColli = true;
            cornerPos.Enqueue(myTransform.position);
            Debug.Log(myTransform.position);
            Debug.Log(collision.contacts[0].point);

            if (Mathf.Abs(collision.contacts[0].point.x - myTransform.position.x) < 0.5f)
                touch = collision.contacts[0].point.x < myTransform.position.x ? InputForward.Left : InputForward.Right;
            else if (Mathf.Abs(collision.contacts[0].point.y - myTransform.position.y) < 0.5f)
                touch = collision.contacts[0].point.y < myTransform.position.y ? InputForward.Down : InputForward.Up;
            //float xOffset, yOffset;
            //xOffset = collision.contacts[0].point.x < myTransform.position.x ? 0.5f : -0.5f;
            //yOffset = collision.contacts[0].point.y < myTransform.position.y ? 0.5f : -0.5f;

            //myTransform.position = new Vector3(Mathf.RoundToInt(myTransform.position.x + xOffset), Mathf.RoundToInt(myTransform.position.y + yOffset), 0);
            //cornerPos.Enqueue(myTransform.position);
            //Debug.Log(myTransform.position);
            //Debug.Log(collision.contacts[0].point);

        }


        //Debug.Log(myTransform.position);
    }

}