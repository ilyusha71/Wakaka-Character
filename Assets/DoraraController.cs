using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoraraController : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        anim.SetFloat("V", Input.GetAxis("Vertical"));
        anim.SetFloat("H", Input.GetAxis("Horizontal"));
    }
}
