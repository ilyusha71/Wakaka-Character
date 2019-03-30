using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class DoraraController : MonoBehaviour
{
    private Animator anim;
    public bool move;


    private void Awake()
    {
        anim = GetComponent<Animator>();
        //blendTree.AddChild(jk);
    }

    private void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 mov = new Vector3(h, v, 0).normalized;
        anim.SetFloat("V", v);
        anim.SetFloat("H", h);
        if (!move) return;

        transform.Translate(mov * Time.deltaTime * 3, Space.Self);
        //transform.position += ;
    }
}
